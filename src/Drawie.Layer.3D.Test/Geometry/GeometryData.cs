namespace SilkNet.Geometry;

public class GeometryData
{
    public float[] Vertices { get; set; }
    public uint [] Indices { get; set; }
    
    public GeometryData(float[] vertices, uint[] indices)
    {
        Vertices = vertices.ToArray();
        Indices = indices.ToArray();
    }
}