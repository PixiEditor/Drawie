struct FragmentInput
{
    float3 fNormal    : TEXCOORD0;
    float3 fPos       : TEXCOORD1;
    float2 fTexCoords : TEXCOORD2;
};

[[vk::binding(1, 0)]]
Sampler2D<float4> uTexture;

[shader("fragment")]
float4 PSMain(FragmentInput input) : SV_Target
{
    return uTexture.Sample(input.fTexCoords);
}