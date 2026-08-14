struct VertexOutput
{
    float4 position : SV_Position;
};

[shader("vertex")]
VertexOutput VSMain(uint vertexId : SV_VertexID)
{
    float2 positions[3] =
    {
        float2(-0.5, -0.5),
        float2( 0.5, -0.5),
        float2( 0.0,  0.5)
    };

    VertexOutput output;
    output.position = float4(positions[vertexId], 0.0, 1.0);

    return output;
}