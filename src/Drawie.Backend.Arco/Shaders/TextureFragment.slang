import Common;

[[vk::binding(0, 1)]]
Sampler2D<float4> textures[];

[shader("fragment")]
float4 fragmentMain(VSOutput input) : SV_Target
{
    return textures[input.textureIndex].Sample(input.normPosition);
}