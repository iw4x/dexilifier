namespace DX9ShaderHLSLifier
{
    using System.Text;

    public static class ShaderStateMachineFormatter
    {
        public static StringBuilder Indent(this StringBuilder sb)
        {
            sb.Append('\t');
            return sb;
        }

        public static void BeginStruct(this StringBuilder sb, string name) {
            sb.AppendLine($"struct {name}");
            sb.AppendLine("{");
        }

        public static void EndStruct(this StringBuilder sb)
        {
            sb.AppendLine("};");
        }

        public static void BeginBlock(this StringBuilder sb) { 
            sb.AppendLine("{"); 
        }

        public static void EndBlock(this StringBuilder sb) { 
            sb.AppendLine("}");
        }

        public static byte ByteChannel(this char chan)
        {
            switch (chan)
            {
                case 'x': return (byte)0;
                case 'y': return (byte)1;
                case 'z': return (byte)2;
                case 'w': return (byte)3;
            }

            throw new System.Exception($"invalid channel {chan}");
        }

        public static char VectorChannel(this byte chan)
        {
            switch (chan)
            {
                case 0: return 'x';
                case 1: return 'y';
                case 2: return 'z';
                case 3: return 'w';
            }

            throw new System.Exception($"invalid channel {chan}");
        }

        public static char ColorChannel(this byte chan)
        {
            switch(chan)
            {
                case 0: return 'r';
                case 1: return 'g';
                case 2: return 'b';
                case 3: return 'a';
            }

            throw new System.Exception($"invalid channel {chan}");
        }

        public static string Declare(this ShaderProgramObject.Type type, bool isHalfPrecision) {

            if (type.ToString().StartsWith("_"))
            {
                throw new System.Exception($"Non declarable this way");
            }

            return isHalfPrecision ? type.ToString().Replace("float", "half") : type.ToString();
        }
    }
}
