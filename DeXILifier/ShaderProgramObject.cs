namespace DX9ShaderHLSLifier
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Resources;
    using System.Runtime.Remoting.Channels;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;

    public class ShaderProgramObject
    {
        private const byte R = 1;
        private const byte G = 1 << 1;
        private const byte B = 1 << 2;
        private const byte A = 1 << 3;

        private const byte X = R;
        private const byte Y = G;
        private const byte Z = B;
        private const byte W = A;

        private const string VX_OUT_NAME = "vout";
        private const string VX_IN_NAME = "vin";
        private const string PX_IN_NAME = "inputVx";
        private const string PX_OUT_NAME = "outColor";


        public enum Type
        {
            @float = 1,
            float2 = 2,
            float3 = 3,
            float4 = 4,

            // Their width is not important
            _Sampler,
            _Matrix
        }


        public struct InstructionModifiers
        {
            public bool isHalfPrecision;
            public bool isAbsolute;
            public bool isSaturated;
        }

        public abstract class ShaderResource
        {
            public virtual int? DeclaredAtLine { get; protected set; }

            protected string decompiledName;


            public ShaderResource(int? lineDeclared)
            {
                DeclaredAtLine = lineDeclared;
            }

            public virtual string GetDecompiledName() { return decompiledName; }

            public abstract void Declare(in List<DecompiledBlock> lines);
        }

        public abstract class InputOutput : ShaderResource
        {
            private const byte ABBREVIATED_NAME_LENGTH = 3;

            public Type Type => type;

            public string MemberName => memberName;

            public bool isHalfPrecision;

            protected Type type;

            protected string memberName;

            protected readonly bool isVertexShader;

            protected InputOutput(bool isVertexShader, int? lineDeclared, string semantic) : base(lineDeclared)
            {
                this.isVertexShader = isVertexShader;
                memberName = semantic;
            }

            public string GetUsageName()
            {
                if (memberName.Length > ABBREVIATED_NAME_LENGTH)
                {
                    return (memberName.Substring(0, ABBREVIATED_NAME_LENGTH) + memberName[memberName.Length - 1]).ToLower();
                }


                return memberName.ToLower();
            }

            protected virtual string GetSemantic()
            {
                string semantic = this.memberName.ToUpper();

                if (semantic.StartsWith("TEXCOORD") || semantic.StartsWith("COLOR"))
                {
                    if (!char.IsNumber(semantic[semantic.Length - 1]))
                    {
                        semantic += '0';
                    }
                }

                return semantic;
            }
        }

        private class Input : InputOutput
        {
            public Input(bool isVertexShader, int? lineDeclared, string semantic, Type type) : base(isVertexShader, lineDeclared, semantic)
            {
                this.type = type; // We can discuss half later
            }

            public override string GetDecompiledName()
            {
                return $"{(isVertexShader ? VX_IN_NAME : PX_IN_NAME)}.{memberName}";
            }

            public override void Declare(in List<DecompiledBlock> lines)
            {
                // half4 color : COLOR0;
                lines.Add(new DecompiledBlock($"{type.Declare(isHalfPrecision)} {memberName} : {GetSemantic()};", indent: 1));
            }
        }

        public class ColorOutput : Output
        {
            public ColorOutput() : base(isVertexShader: false, null, PX_OUT_NAME, Type.float4)
            {
            }

            public override string GetDecompiledName()
            {
                return PX_OUT_NAME;
            }

        }

        public class Output : InputOutput
        {

            public Output(bool isVertexShader, int? lineDeclared, string name, Type type) : base(isVertexShader, lineDeclared, name)
            {
                this.type = type; // We can discuss half later
            }

            public override string GetDecompiledName()
            {
                return $"{(isVertexShader ? VX_OUT_NAME : PX_OUT_NAME)}.{memberName}";
            }

            public override void Declare(in List<DecompiledBlock> lines)
            {
                // half4 color : COLOR0;
                lines.Add(new DecompiledBlock($"{type.Declare(isHalfPrecision)} {memberName} : {GetSemantic()};", indent: 1));
            }
        }

        public class MatrixChannelAccessResource : ExternalResource
        {
            public readonly int index;

            public readonly MatrixResource matrix;

            public MatrixChannelAccessResource(MatrixResource baseResource, int index) : base((Type)baseResource.columns)
            {
                this.matrix = baseResource;
                this.decompiledName = $"{baseResource.GetDecompiledName()}[{index}]";
                this.index = index;
            }


            public override void Declare(in List<DecompiledBlock> lines)
            {
                if (index == 0)
                {
                    matrix.Declare(lines);
                }
            }
        }

        public class MatrixResource : ExternalResource
        {
            public readonly byte rows;
            public readonly byte columns;

            public MatrixResource(string name, string preferredRegister, byte rows, byte columns) : base(name, preferredRegister, Type._Matrix)
            {
                this.rows = rows;
                this.columns = columns;
            }

            public override void Declare(in List<DecompiledBlock> lines)
            {
                lines.Add(new DecompiledBlock($"extern float{rows}x{columns} {GetDecompiledName()};", indent: 0));
            }
        }

        public class ExternalResource : ShaderResource
        {
            public readonly Type type;
            public readonly string preferredRegister;

            public ExternalResource(string name, string preferredRegister, Type t = Type.float4) : this(t)
            {
                this.decompiledName = name;
                this.preferredRegister = preferredRegister;
            }

            protected ExternalResource(Type type) : base(null)
            {
                this.type = type;
            }

            public override void Declare(in List<DecompiledBlock> lines)
            {
                lines.Add(new DecompiledBlock($"extern {type.Declare(false)} {this.GetDecompiledName()} : register({preferredRegister});", indent: 0));
            }
        }

        public class HardcodedExternalConstant : ExternalResource
        {
            private readonly struct KnownConstant
            {
                public bool Valid => !string.IsNullOrEmpty(name);

                public readonly string name;

                public readonly float value;

                private KnownConstant(string name, float value)
                {
                    this.name = name;
                    this.value = value;
                }


                //#define PI 3.1415926538
                public static bool IsAny(float value, out KnownConstant constant)
                {
                    bool approxEquals(float a, float b) { return Math.Abs(a - b) < 0.0001; }

                    constant = default;

                    if (approxEquals(value, (float)Math.PI))
                    {
                        constant = new KnownConstant("PI", (float)Math.PI);
                    }
                    else if (approxEquals(value, (float)(Math.PI * 2f)))
                    {
                        constant = new KnownConstant("TAU", (float)(Math.PI * 2));
                    }
                    else if (approxEquals(value, (float)(Math.PI / 2f)))
                    {
                        constant = new KnownConstant("HALF_PI", (float)(Math.PI / 2));
                    }
                    else if (approxEquals(value, 2.2f))
                    {
                        constant = new KnownConstant("GAMMA_BRIGHT", 2.2f);
                    }
                    else if (approxEquals(value, 2.4f))
                    {
                        constant = new KnownConstant("GAMMA", 2.4f);
                    }
                    else if (approxEquals(value, (float)Math.Log(Math.E, 2)))
                    {
                        constant = new KnownConstant("LOG2E", (float)Math.Log(Math.E, 2));
                    }
                    else if (approxEquals(value, (float)Math.Log10(Math.E)))
                    {
                        constant = new KnownConstant("LOG10E", (float)Math.Log10(Math.E));
                    }
                    else if (approxEquals(value, (float)Math.E))
                    {
                        constant = new KnownConstant("E", (float)Math.E);
                    }
                    else if (approxEquals(value, (float)Math.Sqrt(Math.E)))
                    {
                        constant = new KnownConstant("SQRT_E", (float)Math.Sqrt(Math.E));
                    }
                    else if (approxEquals(value, (float)0.017453))
                    {
                        constant = new KnownConstant("DEG2RAD", (float)0.017453);
                    }
                    else if (approxEquals(value, (float)57.2957))
                    {
                        constant = new KnownConstant("RAD2DEG", (float)57.2957);
                    }
                    else if (approxEquals(value, (float)Math.Cos(1)))
                    {
                        constant = new KnownConstant("COS1", (float)Math.Cos(1));
                    }
                    else if (approxEquals(value, (float)Math.Sin(1)))
                    {
                        constant = new KnownConstant("SIN1", (float)Math.Sin(1));
                    }

                    return constant.Valid;
                }
            }

            public bool ContainsUniversalConstant => anyKnownConstant;

            public readonly float[] values = new float[4];

            private readonly KnownConstant[] knownConstants = new KnownConstant[4];

            private bool anyKnownConstant = false;

            private byte inlinedChannels = 0;


            public HardcodedExternalConstant(string name, float[] values, bool detectKnownConstants = true) : base(name, default)
            {
                values.CopyTo(this.values, 0);

                if (detectKnownConstants)
                {
                    DetectKnownConstants();
                }
            }

            public override void Declare(in List<DecompiledBlock> lines)
            {
                if (inlinedChannels == 0 && !anyKnownConstant)
                {
                    lines.Add(new DecompiledBlock($"static const float4 {this.GetDecompiledName()} = float4({string.Join(", ", values)});", indent: 0, DeclaredAtLine.HasValue ? DeclaredAtLine.Value : 0));
                }
                else
                {
                    for (int i = 0; i < values.Length; i++)
                    {
                        if (knownConstants[i].Valid)
                        {
                            lines.Add(new DecompiledBlock($"#define {knownConstants[i].name} {knownConstants[i].value}", indent: 0, DeclaredAtLine.HasValue ? DeclaredAtLine.Value : 0));
                        }
                        else
                        {
                            bool isInlined = (inlinedChannels & (1 << i)) != 0;
                            if (isInlined)
                            {
                                // No declaration
                            }
                            else
                            {
                                lines.Add(new DecompiledBlock($"static const float {GetDecompiledNameForChannel(i)} = {values[i]};", indent: 0, DeclaredAtLine.HasValue ? DeclaredAtLine.Value : 0));
                            }
                        }
                    }
                }
            }

            public float[] GetConstantValues(byte[] channels)
            {
                float[] selectedValues = new float[channels.Length];
                for (int i = 0; i < channels.Length; i++)
                {
                    selectedValues[i] = values[channels[i]];
                }

                return selectedValues;
            }

            public override string GetDecompiledName()
            {
                return GetDecompiledNameForChannels(new byte[] { 0, 1, 2, 3 });
            }

            public bool IsLikelyInverted(byte channel, out float invertedValue)
            {
                float value = values[channel];

                if (value < 0.01)
                {
                    float invert = 1f / value;
                    int floored = (int)Math.Round(invert);

                    if (Math.Abs(floored - invert) < 0.001)
                    {
                        // This is very close to a constant integer, so it might be that
                        invertedValue = floored;
                        return true;
                    }
                }

                invertedValue = default;
                return false;
            }

            public bool AreChannelsInlined(byte channelMask)
            {
                return (this.inlinedChannels & channelMask) > 0;
            }

            public string GetDecompiledNameForChannels(byte[] channels)
            {
                bool needsExplicitAccess = true;

                if (inlinedChannels == 0 && channels.Length == values.Length)
                {
                    bool accessedInOrder = true;
                    for (int i = 0; i < channels.Length; i++)
                    {
                        accessedInOrder |= channels[i] == i;
                        if (!accessedInOrder)
                        {
                            break;
                        }
                    }

                    if (accessedInOrder)
                    {
                        needsExplicitAccess = false;
                    }
                }

                if (needsExplicitAccess)
                {
                    string[] elements = new string[channels.Length];

                    for (int i = 0; i < channels.Length; i++)
                    {
                        if (knownConstants[channels[i]].Valid)
                        {
                            elements[i] = GetDecompiledNameForChannel(channels[i]);
                        }
                        else
                        {
                            bool isInlined = (inlinedChannels & (1 << i)) != 0;

                            if (isInlined)
                            {
                                elements[i] = values[channels[i]].ToString();
                            }
                            else
                            {
                                elements[i] = GetDecompiledNameForChannel(channels[i], standalone: anyKnownConstant);
                            }
                        }
                    }

                    return elements.Length > 1 ? $"float{elements.Length}({string.Join(", ", elements)})" : elements[0];
                }
                else
                {
                    return base.GetDecompiledName();
                }
            }

            public void SetInlined(byte inlinedChannels)
            {
                this.inlinedChannels = inlinedChannels;
            }

            private string GetDecompiledNameForChannel(int channel, bool standalone = true)
            {
                if (knownConstants[channel].Valid)
                {
                    return knownConstants[channel].name;
                }
                else
                {
                    return $"{this.GetDecompiledName()}{(standalone ? "_" : ".")}{ShaderStateMachineFormatter.VectorChannel((byte)channel)}";
                }
            }

            private void DetectKnownConstants()
            {
                for (int i = 0; i < values.Length; i++)
                {
                    if (KnownConstant.IsAny(values[i], out KnownConstant constant))
                    {
                        knownConstants[i] = constant;
                        anyKnownConstant = true;
                    }
                }
            }
        }

        public class Sampler : ExternalResource
        {
            public enum SamplerType
            {
                Invalid,
                sampler2D,
                sampler3D,
                samplerCUBE
            }

            public SamplerType samplerType = SamplerType.sampler2D;

            public Sampler(string name, string preferredRegister) : base(name, preferredRegister, Type._Sampler)
            {
            }

            public override void Declare(in List<DecompiledBlock> lines)
            {
                lines.Add(new DecompiledBlock($"extern {samplerType} {this.GetDecompiledName()} : register({preferredRegister});", indent: 0));
            }

            public string GetAbbreviatedName()
            {
                return this.decompiledName.ToLower().Replace("sampler", "");
            }

            public override string GetDecompiledName()
            {
                return base.GetDecompiledName();
            }
        }

        public class Comment : Statement
        {
            private readonly string comment;

            public Comment(int line, string comment) : base(line, default, default, default)
            {
                this.comment = comment;
            }

            public override void Print(StringBuilder builder)
            {
                builder.Append($"// {comment}");
            }
        }

        public class Statement
        {
            public Guid Guid { get; }

            public CodeData Destination { get; }
            public CodeData[] Arguments { get; }
            public Instruction Instruction { get; }

            public readonly int[] lines;

            public Statement(int originalLine, Instruction instruction, CodeData destination, params CodeData[] arguments) : this(new int[] { originalLine }, instruction, destination, arguments)
            {

            }

            public Statement(int[] originalLines, Instruction instruction, CodeData destination, params CodeData[] arguments)
            {
                this.lines = originalLines;
                this.Instruction = instruction;
                this.Destination = destination;
                this.Arguments = arguments;
                this.Guid = Guid.NewGuid();

                if (destination != null && arguments != null)
                {
                    destination.NotifyUsedInStatement(this);
                    for (int i = 0; i < arguments.Length; i++)
                    {
                        arguments[i].NotifyUsedInStatement(this);
                    }
                }

                if (instruction != null)
                {
                    // Now we can specify the arguments and/or destination depending on what instruction is used
                    if (instruction.DestinationMaskDictatesInputWidth)
                    {
                        if (destination.ExplicitChannelAccess)
                        {
                            // The operation is "masked" - inputs that are bigger than the destination use only select channels
                            for (int i = 0; i < arguments.Length; i++)
                            {
                                if (destination.UsedChannels.Length < arguments[i].UsedChannels.Length ||
                                    !arguments[i].ExplicitChannelAccess)
                                {
                                    byte[] newChannels = new byte[destination.UsedChannels.Length];
                                    for (int channelIndex = 0; channelIndex < newChannels.Length; channelIndex++)
                                    {
                                        byte maskChannel = destination.UsedChannels[channelIndex];
                                        newChannels[channelIndex] = arguments[i].UsedChannels[maskChannel];
                                    }

                                    arguments[i].SetChannels(newChannels);
                                }
                            }
                        }
                    }
                }
            }

            public bool Simplify()
            {
                if (Destination is CodeData)
                {
                    return Instruction.Simplify(Destination, Arguments);
                }

                return false; // Nothing to do
            }

            public virtual void Print(StringBuilder builder)
            {
                if (Destination.RequiresDeclaration(this))
                {
                    builder.AppendLine();
                    Destination.Declare(builder);
                }

                if (Instruction.FormatLine(Destination, Arguments, out string result))
                {
                    builder.Append(result);
                }
                else
                {
                    builder.Append(Destination.GetDecompiledName());

                    builder.Append(" = ");

                    builder.Append(Instruction.FormatCall(Destination, Arguments));
                }

                builder.Append(";");
            }

            public override string ToString()
            {
                StringBuilder stringBuilder = new StringBuilder();
                Print(stringBuilder);
                return stringBuilder.ToString();
            }
        }

        public abstract class CodeData
        {
            public struct DataModifiers
            {
                public bool isNegated;
                public bool isAbsolute;
                public bool isSaturated;
            }

            public DataModifiers modifiers;

            public abstract int ResourceHashCode { get; }

            public virtual bool RepresentsColor => false;

            public byte[] UsedChannels => usedChannels;

            public bool ExplicitChannelAccess { get; }

            private byte[] usedChannels;

            protected CodeData(byte[] readChannels, bool explicitChannelAccess, DataModifiers modifiers)
            {
                this.modifiers = modifiers;
                this.usedChannels = readChannels;
                this.ExplicitChannelAccess = explicitChannelAccess;
            }

            public void Shrink(int maxWidth)
            {
                byte[] newChannels = new byte[maxWidth];
                Array.Copy(UsedChannels, newChannels, newChannels.Length);

                SetChannels(newChannels);
            }

            public bool UsesChannel(byte channel)
            {
                for (int i = 0; i < usedChannels.Length; i++)
                {
                    if (usedChannels[i] == channel)
                    {
                        return true;
                    }
                }

                return false;
            }

            public virtual void SetChannels(byte[] newChannels)
            {
                if (newChannels.Length <= 0)
                {
                    System.Diagnostics.Debug.Assert(newChannels.Length > 0);
                }

                this.usedChannels = newChannels;
            }

            public void KillChannel(int channelIndex)
            {
                if (channelIndex < UsedChannels.Length)
                {
                    List<byte> newChannels = new List<byte>(UsedChannels);
                    newChannels.RemoveAt(channelIndex);

                    SetChannels(newChannels.ToArray());
                }
                else
                {
                    // Nothing to remove or to shift
                }
            }

            public string GetDecompiledName()
            {
                return GetDecompiledNameForChannels(UsedChannels);
            }

            public abstract byte GetDataWidth();

            public virtual bool RequiresDeclaration(Statement s) { return false; }

            public abstract string GetDecompiledNameForChannels(params byte[] channels);

            public virtual void NotifyUsedInStatement(Statement statement) { }

            public virtual void Declare(StringBuilder builder) { throw new InvalidOperationException(); }

            protected string Decorate(string name, DataModifiers modifiers)
            {
                if (modifiers.isAbsolute)
                {
                    name = $"abs({name})";
                }

                if (modifiers.isSaturated)
                {
                    name = $"saturate({name})";
                }

                if (modifiers.isNegated)
                {
                    name = $"(-{name})";
                }

                return name;
            }
        }

        public class ResourceData : CodeData
        {
            public override int ResourceHashCode => resource.GetHashCode();

            public override bool RepresentsColor => resource is Input input && input.MemberName.ToLower().Contains("color");

            public readonly ShaderResource resource;

            public readonly bool isVertexShader;

            public bool isInverted = false;

            public ResourceData(ShaderResource resource, bool isVertexShader, DataModifiers modifiers, bool swizzle, params byte[] readChannels) : base(readChannels, swizzle, modifiers)
            {
                this.resource = resource;
                this.isVertexShader = isVertexShader;
            }

            public ResourceData Duplicate()
            {
                return new ResourceData(resource, isVertexShader, modifiers, ExplicitChannelAccess, UsedChannels)
                {
                    isInverted = isInverted
                };
            }

            public override string GetDecompiledNameForChannels(params byte[] channels)
            {
                string name = resource.GetDecompiledName();

                if (resource is Sampler)
                {
                    // Name is good, do not change
                }
                else if (resource is HardcodedExternalConstant constant)
                {
                    name = constant.GetDecompiledNameForChannels(channels);
                }
                else
                {
                    bool needExplicitAccess = false;

                    if (resource is MatrixChannelAccessResource wide)
                    {
                        needExplicitAccess |= (int)wide.type != channels.Length;
                    }
                    else if (resource is MatrixResource mx)
                    {
                        //needExplicitAccess |= mx.rows * mx.columns != channels.Length;
                    }
                    else if (resource is ExternalResource external)
                    {
                        needExplicitAccess |= (int)external.type != channels.Length;
                    }
                    else if (resource is InputOutput io)
                    {
                        needExplicitAccess |= (int)io.Type != channels.Length;
                    }

                    if (!needExplicitAccess)
                    {
                        for (int i = 0; i < channels.Length; i++)
                        {
                            if (channels[i] != i)
                            {
                                needExplicitAccess = true;
                                break;
                            }
                        }
                    }

                    if (needExplicitAccess)
                    {
                        name += ".";

                        for (int i = 0; i < channels.Length; i++)
                        {
                            name += RepresentsColor ? channels[i].ColorChannel() : channels[i].VectorChannel();
                        }
                    }
                }

                name = Decorate(name, modifiers);


                if (isInverted)
                {
                    name = $"(1 / {name})";
                }

                return name;
            }

            public override byte GetDataWidth()
            {
                if (resource is ExternalResource external)
                {
                    return (byte)external.type;
                }
                else if (resource is InputOutput io)
                {
                    return (byte)io.Type;
                }
                else
                {
                    throw new Exception($"Cannot get theoretical width of {resource.GetType()}");
                }
            }
        }

        public class VariableData : CodeData
        {
            public override bool RepresentsColor => variable.representsColor;

            public override int ResourceHashCode => variable.GetHashCode();

            public readonly Variable variable;

            public readonly bool forWrite;

            public VariableData(Variable variable, DataModifiers modifiers, bool forWrite, bool swizzle, params byte[] readChannels) : base(readChannels, swizzle, modifiers)
            {
                this.variable = variable;
                this.forWrite = forWrite;

                SetChannels(readChannels);

                if (!forWrite && !variable.HaveChannelsBeenWritten(this.UsedChannels))
                {
                    throw new Exception($"Tried to reference data that was not written yet!");
                }
            }

            public override void SetChannels(byte[] newChannels)
            {
                base.SetChannels(newChannels);

                if (forWrite)
                {
                    for (int i = 0; i < newChannels.Length; i++)
                    {
                        variable.writtenChannels |= (byte)(1 << newChannels[i]);
                    }
                }
                else
                {
                    // Truncate channels because we can't read what we have never written!
                    int lastWrittenChannel = 0;
                    for (int i = 0; i < newChannels.Length; i++)
                    {
                        bool haveIt = variable.HasChannelBeenWritten(newChannels[i]);

                        if (haveIt)
                        {
                            lastWrittenChannel = i;
                        }
                    }

                    if (newChannels.Length - 1 > lastWrittenChannel)
                    {
                        // This is something the compiler does - it lets undefined behaviour exist on members that aren't used
                        // We need to account for it by shrinking the whole statement if one of the reads is invalid
                        // It WILL happen in the middle of a variable so we need to read the entire thing to make sure
                        byte[] truncatedChannels = new byte[lastWrittenChannel + 1];
                        Array.Copy(newChannels, truncatedChannels, truncatedChannels.Length);
                        SetChannels(truncatedChannels);
                    }
                }
            }

            public override bool RequiresDeclaration(Statement s)
            {
                return variable.firstStatementUsed == s;
            }

            public override string GetDecompiledNameForChannels(params byte[] channels)
            {
                string name = variable.decompiledName;

                System.Diagnostics.Debug.Assert(channels.Length > 0);

                if (!variable.HaveChannelsBeenWritten(channels))
                {
                    //throw new Exception($"Read more channels than the variable was written to?");
                }

                bool needsExplicit = 
                    channels.Length != GetWidth(variable.writtenChannels);

                if (!needsExplicit) {
                    for (int i = 0; i < channels.Length; i++)
                    {
                        if (channels[i] != i)
                        {
                            needsExplicit = true;
                            break;
                        }
                    }
                }

                if (needsExplicit)
                {
                    name += ".";

                    for (int i = 0; i < channels.Length; i++)
                    {
                        name += RepresentsColor ? channels[i].ColorChannel() : channels[i].VectorChannel();
                    }
                }

                name = Decorate(name, modifiers);

                return name;
            }

            public void ResetUsage()
            {
                variable.firstStatementUsed = null;
            }

            public override void NotifyUsedInStatement(Statement statement)
            {
                base.NotifyUsedInStatement(statement);

                if (variable.firstStatementUsed == null && !(statement is Comment))
                {
                    variable.firstStatementUsed = statement;
                }

                variable.lastStatementUsed = statement;
            }

            public override void Declare(StringBuilder builder)
            {
                // Inline declaration or full depending on initial write
                bool initialWriteIsCompleteAndInOrder = false;
                if (variable.firstStatementUsed.Destination.UsedChannels.Length == GetWidth(variable.writtenChannels)) {
                    bool inOrder = true;
                    for (int i = 0; i < variable.firstStatementUsed.Destination.UsedChannels.Length; i++)
                    {
                        if (variable.firstStatementUsed.Destination.UsedChannels[i] != i)
                        {
                            inOrder = false;
                            break;
                        }
                    }

                    initialWriteIsCompleteAndInOrder = inOrder;
                }

                if (initialWriteIsCompleteAndInOrder)
                {
                    builder.Append(((Type)GetWidth(variable.writtenChannels)).Declare(variable.isHalfPrecision) + " ");
                }
                else
                {
                    builder.AppendLine($"{(variable is Constant ? "const " : "")}{((Type)GetWidth(variable.writtenChannels)).Declare(variable.isHalfPrecision)} {variable.decompiledName};");
                }
            }

            public override byte GetDataWidth()
            {
                return GetWidth(variable.writtenChannels);
            }
        }

        private class Constant : Variable
        {
            public Constant(int id) : base(id)
            {
            }
        }

        public class Variable
        {
            public int FirstLineUsed => firstStatementUsed.lines[0];
            public int LastLineUsed => lastStatementUsed.lines[lastStatementUsed.lines.Length - 1];

            public bool representsColor = false;

            public bool isHalfPrecision = false;
            public Statement firstStatementUsed;
            public Statement lastStatementUsed;

            public byte writtenChannels;

            public string decompiledName;

            public Variable(int id)
            {
                decompiledName = "var_";

                int letters = 1 + id / 26;

                for (int i = 0; i < letters; i++)
                {
                    char letter = Convert.ToChar(65 + (id % 26));
                    id -= 26;

                    decompiledName += letter;
                }
            }

            public bool HaveChannelsBeenWritten(byte[] channels)
            {
                for (int i = 0; i < channels.Length; i++)
                {
                    if (!HasChannelBeenWritten(channels[i]))
                    {
                        return false;
                    }
                }

                return true;
            }


            public bool HasChannelBeenWritten(int channelIndex)
            {
                byte mask = (byte)(1 << channelIndex);
                if ((writtenChannels & mask) == 0)
                {
                    return false;
                }

                return true;
            }

            public override string ToString()
            {
                return decompiledName;
            }
        }

        private class ShaderState
        {
            public readonly Dictionary<string, ShaderResource> resources;

            private readonly Variable[] registers = new Variable[16];

            private readonly bool isVertexShader;

            private readonly OptimizationParameters parameters;

            private int variableId = 0;

            public ShaderState(bool isVertexShader, OptimizationParameters parameters, Dictionary<string, ShaderResource> resources)
            {
                this.parameters = parameters;
                this.isVertexShader = isVertexShader;
                this.resources = resources;

                if (!isVertexShader)
                {
                    resources.Add("oC0", new ColorOutput());
                }
            }

            public CodeData GetOrigin(byte[] destinationChannels, bool destinationHasExplicitSwizzle, int lineIndex, string name)
            {
                name = GetChannels(name, isDestination: false, out byte[] channels, out bool explicitSwizzle);

                System.Diagnostics.Debug.Assert(destinationChannels.Length <= channels.Length);

                CodeData.DataModifiers modifiers = new CodeData.DataModifiers();

                name = GetNegation(name, out modifiers.isNegated);
                name = GetAbs(name, out modifiers.isAbsolute);
                name = GetSat(name, out modifiers.isSaturated);

                bool isNormalRegister = name[0] == 'r';

                CodeData origin;

                if (isNormalRegister)
                {
                    byte registerIndex;

                    try
                    {
                        registerIndex = Convert.ToByte(name.Substring(1));
                    }
                    catch (FormatException e)
                    {
                        throw e;
                    }

                    if (registerIndex > registers.Length)
                    {
                        throw new Exception($"Programming error: register {registerIndex} unknown");
                    }

                    if (registers[registerIndex] == null)
                    {
                        throw new Exception($"Tried to read from a non-initialized register {registerIndex}!");
                    }

                    origin = new VariableData(registers[registerIndex], modifiers, forWrite: false, explicitSwizzle, channels);
                }
                else
                {
                    if (resources.TryGetValue(name, out ShaderResource resource))
                    {
                        origin = new ResourceData(resource, isVertexShader, modifiers, explicitSwizzle, channels);
                    }
                    else
                    {
                        throw new Exception($"Unknown input {name}");
                    }
                }


                return origin;
            }

            public CodeData CreateDestination(
                int lineIndex, 
                string name, 
                byte[] channels,
                bool explicitSwizzle, 
                CodeData.DataModifiers modifiers)
            {
                bool isOutputRegister = name[0] == 'o';
                bool isNormalRegister = name[0] == 'r';

                if (!isNormalRegister && !isOutputRegister)
                {
                    // The rest is not writeable...
                    throw new Exception($"Unknown destination {name}");
                }

                CodeData destination;
                if (isOutputRegister)
                {
                    if (resources.TryGetValue(name, out ShaderResource resource))
                    {
                        destination = new ResourceData(resource, isVertexShader, modifiers, explicitSwizzle, channels);
                    }
                    else
                    {
                        throw new Exception($"Undeclared output {name}");
                    }
                }
                else if (isNormalRegister)
                {
                    byte registerIndex = Convert.ToByte(name.Substring(1));

                    if (registerIndex > registers.Length)
                    {
                        throw new Exception($"Programming error: register {registerIndex} unknown");
                    }

                    bool shouldOverwrite = registers[registerIndex] == null;


                    if (!shouldOverwrite)
                    {
                        if (parameters.alwaysCreateNewVariables)
                        {
                            shouldOverwrite = true;
                        }
                        else
                        {
                            // If the channels we're about to write is all that's ever been written to the variable
                            // then it's a new variable
                            byte writtenChannels = registers[registerIndex].writtenChannels;
                            for (int channelIndex = 0; channelIndex < channels.Length; channelIndex++)
                            {
                                byte channel = (byte)(1 << channels[channelIndex]);

                                if ((writtenChannels & channel) != 0)
                                {
                                    writtenChannels = (byte)(writtenChannels & ~channel);
                                }
                            }

                            if (writtenChannels == 0)
                            {
                                shouldOverwrite = true;
                            }
                        }
                    }

                    if (shouldOverwrite)
                    {
                        registers[registerIndex] = new Variable(variableId++);
                    }

                    Variable variable = registers[registerIndex];

                    destination = new VariableData(variable, modifiers, forWrite: true, explicitSwizzle, channels);
                }
                else
                {
                    throw new Exception($"unreachable");
                }

                return destination;
            }

            public void GetDestinationInfo(int lineIndex, 
                string parsedName,
                out string name,
                out byte[] channels,
                out bool explicitSwizzle,
                out CodeData.DataModifiers modifiers)
            {
                name = GetChannels(parsedName, isDestination: true, out channels, out explicitSwizzle);

                modifiers = new CodeData.DataModifiers();

                name = GetNegation(name, out modifiers.isNegated);
                name = GetAbs(name, out modifiers.isAbsolute);
                name = GetSat(name, out modifiers.isSaturated);
            }

            private string GetSat(string name, out bool isSaturated)
            {
                isSaturated = name.Contains("_sat");
                if (isSaturated)
                {
                    name = name.Replace("_sat", "");
                }

                return name;
            }

            private string GetAbs(string name, out bool isAbsoluteValued)
            {
                isAbsoluteValued = name.Contains("_abs");
                if (isAbsoluteValued)
                {
                    name = name.Replace("_abs", "");
                }

                return name;
            }


            private string GetNegation(string name, out bool isNegated)
            {
                isNegated = name[0] == '-';
                if (isNegated)
                {
                    name = name.Substring(1);
                }

                return name;
            }

            private string GetChannels(string name, bool isDestination, out byte[] channels, out bool explicitSwizzle)
            {
                channels = new byte[] { 0, 1, 2, 3 };
                byte width = (byte)channels.Length;
                explicitSwizzle = false;

                if (name.Contains('.'))
                {
                    explicitSwizzle = true;
                    int channelsStart = name.IndexOf('.') + 1;

                    width = (byte)(name.Length - channelsStart);

                    if (isDestination)
                    {
                        // We truncate to exactly the right size
                        channels = new byte[width];
                    }

                    for (int i = 0; i < channels.Length; i++)
                    {
                        int repeatingIndex = Math.Min(width - 1, i);
                        char chan = name.Substring(channelsStart + repeatingIndex, 1)[0];
                        channels[i] = chan.ByteChannel(); // XY => XYYY (4 channel wide)
                    }

                    // Remove it from the name
                    name = name.Substring(0, channelsStart - 1);
                }

                return name;
            }
        }

        public string Main => MainName;

        public bool Incomplete => incomplete;

        public DependencyGraph DependencyGraph => dependencyGraph;

        private string InputName => "VSInput";
        private string OutputName => "VSOutput";


        private string MainInput => isVertexShader ? InputName : OutputName;

        private string MainOutput => isVertexShader ? OutputName : "half4";

        private string MainName => isVertexShader ? "VSMain" : "PSMain";

        private readonly List<Statement> statements = new List<Statement>();

        private readonly bool isVertexShader = false;

        private readonly OptimizationParameters parameters;

        private DependencyGraph dependencyGraph = new DependencyGraph();

        private bool incomplete;



        public ShaderProgramObject(bool vertexShader, in OptimizationParameters parameters)
        {
            isVertexShader = vertexShader;
            this.parameters = parameters;
        }

        public static byte GetWidth(byte channels)
        {
            if ((channels & A) > 0)
            {
                return 4;
            }

            if ((channels & B) > 0)
            {
                return 3;
            }

            if ((channels & G) > 0)
            {
                return 2;
            }

            if ((channels & R) > 0)
            {
                return 1;
            }

            throw new Exception("Zero width channel");
        }

        public void Export(out DecompiledBlock[] decompiledLines)
        {
            List<DecompiledBlock> lines = new List<DecompiledBlock>();

            PrintHeader(lines);

            PrintDeclarations(lines);

            PrintInstructions(lines);

            decompiledLines = lines.ToArray();
        }

        public void PrintHeader(in List<DecompiledBlock> lines)
        {
        }

        public void PrintInstructions(in List<DecompiledBlock> lines)
        {
            {
                StringBuilder mainBegin = new StringBuilder();

                mainBegin.AppendLine($"{MainOutput} {MainName}({MainInput} {(isVertexShader ? VX_IN_NAME : PX_IN_NAME)}){(isVertexShader ? string.Empty : $" : SV_Target")}");
                mainBegin.BeginBlock();

                lines.Add(new DecompiledBlock(mainBegin.ToString()));
            }

            {
                DecompiledBlock outputDeclaration;
                if (isVertexShader)
                {
                    outputDeclaration = new DecompiledBlock($"{MainOutput} {VX_OUT_NAME} = ({MainOutput})0;", indent: 1);
                }
                else
                {
                    outputDeclaration = new DecompiledBlock($"{MainOutput} {PX_OUT_NAME};", indent: 1);
                }

                lines.Add(outputDeclaration);
            }

            {
                bool writingOutput = false;
                for (int i = 0; i < statements.Count; i++)
                {
                    Statement statement = statements[i];

                    bool isOutput = statement.Destination is ResourceData rsc && rsc.resource is Output;

                    if (writingOutput != isOutput)
                    {
                        // Add a line before writing an output because it looks nicer
                        lines.Add(default);
                        writingOutput = isOutput;
                    }

                    Guid[] requirements = dependencyGraph.GetRequirementsForStatement(statement.Guid);

                    lines.Add(new DecompiledBlock(statement, requirements));
                }
            }

            lines.Add(default);

            lines.Add(new DecompiledBlock($"return {(isVertexShader ? VX_OUT_NAME : PX_OUT_NAME)};", indent: 1));

            {
                StringBuilder mainEnd = new StringBuilder();

                mainEnd.EndBlock();

                lines.Add(new DecompiledBlock(mainEnd.ToString()));
            }

            lines.Add(default);
        }

        public void PrintDeclarations(in List<DecompiledBlock> lines)
        {
            var orderedResources =
                GetAllResources()
                .OrderBy(o => o is ExternalResource)
                .ThenByDescending(o => o is HardcodedExternalConstant con && con.ContainsUniversalConstant)
                .ThenByDescending(o => o is Sampler)
                .ThenByDescending(o => o is Input)
                .ThenByDescending(o => o is Output)
                .ThenByDescending(o => o is HardcodedExternalConstant)
                .ThenByDescending(o=>(o as ExternalResource)?.preferredRegister ?? null);

            // Includes / game parameters
            foreach (var input in orderedResources)
            {
                if (input is ExternalResource)
                {
                    input.Declare(lines);
                }
            }

            lines.Add(default);

            // Input
            {
                StringBuilder builder = new StringBuilder();
                builder.BeginStruct(MainInput);
                lines.Add(new DecompiledBlock(builder.ToString()));
            }

            IOrderedEnumerable<Input> inputs = orderedResources
                .Where(o=>o is Input)
                .Select(o=>(Input)o)
                .OrderBy(o => o.DeclaredAtLine);

            foreach (var input in inputs)
            {
                if (input is Input)
                {
                    input.Declare(lines);
                }
            }

            {
                StringBuilder builder = new StringBuilder();
                builder.EndStruct();
                lines.Add(new DecompiledBlock(builder.ToString()));
            }

            lines.Add(default);

            // Output
            if (isVertexShader)
            {
                {
                    StringBuilder builder = new StringBuilder();
                    builder.BeginStruct(OutputName);
                    lines.Add(new DecompiledBlock(builder.ToString()));
                }

                IOrderedEnumerable<Output> outputs = orderedResources
                    .Where(o => o is Output)
                    .Select(o => (Output)o)
                    .OrderBy(o => o.DeclaredAtLine);

                foreach (var input in outputs)
                {
                    if (input is Output)
                    {
                        input.Declare(lines);
                    }
                }

                {
                    StringBuilder builder = new StringBuilder();
                    builder.EndStruct();
                    lines.Add(new DecompiledBlock(builder.ToString()));
                }

                lines.Add(default);
            }
        }

        public ICollection<ShaderResource> GetAllResources()
        {
            HashSet<ShaderResource> resources = new HashSet<ShaderResource>();

            for (int i = 0; i < statements.Count; i++)
            {
                Statement statement = statements[i];

                if (statement is Comment)
                {
                    continue;
                }

                List<CodeData> data = new List<CodeData>(statement.Arguments.Length + 1);
                data.Add(statement.Destination);

                for (int argumentIndex = 0; argumentIndex < statement.Arguments.Length; argumentIndex++)
                {
                    data.Add(statement.Arguments[argumentIndex]);
                }

                for (int j = 0; j < data.Count; j++)
                {
                    if (data[j] is ResourceData resourceData)
                    {
                        if (resourceData.resource is MatrixChannelAccessResource wide)
                        {
                            resources.Add(wide.matrix);
                        }
                        else
                        {
                            resources.Add(resourceData.resource);
                        }
                    }
                }
            }

            return resources;
        }

        public void Parse(string header, string body)
        {
            // Parse header
            Dictionary<string, ShaderResource> resources = new Dictionary<string, ShaderResource>();
            var matches = Regex.Matches(header, @" *([0-z]+) +([^ ]+) +([0-9])");
            foreach (Match match in matches)
            {
                var name = match.Groups[1].Value;
                var register = match.Groups[2].Value;
                var size = Convert.ToByte(match.Groups[3].Value);

                if (register.StartsWith("s"))
                {
                    resources.Add(register, new Sampler(name, register));
                }
                else if (register.StartsWith("c"))
                {
                    if (size == 1)
                    {
                        ExternalResource external = new ExternalResource(name, register);
                        resources.Add(register, external);
                    }
                    else if (size > 1)
                    {
                        MatrixResource external = new MatrixResource(name, register, size, 4);
                        for (int i = 0; i < size; i++)
                        {
                            int index = Convert.ToInt32(register.Substring(1));
                            resources.Add($"c{index + i}", new MatrixChannelAccessResource(external, i));
                        }
                    }
                    else
                    {
                        throw new Exception($"Unexpected constant size {size}");
                    }
                }
            }

            // Parse assembly body
            string[] lines = body.Split('\n');
            ShaderState state = new ShaderState(isVertexShader, parameters, resources);

            for (int i = 0; i < lines.Length; i++)
            {
                try
                {
                    Step(state, i, lines[i].Trim());
                }
                catch(Exception e)
                {
                    throw new Exception($"At line {lines[i]} : {e}", e);
                }
            }

            if (incomplete)
            {
                // No optimization
            }
            else
            {
                Optimize();
            }
        }

        private void ParseConstants(ShaderState state, int lineIndex, string line)
        {
            char chr = Convert.ToChar(65 + lineIndex);

            string[] elements = line.Split(',');

            if (elements.Length < 5)
            {
                throw new Exception($"Invalid length {elements.Length}");
            }

            string registerName = elements[0].Trim();

            string[] valueStrings = new string[4];
            Array.Copy(elements, 1, valueStrings, 0, valueStrings.Length);

            float[] valueFloats = new float[valueStrings.Length];
            for (int i = 0; i < valueStrings.Length; i++)
            {
                valueFloats[i] = Convert.ToSingle(valueStrings[i].Trim(), CultureInfo.InvariantCulture);
            }

            state.resources.Add(registerName, new HardcodedExternalConstant($"k{chr}", valueFloats));
        }

        private void ParseDeclaration(ShaderState state, int lineIndex, string line)
        {
            string[] elements = line.Split(' ');

            if (elements.Length < 2)
            {
                throw new Exception($"Invalid length {elements.Length}");
            }


            string registerName = elements[1].Trim();
            string resourceName = elements[0].Trim();

            // Identify type
            Type type = Type.float4;
            if (registerName.Contains("."))
            {
                if (registerName.Contains('z'))
                {
                    type = Type.float3;
                }
                else if (registerName.Contains('y'))
                {
                    type = Type.float2;
                }
                else
                {
                    type = Type.@float;
                }

                registerName = registerName.Substring(0, registerName.IndexOf('.'));
            }


            InputOutput inputOutput;

            if (registerName.StartsWith("v"))
            {
                // It's an input
                inputOutput = new Input(isVertexShader, lineIndex, resourceName, type);
            }
            else if (registerName.StartsWith("o"))
            {
                // It's an output
                inputOutput = new Output(isVertexShader, lineIndex, resourceName, type);
            }
            else if (registerName.StartsWith("s"))
            {
                // Sampler input - already declared in the header
                if (state.resources.TryGetValue(registerName, out ShaderResource resource) && resource is Sampler sampler)
                {
                    switch (resourceName)
                    {
                        default:
                            sampler.samplerType = Sampler.SamplerType.sampler2D;
                            break;

                        case "cube":
                            sampler.samplerType = Sampler.SamplerType.samplerCUBE;
                            break;

                        case "volume":
                            sampler.samplerType = Sampler.SamplerType.sampler3D;
                            break;
                        
                    }
                }

                // No need to add it
                return;
            }
            else
            {
                throw new Exception($"Unknown io resource type {registerName}");
            }

            state.resources.Add(registerName, inputOutput);
        }

        private void Step(ShaderState state, int index, string line)
        {
            if (line == string.Empty || line.Trim().StartsWith("//"))
            {
                return; // skip it
            }

            string statement = line.Split(' ')[0];

            if (statement.StartsWith("def"))
            {
                ParseConstants(state, index, line.Substring(3).Trim());
                return;
            }

            if (statement.StartsWith("dcl_"))
            {
                ParseDeclaration(state, index, line.Substring(4).Trim());
                return;
            }

            InstructionModifiers modifiers = default;

            string instruction = statement;

            if (instruction.EndsWith("_pp"))
            {
                // For now ignore half precision
                instruction = instruction.Substring(0, instruction.Length - 3);
                modifiers.isHalfPrecision = true;
            }

            if (instruction.EndsWith("_sat"))
            {
                instruction = instruction.Substring(0, instruction.Length - 4);
                modifiers.isSaturated = true;
            }

            if (instruction.EndsWith("_abs"))
            {
                instruction = instruction.Substring(0, instruction.Length - 4);
                modifiers.isAbsolute = true;
            }

            ParseStatement(state, index, instruction, line.Substring(statement.Length).Trim(), modifiers);
        }

        private void Optimize()
        {
            ShaderOptimizer optimizer = new ShaderOptimizer(parameters, statements);


            statements.Clear();
            statements.AddRange(optimizer.OptimizedStatements);

            optimizer.ExportDependencyGraph(out dependencyGraph);


            // ?) Simplify operations (zero add, uniform vector multiply)

            // ?) Invert very low values (<0.01) and operations associated

            // ?) Truncate width of statements whenever possible (lowest common denominator based on destination)

            // ?) Pattern recognition to eliminate statements?

        }

        private void ParseStatement(ShaderState state, int index, string instructionStr, string lineContents, InstructionModifiers modifiers)
        {
            Instruction instruction = ParseInstruction(instructionStr, modifiers);

            string[] elements = lineContents.Split(new string[] { ", " }, StringSplitOptions.None);
            string writtenData = elements[0];
            string[] readData = new string[elements.Length - 1];
            Array.Copy(elements, 1, readData, 0, readData.Length);

            if (elements.Length < 2)
            {
                throw new Exception($"Invalid instruction {lineContents}");
            }

            state.GetDestinationInfo(
                index, 
                writtenData, 
                out string destinationName,
                out byte[] destChannels, 
                out bool destSwizzle, 
                out CodeData.DataModifiers destModifiers
            );

            CodeData[] origins = new CodeData[readData.Length];
            for (int i = 0; i < readData.Length; i++)
            {
                origins[i] = state.GetOrigin(destChannels, destSwizzle, index, readData[i]);
            }

            CodeData destination = state.CreateDestination(index, destinationName, destChannels, destSwizzle, destModifiers);

            Statement statement;

            

            if (instruction == null)
            {
                statement = new Comment(index, $"L{index}: {instructionStr} {lineContents}");
            }
            else
            {
                statement = new Statement(index, instruction, destination, origins);
            }

            statements.Add(statement);
        }

        private Instruction ParseInstruction(string instructionStr, InstructionModifiers modifiers)
        {
            Instruction instruction = null;

            switch (instructionStr)
            {
                case "dp2add":
                case "dp3":
                case "dp4":
                    {
                        byte width = Convert.ToByte(instructionStr.Substring(2, 1));
                        instruction = new Instructions.DotProduct(width, modifiers);
                        break;
                    }

                case "mad":
                    {
                        instruction = new Instructions.MultiplyAdd(modifiers);
                        break;
                    }


                case "mul":
                    {
                        instruction = new Instructions.Multiply(modifiers);
                        break;
                    }

                case "mov":
                    {
                        instruction = new Instructions.Move(modifiers);
                        break;
                    }

                case "rcp":
                    {
                        instruction = new Instructions.Reciprocal(modifiers);
                        break;
                    }

                case "rsq":
                    {
                        instruction = new Instructions.ReciprocalSquareRoot(modifiers);
                        break;
                    }

                case "exp":
                    {
                        instruction = new Instructions.Exponential(modifiers);
                        break;
                    }

                case "max":
                    {
                        instruction = new Instructions.Max(modifiers);
                        break;
                    }

                case "min":
                    {
                        instruction = new Instructions.Min(modifiers);
                        break;
                    }

                case "add":
                    {
                        instruction = new Instructions.Add(modifiers);
                        break;
                    }

                case "frc":
                    {
                        instruction = new Instructions.Frac(modifiers);
                        break;
                    }

                case "sincos":
                    {
                        instruction = new Instructions.SinCos(modifiers);
                        break;
                    }

                case "texld":
                    {
                        instruction = new Instructions.SampleTexture(modifiers);
                        break;
                    }

                case "texldp":
                    {
                        instruction = new Instructions.SampleTextureProjected(modifiers);
                        break;
                    }

                case "texldb":
                    {
                        instruction = new Instructions.SampleTextureBias(modifiers);
                        break;
                    }

                case "texldl":
                    {
                        instruction = new Instructions.SampleTextureLod(modifiers);
                        break;
                    }

                case "lrp":
                    {
                        instruction = new Instructions.Lerp(modifiers);
                        break;
                    }

                case "log":
                    {
                        instruction = new Instructions.Log(modifiers);
                        break;
                    }

                case "nrm":
                    {
                        instruction = new Instructions.Normalize(modifiers);
                        break;
                    }

                case "dsx":
                case "dsy":
                    {
                        instruction = new Instructions.DirectionDifference(instructionStr.EndsWith("x"), modifiers);
                        break;
                    }

                case "sge":
                    {
                        instruction = new Instructions.Sge(modifiers);
                        break;
                    }

                case "slt":
                    {
                        instruction = new Instructions.Slt(modifiers);
                        break;
                    }

                case "abs":
                    {
                        instruction = new Instructions.Abs(modifiers);
                        break;
                    }

                case "pow":
                    {
                        instruction = new Instructions.Pow(modifiers);
                        break;
                    }

                case "cmp":
                    {
                        instruction = new Instructions.Compare(modifiers);
                        break;
                    }


                default:
                    // Just do nothing for now
                    incomplete |= true;
                    break;
            }

            return instruction;
        }
    }
}
