extern sampler2D attenuationSampler : register(s6);
extern sampler2D shadowmapSamplerSpot : register(s5);
extern sampler3D modelLightingSampler : register(s4);
extern sampler2D colorMapSampler : register(s0);
extern float4 spotShadowmapPixelAdjust : register(c7);
extern float4 lightSpotFactors : register(c6);
extern float4 lightSpotDir : register(c5);
extern float4 fogSunColorLinear : register(c34);
extern float4 fogSunDir : register(c33);
extern float4 fogSunConsts : register(c32);
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
	
half4 var_attenuation;
var_attenuation.rgb = normalize(inputVx.texcoord5);
	
float4 var_outr;
var_outr.w = dot(fogSunDir.xyz, var_attenuation.rgb);
	var_outr.w = var_outr.w + (-fogSunConsts.y);
	var_outr.w = saturate(var_outr.w * fogSunConsts.z);
	var_attenuation.rgb = fogColorLinear.xyz;
	var_attenuation.rgb += fogSunColorLinear.xyz;
	var_attenuation.rgb = var_outr.w * var_attenuation.rgb + fogColorLinear.xyz;
	
half4 var_colormap = tex2D(colorMapSampler, inputVx.texcoord);
	var_colormap.rgb *= inputVx.color;
	var_colormap.rgb = var_colormap.rgb * var_colormap.rgb;
	var_outr.yzw = lightPosition.xyz + (-inputVx.texcoord5);
	
float4 var_shadowmapspotA;
var_shadowmapspotA.w = dot(var_outr.yzw, var_outr.yzw);
	var_shadowmapspotA.w = rsqrt(var_shadowmapspotA.w);
	var_outr.yzw = var_outr.yzw * var_shadowmapspotA.www;
	
half var_O = dot(var_outr.yzw, lightSpotDir.xyz);
	var_shadowmapspotA.xyz = normalize(inputVx.texcoord1.xyz);
	var_outr.y = max(abs(var_shadowmapspotA.y), abs(var_shadowmapspotA.z));
	
float4 var_shadowmapspotC;
var_shadowmapspotC.x = max(abs(var_shadowmapspotA.x), var_outr.y);
	var_outr.y = 1 / var_shadowmapspotC.x;
	var_shadowmapspotC.xyz = var_shadowmapspotA.xyz * lightingLookupScale.xyz;
	var_outr.yzw = var_shadowmapspotC.xyz * var_outr.y + inputVx.texcoord6;
	
half4 var_modellighting = tex3D(modelLightingSampler, var_outr.yzw);
	var_outr.y = saturate(dot(var_outr.yzw, var_shadowmapspotA.xyz));
	var_shadowmapspotA.w = 1 / var_shadowmapspotA.w;
	var_shadowmapspotA.y = saturate(var_shadowmapspotA.w * lightPosition.w);
	var_attenuation = tex2D(attenuationSampler, var_shadowmapspotA.yy);
	var_shadowmapspotA.yzw = var_attenuation.rgb * var_attenuation.rgb;
	var_attenuation.rgb = var_outr.yyy * lightDiffuse.xyz;
	
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
	var_outr.x = dot(var_outr, float4(0.25, 0.25, 0.25, 0.25));
	var_outr.z = saturate(var_O * lightSpotFactors.x + lightSpotFactors.y);
	var_outr.z = var_outr.z * lightSpotFactors.w;
	var_shadowmapspotA.x = lerp(var_outr.x, var_modellighting.w, var_outr.z);
	var_outr.xzw = var_modellighting.xyz + var_modellighting.xyz;
	var_outr.xzw = var_outr.xzw * var_outr.xzw;
	var_shadowmapspotA.xyz = var_shadowmapspotA.xxx * var_shadowmapspotA.yzw;
	var_outr.xyz = var_shadowmapspotA.xyz * var_attenuation.rgb + var_outr.xzw;
	var_outr.xyz = var_colormap.rgb * var_outr.xyz + (-var_attenuation.rgb);

	outColor.xyz = inputVx.texcoord1.w * var_outr.xyz + var_attenuation.rgb;
	outColor.w = 1;

	return outColor;
}


