extern float4x4 worldMatrix;
extern float4 depthFromClip : register(c23);
extern float4x4 viewProjectionMatrix;

struct VSInput
{

	float4 position : POSITION;
};


struct VSOutput
{

	float4 position : POSITION;
	float texcoord : TEXCOORD0;
};


VSOutput VSMain(VSInput vin)
{

	VSOutput vout = (VSOutput)0;
	
float4 var_posn = float4(vin.position.x, vin.position.y, vin.position.z, 1);
	
float4 var_B = mul(var_posn, worldMatrix);
	var_posn = mul(var_B, viewProjectionMatrix);

	vout.position = var_posn;
	vout.texcoord = dot(var_posn, depthFromClip);

	return vout;
}


