#define LOG2E 1.442695
extern float4 baseLightingCoords : register(c8);
extern float4x4 worldMatrix;
extern float4 fogSunDir : register(c33);
extern float4 fogSunConsts : register(c32);
extern float4 fogConsts : register(c21);
extern float4x4 viewProjectionMatrix;

struct VSInput
{

	float4 position : POSITION;
	float4 color : COLOR0;
	float4 texcoord : TEXCOORD0;
	float4 normal : NORMAL;
};


struct VSOutput
{

	float4 position : POSITION;
	float4 color : COLOR0;
	float2 texcoord : TEXCOORD0;
	float4 texcoord1 : TEXCOORD1;
	float3 texcoord5 : TEXCOORD5;
	float3 texcoord6 : TEXCOORD6;
};


VSOutput VSMain(VSInput vin)
{

	VSOutput vout = (VSOutput)0;
	
float4 var_E = float4(vin.position.x, vin.position.y, vin.position.z, 1);
	
float4 var_posn = mul(var_E, worldMatrix);

	vout.position = mul(var_posn, viewProjectionMatrix);

	
float4 var_tex1 = vin.normal / float4(127, 127, 127, 255) + (1 / float4(-1, -1, -1, 1.328125));
	var_tex1.xyz = var_tex1.www * var_tex1.xyz;

	vout.texcoord1.xyz = mul(var_tex1.xyz, (float3x3)worldMatrix);

	var_tex1.x = dot(fogSunDir.xyz, var_posn.xyz);
	var_tex1.y = dot(var_posn.xyz, var_posn.xyz);
	var_tex1.y = rsqrt(var_tex1.y);
	var_tex1.x = var_tex1.x * var_tex1.y - fogSunConsts.y;
	var_tex1.x = saturate(var_tex1.x * fogSunConsts.z);
	var_tex1.y = 1 / var_tex1.y;
	var_posn.xw = fogSunConsts.xw;
	var_tex1.yz = var_tex1.y * var_posn.wx + fogConsts.w;
	var_tex1.yz = var_tex1.yz * float2(LOG2E, LOG2E);
	var_tex1.y = exp(var_tex1.y);
	var_tex1.w = exp(var_tex1.z);
	var_tex1.yz = max(var_tex1.yw, fogConsts.yy);
	var_tex1.z = (-var_tex1.y) + var_tex1.z;

	vout.texcoord1.w = saturate(var_tex1.x * var_tex1.z + var_tex1.y);
	vout.texcoord5 = var_posn.xyz;

	
float4 var_A = (1 / float4(1024, 1024, 32768, 32768)) * vin.texcoord.zxzx;
	
float4 var_B = vin.texcoord.wywy * float4(0.25, 0.25, 0.0078125, 0.0078125) + var_A;
	var_posn = frac(var_B);
	var_B.xy = var_posn.xy * (-0.03125) + var_posn.zw;
	var_B.zw -= var_posn.zw;
	
float4 var_texd = var_B * float4(32, 32, -2, -2) + float4(-15, -15, 1, 1);
	var_texd.zw = var_texd.zw * var_posn.xy + var_texd.zw;
	var_posn.xy = exp(var_texd.xy);

	vout.texcoord = var_texd.zw * var_posn.xy;
	vout.texcoord6 = baseLightingCoords.xyz;
	vout.color = vin.color;

	return vout;
}


Matrix[0].xyz);
	vout.texcoord6 = baseLightingCoords.xyz;
	vout.color = vin.color;

	return vout;
}


