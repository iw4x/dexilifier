extern float4x4 worldMatrix;
extern float4x4 viewProjectionMatrix;

struct VSInput
{

	float4 position : POSITION;
	float4 texcoord : TEXCOORD0;
	float4 normal : NORMAL;
	float4 texcoord1 : TEXCOORD1;
};


struct VSOutput
{

	float4 position : POSITION;
	float2 texcoord : TEXCOORD0;
	float3 texcoord1 : TEXCOORD1;
	float3 texcoord2 : TEXCOORD2;
	float3 texcoord3 : TEXCOORD3;
};


VSOutput VSMain(VSInput vin)
{

	VSOutput vout = (VSOutput)0;
	
float4 var_A = float4(vin.position.x, vin.position.y, vin.position.z, 1);
	
float4 var_posn = mul(var_A, worldMatrix);

	vout.position = mul(var_posn, viewProjectionMatrix);

	
float4 var_C = vin.normal / float4(127, 127, 127, 255) - float4(1, 1, 1, -1.328125);
	var_C.xyz = var_C.www * var_C.xyz;
	var_posn.xyz = mul(var_C.xyz, (float3x3)worldMatrix);
	
float4 var_tex2 = vin.texcoord1 / float4(127, 127, 127, 255) - float4(1, 1, 1, -1.328125);
	var_tex2.xyz = var_tex2.www * var_tex2.xyz;
	
float3 var_tex1 = mul(var_tex2.xyz, (float3x3)worldMatrix);
	var_tex2.xyz = var_posn.zxy * var_tex1.yzx;
	var_tex2.xyz = var_posn.yzx * var_tex1.zxy + (-var_tex2.xyz);

	vout.texcoord3 = var_posn.xyz;
	vout.texcoord1 = var_tex1;
	vout.texcoord2 = var_tex2.xyz * vin.position.www;
	vout.texcoord = vin.texcoord.xy;

	return vout;
}


