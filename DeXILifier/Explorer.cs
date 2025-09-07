namespace DeXILifier
{
    using DeXILifier.Models;
    using Newtonsoft.Json;
    using SharpDX.Direct3D9;
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Reflection.Emit;
    using System.Security.Cryptography;
    using System.Text;
    using System.Windows.Forms;

    internal class Explorer
    {
        const byte TECHNIQUESET_IMAGE = 0;
        const byte TECHNIQUE_IMAGE = 1;
        const byte VS_IMAGE = 2;
        const byte PS_IMAGE = 3;
        const byte UNKNOWN_IMAGE = 4;

        public class TechniqueSetTreeNode : TreeNode
        {

            public IReadOnlyList<TechniqueTreeNode> Techniques => Nodes.OfType<TechniqueTreeNode>().ToList();

            public readonly string originalPath;

            public readonly TechniqueSet techset;

            public TechniqueSetTreeNode(string originalPath, TechniqueSet technique) : base(technique.name, TECHNIQUESET_IMAGE, TECHNIQUESET_IMAGE)
            {
                this.originalPath = originalPath;
                this.techset = technique;
            }
        }

        public class TechniqueTreeNode : TreeNode
        {
            public readonly string originalPath;

            public readonly List<MaterialTechniqueType> usedInTypes = new List<MaterialTechniqueType>();
            public readonly Technique technique;

            public TechniqueTreeNode(string originalPath, Technique technique) : base(technique.name, TECHNIQUE_IMAGE, TECHNIQUE_IMAGE)
            {
                this.originalPath = originalPath;
                this.technique = technique;
            }
        }

        public delegate void OnSelectedFileDelegate<T>(string folderPath, T file);
        public event OnSelectedFileDelegate<TechniqueSetTreeNode> OnSelectedTechniqueSet;
        public event OnSelectedFileDelegate<TechniqueTreeNode> OnSelectedTechnique;

        public event Action<string> OnSelectedFile;

        private readonly Color loadingColor = Color.DarkGray;
        private readonly Color goodColor = Color.Green;
        private readonly Color failColor = Color.Orange;
        private readonly Color hardErrorColor = Color.Red;

        private readonly Dictionary<TreeNode, string> filenameForNode = new Dictionary<TreeNode, string>();

        private readonly TreeView treeView;

        private readonly TabControl tabView;

        private readonly TabPage compiledShaderTab;

        private readonly TabPage treeViewTab;

        private bool singleFileMode = false;

        public Explorer(MainWindow mainWindow)
        {
            treeView = mainWindow.explorerTreeView;
            treeViewTab = mainWindow.explorerTab;
            compiledShaderTab = mainWindow.compiledShaderTab;
            tabView = mainWindow.tabView;

            tabView.SelectedIndexChanged += TabView_TabIndexChanged;
        }

        private void TabView_TabIndexChanged(object sender, EventArgs e)
        {
            if (tabView.SelectedTab == treeViewTab)
            {
                if (treeView.SelectedNode is TreeNode node)
                {
                    treeView.Focus();
                    //= node;
                }
            }
        }

        public void Initialize()
        {
            treeView.Nodes.Clear();

            treeView.AfterSelect += TreeView_AfterSelect;

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length > 1)
            {
                if (Directory.Exists(args[1]))
                {
                    ExploreFolder(args[1]);
                }
                else if (File.Exists(args[1]))
                {
                    OpenFile(args[1]);
                }
            }
        }

        private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (filenameForNode.TryGetValue(e.Node, out string path))
            {
                if (e.Node is TechniqueSetTreeNode techset)
                {
                    OnSelectedTechniqueSet?.Invoke(Path.GetDirectoryName(Path.GetDirectoryName(path)), techset);
                }
                else if (e.Node is TechniqueTreeNode technique)
                {
                    OnSelectedTechnique?.Invoke(Path.GetDirectoryName(Path.GetDirectoryName(path)), technique);
                }
                else
                {
                    OnSelectedFile?.Invoke(path);
                }
            }
        }

        public void OpenFile(string path)
        {
            treeView.Nodes.Clear();

            if (path.EndsWith(".iw4x.json"))
            {
                OpenIW4OFFile(path);
            }
            else if (path.EndsWith(".cso"))
            {
                OpenCompiledShader(path);
            }
        }

        private void OpenCompiledShader(string path)
        {
            singleFileMode = true;
            treeViewTab.Enabled = false;

            ShaderWork.RecreateShaderTask(path, (decompiled, except) =>
            {
                treeView.Invoke(new MethodInvoker(() =>
                {
                    if (except == null)
                    {
                        // All good
                        tabView.SelectTab(compiledShaderTab);
                        OnSelectedFile?.Invoke(path);
                    }
                    else
                    {
                        MessageBox.Show($"While trying to open {path}: {except}", "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }));
            }, parameters: default);
        }

        public void ExploreFolder(string folder)
        {
            singleFileMode = false;
            treeViewTab.Enabled = true;

            treeView.Parent.SuspendLayout();
            treeView.Enabled = false;
            treeView.Nodes.Clear();
            filenameForNode.Clear();

            ExploreFolderForShaders(folder);

            treeView.Parent.ResumeLayout(true);
            treeView.Enabled = true;
            tabView.SelectTab(treeViewTab);
        }

        public void OpenIW4OFFile(string filePath)
        {
            // TODO
            singleFileMode = false;
            treeViewTab.Enabled = true;

            treeView.Enabled = false;
            filenameForNode.Clear();
            treeView.Parent.SuspendLayout();

            if (Path.GetDirectoryName(filePath).EndsWith("techniques"))
            {
                ExploreTechnique(filePath);
            }
            else
            {
                ExploreTechniqueSet(filePath);
            }

            treeView.Parent.ResumeLayout(true);
            treeView.Enabled = true;
            tabView.SelectTab(treeViewTab);
        }

        public void ExploreTechniqueSet(string path)
        {
            string txt = File.ReadAllText(path);
            if (JsonConvert.DeserializeObject<TechniqueSet>(txt) is TechniqueSet techset)
            {
                TechniqueSetTreeNode techsetParent = new TechniqueSetTreeNode(path, techset);
                string techsetDir = Path.GetDirectoryName(path);
                string parentDir = Path.GetDirectoryName(techsetDir);

                Dictionary<string, TreeNode> knownTechniques = new Dictionary<string, TreeNode>();

                for (int i = 0; i < (int)MaterialTechniqueType.TECHNIQUE_COUNT; i++)
                {
                    if (techset.GetTechnique((MaterialTechniqueType)i, out string name))
                    {
                        string fullPath = Path.Combine(parentDir, "techniques", name) + ".iw4x.json";

                        if (knownTechniques.TryGetValue(fullPath, out TreeNode tech))
                        {
                            // Pass
                        }
                        else
                        {
                            tech = ExploreTechnique(fullPath, techsetParent.Nodes);
                            if (tech is TechniqueTreeNode)
                            {
                                knownTechniques.Add(fullPath, tech);
                            }
                        }

                        if (tech is TechniqueTreeNode techniqueTreeNode)
                        {
                            techniqueTreeNode.usedInTypes.Add((MaterialTechniqueType)i);
                        }
                    }
                }

                techsetParent.Expand();

                filenameForNode[techsetParent] = path;

                treeView.Nodes.Add(techsetParent);
            }
        }

        private TechniqueTreeNode ExploreTechnique(string path, TreeNodeCollection parentCollection=null)
        {

            string txt = File.ReadAllText(path);
            if (JsonConvert.DeserializeObject<Technique>(txt) is Technique technique)
            {
                TechniqueTreeNode techniqueParent = new TechniqueTreeNode(path, technique);
                string techDir = Path.GetDirectoryName(path);
                string parentDir = Path.GetDirectoryName(techDir);

                string[] csoFiles = new string[]
                {
                    Path.Combine(parentDir, "ps", $"{technique.PixelShader}.cso"),
                    Path.Combine(parentDir, "vs", $"{technique.VertexShader}.cso"),
                };

                for (int i = 0; i < csoFiles.Length; i++)
                {
                    string folder = Path.GetDirectoryName(csoFiles[i]);
                    TreeNode node = AddShaderToTreeView(Path.GetFileNameWithoutExtension(csoFiles[i]), csoFiles[i], techniqueParent.Nodes);
                    filenameForNode[node] = csoFiles[i];
                }

                techniqueParent.ExpandAll();

                if (parentCollection == null)
                {
                    treeView.Nodes.Add(techniqueParent);
                }
                else
                {
                    parentCollection.Add(techniqueParent);
                }

                filenameForNode[techniqueParent] = path;

                return techniqueParent;
            }

            return null;
        }

        private void ExploreFolderForShaders(string folder)
        {
            string[] csoFiles = Directory.GetFiles(folder, "*.cso");

            for (int i = 0; i < csoFiles.Length; i++)
            {
                TreeNode node = AddShaderToTreeView(Path.GetFileNameWithoutExtension(csoFiles[i].Substring(folder.Length)), csoFiles[i], treeView.Nodes);
                filenameForNode[node] = csoFiles[i];
            }

        }

        private TreeNode AddShaderToTreeView(string name, string path, TreeNodeCollection into)
        {
            TreeNode shaderNode = new TreeNode();
            shaderNode.Text = name;
            into.Add(shaderNode);

            RecreateShader(treeView, shaderNode, path);

            return shaderNode;
        }


        private void RecreateShader(
            Control forControl, 
            TreeNode node,
            string filePath,
            OptimizationParameters parameters = default)
        {
            node.ForeColor = loadingColor;

            ShaderWork.RecreateShaderTask(filePath, (decompiled, except) =>
            {
                forControl.Invoke(new MethodInvoker(() =>
                {
                    if (except == null)
                    {
                        node.ForeColor = decompiled.incomplete ? this.failColor : this.goodColor;
                        node.ImageIndex = decompiled.isVertexShader ? VS_IMAGE : PS_IMAGE;
                        node.SelectedImageIndex = decompiled.isVertexShader ? VS_IMAGE : PS_IMAGE;
                    }
                    else
                    {
                        node.ForeColor = this.hardErrorColor;
                        node.SelectedImageIndex = UNKNOWN_IMAGE;
                        node.ImageIndex = UNKNOWN_IMAGE;
                    }

                }));
            }, parameters);
        }
    }
}
