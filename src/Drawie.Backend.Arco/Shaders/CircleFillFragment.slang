import Common;

float linearstep(float edge0, float edge1, float x) {
    return clamp((x - edge0) / (edge1 - edge0), 0.0, 1.0);
}

[shader("fragment")]
float4 fragmentMain(VSOutput input) : SV_Target
{
    float d = distance(input.normPosition, 0.5);
    
    float coverage;
    
    if (input.antiAliasing.x != 0)
    {
        float pixelSize = input.antiAliasing.y;
        d += pixelSize;
        coverage = 1.0 - linearstep(0.5 - pixelSize, 0.5 + pixelSize, d);
    }
    else
    {
        coverage = step(d, 0.5);
    }
    
    float alpha = input.color.a * coverage;
    
    return float4(input.color.rgb, alpha);
}