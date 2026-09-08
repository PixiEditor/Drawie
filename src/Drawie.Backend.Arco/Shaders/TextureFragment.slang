import Common;

[[vk::binding(0, 1)]]
Sampler2D<float4> texture;

[shader("fragment")]
float4 fragmentMain(VSOutput input) : SV_Target
{
    return texture.Sample(input.normPosition);
}