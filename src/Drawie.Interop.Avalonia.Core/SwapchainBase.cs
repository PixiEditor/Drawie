using Avalonia;
using Avalonia.Rendering.Composition;
using Drawie.Interop.Avalonia.Core.Utils;

namespace Drawie.Interop.Avalonia.Core;

public abstract class SwapchainBase<TImage> : IAsyncDisposable where TImage : class, ISwapchainImage
{
    protected ICompositionGpuInterop Interop { get; }
    protected CompositionDrawingSurface Target { get; }
    private readonly List<TImage> _pendingImages = new();
    private bool isDisposed;

    public SwapchainBase(ICompositionGpuInterop interop, CompositionDrawingSurface target)
    {
        Interop = interop;
        Target = target;
    }

    static bool IsBroken(TImage image) => image.LastPresent?.IsFaulted == true;

    static bool IsReady(TImage image) =>
        image.LastPresent == null || image.LastPresent.Status == TaskStatus.RanToCompletion;

    TImage? CleanupAndFindNextImage(PixelSize size)
    {
        if (isDisposed)
            return null;

        TImage? firstFound = null;
        var foundMultiple = false;

        for (var c = _pendingImages.Count - 1; c > -1; c--)
        {
            var image = _pendingImages[c];
            var ready = IsReady(image);
            var matches = image.Size == size;
            if (IsBroken(image) || (!matches && ready))
            {
                image.DisposeAsync();
                _pendingImages.RemoveAt(c);
            }

            if (matches && ready)
            {
                if (firstFound == null)
                    firstFound = image;
                else
                    foundMultiple = true;
            }
        }

        // We are making sure that there was at least one image of the same size in flight
        // Otherwise we might encounter UI thread lockups
        return foundMultiple ? firstFound : null;
    }

    protected abstract TImage CreateImage(PixelSize size);

    protected IDisposable BeginDrawCore(PixelSize size, out TImage image)
    {
        if (isDisposed)
        {
            image = null;
            return Disposable.Create(() => { });
        }

        var img = CleanupAndFindNextImage(size) ?? CreateImage(size);

        img.BeginDraw();
        _pendingImages.Remove(img);
        image = img;
        return Disposable.Create(() =>
        {
            if (isDisposed)
                return;

            img.Present();
            _pendingImages.Add(img);
        });
    }

    public async ValueTask DisposeAsync()
    {
        if (isDisposed)
            return;

        isDisposed = true;
        for (var i = 0; i < _pendingImages.Count; i++)
        {
            var img = _pendingImages[i];
            await img.DisposeAsync();
        }
    }
}

public interface ISwapchainImage : IAsyncDisposable
{
    PixelSize Size { get; }
    Task? LastPresent { get; }
    void BeginDraw();
    void Present();
}
