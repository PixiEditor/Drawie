using Avalonia;
using Avalonia.Rendering.Composition;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;
using Drawie.RenderApi;

namespace Drawie.Interop.Avalonia.Core.Controls;

public abstract class DrawieControl : InteropControl
{
    private RenderApiResources resources;

    private PixelSize lastSize = PixelSize.Empty;


    /// <summary>
    ///     If true, intermediate surface will be used to render the frame. This is useful when dealing with non srgb surfaces.
    /// Enabling this will create display-optimized surface and use it for rendering, then draw it to the frame buffer.
    /// </summary>
    protected bool UseIntermediateSurface { get; set; } = true;

    protected override (bool success, string info) InitializeGraphicsResources(Compositor targetCompositor,
        CompositionDrawingSurface compositionDrawingSurface, ICompositionGpuInterop interop)
    {
        try
        {
            resources = IDrawieInteropContext.Current.CreateResources(compositionDrawingSurface, interop);
        }
        catch (Exception e)
        {
            return (false, $"Failed to create resources: {e.Message}");
        }

        return (true, string.Empty);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        using var ctx = IDrawieInteropContext.Current.EnsureContext();
        base.OnDetachedFromVisualTree(e);
    }

    protected override void FreeGraphicsResources()
    {
        using var ctx = IDrawieInteropContext.Current.EnsureContext();

        resources.DisposeAsync();

        resources = null;
    }

    public abstract void Draw(DrawingSurface surface);

    public abstract IExportedTexture GetTexture();

    protected override void RenderFrame(PixelSize size)
    {
        if (resources is { IsDisposed: false })
        {
            using var ctx = IDrawieInteropContext.Current.EnsureContext();
            if (size.Width == 0 || size.Height == 0)
            {
                return;
            }

            if (lastSize != size)
            {
                resources.CreateTemporalObjects(size);
                lastSize = size;
            }

            resources.Render(size, () =>
            {
                return GetTexture();
            });
        }
    }
}
