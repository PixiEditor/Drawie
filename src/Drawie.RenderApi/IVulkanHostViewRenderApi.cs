namespace Drawie.RenderApi;

public interface IVulkanHostViewRenderApi : IHostViewRenderApi
{
    public IVulkanContext Context { get; }
}