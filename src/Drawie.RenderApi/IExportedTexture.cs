namespace Drawie.RenderApi;

public interface IExportedTexture : ITexture, IDisposable
{
    ulong MemorySize { get; }
    public int Width { get;  }
    public int Height { get; }
    IntPtr NativeHandle { get; }
    string? Descriptor { get; }
}
