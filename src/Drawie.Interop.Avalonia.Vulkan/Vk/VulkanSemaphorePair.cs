using System.Runtime.InteropServices;
using Avalonia.Platform;
using Drawie.RenderApi.Vulkan.Extensions;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Semaphore = Silk.NET.Vulkan.Semaphore;

namespace Drawie.Interop.Avalonia.Vulkan.Vk;

public class VulkanSemaphorePair : IDisposable
{
     private readonly VulkanInteropContext _resources;

    public unsafe VulkanSemaphorePair(VulkanInteropContext resources,
        IReadOnlyList<string> supportedHandleTypes, bool exportable)
    {
        _resources = resources;

        var semaphoreExportInfo = new ExportSemaphoreCreateInfo
        {
            SType = StructureType.ExportSemaphoreCreateInfo,
            HandleTypes = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? (supportedHandleTypes.Contains(KnownPlatformGraphicsExternalImageHandleTypes.D3D11TextureNtHandle)
               && !supportedHandleTypes.Contains(KnownPlatformGraphicsExternalImageHandleTypes.VulkanOpaqueNtHandle)
                ? ExternalSemaphoreHandleTypeFlags.D3D11FenceBit
                : ExternalSemaphoreHandleTypeFlags.OpaqueWin32Bit)
            : ExternalSemaphoreHandleTypeFlags.OpaqueFDBit
        };

        var semaphoreCreateInfo = new SemaphoreCreateInfo
        {
            SType = StructureType.SemaphoreCreateInfo,
            PNext = exportable ? &semaphoreExportInfo : null
        };

        resources.Api!.CreateSemaphore(resources.LogicalDevice.Device, semaphoreCreateInfo, null, out var semaphore).ThrowOnError("Failed to create semaphore");
        ImageAvailableSemaphore = semaphore;

        resources.Api.CreateSemaphore(resources.LogicalDevice.Device, semaphoreCreateInfo, null, out semaphore).ThrowOnError("Failed to create semaphore");
        RenderFinishedSemaphore = semaphore;
    }

    public int ExportFd(bool renderFinished)
    {
        if (!_resources.Api!.TryGetDeviceExtension<KhrExternalSemaphoreFd>(_resources.Instance, _resources.LogicalDevice.Device,
                out var ext))
            throw new InvalidOperationException();
        var info = new SemaphoreGetFdInfoKHR()
        {
            SType = StructureType.SemaphoreGetFDInfoKhr,
            Semaphore = renderFinished ? RenderFinishedSemaphore : ImageAvailableSemaphore,
            HandleType = ExternalSemaphoreHandleTypeFlags.OpaqueFDBit
        };
        ext.GetSemaphoreF(_resources.LogicalDevice.Device, info, out var fd).ThrowOnError("Failed to export semaphore");
        return fd;
    }
    
    public IntPtr ExportWin32(bool renderFinished)
    {
        if (!_resources.Api!.TryGetDeviceExtension<KhrExternalSemaphoreWin32>(_resources.Instance, _resources.LogicalDevice.Device,
                out var ext))
            throw new InvalidOperationException();
        var info = new SemaphoreGetWin32HandleInfoKHR()
        {
            SType = StructureType.SemaphoreGetWin32HandleInfoKhr,
            Semaphore = renderFinished ? RenderFinishedSemaphore : ImageAvailableSemaphore,
            HandleType = ExternalSemaphoreHandleTypeFlags.OpaqueWin32Bit
        };
        ext.GetSemaphoreWin32Handle(_resources.LogicalDevice.Device, info, out var fd).ThrowOnError("Failed to export semaphore");
        return fd;
    }

    public IPlatformHandle Export(bool renderFinished)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            return new PlatformHandle(ExportWin32(renderFinished),
                KnownPlatformGraphicsExternalSemaphoreHandleTypes.VulkanOpaqueNtHandle);
        return new PlatformHandle(new IntPtr(ExportFd(renderFinished)),
            KnownPlatformGraphicsExternalSemaphoreHandleTypes.VulkanOpaquePosixFileDescriptor);
    }

    internal Semaphore ImageAvailableSemaphore { get; }
    internal Semaphore RenderFinishedSemaphore { get; }

    public unsafe void Dispose()
    {
        _resources.Api!.DestroySemaphore(_resources.LogicalDevice.Device, ImageAvailableSemaphore, null);
        _resources.Api!.DestroySemaphore(_resources.LogicalDevice.Device, RenderFinishedSemaphore, null);
    } 
}
