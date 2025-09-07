extern sampler3D modelLightingSampler : register(s4);
extern sampler2D colorMapSampler : register(s0);
extern float4 lightingLookupScale : register(c3);
extern float4 fogColorLinear : register(c0);

struct VSOutput
{

    float4 color : COLOR0;
    float2 texcoord : TEXCOORD0;
    float4 texcoord1 : TEXCOORD1;
    float3 texcoord6 : TEXCOORD6;
};


half4 PSMain(VSOutput inputVx) : SV_Target
{

    half4 outColor;
	
    half3 var_A = normalize(inputVx.texcoord1.xyz);
	
    half var_B = max(abs(var_A.y), abs(var_A.z));
	
    half var_C = max(abs(var_A.x), var_B);
	
    float4 var_D;
    var_D.xyz = var_A * lightingLookupScale.xyz;
    var_D.w = 1 / var_C;
    var_D.xyz = var_D.xyz * var_D.w + inputVx.texcoord6;
	
    half4 var_modellighting = tex3D(modelLightingSampler, var_D.xyz);
    var_modellighting.rgb = var_modellighting.rgb + var_modellighting.rgb;
    var_modellighting.rgb = var_modellighting.rgb * var_modellighting.rgb;
	
    half4 var_colormap = tex2D(colorMapSampler, inputVx.texcoord);
	
    half4 var_G = var_colormap * inputVx.color;
    var_modellighting.a = var_colormap.a * (-inputVx.color.a) + 1;
    var_colormap.rgb = var_G.rgb * var_G.rgb;
    var_colormap.rgb = var_G.aaa * var_colormap.rgb;
    var_colormap.a = (-var_modellighting.a) + 1;
    var_G.rgb = var_colormap.aaa * fogColorLinear.xyz;
    var_modellighting.rgb = var_colormap.rgb * var_modellighting.rgb + (-var_G.rgb);
    var_modellighting.rgb = inputVx.texcoord1.w * var_modellighting.rgb + var_G.rgb;
    var_modellighting.a = var_colormap.a;

    outColor = ((-abs(var_colormap.aaaa)) >= 0 ? var_colormap : var_modellighting);

    return outColor;
}


