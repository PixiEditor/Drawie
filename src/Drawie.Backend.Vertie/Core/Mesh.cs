using Drawie.RenderApi.Abstraction;
using Drawie.RenderApi.Abstraction.Buffers;

namespace Drawie.Backend.Vertie.Core;

public class Mesh
{
    public Transform Transform { get; } = new Transform();
    public float[] Vertices { get; set; }
    public uint[] Indicies { get; set; }
    public int IndexCount { get; set; }
    
    public Mesh(float[] vertices, uint[] indicies, int indexCount)
    {
        Vertices = vertices;
        Indicies = indicies;
        IndexCount = indexCount;
    }
}