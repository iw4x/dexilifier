extern sampler2D attenuationSampler : register(s5);
extern sampler3D modelLightingSampler : register(s4);
extern sampler2D colorMapSampler : register(s0);
extern float4 lightSpotFactors : register(c6);
extern float4 lightSpotDir : register(c5);
extern float4 fogSunColorLinear : register(c34);
extern float4 fogSunDir : register(c33);
extern float4 fogSunConsts : register(c32);
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
	
half4 var_modellighting;
var_modellighting.rgb = normalize(inputVx.texcoord5);
	
float4 var_outr;
var_outr.xyz = lightPosition.xyz + inputVx.texcoord5;
	var_outr.w = dot(var_outr.xyz, var_outr.xyz);
	var_outr.w = rsqrt(var_outr.w);
	var_outr.xyz = var_outr.xyz * var_outr.w;
	
half var_B = dot(var_outr.xyz, lightSpotDir.xyz);
	
half4 var_attenuation;
var_attenuation.xyz = normalize(inputVx.texcoord1.xyz);
	var_outr.x = saturate(dot(var_outr.xyz, var_attenuation.xyz));
	
half var_C = saturate(var_B * lightSpotFactors.x + lightSpotFactors.y);
	
half var_D = pow(abs(var_C), abs(lightSpotFactors.z));
	
half4 var_E;
var_E.x = (-var_C >= 0 ? 0 : var_D);
	var_outr.w = 1 / var_outr.w;
	var_outr.w *= saturate(lightPosition.w);
	var_attenuation = tex2D(attenuationSampler, var_outr.w);
	var_E.yzw = var_attenuation.xxy * var_attenuation.xxy;
	var_E.xyz = var_E.x * var_E.yzw;
	var_outr.w = max(abs(var_attenuation.y), abs(var_attenuation.z));
	var_E.w = max(abs(var_attenuation.x), var_outr.w);
	var_outr.w = 1 / var_E.w;
	
half3 var_G = var_attenuation.xyz * lightingLookupScale.xyz;
	var_outr.yzw = var_G.xxy * var_outr.w + inputVx.texcoord6.xxy;
	var_modellighting = tex3D(modelLightingSampler, var_outr.yzw);
	var_outr.yzw = var_E.xxy * var_modellighting.a;
	var_E.xyz = var_modellighting.rgb + var_modellighting.rgb;
	var_modellighting.rgb = var_outr.x * lightDiffuse.xyz;
	var_E.xyz = var_E.xyz * var_E.xyz;
	var_outr.xyz = var_outr.yzw * var_modellighting.rgb + var_E.xyz;
	var_outr.w = dot(fogSunDir.xyz, var_modellighting.rgb);
	var_outr.w = var_outr.w + fogSunConsts.y;
	var_outr.w = saturate(var_outr.w * fogSunConsts.z);
	var_modellighting.rgb = fogColorLinear.xyz;
	var_modellighting.rgb += fogSunColorLinear.xyz;
	var_modellighting.rgb = var_outr.w * var_modellighting.rgb + fogColorLinear.xyz;
	
half4 var_colormap = tex2D(colorMapSampler, inputVx.texcoord);
	var_colormap.rgb *= inputVx.color;
	var_colormap.rgb = var_colormap.rgb * var_colormap.rgb;
	var_outr.xyz = var_colormap.rgb * var_outr.xyz + -var_modellighting.rgb;

	outColor.xyz = inputVx.texcoord1.w * var_outr.xyz + var_modellighting.rgb;
	outColor.w = 1;

	return outColor;
}


