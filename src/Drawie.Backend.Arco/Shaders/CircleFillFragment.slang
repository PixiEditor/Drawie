struct VSOutput
{
    float4 position : SV_Position;
    float2 normPosition : TEXCOORD0;
    float4 color : COLOR0;
};

[shader("fragment")]
float4 fragmentMain(VSOutput input) : SV_Target
{
    float alpha = input.color.a;
    /*return float4(input.color.r * alpha, input.color.g * alpha, input.color.b * alpha, alpha);*/

    float2 step = step(distance(input.normPosition.xy, float2(0.5)), 0.5);
    return float4(input.color.rgb * step.x, alpha);
}