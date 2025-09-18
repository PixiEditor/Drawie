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

    public override Frame Render(VecI size, Action renderAction)
    {
        if (isDisposed)
            return default;

        var draw = Swapchain.BeginDraw(size, out var image);
        renderAction();
        Content.Render(image);
        return new Frame() { PresentFrame = draw.present, ReturnFrame = draw.returnToPool, Size = size, Texture = image };
    }

    public override ITexture CreateTexture(VecI size)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(VulkanResources));

        return Context.CreateTexture(size);
    }

    public override IExportableTexture CreateExportableTexture(VecI size)
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(VulkanResources));

        return Context.CreateExportableTexture(size);
    }

    public override ISemaphorePair CreateSemaphorePair()
    {
        if (isDisposed)
            throw new ObjectDisposedException(nameof(VulkanResources));

        return new VulkanSemaphorePair(Context, GpuInterop.SupportedImageHandleTypes, true);
    }

    public override Frame Render(VecI size, ITexture toBlit)
    {
        if (isDisposed || toBlit is not IVkTexture img)
            return default;

        var present = Swapchain.BeginDraw(size, out var image);
        Content.Render(image, img);
        return new Frame() { PresentFrame = present.present, ReturnFrame = present.returnToPool, Size = size, Texture = image };
    }
}
