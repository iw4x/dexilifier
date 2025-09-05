extern sampler2D attenuationSampler : register(s5);
extern sampler3D modelLightingSampler : register(s4);
extern sampler2D colorMapSampler : register(s0);
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
	
float4 var_outrA;
var_outrA.rgb = normalize(inputVx.texcoord5);
	
float4 var_outr;
var_outr.w = dot(fogSunDir.xyz, var_outrA.rgb);
	var_outr.w = var_outr.w + (-fogSunConsts.y);
	var_outr.w = saturate(var_outr.w * fogSunConsts.z);
	var_outrA.rgb = fogColorLinear.xyz;
	var_outrA.rgb += fogSunColorLinear.xyz;
	var_outrA.rgb = var_outr.w * var_outrA.rgb + fogColorLinear.xyz;
	
half4 var_colormap = tex2D(colorMapSampler, inputVx.texcoord);
	var_colormap.rgb *= inputVx.color;
	var_colormap.rgb = var_colormap.rgb * var_colormap.rgb;
	var_outr.xyz = normalize(inputVx.texcoord1.xyz);
	
half var_B = max(abs(var_outr.y), abs(var_outr.z));
	
half var_C = max(abs(var_outr.x), var_B);
	var_outrA.rgb = lightPosition.xyz + (-inputVx.texcoord5);
	var_outr.w = dot(var_outrA.rgb, var_outrA.rgb);
	var_outr.w = rsqrt(var_outr.w);
	var_outrA.rgb = var_outrA.rgb * var_outr.www;
	
half3 var_D = var_outr.xyz * lightingLookupScale.xyz;
	var_outr.x = saturate(dot(var_outrA.rgb, var_outr.xyz));
	var_outr.xyz *= lightDiffuse.xyz;
	var_outrA.a = 1 / var_outr.w;
	var_outr.w = saturate(var_outrA.a * lightPosition.w);
	
half4 var_attenuation = tex2D(attenuationSampler, var_outr.ww);
	var_attenuation.xyz = var_attenuation.xyz * var_attenuation.xyz;
	var_outr.w = 1 / var_C;
	
half3 var_E = var_D * var_outr.w + inputVx.texcoord6;
	
half4 var_modellighting = tex3D(modelLightingSampler, var_E);
	var_attenuation.xyz = var_modellighting.www * var_attenuation.xyz;
	var_modellighting.xyz = var_modellighting.xyz + var_modellighting.xyz;
	var_modellighting.xyz = var_modellighting.xyz * var_modellighting.xyz;
	var_outr.xyz = var_attenuation.xyz * var_outr.xyz + var_modellighting.xyz;
	var_outr.xyz = var_colormap.rgb * var_outr.xyz + (-var_outrA.rgb);

	outColor.xyz = inputVx.texcoord1.w * var_outr.xyz + var_outrA.rgb;
	outColor.w = 1;

	return outColor;
}


