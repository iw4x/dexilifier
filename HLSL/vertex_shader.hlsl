#define VERTEX 1

#include "constants.hlsli"
#include "vertex_declaration.hlsli"
#include "params.vs.hlsli"

float4 GetLookAtVector(float4 vertexPosition)
{
    return mul(float4(vertexPosition.x, vertexPosition.y, vertexPosition.z, 1), worldMatrix);
}

float3 TransformPosition(float4 vertexPosition)
{
    float4 var_tex1 = float4(vertexPosition.x, vertexPosition.y, vertexPosition.z, 1);
    float4 var_texdA = mul(var_tex1, worldMatrix);
    return mul(GetLookAtVector(vertexPosition), viewProjectionMatrix);
}


float GetFogOpacity(float4 vertexPosition)
{
#if DFOG
    var_tex1.x = dot(fogSunDir.xyz, var_texdA.xyz);
	var_tex1.y = dot(var_texdA.xyz, var_texdA.xyz);
	var_tex1.y = rsqrt(var_tex1.y);
	var_tex1.x = var_tex1.x * var_tex1.y - fogSunConsts.y;
	var_tex1.x = saturate(var_tex1.x * fogSunConsts.z);
	var_tex1.y = 1 / var_tex1.y;
	var_tex1.yz = var_tex1.y * fogSunConsts.wx + fogConsts.w; // <= MAYBE WRONG - Could be FogSunConsts.XW instaed
	var_tex1.yz = var_tex1.yz * float2(LOG2E, LOG2E);
	var_tex1.w = exp(var_tex1.z);
	var_tex1.y = exp(var_tex1.y);
	var_tex1.yz = max(var_tex1.yw, fogConsts.yy);
	var_tex1.z = (-var_tex1.y) + var_tex1.z;
    return saturate(var_tex1.x * var_tex1.z + var_tex1.y);;
#else
    float3 viewDirection = GetLookAtVector(vertexPosition).xyz;
    float distance = length(viewDirection);
    float fogAmount = distance * fogConsts.z + fogConsts.w;
    fogAmount = fogAmount * LOG2E;
    
    float expFogAmount = exp(fogAmount);
    expFogAmount = clamp(expFogAmount, 1, fogConsts.y);
    
    return expFogAmount;
#endif
}

float3 GetNormal(float4 inNormal)
{
    float4 var_F = inNormal / float4(127, 127, 127, 255) - float4(1, 1, 1, -1.328125); // -1.32 ??
    float3 norm = var_F.www * var_F.xyz;

    return mul(norm, (float3x3) worldMatrix);
}

float2 GetTexCoord(float4 inputTexCoord)
{
    VSOutput vout = (VSOutput) 0;
	
    float4 varA = 
        inputTexCoord.wywy / float4(4, 4, 128, 128) + 
        inputTexCoord.zxzx / float4(1024, 1024, _128x128x2, _128x128x2);
	
    float4 fractionalOfA = frac(varA);
    varA.zw -= fractionalOfA.zw;
    varA.xy = fractionalOfA.xy * (-0.03125) + fractionalOfA.zw;
	
    float4 var_texd = varA * float4(32, 32, -2, -2) + float4(-15, -15, 1, 1);
    var_texd.zw = var_texd.zw * fractionalOfA.xy + var_texd.zw;
    
    float2 varB = exp(var_texd.xy);

    return var_texd.zw * varB;
}
