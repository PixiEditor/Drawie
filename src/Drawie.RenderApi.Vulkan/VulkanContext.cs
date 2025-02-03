using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Drawie.RenderApi.Vulkan.ContextObjects;
using Drawie.RenderApi.Vulkan.Exceptions;
using Drawie.RenderApi.Vulkan.Extensions;
using Drawie.RenderApi.Vulkan.Helpers;
using Silk.NET.Core;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;

namespace Drawie.RenderApi.Vulkan;

public abstract class VulkanContext : IDisposable, IVulkanContext
{
    public Vk? Api { get; protected set; }

    public Instance Instance
    {
        get => instance;
        private set => instance = value;
    }

    public bool EnableValidationLayers { get; set; }
#if DEBUG
        = true;
#else
        = false;
#endif
    public PhysicalDevice PhysicalDevice { get; protected set; }

    public VulkanDevice LogicalDevice { get; protected set; }

    public Queue GraphicsQueue { get; set; }

    public uint GraphicsQueueFamilyIndex { get; set; }

    public GpuInfo GpuInfo { get; set; }

    private Instance instance;

    protected List<string> validationLayers = new List<string>();

    protected List<string> deviceExtensions = new List<string>();

    protected ExtDebugUtils extDebugUtils;
    protected DebugUtilsMessengerEXT debugMessenger;

    IntPtr IVulkanContext.LogicalDeviceHandle => LogicalDevice.Device.Handle;
    IntPtr IVulkanContext.PhysicalDeviceHandle => PhysicalDevice.Handle;
    IntPtr IVulkanContext.InstanceHandle => Instance.Handle;
    IntPtr IVulkanContext.GraphicsQueueHandle => GraphicsQueue.Handle;

    public IntPtr GetProcedureAddress(string name, IntPtr instance, IntPtr device)
    {
        return Api!.GetInstanceProcAddr(Instance, name);
    }

    public VulkanContext()
    {
    }

    public abstract void Initialize(IVulkanContextInfo contextInfo);

    protected unsafe void SetupInstance(IVulkanContextInfo contextInfo)
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
            createInfo.EnabledLayerCount = (uint)validationLayers.Count;
            createInfo.PpEnabledLayerNames = (byte**)SilkMarshal.StringArrayToPtr(validationLayers.ToArray());

            DebugUtilsMessengerCreateInfoEXT debugCreateInfo = new();
            PopulateDebugMessengerCreateInfo(ref debugCreateInfo);
            createInfo.PNext = &debugCreateInfo;
        }
        else
        {
            createInfo.EnabledLayerCount = 0;
            createInfo.PNext = null;
        }

        if (Api!.CreateInstance(&createInfo, null, out instance) != Result.Success)
            throw new VulkanException("Failed to create instance.");

        Marshal.FreeHGlobal((nint)appInfo.PApplicationName);
        Marshal.FreeHGlobal((nint)appInfo.PEngineName);
        SilkMarshal.Free((nint)createInfo.PpEnabledExtensionNames);

        if (EnableValidationLayers) SilkMarshal.Free((nint)createInfo.PpEnabledLayerNames);
    }


    protected unsafe void SetupDebugMessenger()
    {
        if (!EnableValidationLayers) return;

        if (!Api!.TryGetInstanceExtension(Instance, out extDebugUtils)) return;

        DebugUtilsMessengerCreateInfoEXT createInfo = new();
        PopulateDebugMessengerCreateInfo(ref createInfo);

        if (extDebugUtils!.CreateDebugUtilsMessenger(Instance, in createInfo, null, out debugMessenger) !=
            Result.Success)
            throw new VulkanException("failed to set up debug messenger!");
    }

    protected unsafe GpuInfo PickPhysicalDevice()
    {
        var devices = Api!.GetPhysicalDevices(Instance);
        foreach (var device in devices)
        {
            if (IsDeviceSuitable(device))
            {
                var props = Api.GetPhysicalDeviceProperties(device);
                var name = props.DeviceName;
                var deviceName = Marshal.PtrToStringAnsi((nint)name);

                if (deviceName == null) throw new VulkanException("Failed to get device name.");

                GpuInfo gpuInfo = new(deviceName, VendorById(props.VendorID));
                PhysicalDevice = device;
                return gpuInfo;
            }
        }

        if (PhysicalDevice.Handle == 0) throw new VulkanException("Failed to find a suitable Vulkan GPU.");

        return new GpuInfo("Unknown", "Unknown");
    }

    private string VendorById(uint vendorId)
    {
        return vendorId switch
        {
            0x1002 => "AMD",
            0x1010 => "ImgTec",
            0x10DE => "NVIDIA",
            0x13B5 => "ARM",
            0x5143 => "Qualcomm",
            0x8086 => "INTEL",
            _ => "Unknown"
        };
    }

    protected abstract void CreateLogicalDevice();

    protected abstract bool IsDeviceSuitable(PhysicalDevice device);

    private unsafe string[] GetExtensions(IVulkanContextInfo contextInfo)
    {
        string[] contextExtensions = contextInfo.GetInstanceExtensions();
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
        Api!.EnumerateInstanceLayerProperties(ref layerCount, null);
        var availableLayers = new LayerProperties[layerCount];
        fixed (LayerProperties* availableLayersPtr = availableLayers)
        {
            Api!.EnumerateInstanceLayerProperties(ref layerCount, availableLayersPtr);
        }

        var availableLayerNames = availableLayers.Select(layer => Marshal.PtrToStringAnsi((IntPtr)layer.LayerName))
            .ToHashSet();

        return validationLayers.All(availableLayerNames.Contains);
    }

    protected unsafe bool CheckDeviceExtensionSupport(PhysicalDevice device)
    {
        uint extensionCount = 0;
        Api!.EnumerateDeviceExtensionProperties(device, (byte*)null, ref extensionCount, null);
        var availableExtensions = new ExtensionProperties[extensionCount];
        fixed (ExtensionProperties* availableExtensionsPtr = availableExtensions)
        {
            Api!.EnumerateDeviceExtensionProperties(device, (byte*)null, ref extensionCount, availableExtensionsPtr);
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

    protected bool TryAddValidationLayer(string layer)
    {
        if (!EnableValidationLayers) return false;

        if (IsLayerAvailable(layer))
        {
            validationLayers.Add(layer);
            return true;
        }

        return false;
    }

    private unsafe bool IsLayerAvailable(string layerName)
    {
        uint layerPropertiesCount;

        Api!.EnumerateInstanceLayerProperties(&layerPropertiesCount, null)
            .ThrowOnError("Failed to enumerate instance layer properties.");

        var layerProperties = new LayerProperties[layerPropertiesCount];

        fixed (LayerProperties* pLayerProperties = layerProperties)
        {
            Api.EnumerateInstanceLayerProperties(&layerPropertiesCount, layerProperties)
                .ThrowOnError("Failed to enumerate instance layer properties.");

            for (var i = 0; i < layerPropertiesCount; i++)
            {
                var currentLayerName = Marshal.PtrToStringAnsi((IntPtr)pLayerProperties[i].LayerName);

                if (currentLayerName == layerName) return true;
            }
        }

        return false;
    }

    public abstract void Dispose();
}
