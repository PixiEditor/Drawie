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
        GraphicsDevice = CreateGraphicsDevice(vulkanContext);
    }

    public IHostViewRenderApi CreateWindowRenderApi()
    {
        VulkanHostViewRenderApi hostViewRenderApi;
        if (windowRenderApis.Count == 0)
        {
            var context = new VulkanWindowContext();
            VulkanContext = context;
            
            hostViewRenderApi = new VulkanHostViewRenderApi(context);

            hostViewRenderApi.Initialized += () => { GraphicsDevice = CreateGraphicsDevice(context); };
            windowRenderApis.Add(hostViewRenderApi);
            return hostViewRenderApi;
        }

        var existingWindowRenderApi = windowRenderApis.First() as VulkanHostViewRenderApi;

        hostViewRenderApi = new VulkanHostViewRenderApi(existingWindowRenderApi.Context);

        windowRenderApis.Add(hostViewRenderApi);
        return hostViewRenderApi;
    }
    
    private static IGraphicsDevice CreateGraphicsDevice(IVulkanContext context)
    {
        if (context is not VulkanContext vulkanContext || vulkanContext.Api is null)
            throw new InvalidOperationException("Vulkan graphics device is available only after the Vulkan context is initialized.");

        return new VulkanGraphicsDevice(vulkanContext);
    }}
