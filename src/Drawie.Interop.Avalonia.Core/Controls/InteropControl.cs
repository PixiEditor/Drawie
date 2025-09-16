using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;
using Drawie.RenderApi;

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
    protected RenderApiResources resources;

    public InteropControl()
    {
        update = UpdateFrame;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        InitializeComposition();
        base.OnLoaded(e);
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        if (initialized)
        {
            surface.Dispose();
            FreeGraphicsResources();
            resources.DisposeAsync();
            resources = null;
        }

        initialized = false;
        base.OnDetachedFromLogicalTree(e);
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
            if (Bounds.Width <= 0 || Bounds.Height <= 0 || double.IsNaN(Bounds.Width) || double.IsNaN(Bounds.Height))
            {
                return;
            }

            updateQueued = true;
            QueueFrameRequested();
        }
    }

    protected void RequestBlit()
    {
        DrawingBackendApi.Current.RenderingDispatcher.EnqueueUIUpdate(update);
    }

    protected virtual void QueueFrameRequested()
    {
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

        resources = InitializeGraphicsResources(compositor, surface, interop, out string createInfo);
        if (resources == null || resources.IsDisposed)
        {
            return (false, createInfo);
        }

        return (true, string.Empty);
    }

    protected abstract RenderApiResources? InitializeGraphicsResources(Compositor targetCompositor,
        CompositionDrawingSurface compositionDrawingSurface, ICompositionGpuInterop interop, out string? info);

    protected abstract void FreeGraphicsResources();
    protected abstract void RenderFrame(PixelSize size);

}
