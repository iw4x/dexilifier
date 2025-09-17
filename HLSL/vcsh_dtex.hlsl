#include "vertex_shader.hlsl"

VSOutput VSMain(VSInput vin)
{
    VSOutput vout = (VSOutput) 0;
	
    vout.texcoord.xy = GetTexCoord(vin.texcoord);

    vout.position = LocalToClip(vin.position);

    vout.texcoord.z = GetFogOpacity(vin.position);
    
#if VERTEX_COLOR
    vout.color = vin.color;
#endif
    
    return vout;
}


half4 PSMain(VSOutput inputVx) : SV_Target
{

    half4 outColor;
	
    half4 var_colormap = tex2D(colorMapSampler, inputVx.texcoord.xy);
	
    half4 var_outr = var_colormap * inputVx.color;
	
    half3 var_C = var_outr.rgb * var_outr.rgb;
    var_outr.rgb = var_outr.rgb * (-var_outr.rgb) + materialColor.xyz;
    var_outr.rgb = materialColor.w * var_outr.rgb + var_C;
    var_outr.rgb -= fogColorLinear.xyz;

    outColor.xyz = inputVx.texcoord.z * var_outr.rgb + fogColorLinear.xyz;
    outColor.w = var_outr.a;

    return outColor;
}



