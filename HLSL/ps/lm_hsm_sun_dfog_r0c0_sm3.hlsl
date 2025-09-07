extern sampler2D shadowmapSamplerSun : register(s4);
extern sampler2D lightmapSamplerSecondary : register(s3);
extern sampler2D lightmapSamplerPrimary : register(s2);
extern sampler2D colorMapSampler : register(s0);
static const float4 kA = float4(1, 0.5, 0, 0.25);
static const float4 kB = float4(4.08, 4.064516, -2.08, -2.064516);
static const float4 kC = float4(8, 0, 0, 0);
extern float4 shadowmapScale : register(c4);
extern float4 fogSunColorLinear : register(c34);
extern float4 fogSunDir : register(c33);
extern float4 fogSunConsts : register(c32);
extern float4 sunShadowmapPixelAdjust : register(c3);
extern float4 shadowmapSwitchPartition : register(c2);
extern float4 lightDiffuse : register(c18);
extern float4 lightPosition : register(c17);
extern float4 fogColorLinear : register(c0);

struct VSOutput
{

	float3 color : COLOR0;
	float4 texcoord : TEXCOORD0;
	float4 texcoord1 : TEXCOORD1;
	float3 texcoord4 : TEXCOORD4;
	float3 texcoord5 : TEXCOORD5;
};


half4 PSMain(VSOutput inputVx) : SV_Target
{

	half4 outColor;

	outColor.w = kA.x;

	
float4 var_A;
var_A.xyz = normalize(inputVx.texcoord1.xyz);
	var_A.x = saturate(dot(lightPosition, var_A.xyz));
	var_A.yz = float2(kA.x, kA.y) * inputVx.texcoord.zw;
	
float4 var_B = tex2D(lightmapSamplerSecondary, var_A.yzzw);
	var_A.y = var_B.w;
	
float2 var_C = inputVx.texcoord.zw * float2(kA.x, kA.y) + float2(kA.z, kA.y);
	
float4 var_D = tex2D(lightmapSamplerSecondary, var_C);
	var_A.z = var_D.w;
	var_A.yz = var_A.yz * float2(kB.x, kB.y) + float2(kB.z, kB.w);
	var_A.y = dot(var_A.yzzw, var_A.yzzw) + kA;
	var_A.y = saturate(rsqrt(var_A.yyyy));
	var_A.yzw = var_D.xyz * var_A.yyy + var_B.xyz;
	var_A.yzw = var_A.yzw * var_A.yzw;
	
float4 var_E = tex2D(lightmapSamplerPrimary, inputVx.texcoord.zwzw);
	var_E.y = ((-abs(var_E.x)) >= 0 ? kA.z : kA.x);
	var_D.xyz = var_A.xxx * lightDiffuse.xyz;
	// L29: if_ne r1.y, -r1.y
	
float4 var_F;
var_F.xy = sunShadowmapPixelAdjust.xy + inputVx.texcoord4.xy;
	var_F.zw = float2(kA.x, kA.z) * inputVx.texcoord4.zz;
	
float4 var_G = tex2Dlod(shadowmapSamplerSun, var_F);
	
float4 var_H;
var_H.xy = (-sunShadowmapPixelAdjust.xy) + inputVx.texcoord4.xy;
	var_H.zw = float2(kA.x, kA.z) * inputVx.texcoord4.zz;
	
float4 var_I = tex2Dlod(shadowmapSamplerSun, var_H);
	var_G.y = var_I.x;
	var_I.xy = sunShadowmapPixelAdjust.zw + inputVx.texcoord4.xy;
	var_I.zw = float2(kA.x, kA.z) * inputVx.texcoord4.zz;
	
float4 var_J = tex2Dlod(shadowmapSamplerSun, var_I);
	var_G.z = var_J.x;
	var_J.xy = (-sunShadowmapPixelAdjust.zw) + inputVx.texcoord4.xy;
	var_J.zw = float2(kA.x, kA.z) * inputVx.texcoord4.zz;
	
float4 var_K = tex2Dlod(shadowmapSamplerSun, var_J);
	var_G.w = var_K.x;
	var_A.x = dot(var_G, kA);
	var_G.zw = inputVx.texcoord4.xy * shadowmapSwitchPartition.ww + shadowmapSwitchPartition.xy;
	var_K.xy = var_G.zw + sunShadowmapPixelAdjust.xy;
	var_K.zw = float2(kA.x, kA.z) * inputVx.texcoord4.zz;
	
float4 var_L = tex2Dlod(shadowmapSamplerSun, var_K);
	
float4 var_M;
var_M.xy = var_G.zw + (-sunShadowmapPixelAdjust.xy);
	var_M.zw = float2(kA.x, kA.z) * inputVx.texcoord4.zz;
	
float4 var_N = tex2Dlod(shadowmapSamplerSun, var_M);
	var_L.y = var_N.x;
	var_N.xy = var_G.zw + sunShadowmapPixelAdjust.zw;
	var_N.zw = float2(kA.x, kA.z) * inputVx.texcoord4.zz;
	
float4 var_O = tex2Dlod(shadowmapSamplerSun, var_N);
	var_L.z = var_O.x;
	var_O.xy = var_G.zw + (-sunShadowmapPixelAdjust.zw);
	var_O.zw = float2(kA.x, kA.z) * inputVx.texcoord4.zz;
	
float4 var_P = tex2Dlod(shadowmapSamplerSun, var_O);
	var_L.w = var_P.x;
	var_E.y = dot(var_L, kA);
	var_G.xy = inputVx.texcoord4.xy;
	
float4 var_Q = var_G * shadowmapScale.xyxy + shadowmapScale.zzzw;
	var_E.zw = max(abs(var_Q.xz), abs(var_Q.yw));
	var_E.zw = saturate((-var_E.zw) + float2(kC.x, kC.x));
	var_D.w = lerp(var_E.y, var_E.x, var_E.w);
	var_E.x = var_E.z * (-var_E.w) + var_E.z;
	var_E.x = ((-abs(var_E.x)) >= 0 ? var_E.z : kA.x);
	var_Q.x = lerp(var_A.x, var_D.w, var_E.x);
	// L71: else 
	var_Q.x = kA.z;
	// L73: endif 
	var_A.xyz = var_Q.xxx * var_D.xyz + var_A.yzw;
	var_E.xyz = normalize(inputVx.texcoord5);
	var_A.w = dot(fogSunDir, var_E);
	var_A.w = var_A.w + (-fogSunConsts.y);
	var_A.w = saturate(var_A.w * fogSunConsts.z);
	
float4 var_R = tex2D(colorMapSampler, inputVx.texcoord);
	var_R.xyz *= inputVx.color;
	var_R.xyz = var_R.xyz * var_R.xyz;
	var_D.xyz = fogColorLinear.xyz;
	var_D.xyz += fogSunColorLinear.xyz;
	var_D.xyz = var_A.www * var_D.xyz + fogColorLinear.xyz;
	var_A.xyz = var_R.xyz * var_A.xyz + (-var_D.xyz);

	outColor.xyz = inputVx.texcoord1.www * var_A.xyz + var_D.xyz;

	return outColor;
}


