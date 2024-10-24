namespace Drawie.Interop.VulkanAvalonia.Vulkan;

public class VulkanResources : IAsyncDisposable
{
    public VulkanInteropContext Context { get; }
    public VulkanSwapchain Swapchain { get; }
    public VulkanContent Content { get; }

    public VulkanResources(VulkanInteropContext context, VulkanSwapchain swapchain, VulkanContent content)
    {
        Context = context;
        Swapchain = swapchain;
        Content = content;
    }

    public async ValueTask DisposeAsync()
    {
        Context.Pool.FreeUsedCommandBuffers();
        Content.Dispose();
        await Swapchain.DisposeAsync();
    }
}
