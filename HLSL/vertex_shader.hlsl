#define VERTEX 1

#include "constants.hlsli"
#include "vertex_declaration.hlsli"
#include "params.vs.hlsli"

float4 LocalToWorld(float4 v) {
    return mul(v, worldMatrix);
}
float4 WorldToClip(float4 v) {
    return mul(v, viewProjectionMatrix);
}
float4 LocalToClip(float4 v) {
    return mul(LocalToWorld(v), viewProjectionMatrix);
}

float3 UnpackNormal(float4 normal) {
    float4 unpacked = (normal - float4(127, 127, 127, -192)) / float4(127, 127, 127, 255);
    unpacked.xyz *= unpacked.w;
    return unpacked;
}

float GetFogOpacity(float3 vertexPosition)
{
	float distance = length(vertexPosition);
#if DFOG
    float4 var_tex1;
	var_tex1.x = dot(vertexPosition, fogSunDir.xyz) * distance - fogSunConsts.y;
	var_tex1.x = saturate(var_tex1.x * fogSunConsts.z);
	var_tex1.yz = fogSunConsts.wx / distance + fogConsts.w; // <= MAYBE WRONG - Could be FogSunConsts.XW instaed
	var_tex1.w = exp(var_tex1.z);
	var_tex1.y = exp(var_tex1.y);
	var_tex1.yz = max(var_tex1.yw, fogConsts.yy);
	var_tex1.z = (-var_tex1.y) + var_tex1.z;
    return saturate(var_tex1.x * var_tex1.z + var_tex1.y);;
#else
    float fogAmount = distance * fogConsts.z + fogConsts.w;    
    float expFogAmount = exp(fogAmount);
    expFogAmount = clamp(expFogAmount, fogConsts.y, 1);
    
    return expFogAmount;
#endif
}

float2 GetTexCoord(float4 inputTexCoord)
{	
    float4 varA = 
        inputTexCoord.wywy / float4(4, 4, 128, 128) + 
        inputTexCoord.zxzx / float4(1024, 1024, _128x128x2, _128x128x2);
    
    float4 fractionalOfA = frac(varA);
    varA.xy = fractionalOfA.zw - fractionalOfA.xy / 32;
    varA.zw -= fractionalOfA.zw;
	
    float4 var_texd = lerp(float4(-15, -15, 1, 1), float4(17, 17, -1, -1), varA);
    var_texd.zw = var_texd.zw * fractionalOfA.xy + var_texd.zw;

    return var_texd.zw * exp2(var_texd.xy);
}
