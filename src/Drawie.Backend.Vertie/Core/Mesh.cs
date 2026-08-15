using System.Numerics;
using Drawie.Backend.Vertie.Rendering;
using Drawie.RenderApi.Abstraction;
using Drawie.RenderApi.Abstraction.Buffers;

namespace Drawie.Backend.Vertie.Core;

public class Mesh
{
    public Transform Transform { get; } = new Transform();
    public IReadOnlyList<Vector3> Vertices => vertices;
    public IReadOnlyList<Vector3> Normals => normals;
    public IReadOnlyList<Vector2> TexCoords => texCoords;
    public IReadOnlyList<uint> Indicies => indicies;
    public int IndexCount => Indicies.Count;
    public Material Material { get; }

    internal bool BuffersInitialized { get; private set; } = false;
    public IBufferGroup Buffers { get; private set; }

    private Vector3[] vertices;
    private Vector3[] normals;
    private Vector2[] texCoords;
    private uint[] indicies;

    public Mesh(Vector3[] vertices, uint[] indicies, Vector3[] normals, Vector2[] texCoords, Material material)
    {
        this.vertices = vertices;
        this.indicies = indicies;
        this.normals = normals;
        this.texCoords = texCoords;
        Material = material;
    }

    internal void GenerateBuffers(IGraphicsDevice device)
    {
        Buffers = device.CreateBufferGroup();
        // TODO: Fragile api, CreateBuffer is assumed to be created in buffer Open func (vao is bound there)
        Buffers.Open(list =>
            {
                list.Buffers.AddRange(
                    device.CreateBuffer(BufferUsage.Vertex, CreateVertexData()),
                device.CreateBuffer(BufferUsage.Index, indicies));
            }
        );

        BuffersInitialized = true;
    }

    private float[] CreateVertexData()
    {
        float[] vertData = new float[vertices.Length * 8];
        for (int i = 0; i < vertices.Length; i++)
        {
            vertData[i * 8 + 0] = vertices[i].X;
            vertData[i * 8 + 1] = vertices[i].Y;
            vertData[i * 8 + 2] = vertices[i].Z;
            vertData[i * 8 + 3] = normals.ElementAtOrDefault(i).X;
            vertData[i * 8 + 4] = normals.ElementAtOrDefault(i).Y;
            vertData[i * 8 + 5] = normals.ElementAtOrDefault(i).Z;
            vertData[i * 8 + 6] = texCoords.ElementAtOrDefault(i).X;
            vertData[i * 8 + 7] = texCoords.ElementAtOrDefault(i).Y;
        }
        return vertData;
    }
}