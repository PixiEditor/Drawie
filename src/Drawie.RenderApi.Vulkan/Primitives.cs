using Drawie.RenderApi.Vulkan.Structs;
using Silk.NET.Maths;

namespace Drawie.RenderApi.Vulkan;

public static class Primitives
{
    public static Vertex[] Vertices = new Vertex[]
    {
        new()
        {
            Position = new Vector2D<float>(-1f, -1f), Color = new Vector3D<float>(1.0f, 0.0f, 0.0f),
            TexCoord = new Vector2D<float>(0.0f, 0.0f)
        },
        new()
        {
            Position = new Vector2D<float>(1f, -1f), Color = new Vector3D<float>(0.0f, 1.0f, 0.0f),
            TexCoord = new Vector2D<float>(1.0f, 0.0f)
        },
        new()
        {
            Position = new Vector2D<float>(1f, 1f), Color = new Vector3D<float>(0.0f, 0.0f, 1.0f),
            TexCoord = new Vector2D<float>(1.0f, 1.0f)
        },
        new()
        {
            Position = new Vector2D<float>(-1f, 1f), Color = new Vector3D<float>(1.0f, 1.0f, 1.0f),
            TexCoord = new Vector2D<float>(0.0f, 1.0f)
        }
    };

    public static ushort[] Indices = new ushort[]
    {
        0, 1, 2, 2, 3, 0
    };
}