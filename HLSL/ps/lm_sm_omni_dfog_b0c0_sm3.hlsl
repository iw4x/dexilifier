extern sampler2D shadowmapSamplerSpot : register(s4);
extern sampler2D lightmapSamplerSecondary : register(s3);
extern sampler2D lightmapSamplerPrimary : register(s2);
extern sampler2D colorMapSampler : register(s0);
extern float4 spotShadowmapPixelAdjust : register(c7);
extern float4 lightFalloffPlacement : register(c6);
extern float4 lightSpotFactors : register(c5);
extern float4 fogSunColorLinear : register(c34);
extern float4 fogSunDir : register(c33);
extern float4 fogSunConsts : register(c32);
extern float4 lightSpotDir : register(c3);
extern float4 lightDiffuse : register(c18);
extern float4 lightPosition : register(c17);
extern float4 fogColorLinear : register(c0);

struct VSOutput
{

	float4 color : COLOR0;
	float4 texcoord : TEXCOORD0;
	float4 texcoord1 : TEXCOORD1;
	float4 texcoord4 : TEXCOORD4;
	float3 texcoord5 : TEXCOORD5;
};


half4 PSMain(VSOutput inputVx) : SV_Target
{

	half4 outColor;
	
float3 var_A;
var_A.xz = float2(1, 0);
	
float4 var_B = var_A.xxzz * spotShadowmapPixelAdjust.xyxx;
	
float4 var_C = inputVx.texcoord4.w * var_B + inputVx.texcoord4;
	
float4 var_D = inputVx.texcoord4.w * (-var_B) + inputVx.texcoord4;
	
float4 var_shadowmapspot = tex2Dproj(shadowmapSamplerSpot, var_C);
	
float var_shadowmapspotA = (tex2Dproj(shadowmapSamplerSpot, var_D)).x;
	var_shadowmapspot.y = var_shadowmapspotA;
	
float4 var_G = var_A.xxzz * spotShadowmapPixelAdjust.zwxx;
	
float4 var_H = inputVx.texcoord4.w * var_G + inputVx.texcoord4;
	
float4 var_I = inputVx.texcoord4.w * (-var_G) + inputVx.texcoord4;
	
float2 var_shadowmapspotB = (tex2Dproj(shadowmapSamplerSpot, var_H)).xy;
	var_shadowmapspot.z = var_shadowmapspotB.x;
	
float var_shadowmapspotC = (tex2Dproj(shadowmapSamplerSpot, var_I)).x;
	var_shadowmapspot.w = var_shadowmapspotC;
	
float4 var_L = var_shadowmapspot + (-inputVx.texcoord4.zzzz);
	
float4 var_outrA = (var_L >= 0 ? float4(1, 1, 1, 1) : float4(0, 0, 0, 0));
	var_outrA.r = dot(var_outrA, float4(0.25, 0.25, 0.25, 0.25));
	var_outrA.gba = lightPosition.xyz + (-inputVx.texcoord5);
	var_shadowmapspotB.x = dot(var_outrA.gba, var_outrA.gba);
	var_shadowmapspotB.x = rsqrt(var_shadowmapspotB.x);
	var_outrA.gba = var_outrA.gba * var_shadowmapspotB.xxx;
	var_shadowmapspotB.x = 1 / var_shadowmapspotB.x;
	var_shadowmapspotB.y = dot(var_outrA.gba, lightSpotDir.xyz);
	var_shadowmapspotB.y = saturate(var_shadowmapspotB.y * lightSpotFactors.x + lightSpotFactors.y);
	var_shadowmapspotB.y = var_shadowmapspotB.y * lightSpotFactors.w;
	
float2 var_lightmapprimary = (tex2D(lightmapSamplerPrimary, inputVx.texcoord.zw)).xy;
	
half var_O = lerp(var_outrA.r, var_lightmapprimary.x, var_shadowmapspotB.y);
	var_outrA.r = saturate(var_shadowmapspotB.x * lightPosition.w);
	var_shadowmapspotB = var_outrA.r * lightFalloffPlacement.xy + lightFalloffPlacement.zw;
	
half3 var_lightmapsecondary = (tex2D(lightmapSamplerSecondary, var_shadowmapspotB)).xyz;
	var_lightmapsecondary = var_lightmapsecondary * var_lightmapsecondary;
	var_lightmapsecondary = var_O.xxx * var_lightmapsecondary;
	var_lightmapprimary = float2(1, 0.5) * inputVx.texcoord.zw;
	
half4 var_lightmapsecondaryA = tex2D(lightmapSamplerSecondary, var_lightmapprimary);
	
float4 var_outr;
var_outr.x = var_lightmapsecondaryA.w;
	var_outr.zw = inputVx.texcoord.zw * float2(1, 0.5) + float2(0, 0.5);
	
half4 var_lightmapsecondaryB = tex2D(lightmapSamplerSecondary, var_outr.zw);
	var_outr.y = var_lightmapsecondaryB.w;
	var_outr.xy = var_outr.xy * float2(4.08, 4.064516) - float2(2.08, 2.064516);
	var_outrA.r = dot(var_outr.xy, var_outr.xy) + 1;
	var_outrA.r = saturate(rsqrt(var_outrA.r));
	var_lightmapsecondaryA.xyz = var_lightmapsecondaryB.xyz * var_outrA.r + var_lightmapsecondaryA.xyz;
	var_lightmapsecondaryA.xyz = var_lightmapsecondaryA.xyz * var_lightmapsecondaryA.xyz;
	var_outr.xyz = normalize(inputVx.texcoord1.xyz);
	var_outrA.r = saturate(dot(var_outrA.gba, var_outr.xyz));
	var_outrA.rgb *= lightDiffuse.xyz;
	var_outrA.rgb = var_lightmapsecondary * var_outrA.rgb + var_lightmapsecondaryA.xyz;
	var_lightmapsecondary = normalize(inputVx.texcoord5);
	var_outrA.a = dot(fogSunDir.xyz, var_lightmapsecondary);
	var_outrA.a = var_outrA.a + (-fogSunConsts.y);
	var_outrA.a = saturate(var_outrA.a * fogSunConsts.z);
	var_lightmapsecondary = fogColorLinear.xyz;
	var_lightmapsecondary += fogSunColorLinear.xyz;
	var_lightmapsecondary = var_outrA.a * var_lightmapsecondary + fogColorLinear.xyz;
	
half4 var_colormap = tex2D(colorMapSampler, inputVx.texcoord.xy);
	var_outrA.a = var_colormap.a * (-inputVx.color.a) + 1;
	
half4 var_U = var_colormap * inputVx.color;
	var_outr.w = (-var_outrA.a) + 1;
	var_lightmapsecondary = var_lightmapsecondary * var_outr.www;
	var_U.rgb = var_U.rgb * var_U.rgb;
	var_outr.xyz = var_U.aaa * var_U.rgb;
	var_outrA.rgb = var_outr.xyz * var_outrA.rgb + (-var_lightmapsecondary);
	var_outrA.rgb = inputVx.texcoord1.w * var_outrA.rgb + var_lightmapsecondary;
	var_outrA.a = var_outr.w;

	outColor = ((-abs(var_outr.wwww)) >= 0 ? var_outr : var_outrA);

	return outColor;
}


