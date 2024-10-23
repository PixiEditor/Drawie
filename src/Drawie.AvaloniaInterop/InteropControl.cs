using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Rendering.Composition;
using Avalonia.VisualTree;
using Drawie.AvaloniaGraphics.Interop;
using Drawie.RenderApi.Vulkan;
using Drawie.Skia;
using DrawiEngine;

namespace Drawie.AvaloniaGraphics;

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

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
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
            
            ICompositionGpuInterop interop = await compositor.TryGetCompositionGpuInterop();

            VulkanInteropContext context = new VulkanInteropContext(interop);

            AvaloniaInteropContextInfo contextInfo = new AvaloniaInteropContextInfo();

            context.Initialize(contextInfo);

            App.InteropContext = context;

            VulkanRenderApi renderApi = new VulkanRenderApi(context);
            SkiaDrawingBackend drawingBackend = new SkiaDrawingBackend();
            DrawingEngine drawingEngine = new DrawingEngine(renderApi, null, drawingBackend);

            drawingEngine.Run();

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
        var size = PixelSize.FromSize(Bounds.Size, root.RenderScaling);
        RenderFrame(size);
        QueueNextFrame();
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