#define PIXEL 1
//#define LIGHT_PROBE 1           // lp
//#define LIGHT_MAP 1             // lm
//#define SHADOW 1                // _sm
//#define SUN 1                   // _sun
//#define DFOG 1                  // _fog=0 _dfog=1
//#define BLENDED 1               // b0
//#define NORMAL_MAP 1            // n0
//#define SPECULAR 1              // s0
//#define VERTEX_COLOR 1          // _nc=0

#include "lib/constants.hlsli"
#include "lib/registers_ps.hlsli"
#include "lib/vertex_declaration.hlsli"
#include "lib/transform.hlsli"
#include "lib/fog.hlsli"
#include "lib/light.hlsli"

half3 GetNormal(VSOutput inputVx, half2 normalMap)
{
#if NORMAL_MAP
    half3 normal = inputVx.wsNormal.xyz;
    normal += normalMap.x * inputVx.tangent.xyz;
    normal += normalMap.y * inputVx.binormal.xyz;
    return normalize(normal.xyz);
#else
    return normalize(inputVx.wsNormal.xyz);
#endif
}

half3 GetViewDir(VSOutput inputVx)
{
#if SPECULAR || DFOG
    return normalize(inputVx.viewDir);
#else
    return half3(1,0,0);
#endif
}

half4 PSMain(VSOutput inputVx) : SV_Target
{
    half4 diffuse = tex2D(colorMapSampler, inputVx.texcoord);
#if VERTEX_COLOR
    diffuse *= inputVx.color;
    half vtxAlpha = inputVx.color.a;
#else
    half vtxAlpha = 1.0;
#endif
    half inverseAlpha = 1 - diffuse.a;

    half2 normalmap = SampleNormalMap(inputVx.texcoord, diffuse.a);
    half3 normalizedNormal = GetNormal(inputVx, normalmap);
    half3 normalizedViewDir = GetViewDir(inputVx);

#if LIGHT_PROBE
    half4 modelLight = SampleModelLighting(normalizedNormal, inputVx.baseLightingCoords);
    half3 light = pow(2 * modelLight.rgb, 2);
    half sunlightVisibility = modelLight.a;
#else
    half3 lightMap = SampleLightmap(inputVx.texcoord, normalmap);
    half3 light = pow(lightMap, 2);
    half sunlightVisibility = tex2D(lightmapSamplerPrimary, inputVx.texcoord.zw).r;
#endif

#if SUN
    half3 directionnalLight = GetSunLight(normalizedNormal);
#if SHADOW
    directionnalLight *= SampleShadowMap(inputVx.shadowPos.xy, inputVx.shadowPos.z, sunlightVisibility);
#endif
    light += directionnalLight * sunlightVisibility;
#endif
    half3 specular = GetSpecular(inputVx.texcoord.xy, normalizedViewDir, normalizedNormal, sunlightVisibility, vtxAlpha);

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