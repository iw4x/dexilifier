#define VERTEX 1
//#define SHADOW 1                // _sm
//#define SUN 1                   // _sun
//#define DFOG 1                  // _fog=0 _dfog=1
//#define BLENDED 1               // b0
//#define NORMAL_MAP 1            // n0
//#define SPECULAR 1              // s0
//#define VERTEX_COLOR 1          // _nc=0

#include "lib/constants.hlsli"
#include "lib/registers_vs.hlsli"
#include "lib/vertex_declaration.hlsli"
#include "lib/fog.hlsli"
#include "lib/transform.hlsli"

VSOutput VSMain(VSInput vin) {
    VSOutput vout = (VSOutput) 0;

	half4 localPos = float4(vin.position.xyz, 1);
    half4 worldPos = LocalToWorld(localPos);
    vout.position = WorldToClip(worldPos);

#if VERTEX_COLOR
    vout.color = vin.color;
#endif

    vout.texcoord.xy = GetTexCoord(vin.texcoord);
#if LIGHT_MAP
    //vout.texcoord.zw
#endif

    float3 localNormal = UnpackNormal(vin.normal);
    float3 worldNormal = LocalToWorld(float4(localNormal.xyz, 0));
    
#if NORMAL_MAP
    float3 localTangent = UnpackNormal(vin.tangent);
    float3 worldTangent = LocalToWorld(float4(localTangent.xyz, 0));
    float3 binormal = cross(worldNormal, worldTangent);

    vout.binormal = binormal * vin.position.w;
    vout.tangent = worldTangent;
#endif

    vout.wsNormal.xyz = worldNormal;
    vout.wsNormal.w = GetFogOpacity(worldPos.xyz);
    
    vout.baseLightingCoords = baseLightingCoords.xyz;
    
#if DFOG || SPECULAR
    // the world is centered around the player so they use the worldPos directly without substracting the camera pos
    vout.viewDir = float4(worldPos.xyz, 0);
#endif
    
    return vout;
}


