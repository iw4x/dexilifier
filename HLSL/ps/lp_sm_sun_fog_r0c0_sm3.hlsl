extern sampler2D shadowmapSamplerSun : register(s5);
extern sampler3D modelLightingSampler : register(s4);
extern sampler2D colorMapSampler : register(s0);
extern float4 sunShadowmapPixelAdjust : register(c5);
extern float4 shadowmapScale : register(c4);
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
	float3 texcoord6 : TEXCOORD6;
};


half4 PSMain(VSOutput inputVx) : SV_Target
{

	half4 outColor;
	
half4 var_colormap = tex2D(colorMapSampler, inputVx.texcoord);
	var_colormap.rgb *= inputVx.color;
	var_colormap.rgb = var_colormap.rgb * var_colormap.rgb;
	
float4 var_I;
var_I.zw = inputVx.texcoord4.xy * shadowmapSwitchPartition.w + shadowmapSwitchPartition.xy;
	
float4 var_K;
var_K.xy = var_I.zw + (-sunShadowmapPixelAdjust.xy);
	var_K.zw = float2(0, 0);
	
float4 var_shadowmapsun = tex2Dlod(shadowmapSamplerSun, var_K);
	
float4 var_shadowmapsunC;
var_shadowmapsunC.y = var_shadowmapsun.x;
	var_shadowmapsun.xy = var_I.zw + sunShadowmapPixelAdjust.zw;
	var_shadowmapsun.zw = float2(0, 0);
	
float4 var_shadowmapsunA = tex2Dlod(shadowmapSamplerSun, var_shadowmapsun);
	var_shadowmapsunC.z = var_shadowmapsunA.x;
	var_shadowmapsunA.xy = var_I.zw + (-sunShadowmapPixelAdjust.zw);
	var_shadowmapsunA.zw = float2(0, 0);
	
float4 var_shadowmapsunB = tex2Dlod(shadowmapSamplerSun, var_shadowmapsunA);
	var_shadowmapsunC.w = var_shadowmapsunB.x;
	
float4 var_shadowmapsunF;
var_shadowmapsunF.xy = var_I.zw + sunShadowmapPixelAdjust.xy;
	var_shadowmapsunF.zw = float2(0, 0);
	var_shadowmapsunC = tex2Dlod(shadowmapSamplerSun, var_shadowmapsunF);
	
float4 var_O = var_shadowmapsunC + (-inputVx.texcoord4.zzzz);
	
float4 var_P = (var_O >= 0 ? float4(1, 1, 1, 1) : float4(0, 0, 0, 0));
	
float4 var_outr;
var_outr.g = dot(var_P, float4(0.25, 0.25, 0.25, 0.25));
	var_P.xyz = normalize(inputVx.texcoord1.xyz);
	var_outr.b = max(abs(var_P.y), abs(var_P.z));
	var_shadowmapsunB.x = max(abs(var_P.x), var_outr.b);
	var_outr.b = 1 / var_shadowmapsunB.x;
	var_I.xy = inputVx.texcoord4.xy;
	
half4 var_R = var_I * shadowmapScale.xyxy + shadowmapScale.zzzw;
	var_shadowmapsunB.xy = max(abs(var_R.xz), abs(var_R.yw));
	var_R.xy = saturate((-var_shadowmapsunB.xy) + float2(8, 8));
	var_outr.a = saturate(dot(lightPosition.xyz, var_P.xyz));
	var_shadowmapsunB.xyz = var_P.xyz * lightingLookupScale.xyz;
	var_P.xyz = var_shadowmapsunB.xyz * var_outr.b + inputVx.texcoord6;
	
half4 var_modellighting = tex3D(modelLightingSampler, var_P.xyz);
	var_shadowmapsunB.x = lerp(var_outr.g, var_modellighting.w, var_R.y);
	
float4 var_C;
var_C.xy = (-sunShadowmapPixelAdjust.xy) + inputVx.texcoord4.xy;
	var_C.zw = float2(0, 0);
	
float4 var_shadowmapsunD = tex2Dlod(shadowmapSamplerSun, var_C);
	
float4 var_shadowmapsunG;
var_shadowmapsunG.y = var_shadowmapsunD.x;
	var_shadowmapsunD.xy = sunShadowmapPixelAdjust.zw + inputVx.texcoord4.xy;
	var_shadowmapsunD.zw = float2(0, 0);
	
float4 var_shadowmapsunE = tex2Dlod(shadowmapSamplerSun, var_shadowmapsunD);
	var_shadowmapsunG.z = var_shadowmapsunE.x;
	var_shadowmapsunE.xy = (-sunShadowmapPixelAdjust.zw) + inputVx.texcoord4.xy;
	var_shadowmapsunE.zw = float2(0, 0);
	var_shadowmapsunF = tex2Dlod(shadowmapSamplerSun, var_shadowmapsunE);
	var_shadowmapsunG.w = var_shadowmapsunF.x;
	
float4 var_A;
var_A.xy = sunShadowmapPixelAdjust.xy + inputVx.texcoord4.xy;
	var_A.zw = float2(0, 0);
	var_shadowmapsunG = tex2Dlod(shadowmapSamplerSun, var_A);
	
float4 var_G = var_shadowmapsunG + (-inputVx.texcoord4.zzzz);
	var_outr = (var_G >= 0 ? float4(1, 1, 1, 1) : float4(0, 0, 0, 0));
	var_outr.r = dot(var_outr, float4(0.25, 0.25, 0.25, 0.25));
	var_outr.g = var_R.x * (-var_R.y) + var_R.x;
	var_outr.g = ((-abs(var_outr.g)) >= 0 ? var_R.x : 1);
	var_modellighting.w = lerp(var_outr.r, var_shadowmapsunB.x, var_outr.g);
	var_modellighting.xyz = var_modellighting.xyz + var_modellighting.xyz;
	var_outr.rgb = var_modellighting.xyz * var_modellighting.xyz;
	var_modellighting.xyz = var_outr.aaa * lightDiffuse.xyz;
	var_outr.rgb = var_modellighting.w * var_modellighting.xyz + var_outr.rgb;
	var_outr.rgb = var_colormap.rgb * var_outr.rgb - fogColorLinear.xyz;

	outColor.xyz = inputVx.texcoord1.w * var_outr.rgb + fogColorLinear.xyz;
	outColor.w = 1;

	return outColor;
}


