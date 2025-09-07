extern sampler2D colorMapSampler : register(s0);

extern float4x4 worldMatrix : register(c4);
extern float4x4 viewProjectionMatrix : register(c0);


struct VSOutput
{

    float4 position : POSITION;
    float4 color : COLOR0;
    float2 texcoord : TEXCOORD0;
};



half4 PSMain(VSOutput inputVx) : SV_Target
{

    half4 outColor;
	
    half4 var_colormap = tex2D(colorMapSampler, inputVx.texcoord);

    outColor = var_colormap * inputVx.color;

    return outColor;
}

struct VSInput
{

    float4 position : POSITION;
    float4 color : COLOR0;
    float4 texcoord : TEXCOORD0;
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
	
    float4 var_E = float4(vin.position.x, vin.position.y, vin.position.z, 1);
    var_texdA.xy = exp(var_texd.xy);

    vout.texcoord = var_texd.zw * var_texdA.xy;

    var_texdA = mul(var_E, worldMatrix);

    vout.position = mul(var_texdA, viewProjectionMatrix);
    vout.color = vin.color;

    return vout;
}


