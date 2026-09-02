struct VSOutput
{
    float4 position : SV_Position;
    float4 color : COLOR0;
};

[shader("fragment")]
float4 fragmentMain(VSOutput input) : SV_Target
{
    /*float alpha = input.color.a;
    return float4(input.color.r * alpha, input.color.g * alpha, input.color.b * alpha, alpha);*/
    return input.color;
}