using Drawie.Backend.Vertie.Core;

namespace Drawie2Sample;

public class Cube : Mesh
{
    private static readonly float[] Vertices = new float[]
    {
        //X    Y      Z       Normals             U     V
        -1f, -1f, -1f,  0.0f,  0.0f, -1.0f, 0.0f, 1.0f,
        1f, -1f, -1f,  0.0f,  0.0f, -1.0f, 1.0f, 1.0f,
        1f,  1f, -1f,  0.0f,  0.0f, -1.0f, 1.0f, 0.0f,
        1f,  1f, -1f,  0.0f,  0.0f, -1.0f, 1.0f, 0.0f,
        -1f,  1f, -1f,  0.0f,  0.0f, -1.0f, 0.0f, 0.0f,
        -1f, -1f, -1f,  0.0f,  0.0f, -1.0f, 0.0f, 1.0f,

        -1f, -1f,  1f,  0.0f,  0.0f,  1.0f, 0.0f, 1.0f,
        1f, -1f,  1f,  0.0f,  0.0f,  1.0f, 1.0f, 1.0f,
        1f,  1f,  1f,  0.0f,  0.0f,  1.0f, 1.0f, 0.0f,
        1f,  1f,  1f,  0.0f,  0.0f,  1.0f, 1.0f, 0.0f,
        -1f,  1f,  1f,  0.0f,  0.0f,  1.0f, 0.0f, 0.0f,
        -1f, -1f,  1f,  0.0f,  0.0f,  1.0f, 0.0f, 1.0f,

        -1f,  1f,  1f, -1.0f,  0.0f,  0.0f, 0.0f, 1.0f,
        -1f,  1f, -1f, -1.0f,  0.0f,  0.0f, 1.0f, 1.0f,
        -1f, -1f, -1f, -1.0f,  0.0f,  0.0f, 1.0f, 0.0f,
        -1f, -1f, -1f, -1.0f,  0.0f,  0.0f, 1.0f, 0.0f,
        -1f, -1f,  1f, -1.0f,  0.0f,  0.0f, 0.0f, 0.0f,
        -1f,  1f,  1f, -1.0f,  0.0f,  0.0f, 0.0f, 1.0f,

        1f,  1f,  1f,  1.0f,  0.0f,  0.0f, 0.0f, 1.0f,
        1f,  1f, -1f,  1.0f,  0.0f,  0.0f, 1.0f, 1.0f,
        1f, -1f, -1f,  1.0f,  0.0f,  0.0f, 1.0f, 0.0f,
        1f, -1f, -1f,  1.0f,  0.0f,  0.0f, 1.0f, 0.0f,
        1f, -1f,  1f,  1.0f,  0.0f,  0.0f, 0.0f, 0.0f,
        1f,  1f,  1f,  1.0f,  0.0f,  0.0f, 0.0f, 1.0f,

        -1f, -1f, -1f,  0.0f, -1.0f,  0.0f, 0.0f, 1.0f,
        1f, -1f, -1f,  0.0f, -1.0f,  0.0f, 1.0f, 1.0f,
        1f, -1f,  1f,  0.0f, -1.0f,  0.0f, 1.0f, 0.0f,
        1f, -1f,  1f,  0.0f, -1.0f,  0.0f, 1.0f, 0.0f,
        -1f, -1f,  1f,  0.0f, -1.0f,  0.0f, 0.0f, 0.0f,
        -1f, -1f, -1f,  0.0f, -1.0f,  0.0f, 0.0f, 1.0f,

        -1f,  1f, -1f,  0.0f,  1.0f,  0.0f, 0.0f, 1.0f,
        1f,  1f, -1f,  0.0f,  1.0f,  0.0f, 1.0f, 1.0f,
        1f,  1f,  1f,  0.0f,  1.0f,  0.0f, 1.0f, 0.0f,
        1f,  1f,  1f,  0.0f,  1.0f,  0.0f, 1.0f, 0.0f,
        -1f,  1f,  1f,  0.0f,  1.0f,  0.0f, 0.0f, 0.0f,
        -1f,  1f, -1f,  0.0f,  1.0f,  0.0f, 0.0f, 1.0f
    };

    private static readonly uint[] Indices =
    {
        0, 1, 3,
        1, 2, 3
    };
    
    public Cube() : base(Vertices, Indices, Indices.Length)
    {
    }
}