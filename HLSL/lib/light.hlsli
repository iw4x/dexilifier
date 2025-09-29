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

    half fresnel = 1.0 - abs(dot(viewDir, normal));
    float envIntensity = 1;
#if LIGHT_MAP
    envIntensity = saturate(dot(specularColor.xyz, 10)) * inputVx.color.a;
#endif
    specularColor *= lerp(envMapParms.x * envIntensity, envMapParms.y * envIntensity, pow(abs(fresnel), envMapParms.z));

    half3 reflectionVector = reflect(viewDir, normal);

    half roughnessLOD = 6 - (specularMap.a * 8);
    half4 reflection = texCUBElod(reflectionProbeSampler, half4(reflectionVector, roughnessLOD));
#if PX
    reflection.rgb = pow(abs(reflection.rgb), envMapParms.w);
#else
    reflection.rgb = pow(reflection.rgb, 2);
#endif

#if SUN
    // phong equation reordered (typicaly the reflection vector is the lightDir reflected by the normal and is then used in R dot V)
    half RdotL = dot(reflectionVector, lightPosition.xyz);
    half reflectionIntensity = (exp(specularMap.a * 6.5) + 7.0) * (RdotL - 0.99925);
    
    reflection.rgb += saturate(exp(reflectionIntensity)) * lightSpecular.xyz * sunlightVisibility;
#endif

    specularColor *= reflection.rgb;

    return specularColor;
#else
    return half3(0, 0, 0);
#endif
}

#if SHADOW || HSHADOW
half SampleShadowMap(float2 shadowPos, float bias, float currentShadow)
{
#if SUN
    float4 adjust = sunShadowmapPixelAdjust;
#else
    float4 adjust = spotShadowmapPixelAdjust;
#endif
#if HSHADOW
    half zSample = bias;
#else
    half zSample = 0;
#endif

    float4 shadowSampleNear;
    shadowSampleNear.x = tex2Dlod(shadowmapSamplerSun, float4(shadowPos + adjust.xy, zSample, 0)).r;
    shadowSampleNear.y = tex2Dlod(shadowmapSamplerSun, float4(shadowPos - adjust.xy, zSample, 0)).r;
    shadowSampleNear.z = tex2Dlod(shadowmapSamplerSun, float4(shadowPos + adjust.zw, zSample, 0)).r;
    shadowSampleNear.w = tex2Dlod(shadowmapSamplerSun, float4(shadowPos - adjust.zw, zSample, 0)).r;

#if !HSHADOW
    shadowSampleNear = shadowSampleNear - bias; // bias
    shadowSampleNear = (shadowSampleNear >= 0 ? 1 : 0); // cutout
#endif
    half shadowNearAverage = dot(shadowSampleNear, 0.25); // average
    
    float2 shadowFarCoord = shadowPos * shadowmapSwitchPartition.w + shadowmapSwitchPartition.xy;
    float4 shadowSampleFar;
    shadowSampleFar.x = tex2Dlod(shadowmapSamplerSun, float4(shadowFarCoord + adjust.xy, zSample, 0)).r;
    shadowSampleFar.y = tex2Dlod(shadowmapSamplerSun, float4(shadowFarCoord - adjust.xy, zSample, 0)).r;
    shadowSampleFar.z = tex2Dlod(shadowmapSamplerSun, float4(shadowFarCoord + adjust.zw, zSample, 0)).r;
    shadowSampleFar.w = tex2Dlod(shadowmapSamplerSun, float4(shadowFarCoord - adjust.zw, zSample, 0)).r;
    
#if !HSHADOW
    shadowSampleFar = shadowSampleFar - bias; // bias
    shadowSampleFar = (shadowSampleFar >= 0 ? 1 : 0); // cutout
#endif
    half shadowFarAverage = dot(shadowSampleFar, 0.25); // average

    half4 shadowScale = half4(shadowPos, shadowFarCoord) * shadowmapScale.xyxy + shadowmapScale.zzzw;
    half2 maxShadow = max(abs(shadowScale.xz), abs(shadowScale.yw));
    maxShadow = saturate(8 - maxShadow);
    half shadow = lerp(currentShadow, shadowFarAverage, maxShadow.y);
    // FIXME: understand this part better and find name for variable
    half var_H = lerp(maxShadow.x, 0, maxShadow.y);
    var_H = (-abs(var_H) >= 0 ? maxShadow.x : 1);
    shadow = lerp(shadow, shadowNearAverage, var_H);
    
    return shadow;
}
#endif
#endif