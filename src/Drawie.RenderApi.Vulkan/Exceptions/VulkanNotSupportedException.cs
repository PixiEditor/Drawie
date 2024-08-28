namespace Drawie.RenderApi.Vulkan.Exceptions;

public class VulkanNotSupportedException : VulkanException
{
    public VulkanNotSupportedException() : base("Vulkan is not supported on this platform.")
    {
    }
}