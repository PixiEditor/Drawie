namespace Drawie.RenderApi.Abstraction.Buffers;

public class NamedBuffer
{
    public string Name { get; }
    public IBuffer Buffer { get; }
    
    public NamedBuffer(string name, IBuffer buffer)
    {
        Name = name;
        Buffer = buffer;
    }
}