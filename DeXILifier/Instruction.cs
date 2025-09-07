namespace DeXILifier
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Xml.Linq;
    using static DeXILifier.Instructions;
    using static DeXILifier.ShaderProgramObject;

    public abstract class Instruction
    {
        public readonly byte? knownWidth;

        public readonly InstructionModifiers modifiers;

        public virtual int InputCount => 2;

        public virtual int OutputMaximumWidth => 4;

        public virtual int OutputMinimumWidth => 1;

        public virtual bool OutputInputAreSameSize => true;

        public virtual bool DestinationMaskDictatesInputWidth => OutputInputAreSameSize;

        protected Instruction(byte width, InstructionModifiers modifiers) : this(modifiers)
        {
            this.knownWidth = width;
        }

        protected Instruction(InstructionModifiers modifiers)
        {
            this.modifiers = modifiers;
        }

        public abstract int[] GetInputsMaximumWidth(IReadOnlyList<CodeData> inputs);

        public virtual bool Simplify(ShaderProgramObject.CodeData destination, ShaderProgramObject.CodeData[] arguments)
        {
            Debug.Assert(arguments.Length == InputCount);
            Debug.Assert(GetInputsMaximumWidth(arguments).Length == InputCount);

            bool modified = false;

            if (destination.UsedChannels.Length > OutputMaximumWidth)
            {
                destination.Shrink(OutputMaximumWidth);
                modified |= true;
            }

            if (GetInputsMaximumWidth(arguments).Length < arguments.Length)
            {
                throw new Exception($"Misconfiguration on {GetType()}");
            }

            // If destinations have "never been written" it means they've been shrank
            if (destination is VariableData varData &&
                !varData.variable.HaveChannelsBeenWritten(destination.UsedChannels))
            {
                byte biggestWrittenChannel = varData.GetDataWidth();
                
                // this does not make sense?
                if (biggestWrittenChannel < destination.UsedChannels.Length)
                {
                    destination.Shrink(biggestWrittenChannel);
                    modified |= true;
                }
            }

            for (int i = 0; i < arguments.Length; i++)
            {
                int maxWidth = GetInputsMaximumWidth(arguments)[i];
                if (arguments[i].UsedChannels.Length > maxWidth)
                {
                    arguments[i].Shrink(maxWidth);
                    modified |= true;
                }
            }

            if (OutputInputAreSameSize)
            {
                int maxWidth = destination.UsedChannels.Length;
                for (int i = 0; i < arguments.Length; i++)
                {
                    if (arguments[i].UsedChannels.Length > maxWidth)
                    {
                        modified |= Shrink(maxWidth, arguments[i]);
                    }
                }
            }

            // Fix double negations
            for (int i = 0; i < arguments.Length; i++)
            {
                if (arguments[i] is ShaderProgramObject.ResourceData data)
                {
                    {
                        if (FlipSignIfMajorityIsNegative(data, out ResourceData newData))
                        {
                            newData.modifiers.isNegated = !data.modifiers.isNegated; // + to -
                            arguments[i] = newData;

                            modified |= true;
                        }
                    }

                    // Invert suspicious constants
                    if (!data.isInverted)
                    {
                        if (InvertConstantIfNecessary(data, out ResourceData newData))
                        {
                            newData.isInverted = !data.isInverted;
                            arguments[i] = newData;

                            modified |= true;
                        }
                    }
                }
            }

            return modified;
        }

        protected bool InvertConstantIfNecessary(ResourceData data, out ResourceData newData)
        {
            if (data.resource is HardcodedExternalConstant constant)
            {
                int invertedChannels = data.UsedChannels.Length;
                float?[] invertedValues = new float?[constant.values.Length];

                for (int channelIndex = 0; channelIndex < data.UsedChannels.Length; channelIndex++)
                {
                    byte channel = data.UsedChannels[channelIndex];
                    byte mask = (byte)(1 << channel);

                    if (constant.AreChannelsInlined(mask) && // No optimization if it's not inlined
                        constant.IsLikelyInverted(channel, out float invertedValue))
                    {
                        invertedValues[channel] = invertedValue;
                    }
                    else
                    {
                        System.Diagnostics.Debug.Assert(channel < invertedValues.Length);
                        System.Diagnostics.Debug.Assert(channel < constant.values.Length);

                        invertedValues[channel] = 1f / constant.values[channel];
                        invertedChannels--;
                    }
                }

                if (invertedChannels > data.UsedChannels.Length / 2)
                {
                    // ⚠ they are now in order!
                    float[] values = new float[data.UsedChannels.Length];
                    byte[] newUsedChannels = new byte[data.UsedChannels.Length];
                    for (int channelIndex = 0; channelIndex < data.UsedChannels.Length; channelIndex++)
                    {
                        System.Diagnostics.Debug.Assert(invertedValues[data.UsedChannels[channelIndex]].HasValue);

                        values[channelIndex] = invertedValues[data.UsedChannels[channelIndex]].Value;
                        newUsedChannels[channelIndex] = (byte)channelIndex; // in order
                    }

                    HardcodedExternalConstant newConstant =
                        new HardcodedExternalConstant(
                            $"opt_{constant.GetDecompiledNameForChannels(data.UsedChannels)}",
                            values,
                            detectKnownConstants: false);

                    newConstant.SetInlined(byte.MaxValue); // Inline all because this is an optimization constant

                    newData = new ResourceData(
                         newConstant,
                         data.isVertexShader,
                         modifiers: data.modifiers,
                         swizzle: true,
                         newUsedChannels
                     );

                    return true;
                }
            }

            newData = default;
            return false;
        }

        protected bool FlipSignIfMajorityIsNegative(ResourceData data, out ResourceData newData)
        {
            if (data.resource is HardcodedExternalConstant constant)
            {
                int negatedChannels = data.UsedChannels.Length;

                for (int channelIndex = 0; channelIndex < data.UsedChannels.Length; channelIndex++)
                {
                    byte channel = data.UsedChannels[channelIndex];
                    byte mask = (byte)(1 << channel);
                    if (!constant.AreChannelsInlined(mask) || // No optimization if it's not inlined
                        constant.values[channel] >= 0)
                    {
                        negatedChannels--;
                    }
                }

                if (negatedChannels > data.UsedChannels.Length / 2) // More than half the channels are double negations, let's change this
                {
                    // ⚠ they are now in order!
                    float[] values = new float[data.UsedChannels.Length];
                    byte[] newUsedChannels = new byte[data.UsedChannels.Length];
                    for (int channelIndex = 0; channelIndex < data.UsedChannels.Length; channelIndex++)
                    {
                        values[channelIndex] = -constant.values[data.UsedChannels[channelIndex]];
                        newUsedChannels[channelIndex] = (byte)channelIndex; // in order
                    }

                    HardcodedExternalConstant newConstant =
                        new HardcodedExternalConstant(
                            $"opt_{constant.GetDecompiledNameForChannels(data.UsedChannels)}",
                            values,
                            detectKnownConstants: false);

                    newConstant.SetInlined(byte.MaxValue); // Inline all because this is an optimization constant

                    newData = new ResourceData(
                         newConstant,
                         data.isVertexShader,
                         modifiers: data.modifiers,
                         swizzle: true,
                         newUsedChannels
                     );

                    return true;
                }
            }

            newData = default;
            return false;
        }

        protected bool NegateConstantIfPossible(ResourceData data, out ResourceData newData)
        {
            if (data.modifiers.isNegated)
            {
                newData = data.Duplicate();
                newData.modifiers.isNegated = false;
                return true;
            }

            newData = default;
            return false;
        }

        protected bool Shrink(int maxWidth, params ShaderProgramObject.CodeData[] arguments)
        {
            bool modified = false;

            for (int i = 0; i < arguments.Length; i++)
            {
                if (arguments[i].UsedChannels.Length > maxWidth)
                {
                    arguments[i].Shrink(maxWidth);
                    modified |= true;
                }
            }

            return modified;
        }

        public virtual bool FormatLine(CodeData destination, IReadOnlyList<CodeData> arguments, out string result)
        {
            result = default;
            return false;
        }

        public string FormatCall(CodeData destination, IReadOnlyList<CodeData> arguments)
        {
            string str = FormatCallInternal(destination, arguments);

            if (modifiers.isSaturated)
            {
                str = string.Format("saturate({0})", str);
            }

            if (modifiers.isAbsolute)
            {
                str = string.Format("abs({0})", str);
            }

            if (!OutputInputAreSameSize && 
                (/*OutputMaximumWidth > destination.UsedChannels.Length || */OutputMinimumWidth > destination.UsedChannels.Length) &&
                arguments.Count > 0)
            {
                // We need to add a swizzle to add implicit vector truncation
                string swizzle = "({0}).";

                for (int i = 0; i < destination.UsedChannels.Length; i++)
                {
                    swizzle += arguments[0].RepresentsColor ? destination.UsedChannels[i].ColorChannel() : destination.UsedChannels[i].VectorChannel();
                }

                str = string.Format(swizzle, str);
            }

            return str;
        }

        protected virtual string FormatCallInternal(CodeData destination, IReadOnlyList<CodeData> arguments)
        {
            string[] strArguments = new string[arguments.Count];

            for (int argumentIndex = 0; argumentIndex < arguments.Count; argumentIndex++)
            {
                CodeData argument = arguments[argumentIndex];
                strArguments[argumentIndex] = argument.GetDecompiledName();
            }

            return FormatCallInternal(strArguments);
        }

        protected abstract string FormatCallInternal(params string[] arguments);
    }
}
