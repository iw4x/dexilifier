namespace DX9ShaderHLSLifier
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal static class FxCInteraction
    {
        public static string fxcPath = "support/fxc.exe";

        public static string ReadShaderFile(string filename)
        {
            string asciiShader;

            if (filename.EndsWith(".cso"))
            {
                asciiShader = GetDecompiledOutput(filename);
            }
            else
            {
                asciiShader = File.ReadAllText(filename);
            }

            return asciiShader;
        }

        public static string GetDecompiledOutput(string csoShaderFilename)
        {

            ProcessStartInfo pinfo = new ProcessStartInfo(fxcPath);
            pinfo.Arguments = $"/dumpbin \"{csoShaderFilename}\"";
            pinfo.CreateNoWindow = true;
            pinfo.UseShellExecute = false;
            pinfo.RedirectStandardOutput = true;
            pinfo.RedirectStandardInput = true;
            pinfo.RedirectStandardError = true;
            pinfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;

            var p = new Process();

            p.StartInfo = pinfo;

            if (p.Start())
            {
                string output = p.StandardOutput.ReadToEnd();

                p.WaitForExit();

                if (p.ExitCode == 0)
                {
                    return output;
                }

                throw new Exception($"Could not decompile shader!");
            }
            else
            {
                throw new Exception($"Could not start FXC");
            }
        }

        public static string GetCompiledOutput(DecompiledShader shader)
        {
            string filename = System.IO.Path.GetTempFileName();
            using (FileStream fs = File.OpenWrite(filename))
            {
                shader.Export(fs);
            }

            ProcessStartInfo pinfo = new ProcessStartInfo(fxcPath);
            pinfo.Arguments = $"/Cc /E {shader.entryPointName} /O3 /WX /Gec /T {(shader.isVertexShader ? "vs_3_0" : "ps_3_0")} \"{filename}\"";
            pinfo.CreateNoWindow = true;
            pinfo.UseShellExecute = false;
            pinfo.RedirectStandardOutput = true;
            pinfo.RedirectStandardInput = true;
            pinfo.RedirectStandardError = true;
            pinfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            var p = Process.Start(pinfo);

            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            StringBuilder stdoutBuilder = new StringBuilder();
            StringBuilder errorBuilder = new StringBuilder();

            p.OutputDataReceived += (sender, args) =>
            {
                stdoutBuilder.AppendLine(args.Data);
            };
            p.ErrorDataReceived += (sender, args) =>
            {
                errorBuilder.AppendLine(args.Data);
            };


            p.WaitForExit();

            string output = stdoutBuilder.ToString();
            string error = errorBuilder.ToString();

            File.Delete(filename);

            if (p.ExitCode == 0)
            {
                return output;
            }

            throw new Exception(error.Replace(filename, ""));
        }
    }
}
