#define SUN 1
//#define DFOG 1
#define SHADOW 1
//#define PREMULTIPLIED_ALPHA 1
#define VERTEX_COLOR 1

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
#if VERTEX_COLOR
    float3 color : COLOR0;
#endif
    float2 texcoord : TEXCOORD0;
    float4 wsNormal : TEXCOORD1; // w contain the depth used by the fog interpolation
#if SHADOW
	float3 shadowPos : TEXCOORD4;
#endif

#if DFOG
    float3 fogNormal : TEXCOORD5;
#endif
    float3 baseLightingCoords : TEXCOORD6;
};

half4 SampleColorMap(VSOutput inputVx)
{
    half4 var_colormap = tex2D(colorMapSampler, inputVx.texcoord);
#if VERTEX_COLOR
    var_colormap.rgb *= inputVx.color;
#endif
    var_colormap.rgb = pow(var_colormap.rgb, 2); // pseudo gamma correction
    
    return var_colormap;
}

half3 GetFogColor(VSOutput inputVx)
{
    half3 fogColor;
    
#if DFOG
    half3 fogNormal = normalize(inputVx.fogNormal);
	half sunFogVal = dot(fogSunDir.xyz, fogNormal);
	sunFogVal = sunFogVal + fogSunConsts.y;
	sunFogVal = saturate(sunFogVal * fogSunConsts.z);
	
    fogColor = lerp(fogColorLinear.xyz, fogSunColorLinear.xyz, sunFogVal);
#else
    fogColor = fogColorLinear.xyz;
#endif
    
    return fogColor;
}

half4 SampleAmbientLighting(half3 normalizedNormal, half3 baseLightingCoords)
{
    half maxNormal = max(abs(normalizedNormal.x), max(abs(normalizedNormal.y), abs(normalizedNormal.z)));
	
    half3 probe_pos = (normalizedNormal * lightingLookupScale.xyz);
    probe_pos = probe_pos / maxNormal + baseLightingCoords;
    
    half4 ambient = tex3D(modelLightingSampler, probe_pos);
    return pow(2 * ambient, 2);
}

#if SHADOW
half SampleShadowMap(float2 shadowPos, float bias, float ambient)
{
    float4 shadowSample;
	shadowSample.x = tex2Dlod(shadowmapSamplerSun, float4(shadowPos + sunShadowmapPixelAdjust.xy, 0, 0));
	shadowSample.y = tex2Dlod(shadowmapSamplerSun, float4(shadowPos - sunShadowmapPixelAdjust.xy, 0, 0));
	shadowSample.z = tex2Dlod(shadowmapSamplerSun, float4(shadowPos + sunShadowmapPixelAdjust.zw, 0, 0));
	shadowSample.w = tex2Dlod(shadowmapSamplerSun, float4(shadowPos - sunShadowmapPixelAdjust.zw, 0, 0));

	shadowSample = shadowSample - bias; // bias
	shadowSample = (shadowSample >= 0 ? 1 : 0); // cutout
	half shadowContribution = dot(shadowSample, 0.25); // average
	
	float2 shadowPartitionCoord = shadowPos * shadowmapSwitchPartition.w + shadowmapSwitchPartition.xy;
	float4 shadowPartSample;
    shadowPartSample.x = tex2Dlod(shadowmapSamplerSun, float4(shadowPartitionCoord + sunShadowmapPixelAdjust.xy, 0, 0));
	shadowPartSample.y = tex2Dlod(shadowmapSamplerSun, float4(shadowPartitionCoord - sunShadowmapPixelAdjust.xy, 0, 0));
	shadowPartSample.z = tex2Dlod(shadowmapSamplerSun, float4(shadowPartitionCoord + sunShadowmapPixelAdjust.zw, 0, 0));
	shadowPartSample.w = tex2Dlod(shadowmapSamplerSun, float4(shadowPartitionCoord - sunShadowmapPixelAdjust.zw, 0, 0));
	
	shadowPartSample = shadowPartSample - bias; // bias
	shadowPartSample = (shadowPartSample >= 0 ? 1 : 0); // cutout
	half shadowPartContribution = dot(shadowPartSample, 0.25); // average

	half4 shadowScale = half4(shadowPos, shadowPartitionCoord) * shadowmapScale.xyxy + shadowmapScale.zzzw;
	// FIXME: understand this pazrt better and find name for variable
	half2 var_N = max(abs(shadowScale.xz), abs(shadowScale.yw));
	var_N = saturate(8 - var_N);
	half var_H = var_N.x * -var_N.y + var_N.x;
	var_H = (-abs(var_H) >= 0 ? var_N.x : 1);
	half shadowPartAmbient = lerp(shadowPartContribution, ambient, var_N.y);
	half shadow = lerp(shadowContribution, shadowPartAmbient, var_H);
    
    return shadow;
}
#endif

#if SUN
half3 GetSunLight(half3 normalizedNormal)
{
    half var_sunA = saturate(dot(lightPosition.xyz, normalizedNormal));
    half3 sunEffect = var_sunA * lightDiffuse.rgb;
    
    return sunEffect;
}
#endif

half4 PSMain(VSOutput inputVx) : SV_Target
{
    half4 outColor;

    half4 diffuse = SampleColorMap(inputVx);
    half3 normalizedNormal = normalize(inputVx.wsNormal.xyz);
    half4 light = SampleAmbientLighting(normalizedNormal, inputVx.baseLightingCoords);
    
#if SUN
    half3 directionnalLight = GetSunLight(normalizedNormal);
#if SHADOW
    directionnalLight *= SampleShadowMap(inputVx.shadowPos.xy, inputVx.shadowPos.z, light.w);
#endif
    light.rgb += directionnalLight;
#endif

#if PREMULTIPLIED_ALPHA
    diffuse.rgb *= diffuse.a;
#endif

    half3 additiveFogColor = GetFogColor(inputVx);
#if PREMULTIPLIED_ALPHA
    additiveFogColor *= diffuse.a;
#endif

    outColor.xyz = lerp(diffuse.rgb * light.rgb, additiveFogColor, 1 - inputVx.wsNormal.w);
    outColor.w = 1;

    return outColor;
}
