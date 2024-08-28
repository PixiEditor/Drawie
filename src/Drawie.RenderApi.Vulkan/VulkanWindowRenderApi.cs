using System.Runtime.InteropServices;
using Drawie.RenderApi.Vulkan.Exceptions;
using Silk.NET.Core;
using Silk.NET.Core.Contexts;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;

namespace Drawie.RenderApi.Vulkan;

public class VulkanWindowRenderApi : IWindowRenderApi
{
    private Vk? vk;
    private Instance instance;
    private bool enableValidationLayers = true;
    private PhysicalDevice physicalDevice;
    private PhysicalDeviceFeatures2 physicalDeviceFeatures;
    
    private readonly string[] validationLayers = 
    {
        "VK_LAYER_KHRONOS_validation"
    };
    
    private ExtDebugUtils extDebugUtils;
    private DebugUtilsMessengerEXT debugMessenger;
    
    //private Queue graphicsQueue;
    private Device logicalDevice;
    
    public GraphicsApi GraphicsApi => GraphicsApi.Vulkan;
    public unsafe void CreateInstance(object surfaceObject)
    {
        if(surfaceObject is not IVkSurface vkSurface)
        {
            throw new VulkanNotSupportedException();
        }
        

        SetupInstance(vkSurface);
        SetupDebugMessenger();
        PickPhysicalDevice();
        CreateLogicalDevice();
        
        physicalDeviceFeatures = GetPhysicalDeviceFeatures2(physicalDevice);
    }
    
    private unsafe PhysicalDeviceFeatures2 GetPhysicalDeviceFeatures2(PhysicalDevice physicalDevice)
    {
        PhysicalDeviceFeatures2 features2 = new();
        PhysicalDeviceFeatures features = new();
        vk!.GetPhysicalDeviceFeatures(physicalDevice, out features);
        features2.Features = features;
        
        return features2;
    }

    public unsafe void DestroyInstance()
    {
        vk!.DestroyDevice(logicalDevice, null);
        if (enableValidationLayers)
        {
            extDebugUtils!.DestroyDebugUtilsMessenger(instance, debugMessenger, null);
        }
        
        //khrSurface!.DestroySurface(instance, surface, null);
        vk!.DestroyInstance(instance, null);
        vk!.Dispose();
    }

    private unsafe void SetupInstance(IVkSurface surface)
    {
        vk = Vk.GetApi();
        
        ThrowIfValidationLayersNotSupported();
        
        ApplicationInfo appInfo = new()
        {
            SType = StructureType.ApplicationInfo,
            PApplicationName = (byte*)Marshal.StringToHGlobalAnsi("Drawie"),
            ApplicationVersion = new Version32(1, 0, 0),
            PEngineName = (byte*)Marshal.StringToHGlobalAnsi("Drawie Engine"),
            EngineVersion = new Version32(1, 0, 0),
            ApiVersion = Vk.Version11
        };
        
        InstanceCreateInfo createInfo = new()
        {
            SType = StructureType.InstanceCreateInfo,
            PApplicationInfo = &appInfo
        };
        
        var extensions = GetExtensions(surface);
        
        createInfo.EnabledExtensionCount = (uint)extensions.Length;
        createInfo.PpEnabledExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(extensions);

        if (enableValidationLayers)
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

        if(vk.CreateInstance(&createInfo, null, out instance) != Result.Success)
        {
            throw new VulkanException("Failed to create instance.");
        }
        
        Marshal.FreeHGlobal((nint)appInfo.PApplicationName);
        Marshal.FreeHGlobal((nint)appInfo.PEngineName);
        SilkMarshal.Free((nint)createInfo.PpEnabledExtensionNames);
        
        if (enableValidationLayers)
        {
            SilkMarshal.Free((nint)createInfo.PpEnabledLayerNames);
        }
    }

    private void PickPhysicalDevice()
    {
        IReadOnlyCollection<PhysicalDevice>? devices = vk!.GetPhysicalDevices(instance);
        foreach (PhysicalDevice device in devices)
        {
            if (IsDeviceSuitable(device))
            {
                physicalDevice = device;
            }
        }
        
        if (physicalDevice.Handle == 0)
        {
            throw new VulkanException("Failed to find a suitable Vulkan GPU.");
        }
    }

