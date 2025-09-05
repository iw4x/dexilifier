namespace DX9ShaderHLSLifier
{
    using DX9ShaderHLSLifier.Models;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using System.Xml.Linq;

    public partial class TechniqueSetView : UserControl
    {
        private Explorer.TechniqueSetTreeNode techsetTreeNode;

        private string basePath;

        public TechniqueSetView()
        {
            InitializeComponent();

            mappingTable.SuspendLayout();

            mappingTable.RowCount = 0;
            mappingTable.RowStyles.Clear();
            mappingTable.Controls.Clear();

            mappingTable.ResumeLayout(true);

            Enabled = false;
        }

        internal void LoadTechniqueSet(string basePath, Explorer.TechniqueSetTreeNode techSetNode)
        {
            this.techsetTreeNode = techSetNode;
            this.basePath = basePath;

            mappingTable.SuspendLayout();

            mappingTable.RowCount = 0;
            mappingTable.RowStyles.Clear();
            mappingTable.Controls.Clear();

            this.techsetTreeNode = techSetNode;

            foreach (Explorer.TechniqueTreeNode techTreeNode in techSetNode.Techniques)
            {
                AddTechniqueTreeNodeToList(techTreeNode);
            }

            //AnalyzeTechniqueSetShaders(techSetNode);

            mappingTable.ResumeLayout(true);

            Enabled = true;
        }

        private void AddTechniqueTreeNodeToList(Explorer.TechniqueTreeNode techNode)
        {
            //add a new RowStyle as a copy of the previous one

            mappingTable.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;

            for (int i = 0; i < techNode.usedInTypes.Count; i++)
            {
                MaterialTechniqueType type = (MaterialTechniqueType)techNode.usedInTypes[i];

                Label name = new Label();
                name.AutoSize = false;
                name.Dock = DockStyle.Fill;
                name.Text = type.ToString().Replace("TECHNIQUE_", "");
                name.TextAlign = ContentAlignment.TopRight;

                mappingTable.RowCount++;
                mappingTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
                mappingTable.Controls.Add(name);

                if (i == 0)
                {
                    Label techniqueLabel = new Label();
                    techniqueLabel.AutoSize = false;
                    techniqueLabel.Dock = DockStyle.Fill;
                    techniqueLabel.Text = techNode.technique.name;
                    techniqueLabel.TextAlign = ContentAlignment.TopLeft;

                    techniqueLabel.Margin = new Padding(0);
                    techniqueLabel.Padding = new Padding(3);

                    mappingTable.Controls.Add(techniqueLabel);

                    mappingTable.SetRowSpan(techniqueLabel, techNode.usedInTypes.Count);
                }
            }

            mappingTable.RowCount++;
            mappingTable.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        }
   
        private void AnalyzeTechniqueSetShaders(Explorer.TechniqueSetTreeNode techSetNode)
        {
            Task.Run(()=>TechniqueSetAnalyzer.Analyze(this.basePath, techSetNode));
        }

        private void exportAllFromTechsetButton_Click(object sender, EventArgs e)
        {
            IReadOnlyList<Explorer.TechniqueTreeNode> techniques = this.techsetTreeNode.Techniques;

            // Cannot use hashset here cause order is important
            HashSet<string> allPixelShaders = new HashSet<string>();
            HashSet<string> allVertexShaders = new HashSet<string>();

            for (int i = 0; i < techniques.Count; i++)
            {
                {
                    allPixelShaders.Add(techniques[i].technique.PixelShader);
                }

                {
                    allVertexShaders.Add(techniques[i].technique.VertexShader);
                }
            }

            int total = allPixelShaders.Count + allVertexShaders.Count;
            Task[] tasks = new Task[total];
            float[] progresses = new float[total];

            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.SelectedPath = basePath;

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            string exportPath = dialog.SelectedPath;
            if (!Directory.Exists(exportPath))
            {
                Directory.CreateDirectory(exportPath);
            }

            var dial = ProgressBarDialog.ShowProgressDialog(ParentForm, () => progresses.Sum());
            
            OptimizationParameters parameters = (ParentForm as MainWindow).CurrentParameters;

            int shaderIndex = 0;
            foreach(var px in allPixelShaders)
            {
                string path = System.IO.Path.Combine(basePath, "ps", px + ".cso");
                int myIndex = shaderIndex;
                tasks[myIndex] = Task.Run(async () =>
                {
                    DecompiledShader? shader = await ShaderWork.RecreateShaderTask(path, parameters, (p)=> progresses[myIndex] = p);
                    if (shader.HasValue)
                    {
                        Directory.CreateDirectory(Path.Combine(exportPath, "ps"));
                        using(FileStream fs = File.OpenWrite(Path.Combine(exportPath, "ps", px)))
                        {
                            shader.Value.Export(fs);
                        }
                    }
                });

                shaderIndex++;
            }

            foreach (var vx in allVertexShaders)
            {
                string path = System.IO.Path.Combine(basePath, "vs", vx + ".cso");
                int myIndex = shaderIndex;
                tasks[myIndex] = Task.Run(async () =>
                {
                    DecompiledShader? shader = await ShaderWork.RecreateShaderTask(path, parameters, (p) => progresses[myIndex] = p);
                    if (shader.HasValue)
                    {
                        Directory.CreateDirectory(Path.Combine(exportPath, "vs"));
                        using (FileStream fs = File.OpenWrite(Path.Combine(exportPath, "vs", vx)))
                        {
                            shader.Value.Export(fs);
                        }
                    }
                });

                shaderIndex++;
            }


            Task.Run(() =>
            {
                Task.WaitAll(tasks);
                dial.HideProgressDialog();
                Process.Start("explorer.exe", exportPath);
            });
        }
    }
}
