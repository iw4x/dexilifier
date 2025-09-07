extern sampler2D lightmapSamplerSecondary : register(s3);
extern sampler2D lightmapSamplerPrimary : register(s2);
extern sampler2D colorMapSampler : register(s0);
extern float4 lightDiffuse : register(c18);
extern float4 lightPosition : register(c17);
extern float4 fogColorLinear : register(c0);

struct VSOutput
{

	float4 color : COLOR0;
	float4 texcoord : TEXCOORD0;
	float4 texcoord1 : TEXCOORD1;
};


half4 PSMain(VSOutput inputVx) : SV_Target
{

	half4 outColor;
	
float2 var_A = float2(1, 0.5) * inputVx.texcoord.zw;
	
float4 var_lightmapsecondary = tex2D(lightmapSamplerSecondary, var_A);
	
float4 var_C;
var_C.x = var_lightmapsecondary.a;
	var_C.zw = inputVx.texcoord.zw * float2(1, 0.5) + float2(0, 0.5);
	
half4 var_lightmapsecondaryA = tex2D(lightmapSamplerSecondary, var_C.zw);
	var_C.y = var_lightmapsecondaryA.w;
	var_C.xy = var_C.xy * float2(4.08, 4.064516) - float2(2.08, 2.064516);
	var_lightmapsecondary.a = dot(var_C.xy, var_C.xy) + 1;
	var_lightmapsecondary.a = saturate(rsqrt(var_lightmapsecondary.a));
	var_lightmapsecondary.rgb = var_lightmapsecondaryA.xyz * var_lightmapsecondary.a + var_lightmapsecondary.rgb;
	var_lightmapsecondary.rgb = var_lightmapsecondary.rgb * var_lightmapsecondary.rgb;
	var_C.xyz = normalize(inputVx.texcoord1.xyz);
	var_lightmapsecondary.a = saturate(dot(lightPosition.xyz, var_C.xyz));
	var_C.xyz = var_lightmapsecondary.aaa * lightDiffuse.xyz;
	
half var_lightmapprimary = (tex2D(lightmapSamplerPrimary, inputVx.texcoord.zw)).x;
	var_lightmapsecondary.rgb = var_lightmapprimary * var_C.xyz + var_lightmapsecondary.rgb;
	
half4 var_colormap = tex2D(colorMapSampler, inputVx.texcoord.xy);
	
half4 var_G = var_colormap * inputVx.color;
	var_lightmapsecondary.a = var_colormap.a * (-inputVx.color.a) + 1;
	var_colormap.rgb = var_G.rgb * var_G.rgb;
	var_colormap.rgb = var_G.aaa * var_colormap.rgb;
	var_colormap.a = (-var_lightmapsecondary.a) + 1;
	var_G.rgb = var_colormap.aaa * fogColorLinear.xyz;
	var_lightmapsecondary.rgb = var_colormap.rgb * var_lightmapsecondary.rgb + (-var_G.rgb);
	var_lightmapsecondary.rgb = inputVx.texcoord1.w * var_lightmapsecondary.rgb + var_G.rgb;
	var_lightmapsecondary.a = var_colormap.a;

	outColor = ((-abs(var_colormap.aaaa)) >= 0 ? var_colormap : var_lightmapsecondary);

	return outColor;
}


