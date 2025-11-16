using System.Runtime.CompilerServices;
using Drawie.RenderApi.Vulkan.ContextObjects;
using Drawie.RenderApi.Vulkan.Exceptions;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan;

public class VulkanOffscreenContext : VulkanContext
{
    public override void Initialize(IVulkanContextInfo contextInfo)
    {
        Api = Vk.GetApi();

        TryAddValidationLayer("VK_LAYER_KHRONOS_validation");

        SetupInstance(contextInfo);
        SetupDebugMessenger();

        GpuInfo = PickPhysicalDevice();

        CreateLogicalDevice();
    }

    protected override unsafe void CreateLogicalDevice()
    {
        var indices = FindGraphicsQueueFamily(Api!, PhysicalDevice);

        var uniqueQueueFamilies = new[] { indices };

        using var mem = GlobalMemory.Allocate(uniqueQueueFamilies.Length * sizeof(DeviceQueueCreateInfo));
        var queueCreateInfos = (DeviceQueueCreateInfo*)Unsafe.AsPointer(ref mem.GetPinnableReference());

        var queuePriority = 1.0f;
        for (var i = 0; i < uniqueQueueFamilies.Length; i++)
        {
            queueCreateInfos[i] = new DeviceQueueCreateInfo
            {
                SType = StructureType.DeviceQueueCreateInfo,
                QueueFamilyIndex = (uint)uniqueQueueFamilies[i],
                QueueCount = 1,
                PQueuePriorities = &queuePriority
            };
        }

        PhysicalDeviceFeatures deviceFeatures = new()
        {
            SamplerAnisotropy = false
        };

        DeviceCreateInfo createInfo = new()
        {
            SType = StructureType.DeviceCreateInfo,
            QueueCreateInfoCount = (uint)uniqueQueueFamilies.Length,
            PQueueCreateInfos = queueCreateInfos,

            PEnabledFeatures = &deviceFeatures,

            EnabledExtensionCount = 0,
            PpEnabledExtensionNames = null
        };

        if (EnableValidationLayers)
        {
            createInfo.EnabledLayerCount = (uint)validationLayers.Count;
            createInfo.PpEnabledLayerNames = (byte**)SilkMarshal.StringArrayToPtr(validationLayers.ToArray());
        }

        if (Api!.CreateDevice(PhysicalDevice, in createInfo, null, out var logicalDevice) != Result.Success)
            throw new VulkanException("Failed to create logical device.");

        LogicalDevice = new VulkanDevice(Api, logicalDevice);

        Api!.GetDeviceQueue(LogicalDevice.Device, (uint)indices, 0, out var graphicsQueue);
        GraphicsQueue = graphicsQueue;

        if (EnableValidationLayers)
            SilkMarshal.Free((nint)createInfo.PpEnabledLayerNames);
    }

    protected override bool IsDeviceSuitable(PhysicalDevice device)
    {
        int graphicsIndex = FindGraphicsQueueFamily(Api!, device);
        if (graphicsIndex < 0)
            return false;

        // No extension requirements, no swapchain requirements
        return true;
    }

    public override unsafe void Dispose()
    {
        LogicalDevice.Dispose();

        if (EnableValidationLayers)
        {
            extDebugUtils?.DestroyDebugUtilsMessenger(Instance, debugMessenger, null);
        }

        // No surface to destroy

        Api!.DestroyInstance(Instance, null);
        Api!.Dispose();
    }

    private static unsafe int FindGraphicsQueueFamily(Vk api, PhysicalDevice device)
    {
        uint count = 0;
        api.GetPhysicalDeviceQueueFamilyProperties(device, ref count, null);
        var props = new QueueFamilyProperties[count];
        fixed (QueueFamilyProperties* p = props)
        {
            api.GetPhysicalDeviceQueueFamilyProperties(device, ref count, p);
        }

        for (int i = 0; i < props.Length; i++)
        {
            if ((props[i].QueueFlags & QueueFlags.QueueGraphicsBit) != 0)
                return i;
        }

        return -1;
    }
}
