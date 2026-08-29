struct VSOutput
{
    float4 position : SV_Position;
    float4 color : COLOR0;
};

[shader("fragment")]
float4 fragmentMain(VSOutput input) : SV_Target
{
    return input.color;
}