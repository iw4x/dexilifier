struct VSInput
{
    float4 position : POSITION;
    float4 color : COLOR0;
    float4 texcoord : TEXCOORD0;
    float4 normal : NORMAL;
    float4 tangent : TEXCOORD2;
};

struct VSOutput
{
    float4 position : POSITION;
    
#if VERTEX_COLOR
    float4 color : COLOR0;
#endif
    
    float4 texcoord : TEXCOORD0;
    
    float4 wsNormal : TEXCOORD1; // w contain the depth used by the fog interpolation
    
#if NORMAL_MAP || DETAIL_NORMAL
    float3 binormal : TEXCOORD2;
    float3 tangent : TEXCOORD3;
#endif
    
#if SHADOW || HSHADOW
    float3 shadowPos : TEXCOORD4;
#endif

#if DFOG || SPECULAR
    float3 viewDir : TEXCOORD5;
#endif
    float3 baseLightingCoords : TEXCOORD6;
};