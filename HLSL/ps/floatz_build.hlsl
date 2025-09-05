
struct VSOutput
{

	float texcoord : TEXCOORD0;
};


half4 PSMain(VSOutput inputVx) : SV_Target
{

	half4 outColor;

	outColor = inputVx.texcoord.xxxx;

	return outColor;
}


