namespace TechsetGenerator
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using SharpDX.D3DCompiler;

    public struct Requirements
    {
        public enum Type
        {
            World,
            Model
        }

        public struct Layer 
        {
            public enum ZMode
            {
                Blend,
                Replace,
                Test
            }

            public bool hasColorMap;  // c0
            public bool hasNormalMap;
            public bool hasDetailMap;
            public ZMode zMode;
        }

        public bool vertexColor; // m => mc
        public bool hsmSupport; // sm => hsm

        public bool NeedsZPrepass => layers.Any(o => o.zMode != Layer.ZMode.Blend);
        public bool NeedsBuildFloatZ => layers.Any(o => o.zMode != Layer.ZMode.Blend);
        public bool NeedsShadowmapBuild => layers.Any(o => o.zMode != Layer.ZMode.Blend);

        public Layer[] layers; // t0c0, r1c1, t2c2
    }


    internal class Program
    {

        static void Main(string[] args)
        {
            string outputFolder = "generated";

            Directory.CreateDirectory(outputFolder);
            ShaderBytecode.Compile()

        }
    }
}
