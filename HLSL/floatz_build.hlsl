//#define ATEST 1


extern float4x4 worldMatrix : register(c4);
extern float4 depthFromClip : register(c23);
extern float4x4 viewProjectionMatrix : register(c0);

extern sampler2D colorMapSampler : register(s0);

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
	
#if ATEST // dtex
    float4 var_A = (1 / float4(1024, 1024, 32768, 32768)) * vin.texcoord.zxzx;
	
    float4 var_B = vin.texcoord.wywy * float4(0.25, 0.25, 0.0078125, 0.0078125) + var_A;
	
    float4 var_texdA = frac(var_B);
    var_B.xy = var_texdA.xy * (-0.03125) + var_texdA.zw;
    var_B.zw -= var_texdA.zw;
	
    float4 var_texd = var_B * float4(32, 32, -2, -2) + float4(-15, -15, 1, 1);
    var_texd.zw = var_texd.zw * var_texdA.xy + var_texd.zw;
    var_texdA.xy = exp(var_texd.xy);

    vout.texcoord.xy = var_texd.zw * var_texdA.xy;
#else
    vout.texcoord.xy = vin.texcoord.xy;
#endif
    
    float4 var_posn = float4(vin.position.x, vin.position.y, vin.position.z, 1);

    float4 varMxIntermediate = mul(var_posn, worldMatrix);
    var_posn = mul(varMxIntermediate, viewProjectionMatrix);

    vout.position = var_posn;
    vout.texcoord.z = dot(var_posn, depthFromClip);
    vout.color = vin.color;

    return vout;
}




half PSMain(VSOutput inputVx) : SV_Target
{
    half4 var_A = tex2D(colorMapSampler, inputVx.texcoord.xy);
    float value = inputVx.color.a * var_A.a - 0.501960814f;
    
    clip(value);
    
    return inputVx.texcoord.x;
}


