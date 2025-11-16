using Drawie.Backend.Core.Surfaces;
using Drawie.RenderApi;

namespace Drawie.Skia;

public class SkiaVkExportedTexture : IExportedTexture
{
    public ulong MemorySize { get; }
    public int Width { get; }
    public int Height { get; }
    public IntPtr NativeHandle { get; }
    public string? Descriptor { get; }

    private IVkTexture nativeTexture;

    public SkiaVkExportedTexture(IVkTexture nativeTexture)
    {
        var exported = nativeTexture.Export();
        Width = nativeTexture.Size.X;
        Height = nativeTexture.Size.Y;
        NativeHandle = exported.handle;
        MemorySize = nativeTexture.MemorySize;
        Descriptor = exported.descriptor;
        this.nativeTexture = nativeTexture;
    }

    public void Dispose()
    {
        nativeTexture.Dispose();
    }
}
