using Avalonia;
using Avalonia.Rendering.Composition;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Surfaces;
using Drawie.Numerics;

namespace Drawie.Interop.Avalonia.Core.Controls;

public abstract class DrawieControl : InteropControl
{
    private RenderApiResources resources;
    private DrawingSurface renderSurface;

    private PixelSize lastSize = PixelSize.Empty;

    protected override (bool success, string info) InitializeGraphicsResources(Compositor targetCompositor,
        CompositionDrawingSurface compositionDrawingSurface, ICompositionGpuInterop interop)
    {
        resources = IDrawieInteropContext.Current.CreateResources(compositionDrawingSurface, interop);

        return (true, string.Empty);
    }

    protected override void FreeGraphicsResources()
    {
        resources?.DisposeAsync();
        renderSurface?.Dispose();
        renderSurface = null;
        resources = null;
    }

    public abstract void Draw(DrawingSurface surface);

    protected override void RenderFrame(PixelSize size)
    {
        if (resources != null)
        {
            if (size.Width == 0 || size.Height == 0)
            {
                return;
            }

            if (renderSurface == null || lastSize != size)
            {
                resources.CreateTemporalObjects(size);

                VecI sizeVec = new VecI(size.Width, size.Height);

                renderSurface?.Dispose();

                renderSurface =
                    DrawingBackendApi.Current.CreateRenderSurface(sizeVec,
                        resources.Texture, SurfaceOrigin.BottomLeft);

                lastSize = size;
            }

            resources.Render(size, () =>
            {
                renderSurface.Canvas.Clear();
                Draw(renderSurface);
                renderSurface.Flush();
            });
        }
    }
}
