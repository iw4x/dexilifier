//#define SHADOW 1                // _sm
//#define SUN 1                   // _sun
//#define DFOG 1                // _fog=0 _dfog=1
//#define BLENDED 1               // b0
//#define NORMAL_MAP 1            // n0
//#define SPECULAR 1            // s0
//#define VERTEX_COLOR 1          // _nc=0

#define FRAGMENT 1

#include "constants.hlsli"
#include "params.hlsli"
#include "vertex_declaration.hlsli"

half3 GetFogColor(half3 viewDir)
{
#if DFOG
    half sunFogVal = dot(fogSunDir.xyz, viewDir);
    sunFogVal = sunFogVal - fogSunConsts.y;
    sunFogVal = saturate(sunFogVal * fogSunConsts.z);
    
    return lerp(fogColorLinear.rgb, fogSunColorLinear.rgb, sunFogVal);
#else
    return fogColorLinear.rgb;
#endif
}

half4 SampleModelLighting(half3 normalizedNormal, half3 baseLightingCoords)
{
    half maxNormal = max(abs(normalizedNormal.x), max(abs(normalizedNormal.y), abs(normalizedNormal.z)));
    
    half3 probe_pos = (normalizedNormal * lightingLookupScale.xyz);
    probe_pos = probe_pos / maxNormal + baseLightingCoords;
    
    return tex3D(modelLightingSampler, probe_pos);
}

#if SHADOW
half SampleShadowMap(float2 shadowPos, float bias, float ambient)
{
    float4 shadowSample;
    shadowSample.x = tex2Dlod(shadowmapSamplerSun, float4(shadowPos + sunShadowmapPixelAdjust.xy, 0, 0)).r;
    shadowSample.y = tex2Dlod(shadowmapSamplerSun, float4(shadowPos - sunShadowmapPixelAdjust.xy, 0, 0)).r;
    shadowSample.z = tex2Dlod(shadowmapSamplerSun, float4(shadowPos + sunShadowmapPixelAdjust.zw, 0, 0)).r;
    shadowSample.w = tex2Dlod(shadowmapSamplerSun, float4(shadowPos - sunShadowmapPixelAdjust.zw, 0, 0)).r;

    shadowSample = shadowSample - bias; // bias
    shadowSample = (shadowSample >= 0 ? 1 : 0); // cutout
    half shadowContribution = dot(shadowSample, 0.25); // average
    
    float2 shadowPartitionCoord = shadowPos * shadowmapSwitchPartition.w + shadowmapSwitchPartition.xy;
    float4 shadowPartSample;
    shadowPartSample.x = tex2Dlod(shadowmapSamplerSun, float4(shadowPartitionCoord + sunShadowmapPixelAdjust.xy, 0, 0)).r;
    shadowPartSample.y = tex2Dlod(shadowmapSamplerSun, float4(shadowPartitionCoord - sunShadowmapPixelAdjust.xy, 0, 0)).r;
    shadowPartSample.z = tex2Dlod(shadowmapSamplerSun, float4(shadowPartitionCoord + sunShadowmapPixelAdjust.zw, 0, 0)).r;
    shadowPartSample.w = tex2Dlod(shadowmapSamplerSun, float4(shadowPartitionCoord - sunShadowmapPixelAdjust.zw, 0, 0)).r;
    
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

half3 GetNormal(VSOutput inputVx, float alpha)
{
#if NORMAL_MAP
    half2 normalIntensity = lerp(half2(-2.07999992, -2.06451607), half2(2, 2), tex2D(normalMapSampler, inputVx.texcoord).wy);
#if BLENDED
    normalIntensity *= alpha;
#endif

    half3 normal = inputVx.wsNormal.xyz;

    normal += normalIntensity.x * inputVx.tangent.xyz;
    normal += normalIntensity.y * inputVx.binormal.xyz;

    return normalize(normal.xyz);
#else
    return normalize(inputVx.wsNormal.xyz);
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

half3 GetSpecular(half2 texcoord, half3 viewDir, half3 normal, half specularIntensity)
{
#if SPECULAR
    half4 specularMap = tex2D(specularMapSampler, texcoord);
    half3 specularColor = pow(specularMap.rgb, 2);
    half invVdotN = 1.0 - abs(dot(viewDir, normal));
    specularColor *= lerp(envMapParms.x, envMapParms.y, pow(invVdotN, envMapParms.z));

    half3 reflectionVector = reflect(viewDir, normal);

    half roughnessLOD = 6 - (specularMap.a * 8);
    half4 reflection = texCUBElod(reflectionProbeSampler, half4(reflectionVector, roughnessLOD));
    reflection.rgb = pow(reflection.rgb, 2);
    
#if SUN
    // this looks like a fresnel but I could be wrong
    half RdotL = dot(reflectionVector, lightPosition.xyz);
    half fresnel = (exp(specularMap.a * 6.5) + 7.0) * (RdotL - 0.99925) * LOG2E;
    
    reflection.rgb += saturate(exp2(fresnel)) * lightSpecular.xyz * specularIntensity;
#endif

    specularColor *= reflection.rgb;

    return specularColor;
#else
    return half3(0, 0, 0);
#endif
}

half4 PSMain(VSOutput inputVx) : SV_Target
{
    half4 diffuse = tex2D(colorMapSampler, inputVx.texcoord);
#if VERTEX_COLOR
    diffuse *= inputVx.color;
#endif
    half inverseAlpha = 1 - diffuse.a;

    half3 normalizedNormal = GetNormal(inputVx, diffuse.a);
    half3 normalizedViewDir = normalize(inputVx.viewDir);

    half4 modelLight = SampleModelLighting(normalizedNormal, inputVx.baseLightingCoords);
    half3 specular = GetSpecular(inputVx.texcoord.xy, normalizedViewDir, normalizedNormal, modelLight.w);

    half3 light = pow(2 * modelLight.rgb, 2);
#if SUN
    half3 directionnalLight = GetSunLight(normalizedNormal);
#if SHADOW
    directionnalLight *= SampleShadowMap(inputVx.shadowPos.xy, inputVx.shadowPos.z, modelLight.w);
#endif
    light += directionnalLight * modelLight.a;
#endif

    diffuse.rgb = pow(diffuse.rgb, 2); // pseudo gamma correction
#if BLENDED
    diffuse.rgb *= diffuse.a; // premultiplication
#endif

    half3 fogColor = GetFogColor(normalizedViewDir);
#if BLENDED
    diffuse.a = 1 - inverseAlpha; // no idea they need that but it's in the asm (maybe it's used by a particular variant at some point)
    fogColor *= diffuse.a;
#endif

    half4 outColor;
    outColor.rgb = lerp(fogColor, diffuse.rgb * light + specular, inputVx.wsNormal.w);
#if BLENDED
    outColor.a = diffuse.a;
    outColor = ((abs(diffuse.a) == 0.0) ? diffuse : outColor); // no clue why they need this but it's in the asm
#else
    outColor.a = 1;
#endif

    return outColor;
}