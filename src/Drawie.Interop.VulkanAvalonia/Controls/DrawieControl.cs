using Avalonia;
using Avalonia.Rendering.Composition;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Surfaces;
using Drawie.Interop.VulkanAvalonia.Vulkan;
using Drawie.Numerics;

namespace Drawie.Interop.VulkanAvalonia.Controls;

public abstract class DrawieControl : InteropControl
{
    private VulkanResources resources;
    private DrawingSurface renderSurface;

    private PixelSize lastSize = PixelSize.Empty;

    protected override (bool success, string info) InitializeGraphicsResources(Compositor targetCompositor,
        CompositionDrawingSurface compositionDrawingSurface, ICompositionGpuInterop interop)
    {
        resources = new VulkanResources(
            DrawieInterop.VulkanInteropContext,
            new VulkanSwapchain(DrawieInterop.VulkanInteropContext, interop, compositionDrawingSurface),
            new VulkanContent(DrawieInterop.VulkanInteropContext));

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
                resources.Content.CreateTemporalObjects(size);

                VecI sizeVec = new VecI(size.Width, size.Height);

                renderSurface?.Dispose();

                renderSurface =
                    DrawingBackendApi.Current.CreateRenderSurface(sizeVec,
                        resources.Content.texture, SurfaceOrigin.BottomLeft);

                lastSize = size;
            }

            using (resources.Swapchain.BeginDraw(size, out var image))
            {
                renderSurface.Canvas.Clear();
                Draw(renderSurface);
                renderSurface.Flush();

                resources.Content.Render(image);
            }
        }
    }
}
