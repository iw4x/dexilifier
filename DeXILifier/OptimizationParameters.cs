namespace DX9ShaderHLSLifier
{
    public struct OptimizationParameters
    {
        public bool reorganize;

        public bool inlineConstants;

        public bool renameVariablesBasedOnUsage;

        public bool reduceInstructions;

        public bool alwaysCreateNewVariables;
    }
}
