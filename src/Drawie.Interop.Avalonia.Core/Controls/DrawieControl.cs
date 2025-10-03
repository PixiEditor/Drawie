using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media;
using Avalonia.Rendering.Composition;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;

namespace Drawie.Interop.Avalonia.Core.Controls;

public abstract class DrawieControl : InteropControl
{
    private RenderApiResources resources;
    private DrawingSurface? framebuffer;
    private Texture intermediateSurface;

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
            resources = IDrawieInteropContext.Current.CreateResources(new InteropData(compositionDrawingSurface, interop));
        }
        catch (Exception e)
        {
            return (false, $"Failed to create resources: {e.Message}");
        }

        return (true, string.Empty);
    }

    protected override void InitializeSoftwareComposition()
    {
        resources = IDrawieInteropContext.Current.CreateResources(new InteropData());
    }

    protected override void RenderSoftware(DrawingContext context)
    {
        if(double.IsNaN(Bounds.Width) || double.IsNaN(Bounds.Height) || Bounds.Width <= 0 || Bounds.Height <= 0)
            return;
        RenderFrame(new PixelSize((int)Bounds.Width, (int)Bounds.Height));
        if (resources?.Texture is AvaloniaBitmapTexture bmp)
        {
            unsafe
            {
                using var locked = bmp.Bitmap.Lock();
                // copy drawie framebuffer to locked framebuffer

                using var pixmap = framebuffer.PeekPixels();
                Buffer.MemoryCopy(pixmap.GetPixels().ToPointer(),
                    locked.Address.ToPointer(),
                    locked.Size.Width * locked.Size.Height * 4,
                    pixmap.BytesSize);

                context.DrawImage(bmp.Bitmap, new Rect(0, 0, bmp.Bitmap.PixelSize.Width, bmp.Bitmap.PixelSize.Height));
            }
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        using var ctx = IDrawieInteropContext.Current.EnsureContext();
        base.OnDetachedFromVisualTree(e);
        framebuffer?.Dispose();
        framebuffer = null;
    }

    protected override void FreeGraphicsResources()
    {
        using var ctx = IDrawieInteropContext.Current.EnsureContext();
        intermediateSurface?.Dispose();
        intermediateSurface = null;

        framebuffer?.Dispose();
        framebuffer = null;

        resources.DisposeAsync();

        resources = null;
    }

    public abstract void Draw(DrawingSurface surface);

    protected override void RenderFrame(PixelSize size)
    {
        if (resources is { IsDisposed: false })
        {
            using var ctx = IDrawieInteropContext.Current.EnsureContext();
            if (size.Width == 0 || size.Height == 0)
            {
                return;
            }

            if (framebuffer == null || lastSize != size)
            {
                resources.CreateTemporalObjects(size);

                VecI sizeVec = new VecI(size.Width, size.Height);

                framebuffer?.Dispose();

                framebuffer =
                    DrawingBackendApi.Current.CreateRenderSurface(sizeVec,
                        resources.Texture, SurfaceOrigin.BottomLeft);

                if (UseIntermediateSurface)
                {
                    intermediateSurface?.Dispose();
                    intermediateSurface = Texture.ForDisplay(sizeVec);
                }

                lastSize = size;
            }

            resources.Render(size, () =>
            {
                framebuffer.Canvas.Clear();
                intermediateSurface?.DrawingSurface.Canvas.Clear();

                if (!UseIntermediateSurface)
                {
                    Draw(framebuffer);
                }
                else
                {
                    Draw(intermediateSurface.DrawingSurface);
                    framebuffer.Canvas.DrawSurface(intermediateSurface.DrawingSurface, 0, 0);
                }

                framebuffer.Flush();
            });
        }
    }
}
