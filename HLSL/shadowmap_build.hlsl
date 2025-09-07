#if ATEST
extern sampler2D colorMapSampler : register(s0);
#endif

extern float4 shadowmapPolygonOffset : register(c2);
extern float4x4 worldViewProjectionMatrix : register(c0);

struct VSInput
{

    float4 position : POSITION;
#if ATEST
    float4 color : COLOR0;
#endif
};


struct VSOutput
{

    float4 position : POSITION;
    float texcoord : TEXCOORD0;
#if ATEST
    float4 color : COLOR0;
#endif

};


VSOutput VSMain(VSInput vin)
{

    VSOutput vout = (VSOutput) 0;
	
    float4 var_posn = float4(vin.position.x, vin.position.y, vin.position.z, 1);

    vout.position = mul(var_posn, worldViewProjectionMatrix);
    vout.position.z = var_posn.x;
    vout.texcoord = var_posn.x;
    
#if ATEST
    vout.color = vin.color;
#endif
    
    return vout;
}


float PSMain(VSOutput inputVx) : SV_Target
{
    half4 outColor;
	
    float2 var_outr;
    
    var_outr.x = ddx(inputVx.texcoord);
    var_outr.y = ddy(inputVx.texcoord);
    
    float result = abs(var_outr.x) + abs(var_outr.y);
    result = shadowmapPolygonOffset.y * result + shadowmapPolygonOffset.x;

    outColor = result + inputVx.texcoord; // X
    
#if ATEST
    float4 sample = tex2D(colorMapSampler, inputVx.texcoord);
    float test = inputVx.color.a * sample.a - 0.501960814; // Weird constant, supposed to be 0.5f ? For half alpha clip ?
    clip(test);
#endif
    
    return outColor;
}


