namespace Drawie.RenderApi.Vulkan;

public class VulkanRenderApi : IRenderApi
{
    public IWindowRenderApi CreateWindowRenderApi()
    {
        return new VulkanWindowRenderApi();
    } 
}