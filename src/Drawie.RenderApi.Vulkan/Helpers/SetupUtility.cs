using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Drawie.RenderApi.Vulkan.Helpers;

public static class SetupUtility
{
    public static unsafe QueueFamilyIndices FindQueueFamilies(Vk vk, PhysicalDevice device, KhrSurface? presentSurface,
        SurfaceKHR? surface)
    {
        var indices = new QueueFamilyIndices();

        uint queueFamilyCount = 0;
        vk.GetPhysicalDeviceQueueFamilyProperties(device, ref queueFamilyCount, null);

        var queueFamilies = new QueueFamilyProperties[queueFamilyCount];
        fixed (QueueFamilyProperties* queueFamiliesPtr = queueFamilies)
        {
            vk.GetPhysicalDeviceQueueFamilyProperties(device, ref queueFamilyCount, queueFamiliesPtr);
        }

        uint i = 0;
        foreach (var queueFamily in queueFamilies)
        {
            if (queueFamily.QueueFlags.HasFlag(QueueFlags.GraphicsBit)) indices.GraphicsFamily = i;

            if (presentSurface != null && surface != null)
            {
                presentSurface.GetPhysicalDeviceSurfaceSupport(device, i, surface.Value, out var presentSupport);
                if (presentSupport) indices.PresentFamily = i;
            }
            else
            {
                indices.PresentFamily =
                    indices.GraphicsFamily; // We can more or less assume that the present family is the same as the graphics family
            }

            if (indices.IsComplete) break;

            i++;
        }

        return indices;
    }

    public static unsafe SwapChainSupportDetails QuerySwapChainSupport(PhysicalDevice physicalDevice,
        SurfaceKHR surface, KhrSurface khrSurface)
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

    public static QueueFamilyIndices FindQueueFamilies(VulkanWindowContext context)
    {
        return FindQueueFamilies(context.Api!, context.PhysicalDevice, context.KhrSurface, context.Surface);
    }
}