using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Drawie.RenderApi.Vulkan.ContextObjects;
using Drawie.RenderApi.Vulkan.Exceptions;
using Drawie.RenderApi.Vulkan.Helpers;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Drawie.RenderApi.Vulkan;

public class VulkanContext : IDisposable
{
    public Vk? Vk { get; private set; }

    public Instance Instance
    {
        get => instance;
        private set => instance = value;
    }

    public bool EnableValidationLayers { get; set; } = true;
    public PhysicalDevice PhysicalDevice { get; private set; }

    public VulkanDevice LogicalDevice { get; private set; }

    public Queue GraphicsQueue { get; set; }
    public Queue PresentQueue { get; set; }

    public uint GraphicsQueueFamilyIndex { get; set; }
    
    public SurfaceKHR? Surface => surface;
    public KhrSurface? KhrSurface => khrSurface;

    public GpuInfo GpuInfo { get; set; }

    private Instance instance;

    private readonly string[] validationLayers =
    {
        "VK_LAYER_KHRONOS_validation"
    };

    private readonly string[] deviceExtensions =
    {
        KhrSwapchain.ExtensionName
    };

    private ExtDebugUtils extDebugUtils;
    private DebugUtilsMessengerEXT debugMessenger;

    private KhrSurface? khrSurface;
    private SurfaceKHR? surface;

    public VulkanContext()
    {
    }

    public void Initialize(IVulkanContextInfo contextInfo)
    {
        Vk = Vk.GetApi();
        SetupInstance(contextInfo);
        SetupDebugMessenger();

        if (contextInfo.HasSurface)
        {
            CreateSurface(contextInfo);
        }

        GpuInfo = PickPhysicalDevice();

        CreateLogicalDevice();
    }

    private unsafe void SetupInstance(IVulkanContextInfo contextInfo)
    {
        ThrowIfValidationLayersNotSupported();

        ApplicationInfo appInfo = new()
        {
            SType = StructureType.ApplicationInfo,
            PApplicationName = (byte*)Marshal.StringToHGlobalAnsi("Drawie"),
            ApplicationVersion = new Version32(1, 0, 0),
            PEngineName = (byte*)Marshal.StringToHGlobalAnsi("Drawie Engine"),
            EngineVersion = new Version32(1, 0, 0),
            ApiVersion = Vk.Version12
        };

        InstanceCreateInfo createInfo = new()
        {
            SType = StructureType.InstanceCreateInfo,
            PApplicationInfo = &appInfo
        };

        var extensions = GetExtensions(contextInfo);

        createInfo.EnabledExtensionCount = (uint)extensions.Length;
        createInfo.PpEnabledExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(extensions);

        if (EnableValidationLayers)
        {
            createInfo.EnabledLayerCount = (uint)validationLayers.Length;
            createInfo.PpEnabledLayerNames = (byte**)SilkMarshal.StringArrayToPtr(validationLayers);

            DebugUtilsMessengerCreateInfoEXT debugCreateInfo = new();
            PopulateDebugMessengerCreateInfo(ref debugCreateInfo);
            createInfo.PNext = &debugCreateInfo;
        }
        else
        {
            createInfo.EnabledLayerCount = 0;
            createInfo.PNext = null;
        }

        if (Vk!.CreateInstance(&createInfo, null, out instance) != Result.Success)
            throw new VulkanException("Failed to create instance.");

        Marshal.FreeHGlobal((nint)appInfo.PApplicationName);
        Marshal.FreeHGlobal((nint)appInfo.PEngineName);
        SilkMarshal.Free((nint)createInfo.PpEnabledExtensionNames);

        if (EnableValidationLayers) SilkMarshal.Free((nint)createInfo.PpEnabledLayerNames);
    }


    private unsafe void SetupDebugMessenger()
    {
        if (!EnableValidationLayers) return;

        if (!Vk!.TryGetInstanceExtension(Instance, out extDebugUtils)) return;

        DebugUtilsMessengerCreateInfoEXT createInfo = new();
        PopulateDebugMessengerCreateInfo(ref createInfo);

        if (extDebugUtils!.CreateDebugUtilsMessenger(Instance, in createInfo, null, out debugMessenger) !=
            Result.Success)
            throw new VulkanException("failed to set up debug messenger!");
    }

    private unsafe GpuInfo PickPhysicalDevice()
    {
        var devices = Vk!.GetPhysicalDevices(Instance);
        foreach (var device in devices)
        {
            if (IsDeviceSuitable(device))
            {
                var props = Vk.GetPhysicalDeviceProperties(device);
                var name = props.DeviceName;
                var deviceName = Marshal.PtrToStringAnsi((nint)name);

                if (deviceName == null) throw new VulkanException("Failed to get device name.");

                GpuInfo gpuInfo = new(deviceName);
                PhysicalDevice = device;
                return gpuInfo;
            }
        }

        if (PhysicalDevice.Handle == 0) throw new VulkanException("Failed to find a suitable Vulkan GPU.");

        return new GpuInfo("Unknown");
    }

    private unsafe void CreateLogicalDevice()
    {
        var indices = SetupUtility.FindQueueFamilies(Vk!, PhysicalDevice, khrSurface, surface);

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

            EnabledExtensionCount = (uint)deviceExtensions.Length,
            PpEnabledExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(deviceExtensions)
        };

        if (EnableValidationLayers)
        {
            createInfo.EnabledLayerCount = (uint)validationLayers.Length;
            createInfo.PpEnabledLayerNames = (byte**)SilkMarshal.StringArrayToPtr(validationLayers);
        }
        else
        {
            createInfo.EnabledLayerCount = 0;
        }

