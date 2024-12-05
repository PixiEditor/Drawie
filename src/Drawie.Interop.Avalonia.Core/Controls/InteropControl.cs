using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Rendering.Composition;
using Avalonia.VisualTree;

namespace Drawie.Interop.Avalonia.Core.Controls;

public abstract class InteropControl : Control
{
    private CompositionSurfaceVisual surfaceVisual;
    private Compositor compositor;

    private readonly Action update;
    private bool updateQueued;

    private CompositionDrawingSurface? surface;

    private string info = string.Empty;
    private bool initialized = false;

    public InteropControl()
    {
        update = UpdateFrame;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        InitializeComposition();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (initialized)
        {
            FreeGraphicsResources();
        }

        initialized = false;
        base.OnDetachedFromVisualTree(e);
    }

    private async void InitializeComposition()
    {
        try
        {
            var selfVisual = ElementComposition.GetElementVisual(this);
            compositor = selfVisual.Compositor;

            surface = compositor.CreateDrawingSurface();
            surfaceVisual = compositor.CreateSurfaceVisual();

            surfaceVisual.Size = new Vector(Bounds.Width, Bounds.Height);

            surfaceVisual.Surface = surface;
            ElementComposition.SetElementChildVisual(this, surfaceVisual);
            var (result, initInfo) = await DoInitialize(compositor, surface);
            info = initInfo;

            initialized = result;
            QueueNextFrame();
        }
        catch (Exception e)
        {
            info = e.Message;
            throw;
        }
    }

    void UpdateFrame()
    {
        updateQueued = false;
        var root = this.GetVisualRoot();
        if (root == null)
        {
            return;
        }

        surfaceVisual.Size = new Vector(Bounds.Width, Bounds.Height);

        if (double.IsNaN(surfaceVisual.Size.X) || double.IsNaN(surfaceVisual.Size.Y))
        {
            return;
        }

        var size = new PixelSize((int)Bounds.Width, (int)Bounds.Height);
        RenderFrame(size);
    }

    public void QueueNextFrame()
    {
        if (initialized && !updateQueued && compositor != null)
        {
            updateQueued = true;
            compositor.RequestCompositionUpdate(update);
        }
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == BoundsProperty)
        {
            QueueNextFrame();
        }

        base.OnPropertyChanged(change);
    }

    private async Task<(bool success, string info)> DoInitialize(Compositor compositor,
        CompositionDrawingSurface surface)
    {
        var interop = await compositor.TryGetCompositionGpuInterop();
        if (interop == null)
        {
            return (false, "Composition interop not available");
        }

        return InitializeGraphicsResources(compositor, surface, interop);
    }

    protected abstract (bool success, string info) InitializeGraphicsResources(Compositor targetCompositor,
        CompositionDrawingSurface compositionDrawingSurface, ICompositionGpuInterop interop);

    protected abstract void FreeGraphicsResources();
    protected abstract void RenderFrame(PixelSize size);
}
