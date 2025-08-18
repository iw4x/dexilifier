namespace DX9ShaderHLSLifier
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using static DX9ShaderHLSLifier.ShaderProgramObject;
    using static ShaderProgramObject;



    public readonly struct ShaderOptimizer
    {
        public class DependencyLeaf
        {
            public readonly DependencyLeaf[] requirements;
            public readonly Statement statement;
            public readonly HashSet<DependencyLeaf> requiredBy = new HashSet<DependencyLeaf>();

            public DependencyLeaf(Statement statement, DependencyLeaf[] requirements)
            {
                this.statement = statement;
                this.requirements = requirements;

                for (int i = 0; i < requirements.Length; i++)
                {
                    requirements[i].requiredBy.Add(this);
                }
            }

            public bool AreRequirementsMet(Predicate<Statement> present)
            {
                for (int i = 0; i < requirements.Length; i++)
                {
                    if (!requirements[i].AreRequirementsMet(present))
                    {
                        return false;
                    }

                    if (!present(requirements[i].statement))
                    {
                        return false;
                    }
                }

                return true;
            }

            public bool UnrollSatisfied(Predicate<Statement> present, Action<Statement> addStatement)
            {
                bool didAnything = false;

                if (AreRequirementsMet(present) && !present(statement))
                {
                    addStatement(statement);
                    didAnything = true;
                }
                else
                {
                    for (int i = 0; i < requirements.Length; i++)
                    {
                        didAnything |= requirements[i].UnrollSatisfied(present, addStatement);
                    }

                    if (didAnything)
                    {
                        return UnrollSatisfied(present, addStatement);
                    }
                }

                return didAnything;
            }
        }

        public class MatrixDotGroup
        {
            public class MxMultiply : Instruction
            {
                public override bool OutputInputAreSameSize => false;
                public override int[] InputsMaximumWidth => new int[] { rows, rows * columns };

                public byte IndividualChannelWidth => columns;

                public override int OutputMaximumWidth => columns;

                private readonly byte rows;
                private readonly byte columns;

                public MxMultiply(bool isHalfPrecision, byte rows, byte columns) : base(new InstructionModifiers() { isHalfPrecision = isHalfPrecision })
                {
                    this.rows = rows;
                    this.columns = columns;
                }

                public override bool Simplify(CodeData destination, CodeData[] arguments)
                {
                    return base.Simplify(destination, arguments);
                }

                protected override string FormatCallInternal(params string[] arguments)
                {
                    return string.Format("mul({0}, {1})", arguments);
                }
            }

            public Statement Front => statements[0];

            public IReadOnlyList<Statement> Statements => statements;

            private int Width => Front.Destination.GetDataWidth();

            private readonly Statement[] statements = new Statement[4];

            private MatrixDotGroup(Statement initialStatement)
            {
                statements[0] = initialStatement;
            }

            public Statement Build()
            {
                List<Statement> sortedStatements =
                    this.statements
                    .Where(o => o != null)
                    .OrderBy(o => o.Destination.UsedChannels[0])
                    .ToList();

                Statement @base = sortedStatements[0];

                MatrixResource matrix =
                    (
                        (
                            @base.Arguments.First(o => o is ResourceData rsc && rsc.resource is MatrixChannelAccessResource mxChannelAccess)
                            as ResourceData
                        ).resource as MatrixChannelAccessResource
                    ).matrix;

                MxMultiply instruction = new MxMultiply(
                    @base.Instruction.modifiers.isHalfPrecision,
                    matrix.rows,
                    matrix.columns
                );

                CodeData destination = @base.Destination;

                byte[] destinationChannels = new byte[instruction.IndividualChannelWidth];
                for (byte i = 0; i < destinationChannels.Length; i++)
                {
                    destinationChannels[i] = i;
                }

                destination.SetChannels(destinationChannels);

                CodeData[] arguments = new CodeData[@base.Arguments.Length];
                for (int i = 0; i < arguments.Length; i++)
                {
                    if (@base.Arguments[i] is ResourceData data && data.resource is MatrixChannelAccessResource wide)
                    {
                        byte[] mxChannels = new byte[wide.matrix.rows * wide.matrix.columns];
                        for (int channel = 0; channel < wide.matrix.rows * wide.matrix.columns; channel++)
                        {
                            mxChannels[channel] = (byte)channel;
                        }

                        arguments[i] = new ResourceData(wide.matrix, data.isVertexShader, data.modifiers, mxChannels);
                    }
                    else
                    {
                        arguments[i] = @base.Arguments[i];
                        if (arguments[i].GetDataWidth() >= instruction.OutputMaximumWidth &&
                            arguments[i].UsedChannels.Length < arguments[i].GetDataWidth())
                        {
                            arguments[i].SetChannels(destinationChannels);
                        }
                    }
                }

                List<int> originalLines = new List<int>();
                for (int i = 0; i < sortedStatements.Count; i++)
                {
                    for (int lineIndex = 0; lineIndex < sortedStatements[i].lines.Length; lineIndex++)
                    {
                        originalLines.Add(sortedStatements[i].lines[lineIndex]);
                    }
                }

                return new Statement(originalLines.ToArray(), instruction, destination, arguments);
            }

            public bool IsComplete()
            {
                return statements[Width - 1] != null;
            }

            public bool Accept(Statement statement)
            {
                if (IsAcceptableStandaloneStatement(statement))
                {
                    int? slot = null;

                    bool equals(byte[] channelsA, byte[] channelsB)
                    {
                        System.Diagnostics.Debug.Assert(channelsA.Length == channelsB.Length);
                        for (int i = 0; i < channelsA.Length; i++)
                        {
                            if (channelsA[i] != channelsB[i])
                            {
                                return false;
                            }
                        }

                        return true;
                    }

                    for (int argumentIndex = 0; argumentIndex < statement.Arguments.Length; argumentIndex++)
                    {
                        for (int i = 0; i < Width; i++)
                        {
                            if (statements[i] == null)
                            {
                                slot = i;
                                break;
                            }

                            if (statements[i].Arguments[argumentIndex].GetType() != statement.Arguments[argumentIndex].GetType())
                            {
                                return false;
                            }

                            if (!equals(statements[i].Arguments[argumentIndex].UsedChannels, statement.Arguments[argumentIndex].UsedChannels))
                            {
                                return false;
                            }

                            if (statements[i].Arguments[argumentIndex] is ResourceData baseData && baseData.resource is MatrixChannelAccessResource wideBase &&
                                statement.Arguments[argumentIndex] is ResourceData comparisonData && comparisonData.resource is MatrixChannelAccessResource wideComparison)
                            {
                                if (wideComparison.index == wideBase.index)
                                {
                                    return false;
                                }
                            }
                        }
                    }

                    System.Diagnostics.Debug.Assert(slot.HasValue);

                    statements[slot.Value] = statement;

                    return true;
                }
                else
                {
                    return false;
                }
            }

            public static bool Accept(Statement statement, out MatrixDotGroup group)
            {
                if (IsAcceptableStandaloneStatement(statement))
                {
                    group = new MatrixDotGroup(statement);
                    return true;
                }

                group = default;
                return false;
            }

            private static bool IsAcceptableStandaloneStatement(Statement statement)
            {
                if (statement.Instruction is Instructions.DotProduct)
                {
                    if (statement.Destination.UsedChannels.Length == 1 && statement.Arguments.Length == 2)
                    {
                        bool atLeastOneMatrixInvolved = false;

                        {
                            if (statement.Arguments[0] is ResourceData rsc &&
                                rsc.resource is MatrixChannelAccessResource ext)
                            {
                                atLeastOneMatrixInvolved |= true;
                            }
                        }

                        {
                            if (statement.Arguments[1] is ResourceData rsc &&
                                rsc.resource is MatrixChannelAccessResource ext)
                            {
                                atLeastOneMatrixInvolved |= true;
                            }
                        }

                        if (atLeastOneMatrixInvolved)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        private readonly struct VariableChannel
        {
            public readonly byte usedChannel;
            public readonly Variable variable;

            public VariableChannel(byte usedChannel, Variable variable)
            {
                this.usedChannel = usedChannel;
                this.variable = variable;
            }
        }


        public IReadOnlyList<Statement> OptimizedStatements => statements;

        private readonly List<Statement> statements;

        private readonly List<DependencyLeaf> leaves;

        private readonly OptimizationParameters parameters;

        public ShaderOptimizer(OptimizationParameters parameters, IEnumerable<Statement> statements)
        {
            this.parameters = parameters;
            this.statements = new List<Statement>(statements);
            this.leaves = new List<DependencyLeaf>();

            Work();
        }

        public void ExportDependencyGraph(out DependencyGraph graph)
        {
            graph = new DependencyGraph(leaves);

            /*
             *
            */
        }

        private void Work()
        {
            if (parameters.inlineConstants)
            {
                InlineConstants();
            }

            AdjustDataSize(); // this is required for reorganization, otherwise undeclared variables are left dangling
            CreateDependencyTree();

            if (parameters.reorganize)
            {
                ReorganizeStatements();
            }

            if (parameters.renameVariablesBasedOnUsage)
            {
                RenameVariables();
            }

            if (parameters.reduceInstructions)
            {
                ReduceInstructions();
            }
        }

        private void ReduceInstructions()
        {
            ReassembleSplitOperations();
            ReassembleMatrixMultiplications();
            HalveVariables();

            AdjustDataSize(); // We have to do it again
        }

        private void CreateDependencyTree()
        {
            List<Statement> workStatements = new List<Statement>(statements);

            // Remove all empty comments for now
            workStatements.RemoveAll(o => o is Comment);

            List<DependencyLeaf> highestLeaves = new List<DependencyLeaf>();
            Dictionary<Statement, DependencyLeaf> existingLeaves = new Dictionary<Statement, DependencyLeaf>();
            for (int i = workStatements.Count - 1; i >= 0; i--)
            {
                Statement statement = workStatements[i];

                if (existingLeaves.TryGetValue(statement, out _))
                {
                    // It's already a codependency
                }
                else
                {
                    DependencyLeaf leaf = GrabLeaf(statement, workStatements, existingLeaves);

                    highestLeaves.Add(leaf);
                }
            }

            workStatements.Clear();

            leaves.Clear();
            leaves.AddRange(highestLeaves);
        }

        private void ReassembleSplitOperations()
        {
            HashSet< System.Type> oftenSplitInstructions = new HashSet<System.Type>
            {
                typeof(Instructions.Exponential),
                typeof(Instructions.Log)
            };

            int streakLength = 0;
            Variable target = null;
            List<Statement> statementsInStreak = new List<Statement>();
            byte channelMask = 0;

            List<Statement[]> streaks = new List<Statement[]>();

            for (int i = 0; i < statements.Count; i++)
            {
                Statement statement = statements[i];

                if (statement is Comment)
                {
                    continue;
                }

                if (oftenSplitInstructions.Contains(statement.Instruction.GetType()) &&
                    statement.Destination.UsedChannels.Length == 1)
                {
                    if (statement.Destination is VariableData varData)
                    {
                        if (target == null || varData.variable == target)
                        {
                            byte chan = varData.UsedChannels[0];
                            if ((chan & channelMask) == 0)
                            {
                                target = varData.variable;
                                streakLength++;
                                channelMask |= chan;
                                statementsInStreak.Add(statement);
                                continue;
                            }
                        }
                    }
                }

                // Break or make streak if >1
                if (streakLength > 1)
                {
                    streaks.Add(statementsInStreak.ToArray());
                }

                channelMask = 0;
                streakLength = 0;
                statementsInStreak.Clear();
                target = null;
            }

            // Group streaks and reduce statements
            for (int i = 0; i < streaks.Count; i++)
            {
                Statement[] streak = streaks[i];
                Statement head = streak[0];
                VariableData headVariableData = head.Destination as VariableData;
                int insertionPoint = statements.IndexOf(head);

                List<int> originalLines = new List<int>();
                byte[] writtenChannels = new byte[streak.Length];
                byte[] readChannels = new byte[streak.Length];

                for (int streakElementIndex = 0; streakElementIndex < streak.Length; streakElementIndex++)
                {
                    System.Diagnostics.Debug.Assert(streak[streakElementIndex].Arguments.Length == 1);

                    writtenChannels[streakElementIndex] = streak[streakElementIndex].Destination.UsedChannels[0];
                    readChannels[streakElementIndex] = streak[streakElementIndex].Arguments[0].UsedChannels[0];

                    originalLines.AddRange(streak[streakElementIndex].lines);
                    statements.Remove(streak[streakElementIndex]);
                }

                CodeData argument;

                if (head.Arguments[0] is ResourceData rsc)
                {
                    argument = rsc.Duplicate();
                    argument.SetChannels(readChannels);
                }
                else if (head.Arguments[0] is VariableData originalDestination)
                {
                    argument = new VariableData(originalDestination.variable, originalDestination.modifiers, forWrite: false, readChannels);
                }
                else
                {
                    throw new Exception($"unreachable");
                }

                // New statement
                Statement groupedExp = new Statement(
                    originalLines.ToArray(),
                    head.Instruction, 
                    new VariableData(headVariableData.variable, headVariableData.modifiers, forWrite: true, writtenChannels),
                    argument);

                statements.Insert(insertionPoint, groupedExp);
            }

            // Refresh variable usages because everything has moved
            RefreshVariableUsage(statements);
        }

        private void HalveVariables()
        {
            Dictionary<Variable, bool> halvedVariables = new Dictionary<Variable, bool>();

            for (int i = 0; i < statements.Count; i++)
            {
                Statement statement = statements[i];

                if (statement is Comment)
                {
                    continue;
                }

                if (statement.Destination is VariableData variable)
                {
                    if (!halvedVariables.ContainsKey(variable.variable))
                    {
                        halvedVariables.Add(variable.variable, true); // True until proven otherwise
                    }

                    halvedVariables[variable.variable] &= statement.Instruction.modifiers.isHalfPrecision;
                }
            }

            foreach(var kv in halvedVariables)
            {
                if (halvedVariables[kv.Key])
                {
                    kv.Key.isHalfPrecision = true;
                }
            }
        }

        private void ReassembleMatrixMultiplications()
        {
            MatrixDotGroup currentGroup = null;

            List<MatrixDotGroup> groups = new List<MatrixDotGroup>();

            for (int i = 0; i < statements.Count; i++)
            {
                Statement statement = statements[i];

                if (statement is Comment)
                {
                    continue;
                }

                if (currentGroup == null)
                {
                    if (MatrixDotGroup.Accept(statement, out currentGroup))
                    {
                        // OK!
                    }
                }
                else
                {
                    if (currentGroup.Accept(statement))
                    {
                        if (currentGroup.IsComplete())
                        {
                            groups.Add(currentGroup);
                            currentGroup = null;
                        }
                        else
                        {
                            // Maybe later
                        }
                    }
                    else
                    {
                        currentGroup = null;
                    }
                }
            }

            if (groups.Count > 0)
            {
                for (int i = 0; i < groups.Count; i++)
                {
                    MatrixDotGroup group = groups[i];

                    int start = statements.IndexOf(group.Front);

                    System.Diagnostics.Debug.Assert(start >= 0);

                    // Remove all elements that we have grouped
                    for (int statementIndex = 0; statementIndex < group.Statements.Count; statementIndex++)
                    {
                        statements.Remove(group.Statements[statementIndex]);
                    }

                    Statement newStatement = group.Build();
                    statements.Insert(start, newStatement); // Put statement back in its place to conserve order of operations
                }

                // Refresh variable usages because everything has moved
                RefreshVariableUsage(statements);
            }
        }

        private void AdjustDataSize()
        {
            ShrinkDataToDestinations();

            while (ShrinkVariablesToUsage())
            {
                ShrinkDataToDestinations();
            }
        }
        
        private void RenameVariables()
        {
            HashSet<string> takenNames = new HashSet<string>();
            HashSet<Variable> renamedVariables = new HashSet<Variable>();

            string getFreeName(string usage, int index=0)
            {
                string name = $"var_{usage}";

                if (index > 0)
                {
                    name += (char)(index + 65 - 1);
                }

                if (takenNames.Contains(name))
                {
                    return getFreeName(usage, index + 1);
                }

                return name;
            }

            for (int i = 0; i < statements.Count; i++)
            {
                Statement statement = statements[i];

                if (statement is Comment)
                {
                    continue;
                }

                // Output-based
                {
                    if (statement.Destination is ResourceData rsc && rsc.resource is Output output)
                    {
                        for (int argumentIndex = 0; argumentIndex < statement.Arguments.Length; argumentIndex++)
                        {
                            if (statement.Arguments[argumentIndex] is VariableData varData && !renamedVariables.Contains(varData.variable))
                            {
                                varData.variable.decompiledName = getFreeName(output.GetUsageName());

                                takenNames.Add(varData.variable.decompiledName);
                                renamedVariables.Add(varData.variable);
                            }
                        }
                    }
                }

                // Input-based
                for (int argumentIndex = 0; argumentIndex < statement.Arguments.Length; argumentIndex++)
                {
                    if (statement.Arguments[argumentIndex] is ResourceData rsc && rsc.resource.GetDecompiledName().ToLower().Contains("color"))
                    {
                        if (statement.Destination is VariableData varData)
                        {
                            varData.variable.representsColor = true;
                        }
                    }
                }

                // Usage based
                for (int argumentIndex = 0; argumentIndex < statement.Arguments.Length; argumentIndex++)
                {
                    if (statement.Arguments[argumentIndex] is ResourceData rsc && rsc.resource is Sampler sampler)
                    {
                        if (statement.Destination is VariableData varData)
                        {
                            if (!renamedVariables.Contains(varData.variable))
                            {
                                varData.variable.decompiledName = getFreeName(sampler.GetAbbreviatedName());

                                takenNames.Add(varData.variable.decompiledName);
                                renamedVariables.Add(varData.variable);
                            }
                        }
                    }
                }

                // Order based
                int renamedStrayVariables = 0;
                for (int argumentIndex = 0; argumentIndex < statement.Arguments.Length; argumentIndex++)
                {
                    if (statement.Arguments[argumentIndex] is ResourceData rsc && rsc.resource is Sampler sampler)
                    {
                        if (statement.Destination is VariableData varData)
                        {
                            if (!renamedVariables.Contains(varData.variable))
                            {
                                varData.variable.decompiledName = getFreeName(string.Empty, renamedStrayVariables++);
                                takenNames.Add(varData.variable.decompiledName);
                                renamedVariables.Add(varData.variable);
                            }
                        }
                    } 
                }
            }
        }

        private void InlineConstants()
        {
            for (int i = 0; i < statements.Count; i++)
            {
                Statement statement = statements[i];

                if (statement is Comment)
                {
                    continue;
                }

                for (int argumentIndex = 0; argumentIndex < statement.Arguments.Length; argumentIndex++)
                {
                    if (statement.Arguments[argumentIndex] is ResourceData data && data.resource is HardcodedExternalConstant constant)
                    {
                        constant.SetInlined(byte.MaxValue);
                    }
                }
            }
        }

        private bool ShrinkVariablesToUsage()
        {
            bool didAnything = false;
            Dictionary<Variable, byte> channelUseMask = new Dictionary<Variable, byte>(statements.Count);

            for (int i = 0; i < statements.Count; i++)
            {
                if (statements[i] is Comment)
                {
                    continue;
                }

                // This could be smarter (shrinking based on reading first AND then on destination)
                CodeData[] candidates = new CodeData[statements[i].Arguments.Length + 1];
                candidates[candidates.Length - 1] = statements[i].Destination;

                for (int dataIndex = 0; dataIndex < candidates.Length; dataIndex++)
                {
                    if (candidates[dataIndex] is VariableData varData)
                    {
                        if (!channelUseMask.ContainsKey(varData.variable))
                        {
                            channelUseMask.Add(varData.variable, 0);
                        }

                        for (int channelIndex = 0; channelIndex < varData.UsedChannels.Length; channelIndex++)
                        {
                            channelUseMask[varData.variable] |= (byte)(1 << varData.UsedChannels[channelIndex]);
                        }
                    }
                }
            }

            foreach(var kv in channelUseMask)
            {
                System.Diagnostics.Debug.Assert(kv.Value > 0);

                Variable variable = kv.Key;
                byte channelsUsed = kv.Value;
                
                if (variable.writtenChannels != channelsUsed)
                {
                    variable.writtenChannels = channelsUsed;
                    didAnything |= true;
                }
            }

            return didAnything;
        }

        private void ShrinkDataToDestinations()
        {
            for (int i = 0; i < statements.Count; i++)
            {
                while (statements[i].Simplify())
                {
                    // roll!
                }
            }
        }

        private void ReorganizeStatements()
        {
            bool done = false;

            List<DependencyLeaf> highestLeaves = this.leaves.OrderByDescending(o => o.requirements.Length).ToList();
            List<Statement> workStatements = new List<Statement>(statements.Count);

            while (!done)
            {
                bool anyChange = false;
                // Recreate program from leaves
                for (int i = 0; i < highestLeaves.Count; i++)
                {
                    anyChange |= highestLeaves[i].UnrollSatisfied(workStatements.Contains, workStatements.Add);
                }

                if (anyChange)
                {
                    continue;
                }

                // check if we're done
                done = true;
                for (int i = 0; i < highestLeaves.Count; i++)
                {
                    done &= highestLeaves[i].AreRequirementsMet(workStatements.Contains);
                }
            }

            RefreshVariableUsage(workStatements);

            statements.Clear();
            statements.AddRange(workStatements);
        }

        private void RefreshVariableUsage(IReadOnlyList<Statement> statements)
        {
            for (int i = 0; i < statements.Count; i++)
            {
                if (statements[i].Destination is VariableData variableData)
                {
                    variableData.ResetUsage();
                }
            }

            for (int i = 0; i < statements.Count; i++)
            {
                if (statements[i].Destination is VariableData variableData)
                {
                    variableData.NotifyUsedInStatement(statements[i]);
                }
            }
        }

        private DependencyLeaf GrabLeaf(Statement statement, List<Statement> statements, Dictionary<Statement, DependencyLeaf> existingLeaves)
        {
            if (existingLeaves.TryGetValue(statement, out DependencyLeaf existingLeaf))
            {
                return existingLeaf;
            }

            List<VariableChannel> requiredVariableChannels = new List<VariableChannel>();
            for (int i = 0; i < statement.Arguments.Length; i++)
            {
                if (statement.Arguments[i] is VariableData variableData)
                {
                    for (int channelIndex = 0; channelIndex < variableData.UsedChannels.Length; channelIndex++)
                    {
                        if (requiredVariableChannels.FindIndex(o=>o.usedChannel == variableData.UsedChannels[channelIndex] && o.variable == variableData.variable) >= 0)
                        {
                            continue; // Do not add redundant dependencies
                        }

                        requiredVariableChannels.Add(new VariableChannel(variableData.UsedChannels[channelIndex], variableData.variable));
                    }
                }
            }

            // Find relatives
            HashSet<Statement> family = new HashSet<Statement>();
            if (requiredVariableChannels.Count > 0)
            {
                // Going up through the code starting from here
                int selfIndex = statements.IndexOf(statement);

                for (int i = selfIndex-1; i >= 0; i--)
                {
                    if (statements[i].Destination is VariableData otherStatementVariableData)
                    {
                        for (int rqVariableIndex = 0; rqVariableIndex < requiredVariableChannels.Count; rqVariableIndex++)
                        {
                            VariableChannel varChan = requiredVariableChannels[rqVariableIndex];
                            Variable lookingForVariable = varChan.variable;

                            if (lookingForVariable == otherStatementVariableData.variable)
                            {
                                // It's the right variable, so now we eliminate the widths
                                for (int otherStatementUsedChannelIndex = 0; otherStatementUsedChannelIndex < otherStatementVariableData.UsedChannels.Length; otherStatementUsedChannelIndex++)
                                {
                                    byte channelUsed = otherStatementVariableData.UsedChannels[otherStatementUsedChannelIndex];

                                    if (channelUsed == varChan.usedChannel)
                                    {
                                        requiredVariableChannels.RemoveAt(rqVariableIndex);
                                        family.Add(statements[i]); // It's okay if we add it multiple times
                                        rqVariableIndex--;
                                        break; // Jump to next variable channel
                                    }
                                }
                            }
                        }
                    }

                    if (requiredVariableChannels.Count <= 0)
                    {
                        break; // We're done seeking dependencies
                    }
                }

                // This will break if the variables and origins/destinations have not been properly shrunk
                System.Diagnostics.Debug.Assert(requiredVariableChannels.Count == 0);

                // Not removing the whole family from remaining statments because some dependencies can be multiple
            }
            else
            {
                // Standalone statement (e.g. putting a konstant value in a variable)
                // Kinda weird that we would end up here in the first place, this should have been grabbed earlier unless it's unused
                // But it happens (e.g. passthrough from vin to vout)
            }

            List<DependencyLeaf> familyLeaves = new List<DependencyLeaf>();

            // Sort family by some criterias so it looks nicer
            Statement[] familyArray =
                family
                    .OrderBy(o=>o.Destination is ResourceData rsc && rsc.resource is Output)
                    .ThenBy(o => o.Destination.UsedChannels.Length)
                    .ThenBy(o => o.Destination.UsedChannels[0])
                    .ToArray();

            foreach (Statement familyMember in familyArray)
            {
                DependencyLeaf familyLeaf = GrabLeaf(familyMember, statements, existingLeaves);
                familyLeaves.Add(familyLeaf);
            }


            DependencyLeaf baseLeaf = new DependencyLeaf(statement, familyLeaves.ToArray());
            existingLeaves.Add(statement, baseLeaf);

            return baseLeaf;
        }

    }
}
