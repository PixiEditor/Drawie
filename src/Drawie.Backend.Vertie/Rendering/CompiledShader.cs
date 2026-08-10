namespace Drawie.Backend.Vertie.Rendering;

public class CompiledShader(byte[] vertex, byte[] fragment)
{
    public byte[] Vertex { get; } = vertex;
    public byte[] Fragment { get; } = fragment;
}