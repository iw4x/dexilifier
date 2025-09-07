extern sampler2D normalMapSampler : register(s4);
extern float4 debugBumpmap : register(c3);

struct VSOutput
{

	float2 texcoord : TEXCOORD0;
	float3 texcoord1_pp : TEXCOORD1_PP0;
	float3 texcoord2_pp : TEXCOORD2_PP0;
	float3 texcoord3_pp : TEXCOORD3_PP0;
};


half4 PSMain(VSOutput inputVx) : SV_Target
{

	half4 outColor;
	
float4 var_normalmap = tex2D(normalMapSampler, inputVx.texcoord);
	var_normalmap.xy = var_normalmap.wy * float2(4.08, 4.064516) - float2(2.08, 2.064516);
	
half3 var_B = inputVx.texcoord1_pp;
	var_normalmap.xzw = var_normalmap.x * var_B + inputVx.texcoord3_pp;
	var_normalmap.xyz = var_normalmap.y * inputVx.texcoord2_pp + var_normalmap.xzw;
	
half3 var_C = normalize(var_normalmap.xyz);
	var_normalmap.xyz = debugBumpmap.yyy * inputVx.texcoord2_pp;
	var_normalmap.xyz = inputVx.texcoord1_pp * debugBumpmap.x + var_normalmap.xyz;
	var_normalmap.xyz = inputVx.texcoord3_pp * debugBumpmap.z + var_normalmap.xyz;
	var_normalmap.xyz = var_C * debugBumpmap.w + var_normalmap.xyz;
	var_normalmap.xyz = var_normalmap.xyz + float3(1, 1, 1);

	outColor.xyz = var_normalmap.xyz * float3(0.5, 0.5, 0.5);
	outColor.w = 1;

	return outColor;
}


