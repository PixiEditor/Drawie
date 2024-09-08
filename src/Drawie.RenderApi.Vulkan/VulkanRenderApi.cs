using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan;

public class VulkanRenderApi : IVulkanRenderApi
{
    private List<IWindowRenderApi> windowRenderApis = new List<IWindowRenderApi>();
    public IReadOnlyCollection<IWindowRenderApi> WindowRenderApis => windowRenderApis;

    IReadOnlyCollection<IVulkanWindowRenderApi> IVulkanRenderApi.WindowRenderApis { get; }

    public IWindowRenderApi CreateWindowRenderApi()
    {
        var windowRenderApi = new VulkanWindowRenderApi();
        windowRenderApis.Add(windowRenderApi);
        return windowRenderApi;
    } 
}