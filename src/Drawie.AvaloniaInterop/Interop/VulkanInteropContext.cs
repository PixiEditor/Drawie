using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using Drawie.RenderApi;
using Drawie.RenderApi.Vulkan;
using Drawie.RenderApi.Vulkan.ContextObjects;
using Drawie.RenderApi.Vulkan.Extensions;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Drawie.AvaloniaGraphics.Interop;

public class VulkanInteropContext : VulkanContext
{
    public DescriptorPool DescriptorPool => descriptorPool; 
    public VulkanCommandBufferPool Pool { get; private set; }

    private List<string> requiredDeviceExtensions = new List<string>();

    private ICompositionGpuInterop gpuInterop;
    private DescriptorPool descriptorPool;

    public VulkanInteropContext(ICompositionGpuInterop gpuInterop)
    {
        this.gpuInterop = gpuInterop;
    }

    public override void Initialize(IVulkanContextInfo contextInfo)
    {
        Api = Vk.GetApi();

        TryAddValidationLayer("VK_LAYER_KHRONOS_validation");

        deviceExtensions.Add("VK_KHR_get_physical_device_properties2");
        deviceExtensions.Add("VK_KHR_external_memory_capabilities");
        deviceExtensions.Add("VK_KHR_external_semaphore_capabilities");
        deviceExtensions.Add("VK_EXT_debug_utils");

        SetupInstance(contextInfo);
        SetupDebugMessenger();

        if (!SetRequiredDeviceExtensions())
        {
            throw new NotSupportedException("Image sharing is not supported by the current backend");
        }

        GpuInfo = PickPhysicalDevice();
        CreateLogicalDevice();
        CreatePool();
    }

    private bool SetRequiredDeviceExtensions()
    {
        requiredDeviceExtensions.Add("VK_KHR_external_memory");
        requiredDeviceExtensions.Add("VK_KHR_external_semaphore");

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (!(gpuInterop.SupportedImageHandleTypes.Contains(KnownPlatformGraphicsExternalImageHandleTypes
                      .D3D11TextureGlobalSharedHandle)
                  || gpuInterop.SupportedImageHandleTypes.Contains(KnownPlatformGraphicsExternalImageHandleTypes
                      .VulkanOpaqueNtHandle))
               )
                return false;
            requiredDeviceExtensions.Add(KhrExternalMemoryWin32.ExtensionName);
            requiredDeviceExtensions.Add(KhrExternalSemaphoreWin32.ExtensionName);
            requiredDeviceExtensions.Add("VK_KHR_dedicated_allocation");
            requiredDeviceExtensions.Add("VK_KHR_get_memory_requirements2");
        }
        else
        {
            if (!gpuInterop.SupportedImageHandleTypes.Contains(KnownPlatformGraphicsExternalImageHandleTypes
                    .VulkanOpaquePosixFileDescriptor)
                || !gpuInterop.SupportedSemaphoreTypes.Contains(KnownPlatformGraphicsExternalSemaphoreHandleTypes
                    .VulkanOpaquePosixFileDescriptor)
               )
                return false;
            requiredDeviceExtensions.Add(KhrExternalMemoryFd.ExtensionName);
            requiredDeviceExtensions.Add(KhrExternalSemaphoreFd.ExtensionName);
        }

