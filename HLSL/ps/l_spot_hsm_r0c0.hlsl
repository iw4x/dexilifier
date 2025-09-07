extern sampler2D attenuationSampler : register(s5);
extern sampler2D shadowmapSamplerSpot : register(s4);
extern sampler2D colorMapSampler : register(s0);
extern float4 spotShadowmapPixelAdjust : register(c6);
extern float4 lightSpotFactors : register(c5);
extern float4 lightSpotDir : register(c3);
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
};


half4 PSMain(VSOutput inputVx) : SV_Target
{

	half4 outColor;
	
float4 var_outr;
var_outr.rg = float2(1, 0);
	
float4 var_B = var_outr.rrgg * spotShadowmapPixelAdjust.xyxx;
	
float4 var_C = inputVx.texcoord4.w * var_B + inputVx.texcoord4;
	
float4 var_D = inputVx.texcoord4.w * (-var_B) + inputVx.texcoord4;
	
float4 var_shadowmapspot = tex2Dproj(shadowmapSamplerSpot, var_C);
	
float var_shadowmapspotA = (tex2Dproj(shadowmapSamplerSpot, var_D)).x;
	var_shadowmapspot.y = var_shadowmapspotA;
	
float4 var_G = var_outr.rrgg * spotShadowmapPixelAdjust.zwxx;
	
float4 var_H = inputVx.texcoord4.w * var_G + inputVx.texcoord4;
	
float4 var_I = inputVx.texcoord4.w * (-var_G) + inputVx.texcoord4;
	
float var_shadowmapspotB = (tex2Dproj(shadowmapSamplerSpot, var_H)).x;
	var_shadowmapspot.z = var_shadowmapspotB;
	
float var_shadowmapspotC = (tex2Dproj(shadowmapSamplerSpot, var_I)).x;
	var_shadowmapspot.w = var_shadowmapspotC;
	var_outr.g = dot(var_shadowmapspot, float4(0.25, 0.25, 0.25, 0.25));
	var_outr.g = var_outr.g + (-1);
	var_outr.r = lightSpotFactors.w * var_outr.g + var_outr.r;
	
float4 var_colormap = tex2D(colorMapSampler, inputVx.texcoord);
	var_outr.gba = var_colormap.rgb * inputVx.color;
	var_outr.gba = var_outr.gba * var_outr.gba;
	var_outr.rgb = var_outr.rrr * var_outr.gba;
	var_colormap.rgb = lightPosition.xyz + (-inputVx.texcoord5);
	var_outr.a = dot(var_colormap.rgb, var_colormap.rgb);
	var_outr.a = rsqrt(var_outr.a);
	var_colormap.rgb = var_colormap.rgb * var_outr.aaa;
	var_outr.a = 1 / var_outr.a;
	var_colormap.a = dot(var_colormap.rgb, lightSpotDir.xyz);
	var_colormap.a = saturate(var_colormap.a * lightSpotFactors.x + lightSpotFactors.y);
	var_shadowmapspot.x = pow(abs(var_colormap.a), abs(lightSpotFactors.z));
	var_colormap.a = ((-var_colormap.a) >= 0 ? 0 : var_shadowmapspot.x);
	var_outr.a *= saturate(lightPosition.w);
	
half3 var_attenuation = (tex2D(attenuationSampler, var_outr.aa)).rgb;
	var_attenuation = var_attenuation * var_attenuation;
	var_attenuation = var_colormap.aaa * var_attenuation;
	var_outr.rgb *= var_attenuation;
	var_attenuation = normalize(inputVx.texcoord1.xyz);
	var_outr.a = saturate(dot(var_colormap.rgb, var_attenuation));
	var_colormap.rgb = var_outr.aaa * lightDiffuse.xyz;
	var_outr.rgb = var_outr.rgb * var_colormap.rgb - fogColorLinear.xyz;

	outColor.xyz = inputVx.texcoord1.w * var_outr.rgb + fogColorLinear.xyz;
	outColor.w = 1;

	return outColor;
}


