using System.Runtime.InteropServices;
using Avalonia.Platform;
using Drawie.RenderApi.Vulkan;
using Drawie.RenderApi.Vulkan.Extensions;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Semaphore = Silk.NET.Vulkan.Semaphore;

namespace Drawie.Interop.Avalonia.Vulkan.Vk;

class VulkanTimelineSemaphore : IDisposable
{
    private VulkanContext _resources;

    public unsafe VulkanTimelineSemaphore(VulkanContext resources)
    {
        _resources = resources;
        var mtlEvent = new ExportMetalObjectCreateInfoEXT
        {
            SType = StructureType.ExportMetalObjectCreateInfoExt,
            ExportObjectType = ExportMetalObjectTypeFlagsEXT.SharedEventBitExt
        };
        
        var semaphoreTypeInfo = new SemaphoreTypeCreateInfoKHR()
        {
            SType = StructureType.SemaphoreTypeCreateInfo,
            SemaphoreType = SemaphoreType.Timeline,
            PNext = &mtlEvent
        };

        var semaphoreCreateInfo = new SemaphoreCreateInfo
        {
            SType = StructureType.SemaphoreCreateInfo,
            PNext = &semaphoreTypeInfo,
        };

        resources.Api.CreateSemaphore(resources.LogicalDevice.Device, in semaphoreCreateInfo, null, out var semaphore).ThrowOnError();
        Handle = semaphore;
    }

    public Semaphore Handle { get; }
    public unsafe void Dispose()
    {
        _resources.Api.DestroySemaphore(_resources.LogicalDevice.Device, Handle, null);
    }

    
    public unsafe IntPtr ExportSharedEvent()
    {
        if (!_resources.Api.TryGetDeviceExtension<ExtMetalObjects>(_resources.Instance, _resources.LogicalDevice.Device, out var ext))
            throw new InvalidOperationException();
        var eventExport = new ExportMetalSharedEventInfoEXT()
        {
            SType = StructureType.ExportMetalSharedEventInfoExt,
            Semaphore = Handle,
        };
        var export = new ExportMetalObjectsInfoEXT()
        {
            SType = StructureType.ExportMetalObjectsInfoExt,
            PNext = &eventExport
        };
        ext.ExportMetalObjects(_resources.LogicalDevice.Device, ref export);
        if (eventExport.MtlSharedEvent == IntPtr.Zero)
            throw new Exception("Unable to export IOSurfaceRef");
        return eventExport.MtlSharedEvent;
    }
    public IPlatformHandle Export()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            return new PlatformHandle(ExportSharedEvent(),
                KnownPlatformGraphicsExternalSemaphoreHandleTypes.MetalSharedEvent);
        throw new PlatformNotSupportedException();
    }
}
