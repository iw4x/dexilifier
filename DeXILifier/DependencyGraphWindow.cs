namespace DX9ShaderHLSLifier
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Forms;

    public partial class DependencyGraphWindow : Form
    {
        private const int MARGIN = 20;

        private const int MARGIN_V = 40;

        public class LeafView : IDisposable
        {
            public event Action<LeafView> OnHover;
            public event Action<LeafView> OnStopHovering;

            public GroupBox groupBox;
            public Label textBox;

            private readonly Control parent;

            public readonly Guid guid;

            public int Width => groupBox.Width;
            public int Height => groupBox.Height;
            public Color LineColor { get; private set; }

            public Point Position
            {
                get => groupBox.Location;
                set => groupBox.Location = value;
            }

            public int ChildrenCount { get; set; }

            public LeafView(Control parentControl, string name, string contents, Guid guid)
            {
                this.parent = parentControl;

                this.guid = guid;

                groupBox = new GroupBox();
                groupBox.Text = name;
                groupBox.AutoSize = true;
                groupBox.Height = 15 * contents.Trim().Split('\n').Length + 40;
                groupBox.Width = 240;

                textBox = new Label();
                textBox.Text = contents.Trim();
                textBox.Font = new Font(FontFamily.GenericMonospace, 8f);
                textBox.Dock = DockStyle.Fill;
                textBox.TextAlign = ContentAlignment.MiddleCenter;
                textBox.BorderStyle = BorderStyle.None;

                groupBox.MouseEnter += GroupBox_MouseHover;
                groupBox.MouseLeave += GroupBox_MouseLeave;

                textBox.MouseEnter += TextBox_MouseHover;
                textBox.MouseLeave += TextBox_MouseLeave;

                groupBox.Controls.Add(textBox);

                parent.Controls.Add(groupBox);

                DeHighlight();
            }

            private void TextBox_MouseLeave(object sender, EventArgs e)
            {
                OnStopHovering?.Invoke(this);
            }

            private void TextBox_MouseHover(object sender, EventArgs e)
            {
                OnHover?.Invoke(this);
            }

            private void GroupBox_MouseLeave(object sender, EventArgs e)
            {
                OnStopHovering?.Invoke(this);
            }

            private void GroupBox_MouseHover(object sender, EventArgs e)
            {
                OnHover?.Invoke(this);
            }

            public void HighlightSelf()
            {
                groupBox.ForeColor = Color.Yellow;
                groupBox.BackColor = Color.Black;

                textBox.BackColor = groupBox.BackColor;
                textBox.ForeColor = groupBox.ForeColor;

                LineColor = textBox.ForeColor;
            }

            public void HighlightAsDependency()
            {
                groupBox.ForeColor = Color.Orange;
                groupBox.BackColor = Color.LightSlateGray;

                textBox.BackColor = groupBox.BackColor;
                textBox.ForeColor = groupBox.ForeColor;

                LineColor = textBox.ForeColor;
            }

            public void DeHighlight()
            {
                groupBox.ForeColor = Color.Black;
                groupBox.BackColor = Color.DarkGray;

                textBox.BackColor = groupBox.BackColor;
                textBox.ForeColor = groupBox.ForeColor;

                LineColor = textBox.ForeColor;
            }

            public void Dispose()
            {
                this.parent.Controls.Remove(groupBox);

                OnHover = null;
                OnStopHovering = null;

                groupBox.Controls.Remove(textBox);

                groupBox.Dispose();
                textBox.Dispose();
            }
        }

        private readonly List<LeafView> views = new List<LeafView>();

        private readonly List<(LeafView a, LeafView b)> connectors = new List<(LeafView a, LeafView b)>();

        private DependencyGraph graph;


        public DependencyGraphWindow()
        {
            InitializeComponent();

            graphPanel.Paint += GraphPanel_Paint;
            graphPanel.Scroll += GraphPanel_Scroll;
        }

        public void CreateGraph(DecompiledShader shader)
        {
            graph = shader.dependencyGraph;

            graphPanel.SuspendLayout();

            List<DependencyGraph.Leaf> nextLevel = new List<DependencyGraph.Leaf>(shader.dependencyGraph.topLevelLeaves);

            Dictionary<Guid, LeafView> leafViews = new Dictionary<Guid, LeafView>();

            for (int i = 0; i < views.Count; i++)
            {
                views[i].Dispose();
            }

            connectors.Clear();
            views.Clear();
            string[] textLines = shader.rawBody.Split('\n');

            Dictionary<int, List<LeafView>> viewPerDepth = new Dictionary<int, List<LeafView>>();

            int depth = 0;
            int currentHeight = 0;
            while (nextLevel.Count > 0)
            {
                viewPerDepth[depth] = new List<LeafView>();
                List<DependencyGraph.Leaf> workList = new List<DependencyGraph.Leaf>(nextLevel);
                nextLevel.Clear();
                int maxHeightOfRow = 0;

                for (int i = 0; i < workList.Count; i++)
                {
                    DependencyGraph.Leaf leaf = workList[i];

                    StringBuilder sb = new StringBuilder();
                    string linesName = "?";

                    bool found = false;

                    foreach (DecompiledBlock block in shader.blocks.Where(o => o.statement == leaf.statement))
                    {
                        if (block.originalLines.Length > 0)
                        {
                            linesName = block.originalLines.Length == 1 ?
                                block.originalLines[0].ToString() :
                                $"{block.originalLines[0]}:{block.originalLines[block.originalLines.Length - 1]}";

                            for (int lineIndex = 0; lineIndex < block.originalLines.Length; lineIndex++)
                            {
                                int line = block.originalLines[lineIndex];
                                string txt = textLines[line];
                                sb.AppendLine(txt);
                            }

                            found = true;
                        }
                    }

                    if (!found)
                    {
                        sb.AppendLine(leaf.statement.ToString());
                    }

                    LeafView view = new LeafView(graphPanel, $"L{linesName}", sb.ToString(), leaf.statement);
                    view.OnHover += View_OnHover;
                    view.OnStopHovering += View_OnStopHovering;

                    maxHeightOfRow = Math.Max(maxHeightOfRow, view.Height);

                    // X is sorted out later
                    view.Position = new Point(0, currentHeight);

                    if (leafViews.TryGetValue(leaf.statement, out LeafView existingLeaf))
                    {
                        existingLeaf.Dispose();
                        leafViews.Remove(leaf.statement);
                    }

                    nextLevel.AddRange(leaf.requirements);
                    leafViews.Add(leaf.statement, view);
                    viewPerDepth[depth].Add(view);
                }

                currentHeight += maxHeightOfRow + MARGIN_V;
                depth++;
            }

            // Arrange them side by side
            HashSet<LeafView> allLeaves = new HashSet<LeafView>(leafViews.Values);

            int[] linesWidth = new int[viewPerDepth.Count];
            int largestLine = 0;

            for (depth = 0; depth < viewPerDepth.Count; depth++)
            {
                int lineWidth = MARGIN;
                for (int viewIndex = 0; viewIndex < viewPerDepth[depth].Count; viewIndex++)
                {
                    LeafView view = viewPerDepth[depth][viewIndex];

                    if (!allLeaves.Contains(view))
                    {
                        viewPerDepth[depth].Remove(view);
                        viewIndex--;
                        continue;
                    }

                    lineWidth += MARGIN + view.Width;
                }

                lineWidth += MARGIN;

                largestLine = Math.Max(lineWidth, largestLine);
                linesWidth[depth] = lineWidth;
            }

            for (depth = 0; depth < viewPerDepth.Count; depth++)
            {
                int x = (largestLine - linesWidth[depth]) / 2;
                for (int viewIndex = 0; viewIndex < viewPerDepth[depth].Count; viewIndex++)
                {
                    LeafView view = viewPerDepth[depth][viewIndex];

                    Point position = view.Position;

                    position.X = x;

                    view.Position = position;

                    x += MARGIN + view.Width;
                }
            }

            // Add lines
            foreach (Guid guid in leafViews.Keys)
            {
                Guid[] requirements = shader.dependencyGraph.GetRequirementsForStatement(guid);
                LeafView parent = leafViews[guid];

                parent.ChildrenCount = requirements.Length;

                for (int i = 0; i < requirements.Length; i++)
                {
                    LeafView child = leafViews[requirements[i]];

                    connectors.Add(
                        (
                           parent, child
                        )
                    );
                }
            }

            views.AddRange(allLeaves);

            graphPanel.ResumeLayout();
        }

        private void View_OnStopHovering(LeafView obj)
        {
            StopHighlight();
        }

        private void View_OnHover(LeafView leafView)
        {
            Highlight(leafView);
        }

        private void Highlight(LeafView leafView)
        {
            StopHighlight();
            leafView.HighlightSelf();

            Guid[] requirements = graph.GetRequirementsForStatement(leafView.guid);
            if (requirements.Length > 0)
            {
                List<LeafView> dependencies = views.FindAll(o => requirements.Contains(o.guid));
                foreach (LeafView dependency in dependencies)
                {
                    dependency.HighlightAsDependency();
                }
            }

            graphPanel.Invalidate();
        }

        private void StopHighlight()
        {
            for (int i = 0; i < views.Count; i++)
            {
                views[i].DeHighlight();
            }

            graphPanel.Invalidate();
        }

        private void GraphPanel_Scroll(object sender, ScrollEventArgs e)
        {
            graphPanel.Invalidate();
        }

        protected void GraphPanel_Paint(object _, PaintEventArgs e)
        {
            Dictionary<LeafView, int> drawnPerParent = new Dictionary<LeafView, int>();

            for (int i = 0; i < connectors.Count; i++)
            {
                LeafView parent = connectors[i].a;
                LeafView child = connectors[i].b;
                using (Pen p = new Pen(parent.LineColor))
                {
                    p.CustomEndCap = new AdjustableArrowCap(5, 5);

                    using (GraphicsPath capPath = new GraphicsPath())
                    {
                        Point a = new Point(parent.Position.X + parent.Width / 2, parent.Position.Y + parent.Height);
                        Point b = new Point(child.Position.X + child.Width / 2, child.Position.Y);

                        if (parent.ChildrenCount > 1)
                        {
                            if (!drawnPerParent.ContainsKey(parent))
                            {
                                drawnPerParent[parent] = 0;
                            }

                            int distribution = parent.Width / 2;
                            int chunk = distribution / (parent.ChildrenCount - 1);

                            int offset = -distribution / 2 + chunk * drawnPerParent[parent];
                            a.X = parent.Position.X + parent.Width / 2 + offset;
                            drawnPerParent[parent]++;
                        }

                        e.Graphics.DrawLine(p, b, a);
                    }
                }
            }
        }
    }
}
