namespace DeXILifier
{
    using DeXILifier.Models;
    using SharpDX;
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;
    using static DeXILifier.TechniqueSetAnalyzer;

    public static class TechniqueSetAnalyzer
    {
        internal readonly struct MasterShader
        {
            public readonly string name;
            public readonly string code;
        }

        internal class Technique
        {
            public readonly string name;
            public readonly string pixelShaderName;
            public readonly string vertexShaderName;

            public Technique(in Explorer.TechniqueTreeNode technique)
            {
                name = technique.technique.name;
                pixelShaderName = technique.technique.PixelShader;
                vertexShaderName = technique.technique.VertexShader;
            }
        }

        internal class TechniqueSet
        {
            public readonly string name;

            private readonly Dictionary<MaterialTechniqueType, Technique> techniquesPerSlot = new Dictionary<MaterialTechniqueType, Technique>();

            public TechniqueSet(in Explorer.TechniqueSetTreeNode techniqueSetTreeNode)
            {
                name = techniqueSetTreeNode.techset.name;

                foreach(var technique in techniqueSetTreeNode.Techniques)
                {
                    var tech = new Technique(technique);
                    for (int typeIndex = 0; typeIndex < technique.usedInTypes.Count; typeIndex++)
                    {
                        MaterialTechniqueType techType = technique.usedInTypes[typeIndex];
                        techniquesPerSlot.Add(techType, tech);
                    }
                }
            }

            public IReadOnlyList<Technique> GetLitTechniques()
            {
                List<Technique> techniques = new List<Technique>();
                for (MaterialTechniqueType i = MaterialTechniqueType.TECHNIQUE_LIT_BEGIN; i < MaterialTechniqueType.TECHNIQUE_LIT_END; i++)
                {
                    if (techniquesPerSlot.TryGetValue(i, out Technique technique))
                    {
                        techniques.Add(technique);
                    }
                }

                return techniques;
            }
        }


        internal static async Task Analyze(string baseBath, Explorer.TechniqueSetTreeNode exploredTechSet)
        {
            TechniqueSet techSet = new TechniqueSet(exploredTechSet);

            await AnalyzeLitGroup(baseBath, techSet);
        }

        private static async Task AnalyzeLitGroup(string baseBath, TechniqueSet techSet)
        {
            IReadOnlyList<Technique> techniques = techSet.GetLitTechniques();

            // Cannot use hashset here cause order is important
            List<string> allPixelShaders = new List<string>();
            List<string> allVertexShaders = new List<string>();

            for (int i = 0; i < techniques.Count; i++)
            {
                if (!allPixelShaders.Contains(techniques[i].pixelShaderName))
                {
                    allPixelShaders.Add(techniques[i].pixelShaderName);
                }

                if (!allVertexShaders.Contains(techniques[i].vertexShaderName))
                {
                    allVertexShaders.Add(techniques[i].vertexShaderName);
                }
            }

            MasterShader masterPixel = await RecomposePixelShader(baseBath, allPixelShaders);
        }

        private static async Task<MasterShader> RecomposePixelShader(string basePath, IReadOnlyList<string> shaderNames)
        {
            for (int i = 0; i < shaderNames.Count; i++)
            {
                string shaderName = shaderNames[i];
                string filePath = System.IO.Path.Combine(basePath, "ps", $"{shaderName}.cso");
                DecompiledShader? shader = await ShaderWork.RecreateShaderTask(
                    filePath,
                    new OptimizationParameters()
                    {
                        inlineConstants = true,
                        reduceInstructions = true,
                        renameVariablesBasedOnUsage = true,
                        reorganize = true
                    }
                );

                if (shader.HasValue)
                {

                }
            }

            return new MasterShader();
        }
    }
}
