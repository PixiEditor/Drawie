using Avalonia;
using Avalonia.Rendering.Composition;
using Drawie.Interop.Avalonia.Core;
using Drawie.RenderApi;

namespace Drawie.Interop.Avalonia.Vulkan.Vk;

public class VulkanResources : RenderApiResources
{
    public VulkanInteropContext Context { get; }
    public VulkanSwapchain Swapchain { get; }
    public override ITexture Texture => Content.texture;
    public VulkanContent Content { get; }

    private bool isDisposed;

    public VulkanResources(CompositionDrawingSurface compositionDrawingSurface, ICompositionGpuInterop interop) : base(
        compositionDrawingSurface, interop)
    {
        Context = DrawieInterop.VulkanInteropContext;
        Swapchain = new VulkanSwapchain(DrawieInterop.VulkanInteropContext, interop, compositionDrawingSurface);
        Content = new VulkanContent(DrawieInterop.VulkanInteropContext);
    }

    public override async ValueTask DisposeAsync()
    {
        isDisposed = true;
        Context.Pool.FreeUsedCommandBuffers();
        Content.Dispose();
        await Swapchain.DisposeAsync();
    }

    public override void CreateTemporalObjects(PixelSize size)
    {
        if (isDisposed)
            return;

        Content.CreateTemporalObjects(size);
    }

    public override void Render(PixelSize size, Action renderAction)
    {
        if (isDisposed)
            return;

        using (Swapchain.BeginDraw(size, out var image))
        {
            renderAction();
            Content.Render(image);
        }
    }
}
