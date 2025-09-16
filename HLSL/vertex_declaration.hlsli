struct VSOutput
{
#if VERTEX_COLOR
    float4 color : COLOR0;
#endif
    float2 texcoord : TEXCOORD0;
    float4 wsNormal : TEXCOORD1; // w contain the depth used by the fog interpolation
#if NORMAL_MAP
    float4 texcoord2 : TEXCOORD2; // TODO: find name for this
    float4 texcoord3 : TEXCOORD3;  // TODO: find name for this
#endif
#if SHADOW
    float3 shadowPos : TEXCOORD4;
#endif
    float3 viewDir : TEXCOORD5;
    float3 baseLightingCoords : TEXCOORD6;
};