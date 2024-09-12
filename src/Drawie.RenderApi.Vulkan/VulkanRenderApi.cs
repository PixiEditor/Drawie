using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan;

public class VulkanRenderApi : IVulkanRenderApi
{
    private List<IWindowRenderApi> windowRenderApis = new List<IWindowRenderApi>();
    public IReadOnlyCollection<IWindowRenderApi> WindowRenderApis => windowRenderApis;

    IReadOnlyCollection<IVulkanWindowRenderApi> IVulkanRenderApi.WindowRenderApis => windowRenderApis.Cast<IVulkanWindowRenderApi>().ToList();

    public IWindowRenderApi CreateWindowRenderApi()
    {
        VulkanWindowRenderApi windowRenderApi;
        if (windowRenderApis.Count == 0)
        {
            windowRenderApi = new VulkanWindowRenderApi();
            windowRenderApis.Add(windowRenderApi);
            return windowRenderApi;
        }
        
        var existingWindowRenderApi = windowRenderApis.First() as VulkanWindowRenderApi;
        
        windowRenderApi = new VulkanWindowRenderApi(existingWindowRenderApi.Instance, existingWindowRenderApi.LogicalDevice,
            existingWindowRenderApi.PhysicalDevice, existingWindowRenderApi.graphicsQueue, existingWindowRenderApi.presentQueue);
        
        windowRenderApis.Add(windowRenderApi);
        return windowRenderApi;
    } 
}