        return true;
    }

    protected override unsafe bool IsDeviceSuitable(PhysicalDevice device)
    {
        if (requiredDeviceExtensions.Any(x => !Api!.IsDeviceExtensionPresent(device, x)))
            return false;

        var physicalDeviceIDProperties = new PhysicalDeviceIDProperties()
        {
            SType = StructureType.PhysicalDeviceIDProperties
        };

        var physicalDeviceProperties2 = new PhysicalDeviceProperties2()
        {
            SType = StructureType.PhysicalDeviceProperties2,
            PNext = &physicalDeviceIDProperties
        };

        Api!.GetPhysicalDeviceProperties2(device, &physicalDeviceProperties2);

        if (gpuInterop.DeviceLuid != null && physicalDeviceIDProperties.DeviceLuidvalid)
        {
            if (!new Span<byte>(physicalDeviceIDProperties.DeviceLuid, 8)
                    .SequenceEqual(gpuInterop.DeviceLuid))
                return false;
        }
        else if (gpuInterop.DeviceUuid != null)
        {
            if (!new Span<byte>(physicalDeviceIDProperties.DeviceUuid, 16)
                    .SequenceEqual(gpuInterop.DeviceUuid))
                return false;
        }

        return true;
    }

    protected override unsafe void CreateLogicalDevice()
    {
        uint queueFamilyCount = 0;
        Api!.GetPhysicalDeviceQueueFamilyProperties(PhysicalDevice, ref queueFamilyCount, null);
        var familyProperties = stackalloc QueueFamilyProperties[(int)queueFamilyCount];
        Api!.GetPhysicalDeviceQueueFamilyProperties(PhysicalDevice, ref queueFamilyCount, familyProperties);

        for (uint i = 0; i < queueFamilyCount; i++)
        {
            var family = familyProperties[i];
            if (!familyProperties[i].QueueFlags.HasFlag(QueueFlags.GraphicsBit))
            {
                continue;
            }

            var priorities = stackalloc float[(int)family.QueueCount];
            for (uint j = 0; j < family.QueueCount; j++)
            {
                priorities[j] = 1.0f;
            }

            var features = new PhysicalDeviceFeatures()
            {
                SamplerAnisotropy = false
            };

            var queueCreateInfo = new DeviceQueueCreateInfo()
            {
                SType = StructureType.DeviceQueueCreateInfo,
                QueueFamilyIndex = i,
                QueueCount = family.QueueCount,
                PQueuePriorities = priorities
            };

            var deviceCreateInfo = new DeviceCreateInfo
            {
                SType = StructureType.DeviceCreateInfo,
                QueueCreateInfoCount = 1,
                PQueueCreateInfos = &queueCreateInfo,
                PpEnabledExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(requiredDeviceExtensions.ToArray()),
                EnabledExtensionCount = (uint)requiredDeviceExtensions.Count,
                PEnabledFeatures = &features
            };

            Device logicalDevice = default;
            Api!.CreateDevice(PhysicalDevice, &deviceCreateInfo, null, out logicalDevice)
                .ThrowOnError("Could not create logical device");
            LogicalDevice = new VulkanDevice(Api, logicalDevice);
            GraphicsQueueFamilyIndex = i;
            break;
        }
    }

    private unsafe void CreatePool()
    {
        Api!.GetDeviceQueue(LogicalDevice.Device, GraphicsQueueFamilyIndex, 0, out var queue);
        GraphicsQueue = queue;

        var poolSize = new DescriptorPoolSize()
        {
            Type = DescriptorType.CombinedImageSampler,
            DescriptorCount = 1,
        };

        fixed (DescriptorPool* descriptorPoolPtr = &descriptorPool)
        {
            DescriptorPoolCreateInfo poolInfo = new()
            {
                SType = StructureType.DescriptorPoolCreateInfo,
                PoolSizeCount = 1,
                PPoolSizes = &poolSize,
                MaxSets = 1,
                Flags = DescriptorPoolCreateFlags.FreeDescriptorSetBit
            };

            Api!.CreateDescriptorPool(LogicalDevice.Device, poolInfo, null, descriptorPoolPtr)
                .ThrowOnError("Could not create descriptor pool");
        }

        Pool = new VulkanCommandBufferPool(Api, LogicalDevice.Device, queue, (uint)GraphicsQueueFamilyIndex);
    }

    public override unsafe void Dispose()
    {
        Api!.DestroyDescriptorPool(LogicalDevice.Device, descriptorPool, null);
        Api!.DestroyDevice(LogicalDevice.Device, null);
        Api!.DestroyInstance(Instance, null);
    }
}