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
	
float4 var_posn = mul(var_E, worldMatrix);

	vout.position = mul(var_posn, viewProjectionMatrix);

	
float4 var_tex2 = vin.texcoord1 / float4(127, 127, 127, 255) + (1 / float4(-1, -1, -1, 1.328125));
	var_tex2.xyz = var_tex2.www * var_tex2.xyz;
	
float3 var_tex1 = mul(var_tex2.xyz, (float3x3)worldMatrix);

	vout.texcoord1 = var_tex1;

	
float4 var_F = vin.normal / float4(127, 127, 127, 255) + (1 / float4(-1, -1, -1, 1.328125));
	var_F.xyz = var_F.www * var_F.xyz;
	var_posn.xyz = mul(var_F.xyz, (float3x3)worldMatrix);

	vout.texcoord3 = var_posn.xyz;

	
float4 var_A = (1 / float4(1024, 1024, 32768, 32768)) * vin.texcoord.zxzx;
	
float4 var_B = vin.texcoord.wywy * float4(0.25, 0.25, 0.0078125, 0.0078125) + var_A;
	var_posn = frac(var_B);
	var_B.xy = var_posn.xy * (-0.03125) + var_posn.zw;
	var_B.zw -= var_posn.zw;
	
float4 var_texd = var_B * float4(32, 32, -2, -2) + float4(-15, -15, 1, 1);
	var_texd.zw = var_texd.zw * var_posn.xy + var_texd.zw;
	var_posn.xy = exp(var_texd.xy);

	vout.texcoord = var_texd.zw * var_posn.xy;

	var_tex2.xyz = var_posn.zxy * var_tex1.yzx;
	var_tex2.xyz = var_posn.yzx * var_tex1.zxy + (-var_tex2.xyz);

	vout.texcoord2 = var_tex2.xyz * vin.position.www;

	return vout;
}



	var_texdA.z = dot(var_F.xyz, worldMatrix[2].xyz);

	vout.texcoord3 = var_texdA.xyz;

	var_tex2.xyz = var_texdA.zxy * var_tex1.yzx;
	var_tex2.xyz = var_texdA.yzx * var_tex1.zxy + -var_tex2.xyz;

	vout.texcoord2 = var_tex2.xyz * vin.position.w;

	return vout;
}


