import Common;

[shader("fragment")]
float4 fragmentMain(VSOutput input) : SV_Target
{
    float d = length(input.normPosition - 0.5);
    
    float coverage;
    
    if (input.antiAliasing.x != 0)
    {
        float pixelSize = input.antiAliasing.y;
        d += 0.5 - pixelSize * 0.5;
        coverage = (1.0 - d) / pixelSize;
    }
    else
    {
        coverage = step(d, 0.5);
    }
    
    float alpha = input.color.a * coverage;
    
    return float4(input.color.rgb, alpha);
}