namespace DX9ShaderHLSLifier
{
    using System.IO;

    public struct DecompiledShader
    {
        public string rawOriginalText;
        public string rawHeader;
        public string rawBody;

        public bool incomplete;

        public DependencyGraph dependencyGraph;

        public bool isVertexShader;
        public string entryPointName;
        public DecompiledBlock[] blocks;

        public void Export(Stream into)
        {
            using (StreamWriter sw = new StreamWriter(into, System.Text.Encoding.ASCII, 2048, leaveOpen: true))
            {
                for (int i = 0; i < blocks.Length; i++)
                {
                    for (int indentIndex = 0; indentIndex < blocks[i].indent; indentIndex++)
                    {
                        sw.Write("\t");
                    }

                    sw.WriteLine(blocks[i].decompiledOutput);
                }
            }
        }
    }
}
