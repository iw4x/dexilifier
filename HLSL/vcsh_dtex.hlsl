#define LOG2E 1.442695
extern float4x4 worldMatrix : register(c4);
extern float4 fogConsts : register(c21);
extern float4x4 viewProjectionMatrix : register(c0);

extern sampler2D colorMapSampler : register(s0);
extern float4 materialColor : register(c1);
extern float4 fogColorLinear : register(c0);

struct VSInput
{
    float4 position : POSITION;
    float4 color : COLOR0;
    float4 texcoord : TEXCOORD0;
};


struct VSOutput
{

    float4 position : POSITION;
    float4 color : COLOR0;
    float3 texcoord : TEXCOORD0;
};


VSOutput VSMain(VSInput vin)
{

    VSOutput vout = (VSOutput) 0;
	
    float4 var_A = (1 / float4(1024, 1024, 32768, 32768)) * vin.texcoord.zxzx;
	
    float4 var_B = vin.texcoord.wywy * float4(0.25, 0.25, 0.0078125, 0.0078125) + var_A;
	
    float4 var_texdA = frac(var_B);
    var_B.xy = var_texdA.xy * (-0.03125) + var_texdA.zw;
    var_B.zw -= var_texdA.zw;
	
    float4 var_texd = var_B * float4(32, 32, -2, -2) + float4(-15, -15, 1, 1);
    var_texd.zw = var_texd.zw * var_texdA.xy + var_texd.zw;
	
    float4 var_texdB = float4(vin.position.x, vin.position.y, vin.position.z, 1);
    var_texdA.xy = exp(var_texd.xy);

    vout.texcoord.xy = var_texd.zw * var_texdA.xy;

    var_texdA = mul(var_texdB, worldMatrix);

    vout.position = mul(var_texdA, viewProjectionMatrix);

    var_texdB.x = length(var_texdA.xyz);
    var_texdB.x = var_texdB.x * fogConsts.z + fogConsts.w;
    var_texdB.x = var_texdB.x * LOG2E;
    var_texdB.x = exp(var_texdB.x);
    var_texdB.x = max(var_texdB.x, fogConsts.y);

    vout.texcoord.z = min(var_texdB.x, 1);
    vout.color = vin.color;

    return vout;
}

half4 PSMain(VSOutput inputVx) : SV_Target
{

    half4 outColor;
	
    half4 var_colormap = tex2D(colorMapSampler, inputVx.texcoord.xy);
	
    half4 var_outr = var_colormap * inputVx.color;
	
    half3 var_C = var_outr.rgb * var_outr.rgb;
    var_outr.rgb = var_outr.rgb * (-var_outr.rgb) + materialColor.xyz;
    var_outr.rgb = materialColor.w * var_outr.rgb + var_C;
    var_outr.rgb -= fogColorLinear.xyz;

    outColor.xyz = inputVx.texcoord.z * var_outr.rgb + fogColorLinear.xyz;
    outColor.w = var_outr.a;

    return outColor;
}



