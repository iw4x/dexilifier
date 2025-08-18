namespace DX9ShaderHLSLifier
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using static DX9ShaderHLSLifier.ShaderProgramObject;

    public static class Instructions
    {
        public class SampleTexture : Instruction
        {
            public override int[] InputsMaximumWidth => new int[] { GetTexCoordWidth(), int.MaxValue };

            public override bool OutputInputAreSameSize => false;

            public override int OutputMaximumWidth => 4;

            private readonly Sampler.SamplerType samplerType;

            public SampleTexture(Sampler.SamplerType type, InstructionModifiers modifiers) : base(modifiers)
            {
                this.samplerType = type;
            }

            public override bool Simplify(CodeData destination, CodeData[] arguments)
            {
                return base.Simplify(destination, arguments);
            }


            protected virtual string GetSuffix() => string.Empty;


            protected virtual int GetTexCoordWidth() => this.samplerType == Sampler.SamplerType.sampler2D ? 2 : 3;

            protected override string FormatCallInternal(params string[] arguments)
            {
                // SWAPPED !!
                return string.Format("tex" + GetDimensionIndicator () + GetSuffix() + "({1}, {0})", arguments);
            }

            private string GetDimensionIndicator()
            {
                switch (samplerType)
                {
                    case Sampler.SamplerType.sampler2D:
                        return "2D";

                    case Sampler.SamplerType.sampler3D:
                        return "3D";

                    case Sampler.SamplerType.samplerCUBE:
                        return "CUBE";
                }

                throw new Exception($"unknown sampler type {samplerType}");
            }
        }

        public class SampleTextureLod : SampleTexture
        {
            public SampleTextureLod(Sampler.SamplerType type, InstructionModifiers modifiers) : base(type, modifiers)
            {
            }

            protected override string GetSuffix()
            {
                return "lod";
            }

            protected override int GetTexCoordWidth()
            {
                return 4;
            }
        }

        public class SampleTextureBias : SampleTexture
        {
            public SampleTextureBias(Sampler.SamplerType type, InstructionModifiers modifiers) : base(type, modifiers)
            {
            }

            protected override string GetSuffix()
            {
                return "bias";
            }

            protected override int GetTexCoordWidth()
            {
                return 4;
            }
        }


        public class SampleTextureProjected : SampleTexture
        {
            public SampleTextureProjected(Sampler.SamplerType type, InstructionModifiers modifiers) : base(type, modifiers)
            {
            }

            protected override string GetSuffix()
            {
                return "proj";
            }

            protected override int GetTexCoordWidth()
            {
                return 4;
            }
        }

        public class Move : Instruction
        {
            public override int[] InputsMaximumWidth => new int[] { 4 };

            public override bool OutputInputAreSameSize => true;

            public Move(InstructionModifiers modifiers) : base(modifiers)
            {
            }

            protected override string FormatCallInternal(params string[] arguments)
            {
                System.Diagnostics.Debug.Assert(arguments.Length == 1);
                return arguments[0];
            }
        }

        public class DotProduct : Instruction
        {
            public override int[] InputsMaximumWidth => new int[] { knownWidth.Value, knownWidth.Value, 1 };

            public override int OutputMaximumWidth => 1;

            public DotProduct(byte width, InstructionModifiers modifiers) : base(width, modifiers)
            {
            }

            public override bool Simplify(ShaderProgramObject.CodeData destination, ShaderProgramObject.CodeData[] arguments)
            {
                bool result = base.Simplify(destination, arguments);

                int inputWidth = 4;
                for (int i = 0; i < arguments.Length; i++)
                {
                    inputWidth = Math.Min(arguments[i].UsedChannels.Length, inputWidth);
                }

                // Shrink it
                result |= Shrink(inputWidth, arguments);


                return result;
            }

            protected override string FormatCallInternal(params string[] arguments)
            {
                System.Diagnostics.Debug.Assert(arguments.Length == 2 || arguments.Length == 3);

                if (knownWidth.HasValue && knownWidth.Value == 2 && arguments[2] != "0")
                {
                    // DP2Add
                    return string.Format("dot({0}, {1}) + {2}", arguments);
                }
                else
                {
                    return string.Format("dot({0}, {1})", arguments);
                }
            }
        }

        public class Multiply : Instruction
        {
            public override int[] InputsMaximumWidth => new int[] {4, 4, 4 };

            public override bool OutputInputAreSameSize => true;

            public Multiply(InstructionModifiers modifiers) : base(modifiers)
            {
            }

            public override bool Simplify(CodeData destination, CodeData[] arguments)
            {
                bool modified = base.Simplify(destination, arguments);

                // If one of the members of the operations is not of the same size, the multiplication must be uniform
                // Otherwise it means everybody is smaller
                if (arguments[0].UsedChannels.Length != arguments[1].UsedChannels.Length &&
                    arguments[0].UsedChannels.Length != 1 &&
                    arguments[1].UsedChannels.Length != 1)
                {
                    byte maxWidth = (byte)Math.Min(arguments[0].UsedChannels.Length, arguments[1].UsedChannels.Length);
                    destination.Shrink(maxWidth);
                    arguments[0].Shrink(maxWidth);
                    arguments[1].Shrink(maxWidth);
                }


                return modified;
            }

            public override bool FormatLine(CodeData destination, IReadOnlyList<CodeData> arguments, out string result)
            {
                System.Diagnostics.Debug.Assert(arguments.Count == 2);
                if ((arguments[0].ResourceHashCode == destination.ResourceHashCode) ^
                    (arguments[1].ResourceHashCode == destination.ResourceHashCode))
                {
                    CodeData otherArg = arguments[0].ResourceHashCode == destination.ResourceHashCode ?
                        arguments[1] : arguments[0];

                    result = $"{destination.GetDecompiledName()} *= {FormatCall(destination, new CodeData[] { otherArg })}";
                    return true;
                }


                return base.FormatLine(destination, arguments, out result);
            }

            protected override string FormatCallInternal(params string[] arguments)
            {
                System.Diagnostics.Debug.Assert(arguments.Length == 1 || arguments.Length == 2);

                if (arguments.Length == 1)
                {
                    return arguments[0];
                }
                else
                {
                    return string.Format("{0} * {1}", arguments);
                }
            }
        }

        public class Pow : Instruction
        {
            public override int[] InputsMaximumWidth => new int[] { 4, 4 };

            public override bool OutputInputAreSameSize => true;

            public Pow(InstructionModifiers modifiers) : base(modifiers)
            {
            }

            protected override string FormatCallInternal(params string[] arguments)
            {
                System.Diagnostics.Debug.Assert(arguments.Length == 2);

                return string.Format("pow(abs({0}), abs({1}))", arguments);
            }
        }

        public class DirectionDifference : Instruction
        {
            public override int[] InputsMaximumWidth => new int[] { 4 };

            public override bool OutputInputAreSameSize => true;

            private readonly bool isX;

            public DirectionDifference(bool isX, InstructionModifiers modifiers) : base(modifiers)
            {
                this.isX = isX;
            }


            protected override string FormatCallInternal(params string[] arguments)
            {
                System.Diagnostics.Debug.Assert(arguments.Length == 1);
                return string.Format("dd" + (isX ? "x" : "y") + "({0})", arguments);
            }
        }

        public class Sge : Instruction
        {
            public override int[] InputsMaximumWidth => new int[] { 4, 4 };

            public override bool OutputInputAreSameSize => true;

            public Sge(InstructionModifiers modifiers) : base(modifiers)
            {
            }

            protected override string FormatCallInternal(params string[] arguments)
            {
                System.Diagnostics.Debug.Assert(arguments.Length == 2);

                return string.Format("{1} <= {0} ? 1.0 : 0.0", arguments);
            }
        }

        public class Slt : Instruction
        {
            public override int[] InputsMaximumWidth => new int[] { 4, 4 };

            public override bool OutputInputAreSameSize => true;

            public Slt(InstructionModifiers modifiers) : base(modifiers)
            {
            }

            protected override string FormatCallInternal(params string[] arguments)
            {
                System.Diagnostics.Debug.Assert(arguments.Length == 2);

                return string.Format("{1} > {0} ? 1.0 : 0.0", arguments);
            }
        }

        public class Abs : Instruction
        {
            public override int[] InputsMaximumWidth => new int[] { 4 };

            public override bool OutputInputAreSameSize => true;

            public Abs(InstructionModifiers modifiers) : base(modifiers)
            {
            }

            protected override string FormatCallInternal(params string[] arguments)
            {
                return string.Format("abs({0})", arguments);
            }
        }

        public class Normalize : Instruction
        {
            public override int[] InputsMaximumWidth => new int[] { 4 };

            public override bool OutputInputAreSameSize => true;

            public Normalize(InstructionModifiers modifiers) : base(modifiers)
            {
            }

            protected override string FormatCallInternal(params string[] arguments)
            {
                System.Diagnostics.Debug.Assert(arguments.Length == 1);

                return string.Format("normalize({0})", arguments);
            }
        }

        public class Log : Instruction
        {
            public override int[] InputsMaximumWidth => new int[] { 4 };

            public override bool OutputInputAreSameSize => true;

            public Log(InstructionModifiers modifiers) : base(modifiers)
            {
            }

            protected override string FormatCallInternal(params string[] arguments)
            {
                System.Diagnostics.Debug.Assert(arguments.Length == 1);

                return string.Format("log({0})", arguments);
            }
        }

        public class Compare : Instruction
        {
            public override int[] InputsMaximumWidth => new int[] { 4, 4, 4 };

            public override bool OutputInputAreSameSize => true;

            public Compare(InstructionModifiers modifiers) : base(modifiers)
            {
            }

            protected override string FormatCallInternal(params string[] arguments)
            {
                System.Diagnostics.Debug.Assert(arguments.Length == 3);

                // TODO: Use step() for this?
                // UPDATE: Actually no, this is faster than a mix on a gpu!
                return string.Format("({0} >= 0 ? {1} : {2})", arguments);
            }
        }

        public class Lerp : Instruction
        {
            public override int[] InputsMaximumWidth => new int[] { 4, 4, 4 };

            public override bool OutputInputAreSameSize => true;

            public Lerp(InstructionModifiers modifiers) : base(modifiers)
            {
            }

            protected override string FormatCallInternal(params string[] arguments)
            {
                System.Diagnostics.Debug.Assert(arguments.Length == 3);

                return string.Format("lerp({0}, {1}, {2})", arguments);
            }
        }


        public class Exponential : Instruction
        {
            public override int[] InputsMaximumWidth => new int[] { 4 };

            public override bool OutputInputAreSameSize => true;

            public Exponential(InstructionModifiers modifiers) : base(modifiers)
            {
            }

            protected override string FormatCallInternal(params string[] arguments)
            {
                System.Diagnostics.Debug.Assert(arguments.Length == 1);

                return string.Format("exp({0})", arguments);
            }
        }


        public class Reciprocal : Instruction
        {
            public override int[] InputsMaximumWidth => new int[] { 4 };

            public override bool OutputInputAreSameSize => true;

            public Reciprocal(InstructionModifiers modifiers) : base(modifiers)
            {
            }

            protected override string FormatCallInternal(params string[] arguments)
            {
                System.Diagnostics.Debug.Assert(arguments.Length == 1);

                return string.Format("1 / {0}", arguments);
            }
        }

        public class ReciprocalSquareRoot : Instruction
        {
            public override int[] InputsMaximumWidth => new int[] { 4 };

            public override bool OutputInputAreSameSize => true;

            public ReciprocalSquareRoot(InstructionModifiers modifiers) : base(modifiers)
            {
            }

            protected override string FormatCallInternal(params string[] arguments)
            {
                System.Diagnostics.Debug.Assert(arguments.Length == 1);

                return string.Format("rsqrt({0})", arguments);
            }
        }
        public class SinCos : Instruction
        {
            public override int[] InputsMaximumWidth => new int[] { 1 };

            public override int OutputMaximumWidth => 2;

            public SinCos(InstructionModifiers modifiers) : base(modifiers)
            {
            }

            protected override string FormatCallInternal(CodeData destination, IReadOnlyList<CodeData> arguments)
            {
                System.Diagnostics.Debug.Assert(arguments.Count == 1);

                if (destination.UsedChannels.Length == 1)
                {
                    if (destination.UsedChannels[0] == 0)
                    {
                        return string.Format("sin({0})", arguments[0].GetDecompiledName());
                    }
                    else if(destination.UsedChannels[0] == 1)
                    {
                        return string.Format("cos({0})", arguments[0].GetDecompiledName());
                    }
                    else {
                        throw new Exception($"Invalid");
                    }
                }
                else
                {
                    return string.Format("float2(sin({0}), cos({0}))", arguments[0].GetDecompiledName());
                }
            }

            protected override string FormatCallInternal(params string[] arguments)
            {
                throw new NotImplementedException("Unreachable");
            }
        }

        public class Add : Instruction
        {
            public override int[] InputsMaximumWidth => new int[] { 4, 4};

            public override bool OutputInputAreSameSize => true;

            public Add(InstructionModifiers modifiers) : base(modifiers)
            {
            }

            public override bool FormatLine(CodeData destination, IReadOnlyList<CodeData> arguments, out string result)
            {
                System.Diagnostics.Debug.Assert(arguments.Count == 2);
                if ((arguments[0].ResourceHashCode == destination.ResourceHashCode) ^
                    (arguments[1].ResourceHashCode == destination.ResourceHashCode))
                {
                    CodeData otherArg = arguments[0].ResourceHashCode == destination.ResourceHashCode ?
                        arguments[1] : arguments[0];

                    result = $"{destination.GetDecompiledName()} += {FormatCall(destination, new CodeData[] { otherArg })}";
                    return true;
                }


                return base.FormatLine(destination, arguments, out result);
            }


            protected override string FormatCallInternal(params string[] arguments)
            {
                System.Diagnostics.Debug.Assert(arguments.Length == 1 || arguments.Length == 2);

                if (arguments.Length == 1)
                {
                    return arguments[0];
                }
                else
                {
                    return string.Format("{0} + {1}", arguments);
                }
            }
        }

        public class Frac : Instruction
        {
            public override int[] InputsMaximumWidth => new int[] { 4 };

            public override bool OutputInputAreSameSize => true;

            public Frac(InstructionModifiers modifiers) : base(modifiers)
            {
            }

            protected override string FormatCallInternal(params string[] arguments)
            {
                System.Diagnostics.Debug.Assert(arguments.Length == 1);

                return string.Format("frac({0})", arguments);
            }
        }

        public class Max : Instruction
        {
            public override int[] InputsMaximumWidth => new int[] { 4, 4 };

            public override bool OutputInputAreSameSize => true;

            public Max(InstructionModifiers modifiers) : base(modifiers)
            {
            }

            protected override string FormatCallInternal(params string[] arguments)
            {
                // TODO : Detect uniform multiplication
                System.Diagnostics.Debug.Assert(arguments.Length == 2);

                return string.Format("max({0}, {1})", arguments);
            }
        }

        public class Min : Instruction
        {
            public override int[] InputsMaximumWidth => new int[] { 4, 4 };

            public override bool OutputInputAreSameSize => true;

            public Min(InstructionModifiers modifiers) : base(modifiers)
            {
            }


            protected override string FormatCallInternal(params string[] arguments)
            {
                // TODO : Detect uniform multiplication
                System.Diagnostics.Debug.Assert(arguments.Length == 2);

                return string.Format("min({0}, {1})", arguments);
            }
        }

        public class MultiplyAdd : Instruction
        {
            public override int[] InputsMaximumWidth => new int[] { 4, 4, 4 };

            public override bool OutputInputAreSameSize => true;

            public MultiplyAdd(InstructionModifiers modifiers) : base(modifiers)
            {
            }

            public override bool Simplify(ShaderProgramObject.CodeData destination, ShaderProgramObject.CodeData[] arguments)
            {
                bool didAnything = base.Simplify(destination, arguments);

                System.Diagnostics.Debug.Assert(arguments.Length == 3);

                // TODO : Detect uniform multiplication

                for (int i = 0; i < arguments.Length; i++)
                {
                    if (arguments[i].UsedChannels.Length > 1)
                    {
                        bool isUniform = true;
                        int baseChannel = arguments[i].UsedChannels[0];
                        for (int channelIndex = 1; channelIndex < arguments[i].UsedChannels.Length; channelIndex++)
                        {
                            if (baseChannel != arguments[i].UsedChannels[channelIndex])
                            {
                                isUniform = false;
                                break;
                            }
                        }

                        if (isUniform)
                        {
                            arguments[i].Shrink(1); // Make "1 channel" wide
                            didAnything |= true;
                        }
                    }
                }                
                
                // If one of the members of the multiplication is not of the same size, the multiplication must be uniform
                // Otherwise it means everybody is smaller
                if (arguments[0].UsedChannels.Length != arguments[1].UsedChannels.Length &&
                    arguments[0].UsedChannels.Length != 1 &&
                    arguments[1].UsedChannels.Length != 1)
                {
                    byte maxWidth = (byte)Math.Min(arguments[0].UsedChannels.Length, arguments[1].UsedChannels.Length);
                    destination.Shrink(maxWidth);
                    arguments[0].Shrink(maxWidth);
                    arguments[1].Shrink(maxWidth);
                    arguments[2].Shrink(maxWidth);
                }


                return didAnything;
            }

            protected override string FormatCallInternal(CodeData destination, IReadOnlyList<CodeData> arguments)
            {
                System.Diagnostics.Debug.Assert(arguments.Count == 3);

                if (IsSimpleWidthIncrease(arguments, out float[] constA, out float[] constB))
                {
                    if (arguments[0] is ResourceData resourceDat) {
                        string[] stringElements = new string[resourceDat.UsedChannels.Length];

                        for (int i = 0; i < stringElements.Length; i++)
                        {
                            int constIndex = i;
                            if (constA.Length == 1 && constB.Length == 1)
                            {
                                constIndex = 0;
                            }

                            if (constA[constIndex] > 0)
                            {
                                stringElements[i] = $"{resourceDat.resource.GetDecompiledName()}.{ShaderStateMachineFormatter.VectorChannel(resourceDat.UsedChannels[i])}";
                            }
                            else
                            {
                                stringElements[i] = constB[constIndex].ToString();
                            }
                        }

                        // Simple float constructor
                        return $"float{stringElements.Length}({string.Join(", ", stringElements)})";
                    }
                }

                string[] strArguments = new string[arguments.Count + 2];

                strArguments[0] = arguments[0].GetDecompiledName(); // First one we don't touch

                // Second one might be inverted
                char multiply = '*';
                char add = '+';

                {
                    CodeData argument = arguments[1];
                    if (argument is ResourceData data)
                    {
                        if (data.isInverted)
                        {
                            ResourceData newData = data.Duplicate();
                            newData.isInverted = false;

                            strArguments[1] = newData.GetDecompiledName();
                            
                            multiply = '/';
                        }
                    }
                }

                {
                    CodeData argument = arguments[2];
                    if (argument is ResourceData data)
                    {
                        if (NegateConstantIfNecessary(data, out ResourceData newData))
                        {
                            strArguments[2] = newData.GetDecompiledName();
                            add = '-';
                        }
                        else
                        {
                            strArguments[2] = argument.GetDecompiledName();
                        }
                    }
                }

                // Fallback
                if (strArguments[1] == null)
                {
                    strArguments[1] = arguments[1].GetDecompiledName();
                }

                if (strArguments[2] == null)
                {
                    strArguments[2] = arguments[2].GetDecompiledName();
                }

                strArguments[3] = multiply.ToString();
                strArguments[4] = add.ToString();

                return FormatCallInternal(strArguments);
            }

            protected override string FormatCallInternal(params string[] arguments)
            {
                System.Diagnostics.Debug.Assert(arguments.Length == 5);

                //                     x   *   y   +   z
                return string.Format("{0} {3} {1} {4} {2}", arguments);
            }

            protected bool IsSimpleWidthIncrease(IReadOnlyList<CodeData> arguments, out float[] aValues, out float[] bValues)
            {
                System.Diagnostics.Debug.Assert(arguments.Count == 3);

                aValues = default;
                bValues = default;

                HardcodedExternalConstant constA = default;
                HardcodedExternalConstant constB = default;

                {
                    if (arguments[1] is ResourceData rsc && rsc.resource is HardcodedExternalConstant argA)
                    {
                        constA = argA;
                    }
                    else
                    {
                        return false;
                    }
                }

                {
                    if (arguments[2] is ResourceData rsc && rsc.resource is HardcodedExternalConstant argB)
                    {
                        constB = argB;
                    }
                    else
                    {
                        return false;
                    }
                }

                if (constA.values.Length != constB.values.Length)
                {
                    return false;
                }

                aValues = constA.GetConstantValues(arguments[1].UsedChannels);
                bValues = constB.GetConstantValues(arguments[2].UsedChannels);

                for (int i = 0; i < aValues.Length; i++)
                {
                    if (aValues[i] > 0 && bValues[i] > 0)
                    {
                        return false;
                    }

                    if (aValues[i] == 0 && bValues[i] == 0)
                    {
                        return false;
                    }
                }

                return true;
            }
        }
  
    }
}
