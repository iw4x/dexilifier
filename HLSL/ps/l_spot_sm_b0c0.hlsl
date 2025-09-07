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

	float4 color : COLOR0;
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
	
float4 var_L = var_shadowmapspot + (-inputVx.texcoord4.zzzz);
	
float4 var_M = (var_L >= 0 ? float4(1, 1, 1, 1) : float4(0, 0, 0, 0));
	var_outr.g = dot(var_M, float4(0.25, 0.25, 0.25, 0.25));
	var_outr.g = var_outr.g + (-1);
	var_outr.r = lightSpotFactors.w * var_outr.g + var_outr.r;
	
float4 var_colormap = tex2D(colorMapSampler, inputVx.texcoord);
	
half4 var_outrA = var_colormap * inputVx.color;
	var_outr.g = var_colormap.a * (-inputVx.color.a) + 1;
	var_colormap.rgb = var_outrA.rgb * var_outrA.rgb;
	var_colormap.rgb = var_outrA.aaa * var_colormap.rgb;
	var_outr.rba = var_outr.rrr * var_colormap.rgb;
	var_colormap.rgb = lightPosition.xyz + (-inputVx.texcoord5);
	var_colormap.a = dot(var_colormap.rgb, var_colormap.rgb);
	var_colormap.a = rsqrt(var_colormap.a);
	var_colormap.rgb = var_colormap.rgb * var_colormap.aaa;
	var_colormap.a = 1 / var_colormap.a;
	var_outrA.r = dot(var_colormap.rgb, lightSpotDir.xyz);
	var_outrA.r = saturate(var_outrA.r * lightSpotFactors.x + lightSpotFactors.y);
	var_shadowmapspotB = pow(abs(var_outrA.r), abs(lightSpotFactors.z));
	var_outrA.r = ((-var_outrA.r) >= 0 ? 0 : var_shadowmapspotB);
	var_colormap.a *= saturate(lightPosition.w);
	
half3 var_attenuation = (tex2D(attenuationSampler, var_colormap.aa)).rgb;
	var_outrA.gba = var_attenuation * var_attenuation;
	var_outrA.rgb = var_outrA.rrr * var_outrA.gba;
	var_outr.rba = var_outr.rba * var_outrA.rgb;
	var_outrA.rgb = normalize(inputVx.texcoord1.xyz);
	var_colormap.r = saturate(dot(var_colormap.rgb, var_outrA.rgb));
	var_colormap.rgb *= lightDiffuse.xyz;
	var_outr.g = (-var_outr.g) + 1;
	var_outrA.rgb = var_outr.ggg * fogColorLinear.xyz;

	outColor.w = var_outr.g;

	var_outr.rgb = var_outr.rba * var_colormap.rgb + (-var_outrA.rgb);

	outColor.xyz = inputVx.texcoord1.w * var_outr.rgb + var_outrA.rgb;

	return outColor;
}


