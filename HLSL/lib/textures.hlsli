#if PIXEL

float2 RemapNormal(float2 normal) {
    return lerp(half2(-2.07999992, -2.06451607), half2(2, 2), normal);
}

half4 SampleDiffuse(VSOutput inputVx)  {
    half4 diffuse = tex2D(colorMapSampler, inputVx.texcoord);

#if DETAIL
    diffuse.rgb += tex2D(detailMapSampler, inputVx.texcoord.xy * detailScale.xy).rgb - 0.5;
#endif

#if VERTEX_COLOR
    diffuse *= inputVx.color;
#endif

    return diffuse;
}

half2 SampleNormalMap(half4 texcoord, float alpha) {
    half2 normalMap = half2(0, 0);

#if NORMAL_MAP
    normalMap = RemapNormal(tex2D(normalMapSampler, texcoord).wy);
#endif

#if DETAIL_NORMAL
    normalMap += RemapNormal(tex2D(detailMapSampler, texcoord.xy * detailScale.xy).wy);
#endif

#if BLENDED
    normalMap *= alpha;
#endif

    return normalMap;
}

#endif