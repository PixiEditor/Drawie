using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Rendering.Composition;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace Drawie.AvaloniaGraphics.Interop;

public class VulkanInteropControl : InteropControl
{
    private VulkanResources resources;
    private DrawingSurface surface;

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
            if (surface == null)
            {
                resources.Content.CreateTemporalObjects(size);

                surface =
                    DrawingBackendApi.Current.CreateRenderSurface(new VecI(size.Width, size.Height),
                        resources.Content.texture);
            }

            resources.Content.PrepareTextureToWrite();
            surface.Canvas.Clear(Colors.Azure);
            using Paint paint = new Paint() { Color = Colors.Green };
            surface.Canvas.DrawRect(0, 0, 100, 100, paint);
            surface.Flush();

            using (resources.Swapchain.BeginDraw(size, out var image))
            {
                resources.Content.Render(image);
            }
        }
    }
}