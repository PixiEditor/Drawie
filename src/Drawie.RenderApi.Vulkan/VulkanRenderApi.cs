using Drawie.RenderApi.Abstraction;
using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan;

public class VulkanRenderApi : IVulkanRenderApi
{
    private List<IWindowRenderApi> windowRenderApis = new List<IWindowRenderApi>();
    public IReadOnlyCollection<IWindowRenderApi> WindowRenderApis => windowRenderApis;
    public IGraphicsDevice GraphicsDevice { get; private set; }
    public IVulkanContext VulkanContext { get; private set; }

    IReadOnlyCollection<IVulkanWindowRenderApi> IVulkanRenderApi.WindowRenderApis =>
        windowRenderApis.Cast<IVulkanWindowRenderApi>().ToList();

    public VulkanRenderApi()
    {
    }

    public VulkanRenderApi(IVulkanContext vulkanContext)
    {
        VulkanContext = vulkanContext;
    }

    public IWindowRenderApi CreateWindowRenderApi()
    {
        VulkanWindowRenderApi windowRenderApi;
        if (windowRenderApis.Count == 0)
        {
            var context = new VulkanWindowContext();
            VulkanContext = context;

            windowRenderApi = new VulkanWindowRenderApi(context);
            windowRenderApis.Add(windowRenderApi);
            return windowRenderApi;
        }

        var existingWindowRenderApi = windowRenderApis.First() as VulkanWindowRenderApi;

        windowRenderApi = new VulkanWindowRenderApi(existingWindowRenderApi.Context);

        windowRenderApis.Add(windowRenderApi);
        return windowRenderApi;
    }
    
    private void CreateGraphicsDevice(IVulkanContext context)
    {
        throw new NotImplementedException();
    }
}
