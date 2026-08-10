namespace Drawie.RenderApi.Abstraction;

public interface INativeObject : IDisposable
{
    public int Handle { get; }
    public uint NativeHandle { get; }
}