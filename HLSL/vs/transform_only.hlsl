extern float4x4 worldMatrix;
extern float4x4 viewProjectionMatrix;

struct VSInput
{

	float4 position : POSITION;
};


struct VSOutput
{

	float4 position : POSITION;
};


VSOutput VSMain(VSInput vin)
{

	VSOutput vout = (VSOutput)0;
	
float4 var_A = float4(vin.position.x, vin.position.y, vin.position.z, 1);
	
float4 var_posn = mul(var_A, worldMatrix);

	vout.position = mul(var_posn, viewProjectionMatrix);

	return vout;
}


