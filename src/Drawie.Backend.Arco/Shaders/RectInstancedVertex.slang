import Common; 

struct Instance 
{ 
    float2 position; 
    float2 size; 
    float4 color; 
    float2 antiAliasing; 
}; 

struct Globals 
{ 
    float2 ViewportSize; 
}
 
StructuredBuffer<Instance> instances; 
ConstantBuffer<Globals> globals; 

struct VSInput 
{
    uint vertexId : SV_VertexID; 
    uint instanceId : SV_InstanceID; 
    uint startInstance : SV_StartInstanceLocation; 
}

[shader("vertex")]
VSOutput vertexMain(VSInput input)
{
    Instance instance = instances[input.instanceId + input.startInstance];

    float2 vertices[6] =
    {
        float2(0, 0),
        float2(1, 0),
        float2(0, 1),
        float2(1, 0),
        float2(1, 1),
        float2(0, 1)
    };

    float aaPixels = 1.0 / instance.size.x;//instance.antiAliasing.y;

    float2 expandedSize = instance.size + aaPixels * 2.0;
    float2 localPosition = vertices[input.vertexId] * expandedSize - aaPixels;
    float2 pixelPosition = instance.position + localPosition;

    float2 clipPosition = float2(
        pixelPosition.x / globals.ViewportSize.x * 2.0 - 1.0,
        1.0 - pixelPosition.y / globals.ViewportSize.y * 2.0
    );

    float2 normPosition = (localPosition + aaPixels) / instance.size;

    VSOutput output;
    output.position = float4(clipPosition, 0, 1);
    output.normPosition = normPosition;
   output.antiAliasing = float2(
       instance.antiAliasing.x,
       aaPixels
   );
    output.color = instance.color;

    return output;
}