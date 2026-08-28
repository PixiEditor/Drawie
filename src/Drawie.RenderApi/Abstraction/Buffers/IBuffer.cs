namespace Drawie.RenderApi.Abstraction.Buffers;

public interface IBuffer
{
    public BufferUsage Usage { get; }
}

public interface IBuffer<T> : IBuffer where T : unmanaged
{
    public ulong Size { get; }
}