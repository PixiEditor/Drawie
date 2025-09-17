using Avalonia;
using Avalonia.Rendering.Composition;
using Drawie.Numerics;
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

    public abstract void CreateTemporalObjects(VecI size);

    public abstract IDisposable Render(VecI size, Action renderAction);

    public abstract ITexture CreateTexture(VecI size);

    public abstract IExportableTexture CreateExportableTexture(VecI size);
    public abstract ISemaphorePair CreateSemaphorePair();

    public abstract IDisposable Render(VecI size, ITexture toBlit);
}
