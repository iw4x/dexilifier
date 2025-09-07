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
	
float4 var_texd = float4(vin.position.x, vin.position.y, vin.position.z, 1);
	
float4 var_posn = mul(var_texd, worldMatrix);

	vout.position = mul(var_posn, viewProjectionMatrix);

	var_texd.x = length(var_posn.xyz);
	var_texd.x = var_texd.x * fogConsts.z + fogConsts.w;
	var_texd.x = var_texd.x * LOG2E;
	var_texd.x = exp(var_texd.x);
	var_texd.x = max(var_texd.x, fogConsts.y);

	vout.texcoord.z = min(var_texd.x, 1);
	vout.color = vin.color;
	vout.texcoord.xy = vin.texcoord.xy;

	return vout;
}


