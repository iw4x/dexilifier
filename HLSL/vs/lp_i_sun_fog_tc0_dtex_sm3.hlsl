#define LOG2E 1.442695
extern float4x4 worldMatrix;
extern float4 fogConsts : register(c21);
extern float4x4 viewProjectionMatrix;

struct VSInput
{

	float4 position : POSITION;
	float4 color : COLOR0;
	float4 texcoord : TEXCOORD0;
	float4 normal : NORMAL;
	float4 texcoord3 : TEXCOORD3;
};


struct VSOutput
{

	float4 position : POSITION;
	float4 color : COLOR0;
	float2 texcoord : TEXCOORD0;
	float4 texcoord1 : TEXCOORD1;
	float3 texcoord6 : TEXCOORD6;
};


VSOutput VSMain(VSInput vin)
{

	VSOutput vout = (VSOutput)0;
	
float4 var_tex1 = float4(vin.position.x, vin.position.y, vin.position.z, 1);
	
float4 var_posn = mul(var_tex1, worldMatrix);

	vout.position = mul(var_posn, viewProjectionMatrix);

	
float4 var_A = (1 / float4(1024, 1024, 32768, 32768)) * vin.texcoord.zxzx;
	
float4 var_B = vin.texcoord.wywy * float4(0.25, 0.25, 0.0078125, 0.0078125) + var_A;
	var_posn = frac(var_B);
	var_B.xy = var_posn.xy * (-0.03125) + var_posn.zw;
	var_B.zw -= var_posn.zw;
	
float4 var_texd = var_B * float4(32, 32, -2, -2) + float4(-15, -15, 1, 1);
	var_texd.zw = var_texd.zw * var_posn.xy + var_texd.zw;
	var_posn.xy = exp(var_texd.xy);

	vout.texcoord = var_texd.zw * var_posn.xy;

	var_tex1.x = length(var_posn.xyz);
	var_tex1.x = var_tex1.x * fogConsts.z + fogConsts.w;
	var_tex1.x *= LOG2E;
	var_tex1.x = exp(var_tex1.x);
	var_tex1.x = max(var_tex1.x, fogConsts.y);

	vout.texcoord1.w = min(var_tex1.x, 1);

	
float4 var_F = vin.normal / float4(127, 127, 127, 255) + (1 / float4(-1, -1, -1, 1.328125));
	var_tex1.yzw = var_F.www * var_F.xyz;

	vout.texcoord1.xyz = mul(var_tex1.yzw, (float3x3)worldMatrix);
	vout.color = vin.color;
	vout.texcoord6 = (1 / float3(256, 256, 256)) * vin.texcoord3.xyz;

	return vout;
}


1.yzw, worldMatrix[1].xyz);
	vout.texcoord1.x = dot(var_tex1.yzw, worldMatrix[0].xyz);
	vout.color = vin.color;
	vout.texcoord6 = (1 / 256) * vin.texcoord3.xyz;

	return vout;
}


