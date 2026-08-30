struct Instance
{
    float2 position;
    float2 size;
    float4 color;
};

[[vk::binding(0, 0)]]
StructuredBuffer<Instance> instances;

struct VSInput
{
    uint vertexId : SV_VertexID;
    uint instanceId : SV_InstanceID;
};

struct VSOutput
{
    float4 position : SV_Position;
    float4 color : COLOR0;
};

[shader("vertex")]
VSOutput vertexMain(VSInput input)
{
    Instance instance = instances[input.instanceId];

    float2 vertices[6] =
    {
        float2(0, 0),
        float2(1, 0),
        float2(0, 1),
        float2(1, 0),
        float2(1, 1),
        float2(0, 1)
    };
    
     float2 pixelPosition =
            instance.position +
            vertices[input.vertexId] * instance.size;
    
        float2 clipPosition = float2(
            pixelPosition.x / 1920.0 * 2.0 - 1.0,
            1.0 - pixelPosition.y / 1080.0 * 2.0
        );

    //float2 position = instance.position + vertices[input.vertexId] * instance.size;

    VSOutput output;
    output.position = float4(clipPosition, 0, 1);
    output.color = instance.color;

    return output;
}
