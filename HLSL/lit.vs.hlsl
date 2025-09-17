//#define SHADOW 1                // _sm
//#define SUN 1                   // _sun
#define DFOG 1                // _fog=0 _dfog=1
//#define BLENDED 1               // b0
//#define NORMAL_MAP 1            // n0
//#define SPECULAR 1            // s0
//#define VERTEX_COLOR 1          // _nc=0

#include "vertex_shader.hlsl"

VSOutput VSMain(VSInput vin)
{
    VSOutput vout = (VSOutput) 0;

    vout.position = TransformPosition(vin.position);
	
    vout.wsNormal.xyz = GetNormal(vin.normal);
    vout.wsNormal.w = GetFogOpacity(vin.position);
    
    vout.texcoord = GetTexCoord(vin.texcoord);
    
    vout.baseLightingCoords = baseLightingCoords.xyz;
    
#if VERTEX_COLOR
    vout.color = vin.color;
#endif
    
#if DFOG
    vout.viewDir = GetLookAtVector(vin.position);
#endif
    
    return vout;
}


