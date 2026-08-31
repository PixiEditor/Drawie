namespace Drawie.RenderApi.Abstraction.Buffers;

public class NamedBuffer
{
    public string Name { get; set; }
    public IBuffer Buffer { get; set; }
    
    public NamedBuffer(string name, IBuffer buffer)
    {
        Name = name;
        Buffer = buffer;
    }
}