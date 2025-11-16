using Avalonia;
using Avalonia.Rendering.Composition;
using Drawie.RenderApi;

namespace Drawie.Interop.Avalonia.Core;

public abstract class RenderApiResources : IAsyncDisposable
{
    public CompositionDrawingSurface Surface { get; }
    public ICompositionGpuInterop GpuInterop { get; }

    public abstract ITexture Texture { get; }

    public abstract bool IsDisposed { get; }

    public RenderApiResources(CompositionDrawingSurface surface, ICompositionGpuInterop gpuInterop)
    {
        Surface = surface;
        GpuInterop = gpuInterop;
    }

    public abstract ValueTask DisposeAsync();

    public abstract void CreateTemporalObjects(PixelSize size);

    public abstract void Render(PixelSize size, Func<IExportedTexture> renderAction);

}
