#if VERTEX
// Vertex shader functions
float GetFogOpacity(float3 viewDirection)
{
	float distance = length(viewDirection); // distance from camera
#if DFOG
    float4 var_tex1;
	var_tex1.x = dot(viewDirection, fogSunDir.xyz) * distance - fogSunConsts.y;
	var_tex1.x = saturate(var_tex1.x * fogSunConsts.z);
	var_tex1.yz = fogSunConsts.wx / distance + fogConsts.w; // <= MAYBE WRONG - Could be FogSunConsts.XW instaed
	var_tex1.w = exp(var_tex1.z);
	var_tex1.y = exp(var_tex1.y);
	var_tex1.yz = max(var_tex1.yw, fogConsts.yy);
	var_tex1.z = var_tex1.z - var_tex1.y;
    return saturate(var_tex1.x * var_tex1.z + var_tex1.y);;
#else
    float fogAmount = distance * fogConsts.z + fogConsts.w;    
    float expFogAmount = exp(fogAmount);
    expFogAmount = clamp(expFogAmount, fogConsts.y, 1);
    
    return expFogAmount;
#endif
}
#endif

// Pixel shader functions
#if PIXEL
half3 GetFogColor(half3 viewDir)
{
#if DFOG
    half sunFogVal = dot(fogSunDir.xyz, viewDir);
    sunFogVal = sunFogVal - fogSunConsts.y;
    sunFogVal = saturate(sunFogVal * fogSunConsts.z);
    
    return lerp(fogColorLinear.rgb, fogSunColorLinear.rgb, sunFogVal);
#else
    return fogColorLinear.rgb;
#endif
}
#endif