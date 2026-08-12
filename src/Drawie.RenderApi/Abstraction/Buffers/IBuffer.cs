namespace Drawie.RenderApi.Abstraction.Buffers;

public interface IBuffer : INativeObject
{
    public BufferUsage Usage { get; }
}

public interface IBuffer<T> : IBuffer where T : unmanaged
{
    public uint Size { get; }
}