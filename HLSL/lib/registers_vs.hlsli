extern float4x4 viewProjectionMatrix : register(c0);
extern float4x4 worldMatrix : register(c4);

#if AMBIENT
extern float4 lightprobeAmbient : register(c8);
#else
extern float4 baseLightingCoords : register(c8);
#endif
extern float4 uvAnimParms : register(c20);
extern float4 fogConsts : register(c21);
extern float4 gameTime : register(c22);
extern float4 fogSunConsts : register(c32);
extern float4 fogSunDir : register(c33);
extern float4x4 shadowLookupMatrix : register(c24);
