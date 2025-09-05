extern float4 shadowmapPolygonOffset : register(c2);

struct VSOutput
{

	float texcoord : TEXCOORD0;
};


half4 PSMain(VSOutput inputVx) : SV_Target
{

	half4 outColor;
	
float2 var_outr;
var_outr.x = ddx(inputVx.texcoord);
	var_outr.y = ddy(inputVx.texcoord);
	var_outr.x = abs(var_outr.x) + abs(var_outr.y);
	var_outr.x = shadowmapPolygonOffset.y * var_outr.x + shadowmapPolygonOffset.x;

	outColor = var_outr.xxxx + inputVx.texcoord.xxxx;

	return outColor;
}


