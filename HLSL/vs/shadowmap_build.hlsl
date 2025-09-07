extern float4x4 worldViewProjectionMatrix;

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

	vout.position = mul(var_posn, worldViewProjectionMatrix);
	vout.position.z = var_posn.x;
	vout.texcoord = var_posn.x;

	return vout;
}


var_posn.x;
	vout.position.z = var_posn.x;
	vout.position.y = dot(var_posn, worldViewProjectionMatrix[1]);
	vout.position.x = dot(var_posn, worldViewProjectionMatrix[0]);

	return vout;
}