        Device logicalDevice = default;
        
        if (Vk!.CreateDevice(PhysicalDevice, in createInfo, null, out logicalDevice) != Result.Success)
            throw new VulkanException("Failed to create logical device.");
        
        LogicalDevice = new VulkanDevice(Vk, logicalDevice);

        Vk!.GetDeviceQueue(LogicalDevice.Device, indices.GraphicsFamily!.Value, 0, out var graphicsQueue);
        GraphicsQueue = graphicsQueue;
        if (indices.GraphicsFamily == indices.PresentFamily)
        {
            PresentQueue = graphicsQueue;
        }
        else
        {
            Vk!.GetDeviceQueue(logicalDevice, indices.PresentFamily!.Value, 0, out var presentQueue);
            PresentQueue = presentQueue;
        }

        if (EnableValidationLayers) SilkMarshal.Free((nint)createInfo.PpEnabledLayerNames);
    }

    private unsafe void CreateSurface(IVulkanContextInfo vkContext)
    {
        if (!Vk!.TryGetInstanceExtension(instance, out khrSurface))
            throw new NotSupportedException("KHR_surface extension not found.");

        surface = new VkNonDispatchableHandle(vkContext.GetSurfaceHandle(instance.Handle)).ToSurface();
    }

    private bool IsDeviceSuitable(PhysicalDevice device)
    {
        var indices = SetupUtility.FindQueueFamilies(Vk!, device, khrSurface, surface);

        var extensionsSupported = CheckDeviceExtensionSupport(device);

        var swapChainAdequate = true;
        if (extensionsSupported && khrSurface != null && surface != null)
        {
            var swapChainSupport = SetupUtility.QuerySwapChainSupport(device, surface.Value, khrSurface);
            swapChainAdequate = swapChainSupport.Formats.Any() && swapChainSupport.PresentModes.Any();
        }

        var features = Vk!.GetPhysicalDeviceFeatures(device);

        return indices.IsComplete && extensionsSupported && swapChainAdequate;
    }

    private unsafe string[] GetExtensions(IVulkanContextInfo contextInfo)
    {
        string[] contextExtensions = contextInfo.GetRequiredExtensions();
        if (EnableValidationLayers)
        {
            return contextExtensions.Append(ExtDebugUtils.ExtensionName).ToArray();
        }

        return contextExtensions;
    }

    private unsafe void PopulateDebugMessengerCreateInfo(ref DebugUtilsMessengerCreateInfoEXT createInfo)
    {
        createInfo = new DebugUtilsMessengerCreateInfoEXT
        {
            SType = StructureType.DebugUtilsMessengerCreateInfoExt,
            MessageSeverity = DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt |
                              DebugUtilsMessageSeverityFlagsEXT.WarningBitExt |
                              DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt,
            MessageType = DebugUtilsMessageTypeFlagsEXT.GeneralBitExt |
                          DebugUtilsMessageTypeFlagsEXT.PerformanceBitExt |
                          DebugUtilsMessageTypeFlagsEXT.ValidationBitExt,
            PfnUserCallback = (DebugUtilsMessengerCallbackFunctionEXT)DebugCallback
        };
    }

    private void ThrowIfValidationLayersNotSupported()
    {
        if (EnableValidationLayers && !CheckValidationLayerSupport())
            throw new VulkanException("validation layers requested, but not available!");
    }

    private unsafe bool CheckValidationLayerSupport()
    {
        uint layerCount = 0;
        Vk!.EnumerateInstanceLayerProperties(ref layerCount, null);
        var availableLayers = new LayerProperties[layerCount];
        fixed (LayerProperties* availableLayersPtr = availableLayers)
        {
            Vk!.EnumerateInstanceLayerProperties(ref layerCount, availableLayersPtr);
        }

        var availableLayerNames = availableLayers.Select(layer => Marshal.PtrToStringAnsi((IntPtr)layer.LayerName))
            .ToHashSet();

        return validationLayers.All(availableLayerNames.Contains);
    }

    private unsafe bool CheckDeviceExtensionSupport(PhysicalDevice device)
    {
        uint extensionCount = 0;
        Vk!.EnumerateDeviceExtensionProperties(device, (byte*)null, ref extensionCount, null);
        var availableExtensions = new ExtensionProperties[extensionCount];
        fixed (ExtensionProperties* availableExtensionsPtr = availableExtensions)
        {
            Vk!.EnumerateDeviceExtensionProperties(device, (byte*)null, ref extensionCount, availableExtensionsPtr);
        }

        var availableExtensionNames = availableExtensions
            .Select(extension => Marshal.PtrToStringAnsi((nint)extension.ExtensionName)).ToHashSet();

        return deviceExtensions.All(availableExtensionNames.Contains);
    }

    private unsafe uint DebugCallback(DebugUtilsMessageSeverityFlagsEXT messageSeverity,
        DebugUtilsMessageTypeFlagsEXT messageTypes, DebugUtilsMessengerCallbackDataEXT* pCallbackData, void* pUserData)
    {
        Console.WriteLine($"validation layer:" + Marshal.PtrToStringAnsi((nint)pCallbackData->PMessage));

        return Vk.False;
    }


    public unsafe void Dispose()
    {
        LogicalDevice.Dispose();
        if (EnableValidationLayers)
        {
            extDebugUtils?.DestroyDebugUtilsMessenger(Instance, debugMessenger, null);
        }
        khrSurface?.DestroySurface(Instance, surface!.Value, null);
        Vk!.DestroyInstance(Instance, null);
        Vk!.Dispose();
    }
}