extern sampler2D attenuationSampler : register(s6);
extern sampler2D shadowmapSamplerSpot : register(s5);
extern sampler3D modelLightingSampler : register(s4);
extern sampler2D colorMapSampler : register(s0);
extern float4 spotShadowmapPixelAdjust : register(c7);
extern float4 lightSpotFactors : register(c6);
extern float4 lightSpotDir : register(c5);
extern float4 lightingLookupScale : register(c3);
extern float4 lightDiffuse : register(c18);
extern float4 lightPosition : register(c17);
extern float4 fogColorLinear : register(c0);

struct VSOutput
{

	float3 color : COLOR0;
	float2 texcoord : TEXCOORD0;
	float4 texcoord1 : TEXCOORD1;
	float4 texcoord4 : TEXCOORD4;
	float3 texcoord5 : TEXCOORD5;
	float3 texcoord6 : TEXCOORD6;
};


half4 PSMain(VSOutput inputVx) : SV_Target
{

	half4 outColor;
	
half4 var_colormap = tex2D(colorMapSampler, inputVx.texcoord);
	var_colormap.rgb *= inputVx.color;
	var_colormap.rgb = var_colormap.rgb * var_colormap.rgb;
	
float4 var_shadowmapspotA;
var_shadowmapspotA.xyz = normalize(inputVx.texcoord1.xyz);
	
float4 var_outr;
var_outr.g = max(abs(var_shadowmapspotA.y), abs(var_shadowmapspotA.z));
	
float4 var_shadowmapspotC;
var_shadowmapspotC.x = max(abs(var_shadowmapspotA.x), var_outr.g);
	
float4 var_modellighting;
var_modellighting.xyz = lightPosition.xyz + (-inputVx.texcoord5);
	var_outr.a = dot(var_modellighting.xyz, var_modellighting.xyz);
	var_outr.a = rsqrt(var_outr.a);
	var_modellighting.xyz = var_modellighting.xyz * var_outr.aaa;
	var_outr.g = 1 / var_shadowmapspotC.x;
	var_shadowmapspotC.xyz = var_shadowmapspotA.xyz * lightingLookupScale.xyz;
	var_shadowmapspotA.x = saturate(dot(var_modellighting.xyz, var_shadowmapspotA.xyz));
	var_modellighting.w = dot(var_modellighting.xyz, lightSpotDir.xyz);
	var_shadowmapspotA.y = saturate(var_modellighting.w * lightSpotFactors.x + lightSpotFactors.y);
	var_modellighting.x = pow(abs(var_shadowmapspotA.y), abs(lightSpotFactors.z));
	var_shadowmapspotA.y = ((-var_shadowmapspotA.y) >= 0 ? 0 : var_modellighting.x);
	var_outr.a = 1 / var_outr.a;
	var_outr.a *= saturate(lightPosition.w);
	
half4 var_attenuation = tex2D(attenuationSampler, var_outr.aa);
	var_attenuation.xyz = var_attenuation.xyz * var_attenuation.xyz;
	var_attenuation.xyz = var_shadowmapspotA.yyy * var_attenuation.xyz;
	
float2 var_A = float2(1, 0);
	
float4 var_B = var_A.xxyy * spotShadowmapPixelAdjust.xyxx;
	
float4 var_D = inputVx.texcoord4.w * (-var_B) + inputVx.texcoord4;
	
float4 var_shadowmapspot = tex2Dproj(shadowmapSamplerSpot, var_D);
	var_shadowmapspotC.y = var_shadowmapspot.x;
	
float4 var_G = var_A.xxyy * spotShadowmapPixelAdjust.zwxx;
	
float4 var_H = inputVx.texcoord4.w * var_G + inputVx.texcoord4;
	var_shadowmapspotA = tex2Dproj(shadowmapSamplerSpot, var_H);
	var_shadowmapspotC.z = var_shadowmapspotA.x;
	
float4 var_I = inputVx.texcoord4.w * (-var_G) + inputVx.texcoord4;
	
float4 var_shadowmapspotB = tex2Dproj(shadowmapSamplerSpot, var_I);
	var_shadowmapspotC.w = var_shadowmapspotB.x;
	
float4 var_C = inputVx.texcoord4.w * var_B + inputVx.texcoord4;
	var_shadowmapspotC = tex2Dproj(shadowmapSamplerSpot, var_C);
	
float4 var_L = var_shadowmapspotC + (-inputVx.texcoord4.zzzz);
	var_outr = (var_L >= 0 ? float4(1, 1, 1, 1) : float4(0, 0, 0, 0));
	var_outr.r = dot(var_outr, float4(0.25, 0.25, 0.25, 0.25));
	var_outr.gba = var_shadowmapspotC.xyz * var_outr.g + inputVx.texcoord6;
	var_modellighting = tex3D(modelLightingSampler, var_outr.gba);
	var_shadowmapspotA.w = lerp(var_outr.r, var_modellighting.w, lightSpotFactors.w);
	var_shadowmapspotA.yzw = var_shadowmapspotA.www * var_attenuation.xyz;
	var_attenuation.xyz = var_shadowmapspotA.xxx * lightDiffuse.xyz;
	var_outr.rgb = var_modellighting.xyz + var_modellighting.xyz;
	var_outr.rgb = var_outr.rgb * var_outr.rgb;
	var_outr.rgb = var_shadowmapspotA.yzw * var_attenuation.xyz + var_outr.rgb;
	var_outr.rgb = var_colormap.rgb * var_outr.rgb - fogColorLinear.xyz;

	outColor.xyz = inputVx.texcoord1.w * var_outr.rgb + fogColorLinear.xyz;
	outColor.w = 1;

	return outColor;
}


