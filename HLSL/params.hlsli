// hard coded sampler registers
extern sampler2D colorMapSampler : register(s0);
extern samplerCUBE reflectionProbeSampler: register(s1);
extern sampler2D lightmapSamplerPrimary: register(s2);
extern sampler2D lightmapSamplerSecondary: register(s3);
// from now on the samplers are placed sequentially based on use
extern sampler3D modelLightingSampler;
#if SHADOW
extern sampler2D shadowmapSamplerSun;
#endif
#if NORMAL_MAP
extern sampler2D normalMapSampler;
#endif
#if SPECULAR
extern sampler2D specularMapSampler;
#endif

// hard coded constant registers
extern float4 fogColorLinear : register(c0);
extern float4 materialColor : register(c1);
extern float4 shadowmapSwitchPartition : register(c2);
extern float4 shadowmapScale : register(c4);
extern float4 lightPosition : register(c17);
extern float4 lightDiffuse : register(c18);
extern float4 lightSpecular : register(c19);
extern float4 fogSunConsts : register(c32);
extern float4 fogSunDir : register(c33);
extern float4 fogSunColorLinear : register(c34);
// from now on the constant registers are placed sequentially based on use
extern float4 lightingLookupScale;
#if SUN
extern float4 sunShadowmapPixelAdjust;
#endif
#if SPOT
extern float4 lightSpotDir;
extern float4 lightSpotFactors;
extern float4 lightFalloffPlacement;
extern float4 spotShadowmapPixelAdjust;
#endif
#if SPECULAR
extern float4 envMapParms;
#endif
extern float4 detailScale;

// Math constant
#define HALF_255 128/255 // 0.501960814
#define LOG2E 1.442695023