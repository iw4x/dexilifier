//#define SHADOW 1                // _sm
//#define SUN 1                   // _sun
//#define DFOG 1                // _fog=0 _dfog=1
//#define BLENDED 1               // b0
//#define NORMAL_MAP 1            // n0
//#define SPECULAR 1            // s0
//#define VERTEX_COLOR 1          // _nc=0

#include "vertex_shader.hlsl"

VSOutput VSMain(VSInput vin) {
    VSOutput vout = (VSOutput) 0;

	half4 localPos = float4(vin.position.xyz, 1);
    half4 worldPos = LocalToWorld(localPos);
    vout.position = WorldToClip(worldPos);

#if VERTEX_COLOR
    vout.color = vin.color;
#endif

    vout.texcoord = GetTexCoord(vin.texcoord);

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
    vout.viewDir = float4(worldPos.xyz, 0); // weird but works, need to either rename the variable ou understand what the matrix really represent
#endif
    
    return vout;
}


