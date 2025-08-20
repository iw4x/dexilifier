
struct VSOutput
{

	float4 color : COLOR0;
};


half4 PSMain(VSOutput inputVx) : SV_Target
{

	half4 outColor;

	outColor = inputVx.color;

	return outColor;
}


