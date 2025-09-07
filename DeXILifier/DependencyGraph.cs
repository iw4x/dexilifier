namespace DeXILifier
{
    using System;
    using System.Collections.Generic;
    using static DeXILifier.ShaderOptimizer;
    using static System.Windows.Forms.VisualStyles.VisualStyleElement.TreeView;

    public readonly struct DependencyGraph
    {
        public class Leaf
        {
            public Guid statement;
            public Leaf[] requirements;
        }

        public readonly Leaf[] topLevelLeaves;

        private readonly Dictionary<Guid, Guid[]> statementsDependencies;

        public DependencyGraph(IReadOnlyList<ShaderOptimizer.DependencyLeaf> topLevelLeaves)
        {
            statementsDependencies = new Dictionary<Guid, Guid[]>();

            this.topLevelLeaves = new Leaf[topLevelLeaves.Count];
            for (int i = 0; i < topLevelLeaves.Count; i++)
            {
                AddToGraph(topLevelLeaves[i], ref this.topLevelLeaves[i]);
            }

            WalkDependencies(topLevelLeaves);
        }

        public Guid[] GetRequirementsForStatement(in Guid statement)
        {
            if (statementsDependencies != null && statementsDependencies.TryGetValue(statement, out Guid[] requirements))
            {
                return requirements;
            }

            return new Guid[0];
        }

        private void WalkDependencies(IReadOnlyList<ShaderOptimizer.DependencyLeaf> topLevelLeaves)
        {
            statementsDependencies.Clear();

            void addLeafToGraph(DependencyLeaf leaf, Dictionary<Guid, Guid[]> requirementsGraph)
            {
                if (requirementsGraph.ContainsKey(leaf.statement.Guid))
                {
                    return;
                }

                List<Guid> dependencies = new List<Guid>();
                for (int i = 0; i < leaf.requirements.Length; i++)
                {
                    addLeafToGraph(leaf.requirements[i], requirementsGraph);
                    dependencies.Add(leaf.requirements[i].statement.Guid);
                }

                requirementsGraph.Add(leaf.statement.Guid, dependencies.ToArray());
            }

            foreach (var leaf in topLevelLeaves)
            {
                addLeafToGraph(leaf, statementsDependencies);
            }
        }

        private void AddToGraph(ShaderOptimizer.DependencyLeaf depLeaf, ref Leaf leaf)
        {
            leaf = new Leaf();
            leaf.statement = depLeaf.statement.Guid;

            Leaf[] neutralLeaves = new Leaf[depLeaf.requirements.Length];
            for (int i = 0; i < depLeaf.requirements.Length; i++)
            {
                AddToGraph(depLeaf.requirements[i], ref neutralLeaves[i]);
            }

            leaf.requirements = neutralLeaves;
        }
    }
}
