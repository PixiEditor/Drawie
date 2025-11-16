using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Drawie.Interop.Avalonia.Vulkan.Vk;
using Drawie.Numerics;
using Drawie.RenderApi.Vulkan.Buffers;
using Drawie.RenderApi.Vulkan.ContextObjects;
using Drawie.RenderApi.Vulkan.Exceptions;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan;

public class VulkanOffscreenContext : VulkanContext
{
    private VulkanCommandBufferPool Pool { get; set; }
    private string[] supportedImageHandleTypes;

    private static string[] s_requiredCommonDeviceExtensions =
    {
        "VK_KHR_external_memory",
        "VK_KHR_external_semaphore",
        "VK_KHR_dedicated_allocation",
    };

    private static string[] s_requiredLinuxDeviceExtensions =
        s_requiredCommonDeviceExtensions.Concat(new[] { "VK_KHR_external_semaphore_fd", "VK_KHR_external_memory_fd" })
            .ToArray();

    private static string[] s_requiredWin32DeviceExtensions = s_requiredCommonDeviceExtensions.Concat(new[]
    {
        "VK_KHR_external_semaphore_win32", "VK_KHR_external_memory_win32"
    }).ToArray();

    public static string[] RequiredDeviceExtensions = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
        ? s_requiredWin32DeviceExtensions
        : s_requiredLinuxDeviceExtensions;


    public override void Initialize(IVulkanContextInfo contextInfo)
    {
        Api = Vk.GetApi();

        TryAddValidationLayer("VK_LAYER_KHRONOS_validation");

        deviceExtensions.AddRange(RequiredDeviceExtensions);

        SetupInstance(contextInfo);
        SetupDebugMessenger();

        GpuInfo = PickPhysicalDevice();

        CreateLogicalDevice();
        CreatePool();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            //TODO: keyed muted
            supportedImageHandleTypes = new[] { "VulkanOpaqueNtHandle", "VulkanOpaqueKmtHandle", };
            /*
            SupportedSemaphoreTypes = new[]
            {
                KnownPlatformGraphicsExternalSemaphoreHandleTypes.VulkanOpaqueNtHandle,
                KnownPlatformGraphicsExternalSemaphoreHandleTypes.VulkanOpaqueKmtHandle
            };
        */
        }
        else
        {
            supportedImageHandleTypes = new[] { "VulkanOpaquePosixFileDescriptor", };
            /*
            SupportedSemaphoreTypes = new[]
            {
                KnownPlatformGraphicsExternalSemaphoreHandleTypes.VulkanOpaquePosixFileDescriptor
            };
        */
        }
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

        PhysicalDeviceFeatures deviceFeatures = new() { SamplerAnisotropy = false };

        DeviceCreateInfo createInfo = new()
        {
            SType = StructureType.DeviceCreateInfo,
            QueueCreateInfoCount = (uint)uniqueQueueFamilies.Length,
            PQueueCreateInfos = queueCreateInfos,
            PEnabledFeatures = &deviceFeatures,
            EnabledExtensionCount = (uint)deviceExtensions.Count,
            PpEnabledExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(deviceExtensions.ToArray()),
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

        return CheckDeviceExtensionSupport(device);
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

    public override ITexture CreateExportableTexture(VecI textureSize)
    {
        return new VulkanTexture(Api!, Instance, LogicalDevice.Device, PhysicalDevice, Pool.CommandPool,
            GraphicsQueue, (uint)GraphicsQueueFamilyIndex, textureSize, supportedImageHandleTypes);
    }

    private unsafe void CreatePool()
    {
        Api!.GetDeviceQueue(LogicalDevice.Device, GraphicsQueueFamilyIndex, 0, out var queue);
        GraphicsQueue = queue;

        Pool = new VulkanCommandBufferPool(Api, LogicalDevice.Device, queue, (uint)GraphicsQueueFamilyIndex);
    }
}
