using Drawie.Backend.Vertie.Core;

namespace Drawie2Sample;

public class Cube : Mesh
{
    private static readonly float[] Vertices =
    {
        // position
        0.0f,  0.8f, 0.0f,
        -0.8f, -0.8f, 0.0f,
        0.8f, -0.8f, 0.0f
    };

    private static readonly uint[] Indices =
    {
        0, 1, 2,
    }; 
    
    public Cube() : base(Vertices, Indices, Indices.Length)
    {
    }
}