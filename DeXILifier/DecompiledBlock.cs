namespace DeXILifier
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;

    public readonly struct DecompiledBlock
    {
        public readonly Guid statement;

        public readonly int[] originalLines;
        public readonly byte indent;
        public readonly string decompiledOutput;

        public readonly string[] mentionedResources;

        public DecompiledBlock(string decompiledOutput, byte indent, params int[] originalLines)
        {
            this.statement = Guid.Empty;
            this.originalLines = originalLines;
            this.decompiledOutput = decompiledOutput;
            this.indent = indent;
            this.mentionedResources = new string[0];
        }

        public DecompiledBlock(ShaderProgramObject.Statement statement, Guid[] requirements)
        {
            StringBuilder sb = new StringBuilder();
            statement.Print(sb);

            this.statement = statement.Guid;

            this.originalLines = statement.lines == null ? new int[0] : statement.lines;
            this.decompiledOutput = sb.ToString();
            this.indent = 1;

            if (statement is ShaderProgramObject.Comment)
            {
                // Ignore
                this.mentionedResources = new string[0];
            }
            else
            {
                HashSet<string> resources = new HashSet<string>();
                for (int i = 0; i < statement.Arguments.Length; i++)
                {
                    resources.Add(statement.Arguments[i].GetDecompiledName());
                }

                resources.Add(statement.Destination.GetDecompiledName());
                this.mentionedResources = resources.ToArray();
            }

        }


        public DecompiledBlock(string output, byte indent=0)
        {
            this.statement = Guid.Empty;
            this.indent = indent;
            this.originalLines = new int[0];
            this.decompiledOutput = output;
            this.mentionedResources = new string[0];
        }
    }
}
