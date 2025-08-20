extern sampler2D shadowmapSamplerSun : register(s5);
extern sampler3D modelLightingSampler : register(s4);
extern sampler2D colorMapSampler : register(s0);
extern float4 sunShadowmapPixelAdjust : register(c5);
extern float4 shadowmapScale : register(c4);
extern float4 fogSunColorLinear : register(c34);
extern float4 fogSunDir : register(c33);
extern float4 fogSunConsts : register(c32);
extern float4 lightingLookupScale : register(c3);
extern float4 shadowmapSwitchPartition : register(c2);
extern float4 lightDiffuse : register(c18);
extern float4 lightPosition : register(c17);
extern float4 fogColorLinear : register(c0);

struct VSOutput
{

	float3 color : COLOR0;
	float2 texcoord : TEXCOORD0;
	float4 texcoord1 : TEXCOORD1;
	float3 texcoord4 : TEXCOORD4;
	float3 texcoord5 : TEXCOORD5;
	float3 texcoord6 : TEXCOORD6;
};


half4 PSMain(VSOutput inputVx) : SV_Target
{

	half4 outColor;
	
half4 var_outrA;
var_outrA.rgb = normalize(inputVx.texcoord5);
	
float4 var_P;
var_P.xyz = normalize(inputVx.texcoord1.xyz);
	
float4 var_outr;
var_outr.w = saturate(dot(lightPosition.xyz, var_P.xyz));
	
float4 var_I;
var_I.zw = inputVx.texcoord4.xy * shadowmapSwitchPartition.w + shadowmapSwitchPartition.xy;
	
float4 var_K;
var_K.xy = var_I.zw + sunShadowmapPixelAdjust.xy;
	var_K.zw = 0;
	
float4 var_shadowmapsun = tex2Dlod(shadowmapSamplerSun, var_K);
	
float4 var_shadowmapsunD;
var_shadowmapsunD.y = var_shadowmapsun.x;
	var_shadowmapsun.xy = var_I.zw + sunShadowmapPixelAdjust.zw;
	var_shadowmapsun.zw = 0;
	
float4 var_shadowmapsunA = tex2Dlod(shadowmapSamplerSun, var_shadowmapsun);
	var_shadowmapsunD.z = var_shadowmapsunA.x;
	var_shadowmapsunA.xy = var_I.zw + sunShadowmapPixelAdjust.zw;
	var_shadowmapsunA.zw = 0;
	
float4 var_shadowmapsunB = tex2Dlod(shadowmapSamplerSun, var_shadowmapsunA);
	var_shadowmapsunD.w = var_shadowmapsunB.x;
	
float4 var_shadowmapsunF;
var_shadowmapsunF.xy = sunShadowmapPixelAdjust.zw + inputVx.texcoord4.xy;
	var_shadowmapsunF.zw = 0;
	
float4 var_shadowmapsunC = tex2Dlod(shadowmapSamplerSun, var_shadowmapsunF);
	
float4 var_shadowmapsunG;
var_shadowmapsunG.w = var_shadowmapsunC.x;
	var_shadowmapsunC.xy = var_I.zw + sunShadowmapPixelAdjust.xy;
	var_shadowmapsunC.zw = 0;
	var_shadowmapsunD = tex2Dlod(shadowmapSamplerSun, var_shadowmapsunC);
	
float4 var_O = var_shadowmapsunD + inputVx.texcoord4.z;
	var_P = (var_O >= 0 ? 1 : 0);
	var_outr.y = dot(var_P.x, 0.25);
	var_outr.z = max(abs(var_P.y), abs(var_P.z));
	var_shadowmapsunB.x = max(abs(var_P.x), var_outr.z);
	var_outr.z = 1 / var_shadowmapsunB.x;
	var_shadowmapsunB.xyz = var_P.xyz * lightingLookupScale.xyz;
	var_P.xyz = var_shadowmapsunB.xyz * var_outr.z + inputVx.texcoord6;
	var_I.xy = inputVx.texcoord4.xy;
	var_outrA = var_I * shadowmapScale.xyxy + shadowmapScale.zzzw;
	var_shadowmapsunB.xy = max(abs(var_outrA.rb), abs(var_outrA.ga));
	var_outrA.rg = saturate(-var_shadowmapsunB.xy + 8);
	
half4 var_modellighting = tex3D(modelLightingSampler, var_P.xyz);
	var_shadowmapsunB.x = lerp(var_outrA.g, var_outr.y, var_modellighting.w);
	
float4 var_C;
var_C.xy = sunShadowmapPixelAdjust.xy + inputVx.texcoord4.xy;
	var_C.zw = 0;
	
float4 var_shadowmapsunE = tex2Dlod(shadowmapSamplerSun, var_C);
	var_shadowmapsunG.y = var_shadowmapsunE.x;
	var_shadowmapsunE.xy = sunShadowmapPixelAdjust.zw + inputVx.texcoord4.xy;
	var_shadowmapsunE.zw = 0;
	var_shadowmapsunF = tex2Dlod(shadowmapSamplerSun, var_shadowmapsunE);
	var_shadowmapsunG.z = var_shadowmapsunF.x;
	
float4 var_A;
var_A.xy = sunShadowmapPixelAdjust.xy + inputVx.texcoord4.xy;
	var_A.zw = 0;
	var_shadowmapsunG = tex2Dlod(shadowmapSamplerSun, var_A);
	
float4 var_G = var_shadowmapsunG + inputVx.texcoord4.z;
	var_outr = (var_G >= 0 ? 1 : 0);
	var_outr.x = dot(var_outr.x, 0.25);
	var_outr.y = var_outrA.r * -var_outrA.g + var_outrA.r;
	var_outr.y = (-abs(var_outr.y) >= 0 ? var_outrA.r : 1);
	var_modellighting.w = lerp(var_outr.y, var_outr.x, var_shadowmapsunB.x);
	var_modellighting.xyz = var_modellighting.xyz + var_modellighting.xyz;
	var_outr.xyz = var_modellighting.xyz * var_modellighting.xyz;
	var_modellighting.xyz = var_outr.w * lightDiffuse.xyz;
	var_outr.w = dot(fogSunDir.xyz, var_outrA.rgb);
	var_outr.w = var_outr.w + fogSunConsts.y;
	var_outr.w = saturate(var_outr.w * fogSunConsts.z);
	var_outrA.rgb = fogColorLinear.xyz;
	var_outrA.rgb += fogSunColorLinear.xyz;
	var_outrA.rgb = var_outr.w * var_outrA.rgb + fogColorLinear.xyz;
	
half4 var_colormap = tex2D(colorMapSampler, inputVx.texcoord);
	var_colormap.rgb *= inputVx.color;
	var_colormap.rgb = var_colormap.rgb * var_colormap.rgb;
	var_outr.xyz = var_modellighting.w * var_modellighting.xyz + var_outr.xyz;
	var_outr.xyz = var_colormap.rgb * var_outr.xyz + -var_outrA.rgb;

	outColor.xyz = inputVx.texcoord1.w * var_outr.xyz + var_outrA.rgb;
	outColor.w = 1;

	return outColor;
}


