namespace Drawie.RenderApi;

public interface IVulkanWindowRenderApi : IWindowRenderApi
{
    public IVulkanContext Context { get; }
    public IVkTexture RenderTexture { get; }
}