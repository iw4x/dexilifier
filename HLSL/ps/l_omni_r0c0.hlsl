extern sampler2D attenuationSampler : register(s4);
extern sampler2D colorMapSampler : register(s0);
extern float4 lightDiffuse : register(c18);
extern float4 lightPosition : register(c17);
extern float4 fogColorLinear : register(c0);

struct VSOutput
{

	float3 color : COLOR0;
	float2 texcoord : TEXCOORD0;
	float4 texcoord1 : TEXCOORD1;
	float3 texcoord5 : TEXCOORD5;
};


half4 PSMain(VSOutput inputVx) : SV_Target
{

	half4 outColor;
	
half4 var_colormap = tex2D(colorMapSampler, inputVx.texcoord);
	var_colormap.rgb *= inputVx.color;
	var_colormap.rgb = var_colormap.rgb * var_colormap.rgb;
	
float4 var_outr;
var_outr.rgb = lightPosition.xyz + (-inputVx.texcoord5);
	var_outr.a = dot(var_outr.rgb, var_outr.rgb);
	var_outr.a = rsqrt(var_outr.a);
	var_outr.rgb = var_outr.rgb * var_outr.aaa;
	var_outr.a = 1 / var_outr.a;
	var_outr.a *= saturate(lightPosition.w);
	
half4 var_attenuation = tex2D(attenuationSampler, var_outr.aa);
	var_attenuation.xyz = var_attenuation.xyz * var_attenuation.xyz;
	var_attenuation.xyz *= var_colormap.rgb;
	
half3 var_B = normalize(inputVx.texcoord1.xyz);
	var_outr.r = saturate(dot(var_outr.rgb, var_B));
	var_outr.rgb *= lightDiffuse.xyz;
	var_outr.rgb = var_attenuation.xyz * var_outr.rgb - fogColorLinear.xyz;

	outColor.xyz = inputVx.texcoord1.w * var_outr.rgb + fogColorLinear.xyz;
	outColor.w = 1;

	return outColor;
}


