#define LOG2E 1.442695
extern float4x4 worldMatrix;
extern float4 fogConsts : register(c21);
extern float4x4 viewProjectionMatrix;

struct VSInput
{

	float4 position : POSITION;
	float4 color : COLOR0;
	float4 texcoord : TEXCOORD0;
};


struct VSOutput
{

	float4 position : POSITION;
	float4 color : COLOR0;
	float3 texcoord : TEXCOORD0;
};


VSOutput VSMain(VSInput vin)
{

	VSOutput vout = (VSOutput)0;
	
float4 var_texdB = float4(vin.position.x, vin.position.y, vin.position.z, 1);
	
float4 var_A = (1 / float4(1024, 1024, 32768, 32768)) * vin.texcoord.zxzx;
	
float4 var_B = vin.texcoord.wywy * float4(0.25, 0.25, 0.0078125, 0.0078125) + var_A;
	
float4 var_texdA = frac(var_B);
	var_B.zw = var_B.xy + -var_texdA.xy;
	var_B.xy = var_texdA.xy * 0.03125 + var_texdA.zw;
	
float4 var_texd = var_B * float4(32, 32, -2, -2) + float4(-15, -15, 1, 1);
	var_texd.zw = var_texd.xy * var_texdA.xy + var_texd.xy;
	var_texdA.xy = exp(var_texd.xy);

	vout.texcoord.xy = var_texd.zw * var_texdA.xy;

	var_texdA = mul(var_texdB, worldMatrix);

	vout.position = mul(var_texdA, viewProjectionMatrix);

	var_texdB.x = dot(var_texdA.xyz, var_texdA.xyz);
	var_texdB.x = rsqrt(var_texdB.x);
	var_texdB.x = 1 / var_texdB.x;
	var_texdB.x = var_texdB.x * fogConsts.z + fogConsts.w;
	var_texdB.x = var_texdB.x * LOG2E;
	var_texdB.x = exp(var_texdB.x);
	var_texdB.x = max(var_texdB.x, fogConsts.y);

	vout.texcoord.z = min(var_texdB.x, 1);
	vout.color = vin.color;

	return vout;
}


