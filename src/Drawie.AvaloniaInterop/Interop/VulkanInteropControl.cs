using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Rendering.Composition;

namespace Drawie.AvaloniaGraphics.Interop;

public class VulkanInteropControl : InteropControl
{
    private VulkanResources resources;
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
        if(resources != null)
        {
            using (resources.Swapchain.BeginDraw(size, out var image))
            {
                resources.Content.Render(image, 0, 0, 0, 1);
            }
        }
    }
}