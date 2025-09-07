namespace DeXILifier.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [System.Serializable]
    public struct Technique
    {
        public struct MaterialPass
        {
            public string vertexDeclaration;
            public string vertexShader;
            public string pixelShader;
        }

        public string VertexDeclaration => passArray[0].vertexDeclaration;
        public string VertexShader => passArray[0].vertexShader;
        public string PixelShader => passArray[0].pixelShader;

        public string name;
        public MaterialPass[] passArray;

    }
}
