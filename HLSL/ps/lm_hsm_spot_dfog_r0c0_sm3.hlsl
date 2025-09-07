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
	
float var_shadowmapspotB = (tex2Dproj(shadowmapSamplerSpot, var_H)).x;
	var_shadowmapspot.z = var_shadowmapspotB;
	
float4 var_shadowmapspotC = tex2Dproj(shadowmapSamplerSpot, var_I);
	var_shadowmapspot.w = var_shadowmapspotC.x;
	var_shadowmapspotC.x = dot(var_shadowmapspot, float4(0.25, 0.25, 0.25, 0.25));
	
float4 var_lightmapprimary = tex2D(lightmapSamplerPrimary, inputVx.texcoord.zw);
	var_shadowmapspot.x = lerp(var_shadowmapspotC.x, var_lightmapprimary.x, lightSpotFactors.w);
	var_shadowmapspotC.xyz = lightPosition.xyz + (-inputVx.texcoord5);
	var_shadowmapspotC.w = dot(var_shadowmapspotC.xyz, var_shadowmapspotC.xyz);
	var_shadowmapspotC.w = rsqrt(var_shadowmapspotC.w);
	var_shadowmapspotC.xyz = var_shadowmapspotC.xyz * var_shadowmapspotC.www;
	var_shadowmapspotC.w = 1 / var_shadowmapspotC.w;
	var_lightmapprimary.x = dot(var_shadowmapspotC.xyz, lightSpotDir.xyz);
	var_lightmapprimary.x = saturate(var_lightmapprimary.x * lightSpotFactors.x + lightSpotFactors.y);
	var_shadowmapspot.y = pow(abs(var_lightmapprimary.x), abs(lightSpotFactors.z));
	var_lightmapprimary.x = ((-var_lightmapprimary.x) >= 0 ? 0 : var_shadowmapspot.y);
	var_shadowmapspotC.w *= saturate(lightPosition.w);
	var_lightmapprimary.yz = var_shadowmapspotC.w * lightFalloffPlacement.xy + lightFalloffPlacement.zw;
	
float4 var_lightmapsecondary = tex2D(lightmapSamplerSecondary, var_lightmapprimary.yz);
	var_lightmapprimary.yzw = var_lightmapsecondary.xyz * var_lightmapsecondary.xyz;
	var_lightmapprimary.xyz = var_lightmapprimary.xxx * var_lightmapprimary.yzw;
	var_lightmapprimary.xyz = var_shadowmapspot.xxx * var_lightmapprimary.xyz;
	var_shadowmapspot.xy = float2(1, 0.5) * inputVx.texcoord.zw;
	
half4 var_lightmapsecondaryA = tex2D(lightmapSamplerSecondary, var_shadowmapspot.xy);
	var_lightmapsecondary.x = var_lightmapsecondaryA.a;
	var_lightmapsecondary.zw = inputVx.texcoord.zw * float2(1, 0.5) + float2(0, 0.5);
	
half4 var_lightmapsecondaryB = tex2D(lightmapSamplerSecondary, var_lightmapsecondary.zw);
	var_lightmapsecondary.y = var_lightmapsecondaryB.w;
	var_lightmapsecondary.xy = var_lightmapsecondary.xy * float2(4.08, 4.064516) - float2(2.08, 2.064516);
	var_shadowmapspotC.w = dot(var_lightmapsecondary.xy, var_lightmapsecondary.xy) + 1;
	var_shadowmapspotC.w = saturate(rsqrt(var_shadowmapspotC.w));
	var_lightmapsecondaryA.rgb = var_lightmapsecondaryB.xyz * var_shadowmapspotC.w + var_lightmapsecondaryA.rgb;
	var_lightmapsecondaryA.rgb = var_lightmapsecondaryA.rgb * var_lightmapsecondaryA.rgb;
	var_lightmapsecondary.xyz = normalize(inputVx.texcoord1.xyz);
	var_shadowmapspotC.x = saturate(dot(var_shadowmapspotC.xyz, var_lightmapsecondary.xyz));
	var_shadowmapspotC.xyz *= lightDiffuse.xyz;
	var_shadowmapspotC.xyz = var_lightmapprimary.xyz * var_shadowmapspotC.xyz + var_lightmapsecondaryA.rgb;
	
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


