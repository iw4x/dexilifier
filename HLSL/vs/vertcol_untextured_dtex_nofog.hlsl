extern float4x4 worldViewProjectionMatrix;

struct VSInput
{

	float4 position : POSITION;
	float4 color : COLOR0;
};


struct VSOutput
{

	float4 position : POSITION;
	float4 color : COLOR0;
};


VSOutput VSMain(VSInput vin)
{

	VSOutput vout = (VSOutput)0;
	
float4 var_posn = float4(vin.position.x, vin.position.y, vin.position.z, 1);

	vout.position = mul(var_posn, worldViewProjectionMatrix);
	vout.color = vin.color;

	return vout;
}


