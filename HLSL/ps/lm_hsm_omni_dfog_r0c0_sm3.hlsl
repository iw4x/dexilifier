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

	float3 color : COLOR0;
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
	
float4 var_shadowmapspotC = tex2Dproj(shadowmapSamplerSpot, var_I);
	var_shadowmapspot.w = var_shadowmapspotC.x;
	var_shadowmapspotC.x = dot(var_shadowmapspot, float4(0.25, 0.25, 0.25, 0.25));
	var_shadowmapspotC.yzw = lightPosition.xyz + (-inputVx.texcoord5);
	var_shadowmapspotB.x = dot(var_shadowmapspotC.yzw, var_shadowmapspotC.yzw);
	var_shadowmapspotB.x = rsqrt(var_shadowmapspotB.x);
	var_shadowmapspotC.yzw = var_shadowmapspotC.yzw * var_shadowmapspotB.xxx;
	var_shadowmapspotB.x = 1 / var_shadowmapspotB.x;
	var_shadowmapspotB.y = dot(var_shadowmapspotC.yzw, lightSpotDir.xyz);
	var_shadowmapspotB.y = saturate(var_shadowmapspotB.y * lightSpotFactors.x + lightSpotFactors.y);
	var_shadowmapspotB.y = var_shadowmapspotB.y * lightSpotFactors.w;
	
float2 var_lightmapprimary = (tex2D(lightmapSamplerPrimary, inputVx.texcoord.zw)).xy;
	
half var_M = lerp(var_shadowmapspotC.x, var_lightmapprimary.x, var_shadowmapspotB.y);
	var_shadowmapspotC.x = saturate(var_shadowmapspotB.x * lightPosition.w);
	var_shadowmapspotB = var_shadowmapspotC.x * lightFalloffPlacement.xy + lightFalloffPlacement.zw;
	
half3 var_lightmapsecondary = (tex2D(lightmapSamplerSecondary, var_shadowmapspotB)).xyz;
	var_lightmapsecondary = var_lightmapsecondary * var_lightmapsecondary;
	var_lightmapsecondary = var_M.xxx * var_lightmapsecondary;
	var_lightmapprimary = float2(1, 0.5) * inputVx.texcoord.zw;
	
half4 var_lightmapsecondaryA = tex2D(lightmapSamplerSecondary, var_lightmapprimary);
	
float4 var_P;
var_P.x = var_lightmapsecondaryA.a;
	var_P.zw = inputVx.texcoord.zw * float2(1, 0.5) + float2(0, 0.5);
	
half4 var_lightmapsecondaryB = tex2D(lightmapSamplerSecondary, var_P.zw);
	var_P.y = var_lightmapsecondaryB.w;
	var_P.xy = var_P.xy * float2(4.08, 4.064516) - float2(2.08, 2.064516);
	var_shadowmapspotC.x = dot(var_P.xy, var_P.xy) + 1;
	var_shadowmapspotC.x = saturate(rsqrt(var_shadowmapspotC.x));
	var_lightmapsecondaryA.rgb = var_lightmapsecondaryB.xyz * var_shadowmapspotC.x + var_lightmapsecondaryA.rgb;
	var_lightmapsecondaryA.rgb = var_lightmapsecondaryA.rgb * var_lightmapsecondaryA.rgb;
	var_P.xyz = normalize(inputVx.texcoord1.xyz);
	var_shadowmapspotC.x = saturate(dot(var_shadowmapspotC.yzw, var_P.xyz));
	var_shadowmapspotC.xyz *= lightDiffuse.xyz;
	var_shadowmapspotC.xyz = var_lightmapsecondary * var_shadowmapspotC.xyz + var_lightmapsecondaryA.rgb;
	
half3 var_colormap = (tex2D(colorMapSampler, inputVx.texcoord.xy)).xyz;
	var_colormap *= inputVx.color;
	var_colormap = var_colormap * var_colormap;
	var_lightmapsecondaryA.rgb = normalize(inputVx.texcoord5);
	var_shadowmapspotC.w = dot(fogSunDir.xyz, var_lightmapsecondaryA.rgb);
	var_shadowmapspotC.w = var_shadowmapspotC.w + (-fogSunConsts.y);
	var_shadowmapspotC.w = saturate(var_shadowmapspotC.w * fogSunConsts.z);
	var_lightmapsecondaryA.rgb = fogColorLinear.xyz;
	var_lightmapsecondaryA.rgb += fogSunColorLinear.xyz;
	var_lightmapsecondaryA.rgb = var_shadowmapspotC.w * var_lightmapsecondaryA.rgb + fogColorLinear.xyz;
	var_shadowmapspotC.xyz = var_colormap * var_shadowmapspotC.xyz + (-var_lightmapsecondaryA.rgb);

	outColor.xyz = inputVx.texcoord1.w * var_shadowmapspotC.xyz + var_lightmapsecondaryA.rgb;
	outColor.w = 1;

	return outColor;
}


