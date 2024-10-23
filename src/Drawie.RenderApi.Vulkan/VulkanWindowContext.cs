using System.Runtime.CompilerServices;
using Drawie.RenderApi.Vulkan.ContextObjects;
using Drawie.RenderApi.Vulkan.Exceptions;
using Drawie.RenderApi.Vulkan.Helpers;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Drawie.RenderApi.Vulkan;

public class VulkanWindowContext : VulkanContext
{
    public Queue PresentQueue { get; set; }

    public SurfaceKHR Surface => surface;
    public KhrSurface KhrSurface => khrSurface;

    private KhrSurface khrSurface;
    private SurfaceKHR surface;


    private unsafe void CreateSurface(IVulkanContextInfo vkContext)
    {
        if (!Api!.TryGetInstanceExtension(Instance, out khrSurface))
            throw new NotSupportedException("KHR_surface extension not found.");

        surface = new VkNonDispatchableHandle(vkContext.GetSurfaceHandle(Instance.Handle)).ToSurface();
    }

    public override void Initialize(IVulkanContextInfo contextInfo)
    {
        Api = Vk.GetApi();
        
        TryAddValidationLayer("VK_LAYER_KHRONOS_validation");
        
        deviceExtensions.Add(KhrSwapchain.ExtensionName);
        
        SetupInstance(contextInfo);
        SetupDebugMessenger();

        if (contextInfo.HasSurface)
        {
            CreateSurface(contextInfo);
        }

        GpuInfo = PickPhysicalDevice();

        CreateLogicalDevice();
    }

    protected override unsafe void CreateLogicalDevice()
    {
        var indices = SetupUtility.FindQueueFamilies(Api!, PhysicalDevice, khrSurface, surface);

        var uniqueQueueFamilies = new[] { indices.GraphicsFamily!.Value };
        if (indices.PresentFamily != null && indices.PresentFamily != indices.GraphicsFamily)
        {
            uniqueQueueFamilies = uniqueQueueFamilies.Append(indices.PresentFamily!.Value).ToArray();
        }

        uniqueQueueFamilies = uniqueQueueFamilies.Distinct().ToArray();

        using var mem = GlobalMemory.Allocate(uniqueQueueFamilies.Length * sizeof(DeviceQueueCreateInfo));
        var queueCreateInfos = (DeviceQueueCreateInfo*)Unsafe.AsPointer(ref mem.GetPinnableReference());

        var queuePriority = 1.0f;
        for (var i = 0; i < uniqueQueueFamilies.Length; i++)
            queueCreateInfos[i] = new DeviceQueueCreateInfo
            {
                SType = StructureType.DeviceQueueCreateInfo,
                QueueFamilyIndex = uniqueQueueFamilies[i],
                QueueCount = 1,
                PQueuePriorities = &queuePriority
            };

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

            EnabledExtensionCount = (uint)deviceExtensions.Count,
            PpEnabledExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(deviceExtensions.ToArray())
        };

        if (EnableValidationLayers)
        {
            createInfo.EnabledLayerCount = (uint)validationLayers.Count;
            createInfo.PpEnabledLayerNames = (byte**)SilkMarshal.StringArrayToPtr(validationLayers.ToArray());
        }
        else
        {
            createInfo.EnabledLayerCount = 0;
        }

        Device logicalDevice = default;

        if (Api!.CreateDevice(PhysicalDevice, in createInfo, null, out logicalDevice) != Result.Success)
            throw new VulkanException("Failed to create logical device.");

        LogicalDevice = new VulkanDevice(Api, logicalDevice);

        Api!.GetDeviceQueue(LogicalDevice.Device, indices.GraphicsFamily!.Value, 0, out var graphicsQueue);
        GraphicsQueue = graphicsQueue;
        if (indices.GraphicsFamily == indices.PresentFamily)
        {
            PresentQueue = graphicsQueue;
        }
        else
        {
            Api!.GetDeviceQueue(logicalDevice, indices.PresentFamily!.Value, 0, out var presentQueue);
            PresentQueue = presentQueue;
        }

        if (EnableValidationLayers) SilkMarshal.Free((nint)createInfo.PpEnabledLayerNames);
    }

    protected override bool IsDeviceSuitable(PhysicalDevice device)
    {
        var indices = SetupUtility.FindQueueFamilies(Api!, device, khrSurface, surface);

        var extensionsSupported = CheckDeviceExtensionSupport(device);

        var swapChainAdequate = false;
        if (extensionsSupported)
        {
            var swapChainSupport = SetupUtility.QuerySwapChainSupport(device, surface, khrSurface);
            swapChainAdequate = swapChainSupport.Formats.Any() && swapChainSupport.PresentModes.Any();
        }

        var features = Api!.GetPhysicalDeviceFeatures(device);

        return indices.IsComplete && extensionsSupported && swapChainAdequate;
    }

    public override unsafe void Dispose()
    {
        LogicalDevice.Dispose();
        if (EnableValidationLayers)
        {
            extDebugUtils?.DestroyDebugUtilsMessenger(Instance, debugMessenger, null);
        }

        khrSurface?.DestroySurface(Instance, surface, null);
        Api!.DestroyInstance(Instance, null);
        Api!.Dispose();
    }
}