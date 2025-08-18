namespace DX9ShaderHLSLifier
{
    using SharpDX.Direct3D9;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    public partial class MainWindow : Form
    {
        public string InputText { set { inputTextBox.Text = value; } }

        private int taskToken = 0;

        private readonly Color defaultBack;
        private readonly Color defaultFore;

        private readonly Color linkedLinesColor = Color.LightGreen;
        private readonly Color linkedResourceColor = Color.LightGray;
        private readonly Color linkedResourceColorSameLine = Color.LimeGreen;

        private readonly Dictionary<int, List<int>> richLinesToInputLines = new Dictionary<int, List<int>>();

        private readonly Dictionary<int, HashSet<string>> richLinesResources = new Dictionary<int, HashSet<string>>();

        private readonly List<Action> cleanupPreviousSelections = new List<Action>();

        private readonly DependencyGraphWindow window;
        private readonly Explorer explorer;

        private OptimizationParameters currentParameters = default;
        
        private readonly Action refreshParameters;

        private string currentShaderPath = string.Empty;


        public MainWindow()
        {
            InitializeComponent();

            explorer = new Explorer(this);
            explorer.OnSelectedFile += Explorer_OnSelectedFile;
            explorer.OnSelectedTechniqueSet += Explorer_OnSelectedTechniqueSet;

            window = new DependencyGraphWindow();

            refreshParameters = DeclareParameters();


            defaultBack = decompilationResultTextBox.BackColor;
            defaultFore = decompilationResultTextBox.ForeColor;

            decompilationResultTextBox.VScroll += DecompilationResultTextBox_VScroll1;
            decompilationResultTextBox.SelectionChanged += DecompilationResultTextBox_SelectionChanged;
        }

        private void Explorer_OnSelectedTechniqueSet(string rootPath, Explorer.TechniqueSetTreeNode techniqueSetTreeNode)
        {
            toolOutputTabs.SelectTab(techniqueSetTab);
            techSetView.LoadTechniqueSet(rootPath, techniqueSetTreeNode);
        }

        private void Explorer_OnSelectedFile(string path)
        {
            currentShaderPath = path;
            LoadCompiledShaderFromPath(path);
        }

        private void DecompilationResultTextBox_VScroll1(object sender, EventArgs e)
        {
            int position = GetScrollPos(decompilationResultTextBox.Handle, 1);

            SetScrollPos(lineNumbersTextBox.Handle, 1, position, true);
            SendMessage(lineNumbersTextBox.Handle, 0x115, 4 + 0x10000 * position, 0);
        }

        private void RefreshLineNumbers()
        {
            {
                StringBuilder lineNumbersSb = new StringBuilder();

                for (int i = 0; i < decompilationResultTextBox.Lines.Length; i++)
                {
                    lineNumbersSb.AppendLine((i + 1).ToString());
                }

                lineNumbersTextBox.Text = lineNumbersSb.ToString();
            }

        }

        private void RefreshOutput()
        {
            LoadCompiledShaderFromPath(currentShaderPath);
        }

        private Action DeclareParameters()
        {
            List<Action> actions = new List<Action>();

            void declareParameter(CheckBox box, Action<bool> setter)
            {
                bool value = box.Checked;
                setter(value);

                void onChecked(object sender, EventArgs e)
                {
                    refreshParameters?.Invoke();
                    RefreshOutput();
                }

                box.CheckedChanged += onChecked;

                actions.Add(() => setter(box.Checked));
            }

            declareParameter(optimizeCheckBox, (b) => currentParameters.reorganize = b);
            declareParameter(renameVariablesBox, (b) => currentParameters.renameVariablesBasedOnUsage = b);
            declareParameter(inlineConstantsCheckBox, (b) => currentParameters.inlineConstants = b);
            declareParameter(reduceInstructionsBox, (b) => currentParameters.reduceInstructions = b);

            return () => { actions.ForEach(action => action.Invoke()); };
        }

        private void DecompilationResultTextBox_SelectionChanged(object sender, EventArgs e)
        {
            decompilationResultTextBox.SelectionChanged -= DecompilationResultTextBox_SelectionChanged;

            decompilationResultTextBox.SuspendLayout();
            inputTextBox.SuspendLayout();

            decompilationResultTextBox.HideSelection = true;
            inputTextBox.HideSelection = true;

            int caretPosition = decompilationResultTextBox.SelectionStart;
            int carentLength = decompilationResultTextBox.SelectionLength;

            RefreshHighlights(caretPosition);

            decompilationResultTextBox.SelectionStart = caretPosition;
            decompilationResultTextBox.SelectionLength = carentLength;

            decompilationResultTextBox.ResumeLayout();
            inputTextBox.ResumeLayout();

            decompilationResultTextBox.SelectionChanged += DecompilationResultTextBox_SelectionChanged;
        }

        private void RefreshHighlights(int caretPosition)
        {
            // Reset previous selection
            for (int i = 0; i < cleanupPreviousSelections.Count; i++)
            {
                cleanupPreviousSelections[i]();
            }

            cleanupPreviousSelections.Clear();

            void colorPositions(RichTextBox control, int position, int length, Color color, Color foreColor=default, bool createCleanupInstruction = true)
            {
                LockWindow(control.Handle);
                control.Select(position, length);
                control.SelectionBackColor = color;

                if (foreColor != default)
                {
                    control.SelectionColor = foreColor;
                }

                if (createCleanupInstruction)
                {
                    cleanupPreviousSelections.Add(() => colorPositions(
                        control,
                        position, 
                        length, 
                        control.BackColor, 
                        foreColor == default ? default : control.ForeColor,
                        createCleanupInstruction: false
                    ));
                }

                LockWindow(IntPtr.Zero);
            }


            int selectedLine = decompilationResultTextBox.GetLineFromCharIndex(caretPosition);
            if (richLinesToInputLines.TryGetValue(selectedLine, out List<int> originalLines))
            {
                {
                    int start = decompilationResultTextBox.GetFirstCharIndexFromLine(selectedLine);
                    int end = decompilationResultTextBox.GetFirstCharIndexFromLine(selectedLine + 1) - 1;

                    colorPositions(decompilationResultTextBox, start, end - start, linkedLinesColor);
                }

                // Multiple assembly lines can end up being a single line...
                foreach (int originalLine in originalLines)
                {
                    int start = inputTextBox.GetFirstCharIndexFromLine(originalLine);

                    if (start >= 0)
                    {
                        int end = inputTextBox.GetFirstCharIndexFromLine(originalLine + 1) - 1;

                        colorPositions(inputTextBox, start, end - start, linkedLinesColor, Color.Black);

                        inputTextBox.Select(start, 0);
                    }
                }

                //... and the other way around
                foreach (var kv in richLinesToInputLines)
                {
                    int richLine = kv.Key;
                    List<int> inputLines = kv.Value;

                    if (richLine == selectedLine)
                    {
                        continue;
                    }

                    if (inputLines.FindAll(originalLines.Contains).Count > 0 &&
                        !string.IsNullOrWhiteSpace(decompilationResultTextBox.Lines[richLine].Trim('\t'))
                    )
                    {
                        int start = decompilationResultTextBox.GetFirstCharIndexFromLine(richLine);
                        int end = decompilationResultTextBox.GetFirstCharIndexFromLine(richLine + 1) - 1;

                        colorPositions(decompilationResultTextBox, start, end - start, linkedLinesColor);
                    }
                }
            }

            if (richLinesResources.TryGetValue(selectedLine, out HashSet<string> thisLineResources))
            {
                // Then we highlight every occurence of a resource in another rich line
                foreach(string thisLineResource in thisLineResources)
                {
                    string abbreviated = thisLineResource;
                    if (abbreviated.Contains("."))
                    {
                        abbreviated = thisLineResource.Substring(0, thisLineResource.IndexOf('.'));
                    }

                    foreach (var kv in richLinesResources)
                    {
                        int otherRichLine = kv.Key;

                        Color color = linkedResourceColor;

                        if (otherRichLine == selectedLine)
                        {
                            color = linkedResourceColorSameLine;
                        }

                        HashSet<string> otherRichLineResource = kv.Value;

                        if (otherRichLineResource.Contains(abbreviated))
                        {
                            string txt = decompilationResultTextBox.Lines[otherRichLine];
                            int start = txt.IndexOf(abbreviated);

                            if (start >= 0)
                            {
                                start += decompilationResultTextBox.GetFirstCharIndexFromLine(otherRichLine);
                                int length = abbreviated.Length;

                                colorPositions(decompilationResultTextBox, start, length, color);
                            }
                        }
                    }
                }
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            explorer.Initialize();

            InitializeGraphics();
        }

        private void InitializeGraphics()
        {
            // SharpDX D3D9 Samples look very promising!


            //PresentParameters pp = new PresentParameters();
            //pp.Windowed = true;
            //pp.SwapEffect = SwapEffect.Discard;
            //pp.EnableAutoDepthStencil = true;
            //pp.AutoDepthStencilFormat = Format.D16;

            //Direct3D d3d = new Direct3D();

            //Device device = new Device(d3d, 0, DeviceType.Hardware, testPanel.Handle, CreateFlags.HardwareVertexProcessing, pp);

        }


        private void LoadCompiledShaderFromPath(string path)
        {
            int token = ++taskToken;

            refreshParameters();
            OptimizationParameters optimizationParameters = currentParameters;

            new Thread(() =>
            {
                try
                {
                    ShaderWork.RecreateShader(path, (output, except)=>
                    {
                    if (token == taskToken)
                    {
                            if (except == null)
                            {
                                this.Invoke(new MethodInvoker(() =>
                                {
                                    decompilationResultTextBox.BackColor = defaultBack;
                                    decompilationResultTextBox.ForeColor = defaultFore;

                                    BuildDecompiledRepresentation(output);

                                    decompilationProgressBar.Value = 100;

                                    toolOutputTabs.SelectTab(shaderTab);
                                }));
                            }
                            else
                            {
                                this.Invoke(new MethodInvoker(() =>
                                {
                                    decompilationResultTextBox.BackColor = Color.DarkRed;
                                    decompilationResultTextBox.ForeColor = Color.White;
                                    decompilationResultTextBox.Text = except.ToString();
                                    decompilationProgressBar.Value = 0;
                                    InputText = output.rawOriginalText;

                                    toolOutputTabs.SelectTab(shaderTab);
                                }));
                            }
                        }
                    }, optimizationParameters, 
                    (p) =>
                    {
                        if (token == taskToken)
                        {
                            void updateProgress()
                            {
                                decompilationProgressBar.Value = (int)p * 100;
                            }

                            this.Invoke(new MethodInvoker(updateProgress));
                        }
                        else
                        {
                            throw new TaskCanceledException();
                        }
                    });
                }
                catch (TaskCanceledException)
                {
                    // Ignore
                }
                catch (Exception ex)
                {
                    if (token == taskToken)
                    {
                        this.Invoke(new MethodInvoker(() =>
                        {
                            decompilationResultTextBox.BackColor = Color.DarkRed;
                            decompilationResultTextBox.ForeColor = Color.White;
                            decompilationResultTextBox.Text = ex.ToString();
                            decompilationProgressBar.Value = 0;
                        }
                    ));
                    }
                }
            }).Start();
        }

        private void BuildDecompiledRepresentation(DecompiledShader decompiledShader)
        {
            settingsGroupBox.Enabled = !decompiledShader.incomplete;

            richLinesResources.Clear();
            richLinesToInputLines.Clear();
            decompilationResultTextBox.SuspendLayout();

            InputText = decompiledShader.rawOriginalText;

            StringBuilder builder = new StringBuilder();

            int richLine = 0;
            int inputOffset = 0;

            // Find where the body starts in the original text
            for (int lineIndex = 0; lineIndex < inputTextBox.Lines.Length; lineIndex++)
            {
                if (inputTextBox.Lines[lineIndex].Length > 0 && decompiledShader.rawBody.StartsWith(inputTextBox.Lines[lineIndex]))
                {
                    inputOffset = lineIndex;
                    break;
                }
            }

            for (int i = 0; i < decompiledShader.blocks.Length; i++)
            {
                string output = decompiledShader.blocks[i].decompiledOutput ?? string.Empty;
                string[] outputLines = output.Split(new string[] { Environment.NewLine }, StringSplitOptions.None);

                for (int subLineIndex = 0; subLineIndex < outputLines.Length; subLineIndex++)
                {
                    for (int indent = 0; indent < decompiledShader.blocks[i].indent; indent++)
                    {
                        builder.Indent();
                    }

                    builder.AppendLine(outputLines[subLineIndex]);

                    if (decompiledShader.blocks[i].originalLines != null && decompiledShader.blocks[i].originalLines.Length > 0)
                    {
                        richLinesToInputLines.Add(richLine, new List<int>());

                        for (int originalLineIndex = 0; originalLineIndex < decompiledShader.blocks[i].originalLines.Length; originalLineIndex++)
                        {
                            int originalLine = decompiledShader.blocks[i].originalLines[originalLineIndex];
                            richLinesToInputLines[richLine].Add(originalLine + inputOffset);
                        }
                    }

                    if (decompiledShader.blocks[i].mentionedResources != null && 
                        decompiledShader.blocks[i].mentionedResources.Length > 0)
                    {
                        richLinesResources.Add(
                            richLine, 
                            new HashSet<string>(decompiledShader.blocks[i].mentionedResources.Select(o => o.Split('.')[0]))
                        );
                    }

                    richLine++;
                }
            }

            if (!decompiledShader.incomplete)
            {
                window.CreateGraph(decompiledShader);
            }

            decompilationResultTextBox.Text = builder.ToString();
            
            LockWindow(decompilationResultTextBox.Handle);
            decompilationResultTextBox.SelectionChanged -= DecompilationResultTextBox_SelectionChanged;

            ColorSyntax(decompilationResultTextBox);

            decompilationResultTextBox.SelectionChanged += DecompilationResultTextBox_SelectionChanged;
            LockWindow(IntPtr.Zero);

            RefreshLineNumbers();

            decompilationResultTextBox.ResumeLayout();

            BuildRecompiledOutput(decompiledShader);
        }

        private void ColorSyntax(RichTextBox box)
        {
            ColorSyntax(box, @"[^A-z0-9] *((?:[0-9]|\.)+)", Color.DarkOrange);
            ColorSyntax(box, @"([0-z]+)\(.*\)", Color.DarkGreen);
            ColorSyntax(box, @"(\/\/.*)", Color.Magenta);
            ColorSyntax(box, @"#(.*)", Color.DarkGray);
            ColorSyntax(box, @"((?:float|half)[0-9](?:x[0-9])?)", Color.Blue);
        }


        private void ColorSyntax(RichTextBox box, string pattern, Color color, bool bold = true)
        {
            Regex regex = new Regex(pattern);
            
            MatchCollection collection = regex.Matches(box.Text);

            foreach(Match m in collection)
            {
                for (int i = 1; i < m.Groups.Count; i++)
                {
                    Group group = m.Groups[i];
                    int position = group.Index;
                    int length = group.Length;

                    box.Select(position, length);
                    box.SelectionColor = color;
                }
            }
        }

        private void BuildRecompiledOutput(DecompiledShader shader)
        {
            int token = taskToken;
            Task.Run(() =>
            {
                string txt = string.Empty;
                bool isError = false;

                try
                {
                    txt = FxCInteraction.GetCompiledOutput(shader);
                }
                catch (Exception e)
                {
                    txt = e.Message;
                    isError = true;
                }

                if (token == taskToken)
                {
                    this.Invoke(new MethodInvoker(() =>
                    {
                        if (token == taskToken)
                        {
                            outputWindow.BackColor = isError ? Color.DarkRed : inputTextBox.BackColor;
                            outputWindow.ForeColor = isError ? Color.White : inputTextBox.ForeColor;
                            outputWindow.Text = txt;
                        }
                    }));
                }
            });
        }

        private void openFolderButton_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                dialog.ShowNewFolderButton = false;
                dialog.RootFolder = Environment.SpecialFolder.MyComputer;
                dialog.SelectedPath = "L:\\IW4X_FG\\userrawE\\dump\\mp_nightshift";
                DialogResult result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    explorer.ExploreFolder(dialog.SelectedPath);
                }
            }
        }

        private void openFileButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.CheckFileExists = true;
                dialog.RestoreDirectory = true;
                dialog.SupportMultiDottedExtensions = true;

                DialogResult result = dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    explorer.OpenFile(dialog.FileName);
                }
            }
        }
        private void showRelationalTreeButton_Click(object sender, EventArgs e)
        {
            window.ShowDialog(this);
        }

        private void copyToClipboard_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(decompilationResultTextBox.Text, TextDataFormat.Text);
        }

        const int EM_LINESCROLL = 0x00B6;

        [DllImport("user32.dll")]
        static extern int SetScrollPos(IntPtr hWnd, int nBar,
                               int nPos, bool bRedraw);
        [DllImport("user32.dll")]
        static extern int SendMessage(IntPtr hWnd, int wMsg,
                                       int wParam, int lParam);

        [DllImport("user32.dll")]
        static extern int GetScrollPos(IntPtr hWnd, int nBar);

        [DllImport("user32.dll", EntryPoint = "LockWindowUpdate", SetLastError = true, CharSet = CharSet.Auto)]
        static extern IntPtr LockWindow(IntPtr handle);
    }
}
