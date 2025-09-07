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

	float4 color : COLOR0;
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
var_C.r = var_lightmapsecondary.a;
	var_C.ba = inputVx.texcoord.zw * float2(1, 0.5) + float2(0, 0.5);
	
half4 var_lightmapsecondaryA = tex2D(lightmapSamplerSecondary, var_C.ba);
	var_C.g = var_lightmapsecondaryA.w;
	var_C.rg = var_C.rg * float2(4.08, 4.064516) - float2(2.08, 2.064516);
	var_lightmapsecondary.a = dot(var_C.rg, var_C.rg) + 1;
	var_lightmapsecondary.a = saturate(rsqrt(var_lightmapsecondary.a));
	var_lightmapsecondary.rgb = var_lightmapsecondaryA.xyz * var_lightmapsecondary.a + var_lightmapsecondary.rgb;
	var_lightmapsecondary.rgb = var_lightmapsecondary.rgb * var_lightmapsecondary.rgb;
	var_C.rgb = normalize(inputVx.texcoord1.xyz);
	var_lightmapsecondary.a = saturate(dot(lightPosition.xyz, var_C.rgb));
	var_C.rgb = var_lightmapsecondary.aaa * lightDiffuse.xyz;
	
half var_lightmapprimary = (tex2D(lightmapSamplerPrimary, inputVx.texcoord.zw)).x;
	var_lightmapsecondary.rgb = var_lightmapprimary * var_C.rgb + var_lightmapsecondary.rgb;
	var_C.rgb = normalize(inputVx.texcoord5);
	var_lightmapsecondary.a = dot(fogSunDir.xyz, var_C.rgb);
	var_lightmapsecondary.a = var_lightmapsecondary.a + (-fogSunConsts.y);
	var_lightmapsecondary.a = saturate(var_lightmapsecondary.a * fogSunConsts.z);
	var_C.rgb = fogColorLinear.xyz;
	var_C.rgb += fogSunColorLinear.xyz;
	var_C.rgb = var_lightmapsecondary.a * var_C.rgb + fogColorLinear.xyz;
	
half4 var_colormap = tex2D(colorMapSampler, inputVx.texcoord.xy);
	var_lightmapsecondary.a = var_colormap.a * (-inputVx.color.a) + 1;
	
half4 var_G = var_colormap * inputVx.color;
	
half4 var_outr;
var_outr.w = (-var_lightmapsecondary.a) + 1;
	var_C.rgb = var_C.rgb * var_outr.www;
	var_G.rgb = var_G.rgb * var_G.rgb;
	var_outr.xyz = var_G.aaa * var_G.rgb;
	var_lightmapsecondary.rgb = var_outr.xyz * var_lightmapsecondary.rgb + (-var_C.rgb);
	var_lightmapsecondary.rgb = inputVx.texcoord1.w * var_lightmapsecondary.rgb + var_C.rgb;
	var_lightmapsecondary.a = var_outr.w;

	outColor = ((-abs(var_outr.wwww)) >= 0 ? var_outr : var_lightmapsecondary);

	return outColor;
}


