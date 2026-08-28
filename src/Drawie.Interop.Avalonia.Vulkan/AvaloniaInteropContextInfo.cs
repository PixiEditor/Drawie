using System.Runtime.InteropServices;
using Avalonia.Vulkan;
using Drawie.RenderApi;

namespace Drawie.Interop.Avalonia.Vulkan;

public class AvaloniaInteropContextInfo : IVulkanContextInfo
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

        if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            enabledExtensions.Add("VK_KHR_portability_enumeration");

        return enabledExtensions.ToArray();
    }

    public ulong GetSurfaceHandle(IntPtr instanceHandle)
    {
        throw new VulkanException("Interop doesn't have a surface");
    }

    public bool HasSurface => false;
}
