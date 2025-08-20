extern sampler3D modelLightingSampler : register(s4);
extern sampler2D colorMapSampler : register(s0);
extern float4 lightingLookupScale : register(c3);
extern float4 lightDiffuse : register(c18);
extern float4 lightPosition : register(c17);
extern float4 fogColorLinear : register(c0);

struct VSOutput
{

	float3 color : COLOR0;
	float2 texcoord : TEXCOORD0;
	float4 texcoord1 : TEXCOORD1;
	float3 texcoord6 : TEXCOORD6;
};


half4 PSMain(VSOutput inputVx) : SV_Target
{

	half4 outColor;
	
half4 var_colormap = tex2D(colorMapSampler, inputVx.texcoord);
	var_colormap.rgb *= inputVx.color;
	var_colormap.rgb = var_colormap.rgb * var_colormap.rgb;
	
float4 var_outr;
var_outr.rgb = normalize(inputVx.texcoord1.xyz);
	
half3 var_D = var_outr.rgb * lightingLookupScale.xyz;
	var_outr.r = saturate(dot(lightPosition.xyz, var_outr.rgb));
	
half var_B = max(abs(var_outr.g), abs(var_outr.b));
	
half var_C = max(abs(var_outr.r), var_B);
	var_outr.a = 1 / var_C;
	var_outr.gba = var_D.xxy * var_outr.a + inputVx.texcoord6.xxy;
	
half4 var_modellighting = tex3D(modelLightingSampler, var_outr.gba);
	var_outr.gba = var_modellighting.xxy + var_modellighting.xxy;
	var_modellighting.xyz = var_outr.r * lightDiffuse.xyz;
	var_outr.gba = var_outr.rgb * var_outr.rgb;
	var_outr.rgb = var_modellighting.w * var_modellighting.xyz + var_outr.gba;
	var_outr.rgb = var_colormap.rgb * var_outr.rgb + fogColorLinear.xyz;

	outColor.xyz = inputVx.texcoord1.w * var_outr.rgb + fogColorLinear.xyz;
	outColor.w = 1;

	return outColor;
}