    private unsafe void CreateLogicalDevice()
    {
        var indices = FindQueueFamilies(physicalDevice);
        DeviceQueueCreateInfo queueCreateInfo = new()
        {
            SType = StructureType.DeviceQueueCreateInfo,
            QueueFamilyIndex = indices.GraphicsFamily!.Value,
            QueueCount = 1,
        };
        
        float queuePriority = 1.0f;
        queueCreateInfo.PQueuePriorities = &queuePriority;
        PhysicalDeviceFeatures deviceFeatures = new();
        
        DeviceCreateInfo createInfo = new()
        {
            SType = StructureType.DeviceCreateInfo,
            QueueCreateInfoCount = 1,
            PQueueCreateInfos = &queueCreateInfo,

            PEnabledFeatures = &deviceFeatures,

            EnabledExtensionCount = 0
        };

        if (enableValidationLayers)
        {
            createInfo.EnabledLayerCount = (uint)validationLayers.Length;
            createInfo.PpEnabledLayerNames = (byte**)SilkMarshal.StringArrayToPtr(validationLayers);
        }
        else
        {
            createInfo.EnabledLayerCount = 0;
        }

        if (vk!.CreateDevice(physicalDevice, in createInfo, null, out logicalDevice) != Result.Success)
        {
            throw new VulkanException("failed to create logical device.");
        }

        //vk!.GetDeviceQueue(logicalDevice, indices.GraphicsFamily!.Value, 0, out graphicsQueue);

        if (enableValidationLayers)
        {
            SilkMarshal.Free((nint)createInfo.PpEnabledLayerNames);
        }
    }
    
    private bool IsDeviceSuitable(PhysicalDevice device)
    {
        QueueFamilyIndices indices = FindQueueFamilies(device);
        return indices.IsComplete;
    }

    private unsafe QueueFamilyIndices FindQueueFamilies(PhysicalDevice device)
    {
        var indices = new QueueFamilyIndices();

        uint queueFamilityCount = 0;
        vk!.GetPhysicalDeviceQueueFamilyProperties(device, ref queueFamilityCount, null);

        var queueFamilies = new QueueFamilyProperties[queueFamilityCount];
        fixed (QueueFamilyProperties* queueFamiliesPtr = queueFamilies)
        {
            vk!.GetPhysicalDeviceQueueFamilyProperties(device, ref queueFamilityCount, queueFamiliesPtr);
        }


        uint i = 0;
        foreach (var queueFamily in queueFamilies)
        {
            if (queueFamily.QueueFlags.HasFlag(QueueFlags.GraphicsBit))
            {
                indices.GraphicsFamily = i;
            }

            if (indices.IsComplete)
            {
                break;
            }

            i++;
        }

        return indices; 
    }

    private unsafe string[] GetExtensions(IVkSurface surface)
    {
        var windowExtensions = surface.GetRequiredExtensions(out var count);
        var extensions = SilkMarshal.PtrToStringArray((nint)windowExtensions, (int)count);

        if (enableValidationLayers)
        {
            return extensions.Append(ExtDebugUtils.ExtensionName).ToArray();
        }
        
        return extensions;
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

    private unsafe void SetupDebugMessenger()
    {
        if(!enableValidationLayers)
        {
            return;
        }
        
        if (!vk!.TryGetInstanceExtension(instance, out extDebugUtils)) return;

        DebugUtilsMessengerCreateInfoEXT createInfo = new();
        PopulateDebugMessengerCreateInfo(ref createInfo);

        if (extDebugUtils!.CreateDebugUtilsMessenger(instance, in createInfo, null, out debugMessenger) != Result.Success)
        {
            throw new Exception("failed to set up debug messenger!");
        } 
    }

    private void ThrowIfValidationLayersNotSupported()
    {
        if (enableValidationLayers && !CheckValidationLayerSupport())
        {
            throw new VulkanException("validation layers requested, but not available!");
        }
    }
    
    private unsafe bool CheckValidationLayerSupport()
    {
        uint layerCount = 0;
        vk!.EnumerateInstanceLayerProperties(ref layerCount, null);
        var availableLayers = new LayerProperties[layerCount];
        fixed (LayerProperties* availableLayersPtr = availableLayers)
        {
            vk!.EnumerateInstanceLayerProperties(ref layerCount, availableLayersPtr);
        }

        var availableLayerNames = availableLayers.Select(layer => Marshal.PtrToStringAnsi((IntPtr)layer.LayerName)).ToHashSet();

        return validationLayers.All(availableLayerNames.Contains);
    }
    
    private unsafe uint DebugCallback(DebugUtilsMessageSeverityFlagsEXT messageSeverity, DebugUtilsMessageTypeFlagsEXT messageTypes, DebugUtilsMessengerCallbackDataEXT* pCallbackData, void* pUserData)
    {
        Console.WriteLine($"validation layer:" + Marshal.PtrToStringAnsi((nint)pCallbackData->PMessage));

        return Vk.False;
    } 
}