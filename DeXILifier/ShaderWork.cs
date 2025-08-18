namespace DX9ShaderHLSLifier
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    public static class ShaderWork
    {
        public delegate void DecompilationRequestDelegate(DecompiledShader shader, Exception error);

        public static void RecreateShader(string filePath, DecompilationRequestDelegate onDecompiled, OptimizationParameters parameters=default, Action<float> progress01=null)
        {
            DecompiledShader output = default;
            try
            {
                string asciiDecompiledCode = FxCInteraction.ReadShaderFile(filePath);
                RecreateShaderInternal(ref output, asciiDecompiledCode, parameters, progress01);
                onDecompiled(output, default);
            }
            catch (Exception e)
            {
                onDecompiled(output, e);
            }
        }

        public static async Task<DecompiledShader?> RecreateShaderTask(string filePath, OptimizationParameters parameters = default, Action<float> progress01 = null)
        {
            DecompiledShader? shad = null;

            await Task.Run(() =>
            {
                RecreateShader(filePath, (output, e)=>shad = e == null ? output : (DecompiledShader?)null, parameters, progress01);
            });

            return shad;
        }

        public static Task RecreateShaderTask(string filePath, DecompilationRequestDelegate onDecompiled, OptimizationParameters parameters = default, Action<float> progress01 = null)
        {
            return Task.Run(() =>
            {
                RecreateShader(filePath, onDecompiled, parameters, progress01);
            });
        }

        private static void RecreateShaderInternal(ref DecompiledShader decompiled, string asciiDecompiledCode, OptimizationParameters parameters, Action<float> progress01)
        {
            var headerMatch = Regex.Match(asciiDecompiledCode, @"   (?:-| )*\r\n((?:\/\/   (?:[0-z]*) +(?:[A-z][0-9]{1,3}).*\r\n)+)");
            string header = headerMatch.Groups[1].Value;
            var bodyMatch = Regex.Match(asciiDecompiledCode, @"s_3_0\r\n((?:    .*\r\n)+)");
            string body = bodyMatch.Groups[1].Value;
            bool isVertex = "vs" == Regex.Match(asciiDecompiledCode, @"(.s)_3_").Groups[1].Value;

            progress01?.Invoke(0.1f);

            decompiled.rawOriginalText = asciiDecompiledCode;

            ShaderProgramObject stateMachine = new ShaderProgramObject(isVertex);

            progress01?.Invoke(0.1f);

            stateMachine.Parse(header, body);

            progress01?.Invoke(0.5f);

            decompiled.incomplete = stateMachine.Incomplete;

            if (!stateMachine.Incomplete)
            {
                stateMachine.Optimize(parameters);
                decompiled.dependencyGraph = stateMachine.DependencyGraph;
            }

            decompiled.rawHeader = header;
            decompiled.rawBody = body;

            decompiled.isVertexShader = isVertex;
            decompiled.entryPointName = stateMachine.Main;

            progress01?.Invoke(0.9f);

            stateMachine.Export(out decompiled.blocks);
        }

    }
}
