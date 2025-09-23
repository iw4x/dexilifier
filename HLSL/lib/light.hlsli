#if PIXEL

#if LIGHT_PROBE
half4 SampleModelLighting(half3 normal, half3 baseLightingCoords) {
	half3 absNormal = abs(normal);
	half longestSide = max(absNormal.x, max(absNormal.y, absNormal.z));
    half3 lightingCoords = (normal * lightingLookupScale.xyz) / longestSide + baseLightingCoords;
    return tex3D(modelLightingSampler, lightingCoords);
}
#endif

#if LIGHT_MAP
half3 SampleLightmap(half4 texcoord, half2 normalMap) {
	half4 lightMapSecondary1 = tex2D(lightmapSamplerSecondary, texcoord.zw * float2(1.0, 0.5));
	half4 lightMapSecondary2 = tex2D(lightmapSamplerSecondary, (texcoord.zw * float2(1.0, 0.5)) + float2(0.0, 0.5));

    half2 lightmapNormal = RemapNormal(half2(lightMapSecondary1.a, lightMapSecondary2.a));
	half lightmapIntensity = length(half3(lightmapNormal, 1));

    lightMapSecondary2.rgb /= lightmapIntensity;
#if NORMAL_MAP || DETAIL_NORMAL
    half NdotL = dot(normalMap, lightmapNormal);
	half normalMapIntensity = length(half3(normalMap, 1));
    
    lightMapSecondary1.rgb /= normalMapIntensity;
    lightMapSecondary2.rgb /= normalMapIntensity / (NdotL + 1);
#endif
    return lightMapSecondary1.rgb + lightMapSecondary2.rgb;
}
#endif

#if SUN
half3 GetSunLight(half3 normalizedNormal)
{
    half NdotL = saturate(dot(lightPosition.xyz, normalizedNormal));
    return NdotL * lightDiffuse.rgb;
}
#endif

half3 GetSpecular(VSOutput inputVx, half3 viewDir, half3 normal, half sunlightVisibility)
{
#if SPECULAR
    half4 specularMap = tex2D(specularMapSampler, inputVx.texcoord.xy);

    half3 specularColor = pow(specularMap.rgb, 2);
#if VERTEX_COLOR && BLENDED
    specularColor *= inputVx.color.a;
#endif

    float envIntensity = 1;
#if LIGHT_MAP
    envIntensity = saturate(dot(specularColor.xyz, 10)) * inputVx.color.a;
#endif
    half invVdotN = 1.0 - abs(dot(viewDir, normal));
    specularColor *= lerp(envMapParms.x * envIntensity, envMapParms.y * envIntensity, pow(abs(invVdotN), envMapParms.z));

    half3 reflectionVector = reflect(viewDir, normal);

    half roughnessLOD = 6 - (specularMap.a * 8);
    half4 reflection = texCUBElod(reflectionProbeSampler, half4(reflectionVector, roughnessLOD));
#if PX
    reflection.rgb = pow(abs(reflection.rgb), envMapParms.w);
#else
    reflection.rgb = pow(reflection.rgb, 2);
#endif
    
#if SUN
    // this looks like a fresnel but I could be wrong
    half RdotL = dot(reflectionVector, lightPosition.xyz);
    half fresnel = (exp(specularMap.a * 6.5) + 7.0) * (RdotL - 0.99925);
    
    reflection.rgb += saturate(exp(fresnel)) * lightSpecular.xyz * sunlightVisibility;
#endif

    specularColor *= reflection.rgb;

    return specularColor;
#else
    return half3(0, 0, 0);
#endif
}

#if SHADOW
half SampleShadowMap(float2 shadowPos, float bias, float sunlightVisibility)
{
#if SUN
    float4 adjust = sunShadowmapPixelAdjust;
#else
    float4 adjust = spotShadowmapPixelAdjust;
#endif

    float4 shadowSample;
    shadowSample.x = tex2Dlod(shadowmapSamplerSun, float4(shadowPos + adjust.xy, 0, 0)).r;
    shadowSample.y = tex2Dlod(shadowmapSamplerSun, float4(shadowPos - adjust.xy, 0, 0)).r;
    shadowSample.z = tex2Dlod(shadowmapSamplerSun, float4(shadowPos + adjust.zw, 0, 0)).r;
    shadowSample.w = tex2Dlod(shadowmapSamplerSun, float4(shadowPos - adjust.zw, 0, 0)).r;

    shadowSample = shadowSample - bias; // bias
    shadowSample = (shadowSample >= 0 ? 1 : 0); // cutout
    half shadowContribution = dot(shadowSample, 0.25); // average
    
    float2 shadowPartitionCoord = shadowPos * shadowmapSwitchPartition.w + shadowmapSwitchPartition.xy;
    float4 shadowPartSample;
    shadowPartSample.x = tex2Dlod(shadowmapSamplerSun, float4(shadowPartitionCoord + adjust.xy, 0, 0)).r;
    shadowPartSample.y = tex2Dlod(shadowmapSamplerSun, float4(shadowPartitionCoord - adjust.xy, 0, 0)).r;
    shadowPartSample.z = tex2Dlod(shadowmapSamplerSun, float4(shadowPartitionCoord + adjust.zw, 0, 0)).r;
    shadowPartSample.w = tex2Dlod(shadowmapSamplerSun, float4(shadowPartitionCoord - adjust.zw, 0, 0)).r;
    
    shadowPartSample = shadowPartSample - bias; // bias
    shadowPartSample = (shadowPartSample >= 0 ? 1 : 0); // cutout
    half shadowPartContribution = dot(shadowPartSample, 0.25); // average

    half4 shadowScale = half4(shadowPos, shadowPartitionCoord) * shadowmapScale.xyxy + shadowmapScale.zzzw;
    // FIXME: understand this part better and find name for variable
    half2 var_N = max(abs(shadowScale.xz), abs(shadowScale.yw));
    var_N = saturate(8 - var_N);
    half var_H = var_N.x * -var_N.y + var_N.x;
    var_H = (-abs(var_H) >= 0 ? var_N.x : 1);
    half shadowPartAmbient = lerp(shadowPartContribution, sunlightVisibility, var_N.y);
    half shadow = lerp(shadowContribution, shadowPartAmbient, var_H);
    
    return shadow;
}
#endif
#endif