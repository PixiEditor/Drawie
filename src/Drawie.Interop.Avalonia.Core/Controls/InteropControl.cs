using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.VisualTree;
using Drawie.Backend.Core.Bridge;

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
        if (IDrawieInteropContext.Current != null)
        {
            InitializeComposition();
        }
        else
        {
            DrawingBackendApi.OnBackendInitialized += InitializeComposition;
        }

        base.OnLoaded(e);
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        if (initialized)
        {
            surface.Dispose();
            FreeGraphicsResources();
        }

        initialized = false;
        base.OnDetachedFromLogicalTree(e);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (initialized)
        {
            surface.Dispose();
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

            if (selfVisual?.Compositor == null)
            {
                return;
            }

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

    public override void Render(DrawingContext context)
    {
        if (!string.IsNullOrEmpty(info))
        {
            context.DrawText(new FormattedText(info, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, Typeface.Default, 12, Brushes.White),
                new Point(0, 0));
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
        try
        {
            RenderFrame(size);
            info = string.Empty;
        }
        catch (Exception e)
        {
            info = $"Error rendering frame: {e.Message}. Try updating graphics drivers or change Render API in settings if issue persists.";
            return;
        }
    }

    public void QueueNextFrame()
    {
        if (initialized && !updateQueued && compositor != null && surface is { IsDisposed: false })
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
