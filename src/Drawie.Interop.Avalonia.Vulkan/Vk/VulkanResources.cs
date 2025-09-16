using Avalonia;
using Avalonia.Rendering.Composition;
using Drawie.Interop.Avalonia.Core;
using Drawie.Numerics;
using Drawie.RenderApi;
using Drawie.RenderApi.Vulkan.Buffers;

namespace Drawie.Interop.Avalonia.Vulkan.Vk;

public class VulkanResources : RenderApiResources
{
    public VulkanInteropContext Context { get; }
    public VulkanSwapchain Swapchain { get; }
    public override ITexture Texture => Content.texture;

    public override bool IsDisposed => isDisposed;
    public VulkanContent Content { get; }

    private bool isDisposed;

    public VulkanResources(CompositionDrawingSurface compositionDrawingSurface, ICompositionGpuInterop interop) : base(
        compositionDrawingSurface, interop)
    {
        Context = DrawieInterop.VulkanInteropContext;
        Swapchain = new VulkanSwapchain(Context, interop, compositionDrawingSurface);
        Content = new VulkanContent(Context);
    }

    public override async ValueTask DisposeAsync()
    {
        if (isDisposed)
            return;

        isDisposed = true;

        Context.Pool.FreeUsedCommandBuffers();
        Content.Dispose();
        await Swapchain.DisposeAsync();
    }

    public override void CreateTemporalObjects(VecI size)
    {
        if (isDisposed)
            return;

        Content.CreateTemporalObjects(size);
    }

    public override IDisposable? Render(VecI size, Action renderAction)
    {
        if (isDisposed)
            return null;

        var present = Swapchain.BeginDraw(size, out var image);
        renderAction();
        Content.Render(image);
        return present;
    }

    public override ITexture CreateTexture(VecI size)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(VulkanResources));

        return Context.CreateTexture(size);
    }

    public override ITexture CreateSwapchainTexture(VecI size)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(VulkanResources));

        return Swapchain.CreateImage(size);
    }

    public override ITexture CreateExportableTexture(VecI size)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(VulkanResources));

        return Context.CreateExportableTexture(size);
    }
}
