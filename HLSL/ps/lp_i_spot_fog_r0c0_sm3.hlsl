extern sampler2D attenuationSampler : register(s5);
extern sampler3D modelLightingSampler : register(s4);
extern sampler2D colorMapSampler : register(s0);
extern float4 lightSpotFactors : register(c6);
extern float4 lightSpotDir : register(c5);
extern float4 lightingLookupScale : register(c3);
extern float4 lightDiffuse : register(c18);
extern float4 lightPosition : register(c17);
extern float4 fogColorLinear : register(c0);

struct VSOutput
{

	float3 color : COLOR0;
	float2 texcoord : TEXCOORD0;
	float4 texcoord1 : TEXCOORD1;
	float3 texcoord5 : TEXCOORD5;
	float3 texcoord6 : TEXCOORD6;
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
	
half var_B = dot(var_outr.rgb, lightSpotDir.xyz);
	
half4 var_attenuation;
var_attenuation.xyz = normalize(inputVx.texcoord1.xyz);
	var_outr.r = saturate(dot(var_outr.rgb, var_attenuation.xyz));
	var_outr.a = max(abs(var_attenuation.y), abs(var_attenuation.z));
	
half4 var_E;
var_E.w = max(abs(var_attenuation.x), var_outr.a);
	
half var_C = saturate(var_B * lightSpotFactors.x + lightSpotFactors.y);
	
half var_D = pow(abs(var_C), abs(lightSpotFactors.z));
	var_E.x = ((-var_C) >= 0 ? 0 : var_D);
	var_outr.a = 1 / var_outr.a;
	var_outr.a *= saturate(lightPosition.w);
	var_attenuation = tex2D(attenuationSampler, var_outr.aa);
	var_E.yzw = var_attenuation.xyz * var_attenuation.xyz;
	var_E.xyz = var_E.xxx * var_E.yzw;
	var_outr.a = 1 / var_E.w;
	
half3 var_G = var_attenuation.xyz * lightingLookupScale.xyz;
	var_outr.gba = var_G * var_outr.a + inputVx.texcoord6;
	
half4 var_modellighting = tex3D(modelLightingSampler, var_outr.gba);
	var_outr.gba = var_E.xyz * var_modellighting.www;
	var_E.xyz = var_modellighting.xyz + var_modellighting.xyz;
	var_modellighting.xyz = var_outr.rrr * lightDiffuse.xyz;
	var_E.xyz = var_E.xyz * var_E.xyz;
	var_outr.rgb = var_outr.gba * var_modellighting.xyz + var_E.xyz;
	var_outr.rgb = var_colormap.rgb * var_outr.rgb - fogColorLinear.xyz;

	outColor.xyz = inputVx.texcoord1.w * var_outr.rgb + fogColorLinear.xyz;
	outColor.w = 1;

	return outColor;
}


