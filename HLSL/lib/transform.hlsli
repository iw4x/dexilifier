#if VERTEX

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

#endif
