extern sampler2D lightmapSamplerSecondary : register(s3);
extern sampler2D lightmapSamplerPrimary : register(s2);
extern sampler2D colorMapSampler : register(s0);
extern float4 fogSunColorLinear : register(c34);
extern float4 fogSunDir : register(c33);
extern float4 fogSunConsts : register(c32);
extern float4 lightDiffuse : register(c18);
extern float4 lightPosition : register(c17);
extern float4 fogColorLinear : register(c0);

struct VSOutput
{

	float3 color : COLOR0;
	float4 texcoord : TEXCOORD0;
	float4 texcoord1 : TEXCOORD1;
	float3 texcoord5 : TEXCOORD5;
};


half4 PSMain(VSOutput inputVx) : SV_Target
{

	half4 outColor;
	
float2 var_A = float2(1, 0.5) * inputVx.texcoord.zw;
	
float4 var_lightmapsecondary = tex2D(lightmapSamplerSecondary, var_A);
	
float4 var_C;
var_C.x = var_lightmapsecondary.w;
	var_C.zw = inputVx.texcoord.zw * float2(1, 0.5) + float2(0, 0.5);
	
half4 var_lightmapsecondaryA = tex2D(lightmapSamplerSecondary, var_C.zw);
	var_C.y = var_lightmapsecondaryA.w;
	var_C.xy = var_C.xy * float2(4.08, 4.064516) - float2(2.08, 2.064516);
	var_lightmapsecondary.w = dot(var_C.xy, var_C.xy) + 1;
	var_lightmapsecondary.w = saturate(rsqrt(var_lightmapsecondary.w));
	var_lightmapsecondary.xyz = var_lightmapsecondaryA.xyz * var_lightmapsecondary.w + var_lightmapsecondary.xyz;
	var_lightmapsecondary.xyz = var_lightmapsecondary.xyz * var_lightmapsecondary.xyz;
	var_C.xyz = normalize(inputVx.texcoord1.xyz);
	var_lightmapsecondary.w = saturate(dot(lightPosition.xyz, var_C.xyz));
	var_C.xyz = var_lightmapsecondary.www * lightDiffuse.xyz;
	
half3 var_lightmapprimary = (tex2D(lightmapSamplerPrimary, inputVx.texcoord.zw)).xyz;
	var_lightmapsecondary.xyz = var_lightmapprimary.r * var_C.xyz + var_lightmapsecondary.xyz;
	
half3 var_colormap = (tex2D(colorMapSampler, inputVx.texcoord.xy)).xyz;
	var_colormap *= inputVx.color;
	var_colormap = var_colormap * var_colormap;
	var_lightmapprimary = normalize(inputVx.texcoord5);
	var_lightmapsecondary.w = dot(fogSunDir.xyz, var_lightmapprimary);
	var_lightmapsecondary.w = var_lightmapsecondary.w + (-fogSunConsts.y);
	var_lightmapsecondary.w = saturate(var_lightmapsecondary.w * fogSunConsts.z);
	var_lightmapprimary = fogColorLinear.xyz;
	var_lightmapprimary += fogSunColorLinear.xyz;
	var_lightmapprimary = var_lightmapsecondary.w * var_lightmapprimary + fogColorLinear.xyz;
	var_lightmapsecondary.xyz = var_colormap * var_lightmapsecondary.xyz + (-var_lightmapprimary);

	outColor.xyz = inputVx.texcoord1.w * var_lightmapsecondary.xyz + var_lightmapprimary;
	outColor.w = 1;

	return outColor;
}


