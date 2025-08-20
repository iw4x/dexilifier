//#define SUN 1
//#define DFOG 1
#define SHADOW 1

extern sampler3D modelLightingSampler : register(s4);
extern sampler2D colorMapSampler : register(s0);
extern float4 lightingLookupScale : register(c3);
extern float4 fogColorLinear : register(c0);

#if SUN
extern float4 lightDiffuse : register(c18);
extern float4 lightPosition : register(c17);
#endif

#if DFOG
extern float4 fogSunColorLinear : register(c34);
extern float4 fogSunDir : register(c33);
extern float4 fogSunConsts : register(c32);
#endif

#if SHADOW
extern sampler2D shadowmapSamplerSun : register(s5);
extern float4 sunShadowmapPixelAdjust : register(c5);
extern float4 shadowmapScale : register(c4);
extern float4 shadowmapSwitchPartition : register(c2);
#endif

struct VSOutput
{

    float3 color : COLOR0;
    float2 texcoord : TEXCOORD0; // Texcoord
    float4 texcoord1 : TEXCOORD1; // Normal (Worldspace) (UCHAR)
#if SUN
	float3 texcoord4 : TEXCOORD4;
#endif

#if DFOG
    float3 texcoord5 : TEXCOORD5;
#endif
    float3 texcoord6 : TEXCOORD6; // BaseLightingCoords
};

half4 SampleColorMap(VSOutput inputVx)
{
    half4 var_colormap = tex2D(colorMapSampler, inputVx.texcoord);
    var_colormap.rgb *= inputVx.color;
    var_colormap.rgb = var_colormap.rgb * var_colormap.rgb;
    
    return var_colormap;

}

half3 GetFogColor(VSOutput inputVx)
{
    half3 fogColor;
    
#if DFOG
    half3 var_G = normalize(inputVx.texcoord5);
	half sunFogVal = dot(fogSunDir.xyz, var_G);
	sunFogVal = sunFogVal + fogSunConsts.y;
	sunFogVal = saturate(sunFogVal * fogSunConsts.z);
	
	half3 fogColorComputed = fogSunColorLinear.xyz - fogColorLinear.xyz;
	
    fogColor = sunFogVal * fogColorComputed + fogColorLinear.xyz;
#else
    fogColor = fogColorLinear.xyz;
#endif
    
    return fogColor;
}

half3 MixColor(VSOutput inputVx, half3 baseColor, half3 lightingColor)
{
    half3 colorMix = baseColor * lightingColor - GetFogColor(inputVx);
    return colorMix;
}

half3 SampleLighting(float3 normal, float3 baseLightingCoords)
{
    half3 normalizedNorm = normalize(normal);
    
    half yzNormal = max(abs(normalizedNorm.y), abs(normalizedNorm.z));
	
    half maxNormal = max(abs(normalizedNorm.x), yzNormal);
	
    float4 var_D;
    var_D.w = 1 / maxNormal;
    var_D.xyz = normalizedNorm * lightingLookupScale.xyz;
    var_D.xyz = var_D.xyz * var_D.w + baseLightingCoords;
	
	
    half4 var_modellighting = tex3D(modelLightingSampler, var_D.xyz);
    var_modellighting.rgb = var_modellighting.rgb + var_modellighting.rgb;
    var_modellighting.rgb = var_modellighting.rgb * var_modellighting.rgb;
    
#if SUN
    half var_sunA = saturate(dot(lightPosition.xyz, normalizedNorm));
    half3 sunEffect = var_sunA * lightDiffuse.rgb;
    var_modellighting.rgb += sunEffect;
#endif
    
    return var_modellighting;
}

half3 AddFogColor(VSOutput inputVx, half3 colorMix)
{
    half3 additiveFogColor = GetFogColor(inputVx);
    
    return inputVx.texcoord1.w * colorMix + additiveFogColor;
}


half4 PSMain(VSOutput inputVx) : SV_Target
{
    half4 outColor;
	
    half4 var_colormap = SampleColorMap(inputVx);
    half3 var_modellighting = SampleLighting(inputVx.texcoord1.xyz, inputVx.texcoord6);
    
    half3 colorMix = MixColor(inputVx, var_colormap.rgb, var_modellighting);
	    
    outColor.xyz = AddFogColor(inputVx, colorMix);
    outColor.w = 1;

    return outColor;
}