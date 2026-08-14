namespace Drawie.RenderApi;

public interface IVulkanRenderApi : IRenderApi
{
    public new IReadOnlyCollection<IVulkanHostViewRenderApi> WindowRenderApis { get; }
    public IVulkanContext VulkanContext { get; }
}