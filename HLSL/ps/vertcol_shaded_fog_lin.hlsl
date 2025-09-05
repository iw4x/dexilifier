extern sampler2D colorMapSampler : register(s0);
extern float4 materialColor : register(c1);
extern float4 fogColorLinear : register(c0);

struct VSOutput
{

	float4 color : COLOR0;
	float3 texcoord : TEXCOORD0;
};


half4 PSMain(VSOutput inputVx) : SV_Target
{

	half4 outColor;
	
half4 var_colormap = tex2D(colorMapSampler, inputVx.texcoord.xy);
	
half4 var_outr = var_colormap * inputVx.color;
	
half3 var_C = var_outr.rgb * var_outr.rgb;
	var_outr.rgb = var_outr.rgb * (-var_outr.rgb) + materialColor.xyz;
	var_outr.rgb = materialColor.w * var_outr.rgb + var_C;
	var_outr.rgb -= fogColorLinear.xyz;

	outColor.xyz = inputVx.texcoord.z * var_outr.rgb + fogColorLinear.xyz;
	outColor.w = var_outr.a;

	return outColor;
}


