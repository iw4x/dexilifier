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
	
float4 var_E = float4(vin.position.x, vin.position.y, vin.position.z, 1);
	
float4 var_A = (1 / float4(1024, 1024, 32768, 32768)) * vin.texcoord.zxzx;
	
float4 var_B = vin.texcoord.wywy * float4(0.25, 0.25, 0.0078125, 0.0078125) + var_A;
	
float4 var_texdA = frac(var_B);
	var_B.zw = var_B.xy + -var_texdA.xy;
	var_B.xy = var_texdA.xy * 0.03125 + var_texdA.zw;
	
float4 var_texd = var_B * float4(32, 32, -2, -2) + float4(-15, -15, 1, 1);
	var_texd.zw = var_texd.xy * var_texdA.xy + var_texd.xy;
	var_texdA.xy = exp(var_texd.xy);

	vout.texcoord = var_texd.zw * var_texdA.xy;

	var_texdA = mul(var_E, worldMatrix);

	vout.position.z = dot(var_texdA, viewProjectionMatrix[2]);
	vout.position.y = dot(var_texdA, viewProjectionMatrix[1]);
	vout.position.x = dot(var_texdA, viewProjectionMatrix[0]);

	
float4 var_tex2 = vin.texcoord1 / float4(127, 127, 127, 255) - float4(1, 1, 1, -1.328125);
	var_tex2.xyz = var_tex2.w * var_tex2.xyz;
	
float4 var_tex1 = mul(var_tex2, worldMatrix);

	vout.texcoord1 = var_tex1.xyz;

	
float4 var_F = vin.normal / float4(127, 127, 127, 255) - float4(1, 1, 1, -1.328125);
	var_F.xyz = var_F.w * var_F.xyz;

	vout.position.w = dot(var_texdA, viewProjectionMatrix[3]);

	var_texdA.x = dot(var_F.xyz, worldMatrix[0].xyz);
	var_texdA.y = dot(var_F.xyz, worldMatrix[1].xyz);
	var_texdA.z = dot(var_F.xyz, worldMatrix[2].xyz);

	vout.texcoord3 = var_texdA.xyz;

	var_tex2.xyz = var_texdA.zxy * var_tex1.yzx;
	var_tex2.xyz = var_texdA.yzx * var_tex1.zxy + -var_tex2.xyz;

	vout.texcoord2 = var_tex2.xyz * vin.position.w;

	return vout;
}


