struct VertexInput
{
    float3 vPos : POSITION;
};

struct VertexOutput
{
    float4 position : SV_Position;
    float3 fPos : TEXCOORD0;
};

[[vk::binding(0, 0)]]
cbuffer Transform
{
    float4x4 uModel;
    float4x4 uView;
    float4x4 uProjection;
};

[shader("vertex")]
VertexOutput VSMain(VertexInput input)
{
    VertexOutput output;

    output.position = mul(mul(mul(uProjection, uView), uModel), float4(input.vPos, 1.0));

    // We want to know the fragment's position in World space, so we multiply ONLY by uModel and not uView or uProjection
    output.fPos = mul(uModel, float4(input.vPos, 1.0)).xyz;

    return output;
}

