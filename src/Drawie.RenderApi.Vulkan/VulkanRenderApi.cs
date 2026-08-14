using Drawie.RenderApi.Abstraction;
using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan;

public class VulkanRenderApi : IVulkanRenderApi
{
    private List<IHostViewRenderApi> windowRenderApis = new List<IHostViewRenderApi>();
    public IReadOnlyCollection<IHostViewRenderApi> WindowRenderApis => windowRenderApis;
    public IGraphicsDevice GraphicsDevice { get; private set; }
    public IVulkanContext VulkanContext { get; private set; }

    IReadOnlyCollection<IVulkanHostViewRenderApi> IVulkanRenderApi.WindowRenderApis =>
        windowRenderApis.Cast<IVulkanHostViewRenderApi>().ToList();

    public VulkanRenderApi()
    {
    }

    public VulkanRenderApi(IVulkanContext vulkanContext)
    {
        VulkanContext = vulkanContext;
    }

    public IHostViewRenderApi CreateWindowRenderApi()
    {
        VulkanHostViewRenderApi hostViewRenderApi;
        if (windowRenderApis.Count == 0)
        {
            var context = new VulkanWindowContext();
            VulkanContext = context;

            hostViewRenderApi = new VulkanHostViewRenderApi(context);
            windowRenderApis.Add(hostViewRenderApi);
            return hostViewRenderApi;
        }

        var existingWindowRenderApi = windowRenderApis.First() as VulkanHostViewRenderApi;

        hostViewRenderApi = new VulkanHostViewRenderApi(existingWindowRenderApi.Context);

        windowRenderApis.Add(hostViewRenderApi);
        return hostViewRenderApi;
    }
    
    private void CreateGraphicsDevice(IVulkanContext context)
    {
        throw new NotImplementedException();
    }
}
