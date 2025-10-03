using Avalonia;
using Avalonia.Rendering.Composition;
using Drawie.RenderApi;

namespace Drawie.Interop.Avalonia.Core;

public abstract class RenderApiResources : IAsyncDisposable
{
    protected InteropData Interop { get; }

    public abstract ITexture Texture { get; }

    public abstract bool IsDisposed { get; }

    public RenderApiResources(InteropData data)
    {
        Interop = data;
    }

    public abstract ValueTask DisposeAsync();

    public abstract void CreateTemporalObjects(PixelSize size);

    public abstract void Render(PixelSize size, Action renderAction);
}
