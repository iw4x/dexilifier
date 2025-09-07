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
};


struct VSOutput
{

	float4 position : POSITION;
	float4 color : COLOR0;
	float4 texcoord : TEXCOORD0;
	float4 texcoord1 : TEXCOORD1;
};


VSOutput VSMain(VSInput vin)
{

	VSOutput vout = (VSOutput)0;
	
float4 var_tex1 = float4(vin.position.x, vin.position.y, vin.position.z, 1);
	
float4 var_posn = mul(var_tex1, worldMatrix);

	vout.position = mul(var_posn, viewProjectionMatrix);

	var_tex1.x = dot(var_posn.xyz, var_posn.xyz);
	
float4 var_C = vin.normal / float4(127, 127, 127, 255) - float4(1, 1, 1, -1.328125);
	var_tex1.yzw = var_C.www * var_C.xyz;

	vout.texcoord1.xyz = mul(var_tex1.yzw, (float3x3)worldMatrix);

	var_tex1.x = sqrt(var_tex1.x);
	var_tex1.x = var_tex1.x * fogConsts.z + fogConsts.w;
	var_tex1.x = var_tex1.x * LOG2E;
	var_tex1.x = exp(var_tex1.x);
	var_tex1.x = max(var_tex1.x, fogConsts.y);

	vout.texcoord1.w = min(var_tex1.x, 1);
	vout.color = vin.color;
	vout.texcoord = vin.texcoord;

	return vout;
}


