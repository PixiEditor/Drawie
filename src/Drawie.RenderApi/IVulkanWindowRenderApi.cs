namespace Drawie.RenderApi;

public interface IVulkanWindowRenderApi : IWindowRenderApi
{
    public IVulkanContext Context { get; }
}