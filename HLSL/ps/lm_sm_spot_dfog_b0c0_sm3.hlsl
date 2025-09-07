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
	
float var_shadowmapspotB = (tex2Dproj(shadowmapSamplerSpot, var_H)).x;
	var_shadowmapspot.z = var_shadowmapspotB;
	
float var_shadowmapspotC = (tex2Dproj(shadowmapSamplerSpot, var_I)).x;
	var_shadowmapspot.w = var_shadowmapspotC;
	
float4 var_L = var_shadowmapspot + (-inputVx.texcoord4.zzzz);
	
float4 var_outr = (var_L >= 0 ? float4(1, 1, 1, 1) : float4(0, 0, 0, 0));
	var_outr.r = dot(var_outr, float4(0.25, 0.25, 0.25, 0.25));
	
float4 var_lightmapprimary = tex2D(lightmapSamplerPrimary, inputVx.texcoord.zw);
	var_shadowmapspot.x = lerp(var_outr.r, var_lightmapprimary.r, lightSpotFactors.w);
	var_outr.rgb = lightPosition.xyz + (-inputVx.texcoord5);
	var_outr.a = dot(var_outr.rgb, var_outr.rgb);
	var_outr.a = rsqrt(var_outr.a);
	var_outr.rgb = var_outr.rgb * var_outr.aaa;
	var_outr.a = 1 / var_outr.a;
	var_lightmapprimary.r = dot(var_outr.rgb, lightSpotDir.xyz);
	var_lightmapprimary.r = saturate(var_lightmapprimary.r * lightSpotFactors.x + lightSpotFactors.y);
	var_shadowmapspot.y = pow(abs(var_lightmapprimary.r), abs(lightSpotFactors.z));
	var_lightmapprimary.r = ((-var_lightmapprimary.r) >= 0 ? 0 : var_shadowmapspot.y);
	var_outr.a *= saturate(lightPosition.w);
	var_lightmapprimary.gb = var_outr.a * lightFalloffPlacement.xy + lightFalloffPlacement.zw;
	
float4 var_lightmapsecondary = tex2D(lightmapSamplerSecondary, var_lightmapprimary.gb);
	var_lightmapprimary.gba = var_lightmapsecondary.xyz * var_lightmapsecondary.xyz;
	var_lightmapprimary.rgb = var_lightmapprimary.rrr * var_lightmapprimary.gba;
	var_lightmapprimary.rgb = var_shadowmapspot.xxx * var_lightmapprimary.rgb;
	var_shadowmapspot.xy = float2(1, 0.5) * inputVx.texcoord.zw;
	
half4 var_lightmapsecondaryA = tex2D(lightmapSamplerSecondary, var_shadowmapspot.xy);
	var_lightmapsecondary.x = var_lightmapsecondaryA.w;
	var_lightmapsecondary.zw = inputVx.texcoord.zw * float2(1, 0.5) + float2(0, 0.5);
	
half4 var_lightmapsecondaryB = tex2D(lightmapSamplerSecondary, var_lightmapsecondary.zw);
	var_lightmapsecondary.y = var_lightmapsecondaryB.w;
	var_lightmapsecondary.xy = var_lightmapsecondary.xy * float2(4.08, 4.064516) - float2(2.08, 2.064516);
	var_outr.a = dot(var_lightmapsecondary.xy, var_lightmapsecondary.xy) + 1;
	var_outr.a = saturate(rsqrt(var_outr.a));
	var_lightmapsecondaryA.xyz = var_lightmapsecondaryB.xyz * var_outr.a + var_lightmapsecondaryA.xyz;
	var_lightmapsecondaryA.xyz = var_lightmapsecondaryA.xyz * var_lightmapsecondaryA.xyz;
	var_lightmapsecondary.xyz = normalize(inputVx.texcoord1.xyz);
	var_outr.r = saturate(dot(var_outr.rgb, var_lightmapsecondary.xyz));
	var_outr.rgb *= lightDiffuse.xyz;
	var_outr.rgb = var_lightmapprimary.rgb * var_outr.rgb + var_lightmapsecondaryA.xyz;
	var_lightmapprimary.rgb = normalize(inputVx.texcoord5);
	var_outr.a = dot(fogSunDir.xyz, var_lightmapprimary.rgb);
	var_outr.a = var_outr.a + (-fogSunConsts.y);
	var_outr.a = saturate(var_outr.a * fogSunConsts.z);
	var_lightmapprimary.rgb = fogColorLinear.xyz;
	var_lightmapprimary.rgb += fogSunColorLinear.xyz;
	var_lightmapprimary.rgb = var_outr.a * var_lightmapprimary.rgb + fogColorLinear.xyz;
	
half4 var_colormap = tex2D(colorMapSampler, inputVx.texcoord.xy);
	var_outr.a = var_colormap.a * (-inputVx.color.a) + 1;
	
half4 var_S = var_colormap * inputVx.color;
	var_lightmapsecondary.w = (-var_outr.a) + 1;
	var_lightmapprimary.rgb = var_lightmapprimary.rgb * var_lightmapsecondary.www;
	var_S.rgb = var_S.rgb * var_S.rgb;
	var_lightmapsecondary.xyz = var_S.aaa * var_S.rgb;
	var_outr.rgb = var_lightmapsecondary.xyz * var_outr.rgb + (-var_lightmapprimary.rgb);
	var_outr.rgb = inputVx.texcoord1.w * var_outr.rgb + var_lightmapprimary.rgb;
	var_outr.a = var_lightmapsecondary.w;

	outColor = ((-abs(var_lightmapsecondary.wwww)) >= 0 ? var_lightmapsecondary : var_outr);

	return outColor;
}


