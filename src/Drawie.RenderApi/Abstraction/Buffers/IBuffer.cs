namespace Drawie.RenderApi.Abstraction.Buffers;

public interface IBuffer : INativeObject
{
    public BufferUsage Usage { get; }
}