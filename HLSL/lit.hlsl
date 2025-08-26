//#define SHADOW 1                // _sm
#define SUN 1                   // _sun
//#define DFOG 1                // _fog=0 _dfog=1
//#define BLENDED 1               // b0
#define NORMAL_MAP 1            // n0
#define VERTEX_COLOR 1          // _nc=0

// hard coded registers
extern sampler2D colorMapSampler : register(s0);
extern sampler2D reflectionProbeSampler: register(s1);
extern sampler2D lightmapSamplerPrimary: register(s2);
extern sampler2D lightmapSamplerSecondary: register(s3);
// from now on the samplers are placed sequentially based on use
extern sampler3D modelLightingSampler;
#if SHADOW
extern sampler2D shadowmapSamplerSun;
#endif
#if NORMAL_MAP
extern sampler2D normalMapSampler;
#endif

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
extern float4 sunShadowmapPixelAdjust : register(c5);
extern float4 shadowmapScale : register(c4);
extern float4 shadowmapSwitchPartition : register(c2);
#endif

struct VSOutput
{
#if VERTEX_COLOR
    float4 color : COLOR0;
#endif
    float2 texcoord : TEXCOORD0;
    float4 wsNormal : TEXCOORD1; // w contain the depth used by the fog interpolation
#if NORMAL_MAP
    float4 texcoord2 : TEXCOORD2;
    float4 texcoord3 : TEXCOORD3;
#endif
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
    var_colormap *= inputVx.color;
    half alpha = 1 - (var_colormap.a * inputVx.color.a);
#endif
    var_colormap.rgb = pow(var_colormap.rgb, 2); // pseudo gamma correction
    
#if BLENDED
    var_colormap.rgb *= var_colormap.a;
    var_colormap.a = 1 - alpha;
#endif
    
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

half4 SampleModelLighting(half3 normalizedNormal, half3 baseLightingCoords)
{
    half maxNormal = max(abs(normalizedNormal.x), max(abs(normalizedNormal.y), abs(normalizedNormal.z)));
    
    half3 probe_pos = (normalizedNormal * lightingLookupScale.xyz);
    probe_pos = probe_pos / maxNormal + baseLightingCoords;
    
    half4 ambient = tex3D(modelLightingSampler, probe_pos);
    ambient.rgb = pow(2 * ambient.rgb, 2);
    return ambient;
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
    // FIXME: understand this part better and find name for variable
    half2 var_N = max(abs(shadowScale.xz), abs(shadowScale.yw));
    var_N = saturate(8 - var_N);
    half var_H = var_N.x * -var_N.y + var_N.x;
    var_H = (-abs(var_H) >= 0 ? var_N.x : 1);
    half shadowPartAmbient = lerp(shadowPartContribution, ambient, var_N.y);
    half shadow = lerp(shadowContribution, shadowPartAmbient, var_H);
    
    return shadow;
}
#endif

half3 GetNormal(VSOutput inputVx)
{
#if NORMAL_MAP
    half4 normalMap = tex2D(normalMapSampler, inputVx.texcoord);
    normalMap.xy = normalMap.wy * half2(4.07999992, 4.06451607) - half2(2.07999992, 2.06451607); // FIXME: find clean variable
    
    half3 normal = inputVx.wsNormal.xyz;
    normal += normalMap.x * inputVx.texcoord3.xyz;
    normal += normalMap.y * inputVx.texcoord2.xyz;

    return normalize(normal.xyz);
#else
    hreturn = normalize(inputVx.wsNormal.xyz);
#endif
}

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
    half3 normalizedNormal = GetNormal(inputVx);
    half4 light = SampleModelLighting(normalizedNormal, inputVx.baseLightingCoords);

#if SUN
    half3 directionnalLight = GetSunLight(normalizedNormal);
#if SHADOW
    directionnalLight *= SampleShadowMap(inputVx.shadowPos.xy, inputVx.shadowPos.z, light.w);
#endif
    light.rgb += directionnalLight * light.a;
#endif

    half3 fogColor = GetFogColor(inputVx);
#if BLENDED
    fogColor *= diffuse.a;
#endif

    outColor.rgb = lerp(fogColor, diffuse.rgb * light.rgb, inputVx.wsNormal.w);
#if BLENDED
    outColor.a = diffuse.a;
    outColor = ((abs(diffuse.a) == 0.0) ? diffuse : outColor); // no clue why they need this but it's in the asm
#else
    outColor.a = 1;
#endif

    return outColor;
}