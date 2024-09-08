using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Drawie.RenderApi.Vulkan.Helpers;

public static class SetupUtility
{
    public static unsafe QueueFamilyIndices FindQueueFamilies(Vk vk, PhysicalDevice device, SurfaceKHR surface,
        KhrSurface khrSurface)
    {
        var indices = new QueueFamilyIndices();

        uint queueFamilityCount = 0;
        vk.GetPhysicalDeviceQueueFamilyProperties(device, ref queueFamilityCount, null);

        var queueFamilies = new QueueFamilyProperties[queueFamilityCount];
        fixed (QueueFamilyProperties* queueFamiliesPtr = queueFamilies)
        {
            vk.GetPhysicalDeviceQueueFamilyProperties(device, ref queueFamilityCount, queueFamiliesPtr);
        }

        uint i = 0;
        foreach (var queueFamily in queueFamilies)
        {
            if (queueFamily.QueueFlags.HasFlag(QueueFlags.GraphicsBit)) indices.GraphicsFamily = i;

            khrSurface!.GetPhysicalDeviceSurfaceSupport(device, i, surface, out var presentSupport);

            if (presentSupport) indices.PresentFamily = i;

            if (indices.IsComplete) break;

            i++;
        }

        return indices;
    }

    public static unsafe SwapChainSupportDetails QuerySwapChainSupport(PhysicalDevice physicalDevice, SurfaceKHR surface, KhrSurface khrSurface)
    {
        var details = new SwapChainSupportDetails();

        khrSurface!.GetPhysicalDeviceSurfaceCapabilities(physicalDevice, surface, out var capabilities);
        details.Capabilities = capabilities;

        uint formatCount = 0;
        khrSurface.GetPhysicalDeviceSurfaceFormats(physicalDevice, surface, ref formatCount, null);

        if (formatCount != 0)
        {
            details.Formats = new SurfaceFormatKHR[formatCount];
            fixed (SurfaceFormatKHR* formatsPtr = details.Formats)
            {
                khrSurface.GetPhysicalDeviceSurfaceFormats(physicalDevice, surface, ref formatCount, formatsPtr);
            }
        }
        else
        {
            details.Formats = Array.Empty<SurfaceFormatKHR>();
        }

        uint presentModeCount = 0;
        khrSurface.GetPhysicalDeviceSurfacePresentModes(physicalDevice, surface, ref presentModeCount, null);

        if (presentModeCount != 0)
        {
            details.PresentModes = new PresentModeKHR[presentModeCount];
            fixed (PresentModeKHR* formatsPtr = details.PresentModes)
            {
                khrSurface.GetPhysicalDeviceSurfacePresentModes(physicalDevice, surface, ref presentModeCount,
                    formatsPtr);
            }
        }
        else
        {
            details.PresentModes = Array.Empty<PresentModeKHR>();
        }

        return details;
    }
}