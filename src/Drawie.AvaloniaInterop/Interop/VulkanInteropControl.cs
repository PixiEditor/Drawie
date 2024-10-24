using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Rendering.Composition;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace Drawie.AvaloniaGraphics.Interop;

public class VulkanInteropControl : InteropControl
{
    private VulkanResources resources;
    private DrawingSurface renderSurface;

    private PixelSize lastSize = PixelSize.Empty;

    protected override (bool success, string info) InitializeGraphicsResources(Compositor targetCompositor,
        CompositionDrawingSurface compositionDrawingSurface, ICompositionGpuInterop interop)
    {
        resources = new VulkanResources(
            App.InteropContext,
            new VulkanSwapchain(App.InteropContext, interop, compositionDrawingSurface),
            new VulkanContent(App.InteropContext));

        return (true, string.Empty);
    }

    protected override void FreeGraphicsResources()
    {
        resources?.DisposeAsync();
        resources = null;
    }

    protected override void RenderFrame(PixelSize size)
    {
        if (resources != null)
        {
            if(size.Width == 0 || size.Height == 0)
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
                renderSurface.Canvas.Clear(Colors.Azure);
                using Paint paint = new Paint() { Color = Colors.Green };
                renderSurface.Canvas.DrawRect(0, 0, 100, 100, paint);
                renderSurface.Flush();

                resources.Content.Render(image);
            }
        }
    }
}