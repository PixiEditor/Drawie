using Drawie.RenderApi.Vulkan.Exceptions;

namespace Drawie.RenderApi.Vulkan;

public class OffscreenVulkanContextInfo : IVulkanContextInfo
{
    public string[] GetInstanceExtensions()
    {
        var enabledExtensions = new List<string>()
        {
            "VK_KHR_get_physical_device_properties2",
            "VK_KHR_external_memory_capabilities",
            "VK_KHR_external_semaphore_capabilities",
            "VK_EXT_debug_utils"
        };

        return enabledExtensions.ToArray();
    }

    public ulong GetSurfaceHandle(IntPtr instanceHandle)
    {
        throw new VulkanException("Offscreen context doesn't have a surface");
    }

    public bool HasSurface => false;
}
