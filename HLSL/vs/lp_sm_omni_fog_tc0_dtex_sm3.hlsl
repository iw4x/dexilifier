#define LOG2E 1.442695
extern float4 baseLightingCoords : register(c8);
extern float4x4 worldMatrix;
extern float4x4 shadowLookupMatrix;
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
	float4 texcoord4 : TEXCOORD4;
	float3 texcoord5 : TEXCOORD5;
	float3 texcoord6 : TEXCOORD6;
};


VSOutput VSMain(VSInput vin)
{

	VSOutput vout = (VSOutput)0;
	
float4 var_E = float4(vin.position.x, vin.position.y, vin.position.z, 1);
	
float4 var_tex4 = mul(var_E, worldMatrix);

	vout.texcoord4 = mul(var_tex4, shadowLookupMatrix);
	vout.position = mul(var_tex4, viewProjectionMatrix);
	vout.texcoord5 = var_tex4.xyz;

	
float4 var_A = (1 / float4(1024, 1024, 32768, 32768)) * vin.texcoord.zxzx;
	
float4 var_B = vin.texcoord.wywy * float4(0.25, 0.25, 0.0078125, 0.0078125) + var_A;
	var_tex4 = frac(var_B);
	var_B.xy = var_tex4.xy * (-0.03125) + var_tex4.zw;
	var_B.zw -= var_tex4.zw;
	
float4 var_texd = var_B * float4(32, 32, -2, -2) + float4(-15, -15, 1, 1);
	var_texd.zw = var_texd.zw * var_tex4.xy + var_texd.zw;
	var_tex4.xy = exp(var_texd.xy);

	vout.texcoord = var_texd.zw * var_tex4.xy;

	
float4 var_tex1 = vin.normal / float4(127, 127, 127, 255) + (1 / float4(-1, -1, -1, 1.328125));
	var_tex1.xyz = var_tex1.www * var_tex1.xyz;

	vout.texcoord1.xyz = mul(var_tex1.xyz, (float3x3)worldMatrix);

	var_tex1.x = length(var_tex4.xyz);
	var_tex1.x = var_tex1.x * fogConsts.z + fogConsts.w;
	var_tex1.x = var_tex1.x * LOG2E;
	var_tex1.x = exp(var_tex1.x);
	var_tex1.x = max(var_tex1.x, fogConsts.y);

	vout.texcoord1.w = min(var_tex1.x, 1);
	vout.texcoord6 = baseLightingCoords.xyz;
	vout.color = vin.color;

	return vout;
}


t.texcoord1.y = dot(var_tex1.xyz, worldMatrix[1].xyz);
	vout.texcoord1.x = dot(var_tex1.xyz, worldMatrix[0].xyz);
	vout.texcoord6 = baseLightingCoords.xyz;
	vout.color = vin.color;

	return vout;
}


