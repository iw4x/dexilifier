extern sampler2D shadowmapSamplerSun : register(s4);
extern sampler2D lightmapSamplerSecondary : register(s3);
extern sampler2D lightmapSamplerPrimary : register(s2);
extern sampler2D colorMapSampler : register(s0);
static const float4 kA = float4(1, 0.5, 0, 0.25);
static const float4 kB = float4(4.08, 4.064516, -2.08, -2.064516);
static const float4 kC = float4(8, 0, 0, 0);
extern float4 shadowmapScale : register(c4);
extern float4 sunShadowmapPixelAdjust : register(c3);
extern float4 shadowmapSwitchPartition : register(c2);
extern float4 lightDiffuse : register(c18);
extern float4 lightPosition : register(c17);
extern float4 fogColorLinear : register(c0);

struct VSOutput
{

	float4 color : COLOR0;
	float4 texcoord : TEXCOORD0;
	float4 texcoord1 : TEXCOORD1;
	float3 texcoord4 : TEXCOORD4;
};


half4 PSMain(VSOutput inputVx) : SV_Target
{

	half4 outColor;
	
float2 var_A = float2(kA.x, kA.y) * inputVx.texcoord.zw;
	
float4 var_B = tex2D(lightmapSamplerSecondary, var_A);
	
float4 var_C;
var_C.x = var_B.w;
	var_C.zw = inputVx.texcoord.zw * float2(kA.x, kA.y) + float2(kA.z, kA.y);
	
float4 var_D = tex2D(lightmapSamplerSecondary, var_C.zwzw);
	var_C.y = var_D.w;
	var_C.xy = var_C.xy * float2(kB.x, kB.y) + float2(kB.z, kB.w);
	var_B.w = dot(var_C, var_C) + kA;
	var_B.w = saturate(rsqrt(var_B.wwww));
	var_B.xyz = var_D.xyz * var_B.www + var_B.xyz;
	var_B.xyz = var_B.xyz * var_B.xyz;
	var_C.xyz = normalize(inputVx.texcoord1.xyz);
	var_B.w = saturate(dot(lightPosition, var_C));
	
float4 var_E = tex2D(lightmapSamplerPrimary, inputVx.texcoord.zwzw);
	var_E.y = ((-abs(var_E.x)) >= 0 ? kA.z : kA.x);
	var_D.xyz = var_B.www * lightDiffuse.xyz;
	// L27: if_ne r1.y, -r1.y
	
float4 var_F;
var_F.xy = sunShadowmapPixelAdjust.xy + inputVx.texcoord4.xy;
	var_F.zw = float2(kA.z, kA.z);
	
float4 var_G = tex2Dlod(shadowmapSamplerSun, var_F);
	
float4 var_H;
var_H.xy = (-sunShadowmapPixelAdjust.xy) + inputVx.texcoord4.xy;
	var_H.zw = float2(kA.z, kA.z);
	
float4 var_I = tex2Dlod(shadowmapSamplerSun, var_H);
	var_G.y = var_I.x;
	var_I.xy = sunShadowmapPixelAdjust.zw + inputVx.texcoord4.xy;
	var_I.zw = float2(kA.z, kA.z);
	
float4 var_J = tex2Dlod(shadowmapSamplerSun, var_I);
	var_G.z = var_J.x;
	var_J.xy = (-sunShadowmapPixelAdjust.zw) + inputVx.texcoord4.xy;
	var_J.zw = float2(kA.z, kA.z);
	
float4 var_K = tex2Dlod(shadowmapSamplerSun, var_J);
	var_G.w = var_K.x;
	
float4 var_L = var_G + (-inputVx.texcoord4.zzzz);
	
float4 var_M = (var_L >= 0 ? kA : kA);
	var_B.w = dot(var_M, kA);
	var_M.zw = inputVx.texcoord4.xy * shadowmapSwitchPartition.ww + shadowmapSwitchPartition.xy;
	var_K.xy = var_M.zw + sunShadowmapPixelAdjust.xy;
	var_K.zw = float2(kA.z, kA.z);
	
float4 var_N = tex2Dlod(shadowmapSamplerSun, var_K);
	
float4 var_O;
var_O.xy = var_M.zw + (-sunShadowmapPixelAdjust.xy);
	var_O.zw = float2(kA.z, kA.z);
	
float4 var_P = tex2Dlod(shadowmapSamplerSun, var_O);
	var_N.y = var_P.x;
	var_P.xy = var_M.zw + sunShadowmapPixelAdjust.zw;
	var_P.zw = float2(kA.z, kA.z);
	
float4 var_Q = tex2Dlod(shadowmapSamplerSun, var_P);
	var_N.z = var_Q.x;
	var_Q.xy = var_M.zw + (-sunShadowmapPixelAdjust.zw);
	var_Q.zw = float2(kA.z, kA.z);
	
float4 var_R = tex2Dlod(shadowmapSamplerSun, var_Q);
	var_N.w = var_R.x;
	
float4 var_S = var_N + (-inputVx.texcoord4.zzzz);
	
float4 var_T = (var_S >= 0 ? kA : kA);
	var_E.y = dot(var_T, kA);
	var_M.xy = inputVx.texcoord4.xy;
	
float4 var_U = var_M * shadowmapScale.xyxy + shadowmapScale.zzzw;
	var_E.zw = max(abs(var_U.xz), abs(var_U.yw));
	var_E.zw = saturate((-var_E.zw) + float2(kC.x, kC.x));
	var_D.w = lerp(var_E.y, var_E.x, var_E.w);
	var_E.x = var_E.z * (-var_E.w) + var_E.z;
	var_E.x = ((-abs(var_E.x)) >= 0 ? var_E.z : kA.x);
	var_U.x = lerp(var_B.w, var_D.w, var_E.x);
	// L73: else 
	var_U.x = kA.z;
	// L75: endif 
	var_B.xyz = var_U.xxx * var_D.xyz + var_B.xyz;
	
float4 var_V = tex2D(colorMapSampler, inputVx.texcoord);
	
float4 var_W = var_V * inputVx.color;
	var_V.xyz = var_W.xyz * var_W.xyz;
	var_W.xyz *= var_V.xyz;
	var_B.w = var_V.w * (-inputVx.color.a) + kA.x;
	var_W.w = (-var_B.w) + kA.x;
	var_V.xyz = var_W.www * fogColorLinear.xyz;
	var_B.xyz = var_W.xyz * var_B.xyz + (-var_V.xyz);
	var_B.xyz = inputVx.texcoord1.www * var_B.xyz + var_V.xyz;
	var_B.w = var_W.w;

	outColor = ((-abs(var_W.wwww)) >= 0 ? var_W : var_B);

	return outColor;
}


