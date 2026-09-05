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
    
     float pixelSize = 1 / instance.size.x;
     float grow = instance.size.x * pixelSize + pixelSize;
     float2 vertexPos = vertices[input.vertexId] * grow;
     
     float2 localPosition = vertexPos;
     
     float2 pixelPosition =
            instance.position + vertexPos * instance.size;
    
    float2 clipPosition = float2(
        pixelPosition.x / globals.ViewportSize.x * 2.0 - 1.0,
        1.0 - pixelPosition.y / globals.ViewportSize.y * 2.0
    );

    float2 uv = localPosition;

    VSOutput output;
    output.position = float4(clipPosition, 0, 1);
    output.normPosition = uv;
   output.antiAliasing = float2(
       instance.antiAliasing.x,
       pixelSize
   );
    output.color = instance.color;

    return output;
}