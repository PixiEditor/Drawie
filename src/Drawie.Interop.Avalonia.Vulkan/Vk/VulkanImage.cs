using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Platform;
using Drawie.Interop.Avalonia.Core;
using Drawie.Numerics;
using Drawie.RenderApi;
using Drawie.RenderApi.Vulkan.Extensions;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Semaphore = Silk.NET.Vulkan.Semaphore;

namespace Drawie.Interop.Avalonia.Vulkan.Vk;

public class VulkanImage : IDisposable, IExportableTexture, IVkTexture
{
    private readonly VulkanInteropContext _vk;
    private readonly Instance _instance;
    private readonly Device _device;
    private readonly PhysicalDevice _physicalDevice;
    private readonly VulkanCommandBufferPool _commandBufferPool;
    private ImageLayout _currentLayout;
    private AccessFlags _currentAccessFlags;
    private uint _tiling;
    private ImageUsageFlags _imageUsageFlags { get; }
    private ImageView _imageView { get; set; }
    private DeviceMemory _imageMemory { get; set; }
    //private readonly SharpDX.Direct3D11.Texture2D? _d3dTexture2D;

    internal Image InternalHandle { get; private set; }
    internal Format Format { get; }
    internal ImageAspectFlags AspectFlags { get; }

    public ulong Handle => InternalHandle.Handle;
    public ulong ViewHandle => _imageView.Handle;
    public uint QueueFamily => _vk.GraphicsQueueFamilyIndex;
    public uint ImageFormat => (uint)Format;
    public ulong ImageHandle => InternalHandle.Handle;
    public uint UsageFlags => (uint)_imageUsageFlags;
    public uint Layout => (uint)_currentLayout;
    public uint TargetSharingMode => (uint)SharingMode.Exclusive;

    uint IVkTexture.Tiling => _tiling;

    public void MakeReadOnly()
    {
        throw new NotImplementedException();
    }

    public void MakeWriteable()
    {
        throw new NotImplementedException();
    }

    public void TransitionLayout(ulong to, ulong readBit)
    {
        TransitionLayout((ImageLayout)to, (AccessFlags)readBit);
    }

    public void TransitionLayout(IntPtr commandBufferHandle, ulong to, ulong readBit)
    {
        CommandBuffer commandBuffer = new CommandBuffer(commandBufferHandle);
        TransitionLayout(commandBuffer, (ImageLayout)to, (AccessFlags)readBit);
    }

    public void TransitionLayout(uint to)
    {
        throw new NotImplementedException();
    }

    public ulong MemoryHandle => _imageMemory.Handle;
    public DeviceMemory DeviceMemory => _imageMemory;
    public uint MipLevels { get; }
    public Silk.NET.Vulkan.Vk Api { get; }
    public VecI Size { get; }

    public ulong MemorySize { get; }

    public uint CurrentLayout => (uint)_currentLayout;


    public unsafe VulkanImage(VulkanInteropContext vk, uint format, VecI size,
        bool exportable, IReadOnlyList<string> supportedHandleTypes)
    {
        _vk = vk;
        _instance = vk.Instance;
        _device = vk.LogicalDevice.Device;
        _physicalDevice = vk.PhysicalDevice;
        _commandBufferPool = vk.Pool;
        Format = (Format)format;
        Api = vk.Api!;
        Size = size;
        MipLevels = 1; //mipLevels;
        _imageUsageFlags =
            ImageUsageFlags.ColorAttachmentBit | ImageUsageFlags.TransferDstBit |
            ImageUsageFlags.TransferSrcBit | ImageUsageFlags.SampledBit;

        //MipLevels = MipLevels != 0 ? MipLevels : (uint)Math.Floor(Math.Log(Math.Max(Size.Width, Size.Height), 2));

        var handleType = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? (supportedHandleTypes.Contains(KnownPlatformGraphicsExternalImageHandleTypes.D3D11TextureNtHandle)
               && !supportedHandleTypes.Contains(KnownPlatformGraphicsExternalImageHandleTypes.VulkanOpaqueNtHandle)
                ? ExternalMemoryHandleTypeFlags.D3D11TextureBit
                : ExternalMemoryHandleTypeFlags.OpaqueWin32Bit)
            : ExternalMemoryHandleTypeFlags.OpaqueFDBit;

        var externalMemoryCreateInfo = new ExternalMemoryImageCreateInfo
        {
            SType = StructureType.ExternalMemoryImageCreateInfo, HandleTypes = handleType
        };

        var imageCreateInfo = new ImageCreateInfo
        {
            PNext = exportable ? &externalMemoryCreateInfo : null,
            SType = StructureType.ImageCreateInfo,
            ImageType = ImageType.Type2D,
            Format = Format,
            Extent =
                new Extent3D((uint?)Size.X,
                    (uint?)Size.Y, 1),
            MipLevels = MipLevels,
            ArrayLayers = 1,
            Samples = SampleCountFlags.Count1Bit,
            Tiling = Tiling,
            Usage = _imageUsageFlags,
            SharingMode = SharingMode.Exclusive,
            InitialLayout = ImageLayout.Undefined,
            Flags = ImageCreateFlags.CreateMutableFormatBit
        };

        Api
            .CreateImage(_device, imageCreateInfo, null, out var image).ThrowOnError("Failed to create image");
        InternalHandle = image;

        Api.GetImageMemoryRequirements(_device, InternalHandle,
            out var memoryRequirements);

        var dedicatedAllocation = new MemoryDedicatedAllocateInfoKHR
        {
            SType = StructureType.MemoryDedicatedAllocateInfoKhr, Image = image
        };

        var fdExport = new ExportMemoryAllocateInfo
        {
            HandleTypes = handleType, SType = StructureType.ExportMemoryAllocateInfo, PNext = &dedicatedAllocation
        };

        ImportMemoryWin32HandleInfoKHR handleImport = default;
        /*if (handleType == ExternalMemoryHandleTypeFlags.D3D11TextureBit && exportable)
        {
            var d3dDevice = vk.D3DDevice ?? throw new NotSupportedException("Vulkan D3DDevice wasn't created");
            _d3dTexture2D = D3DMemoryHelper.CreateMemoryHandle(d3dDevice, size, Format);
            using var dxgi = _d3dTexture2D.QueryInterface<SharpDX.DXGI.Resource1>();

            handleImport = new ImportMemoryWin32HandleInfoKHR
            {
                PNext = &dedicatedAllocation,
                SType = StructureType.ImportMemoryWin32HandleInfoKhr,
                HandleType = ExternalMemoryHandleTypeFlags.D3D11TextureBit,
                Handle = dxgi.CreateSharedHandle(null, SharedResourceFlags.Read | SharedResourceFlags.Write),
            };
        }*/

        var memoryAllocateInfo = new MemoryAllocateInfo
        {
            PNext =
                exportable ? handleImport.Handle != IntPtr.Zero ? &handleImport : &fdExport : null,
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = memoryRequirements.Size,
            MemoryTypeIndex = (uint)VulkanMemoryHelper.FindSuitableMemoryTypeIndex(
                Api,
                _physicalDevice,
                memoryRequirements.MemoryTypeBits, MemoryPropertyFlags.DeviceLocalBit)
        };

        Api.AllocateMemory(_device, memoryAllocateInfo, null,
            out var imageMemory).ThrowOnError("Failed to allocate image memory");

        _imageMemory = imageMemory;


        MemorySize = memoryRequirements.Size;

        Api.BindImageMemory(_device, InternalHandle, _imageMemory, 0).ThrowOnError("Failed to bind image memory");
        var componentMapping = new ComponentMapping(
            ComponentSwizzle.Identity,
            ComponentSwizzle.Identity,
            ComponentSwizzle.Identity,
            ComponentSwizzle.Identity);

        AspectFlags = ImageAspectFlags.ColorBit;

        var subresourceRange = new ImageSubresourceRange(AspectFlags, 0, MipLevels, 0, 1);

        var imageViewCreateInfo = new ImageViewCreateInfo
        {
            SType = StructureType.ImageViewCreateInfo,
            Image = InternalHandle,
            ViewType = ImageViewType.Type2D,
            Format = Format,
            Components = componentMapping,
            SubresourceRange = subresourceRange
        };

        Api
            .CreateImageView(_device, imageViewCreateInfo, null, out var imageView)
            .ThrowOnError("Failed to create image view");

        _imageView = imageView;

        _currentLayout = ImageLayout.Undefined;

        TransitionLayout(ImageLayout.ColorAttachmentOptimal, AccessFlags.NoneKhr);
    }

    public void PrepareForImport(object waitSemaphore, object signalSemaphore)
    {
        if (signalSemaphore is not Silk.NET.Vulkan.Semaphore vkSemaphore)
            throw new ArgumentException("The semaphore must be a Vulkan semaphore.", nameof(signalSemaphore));

        if (waitSemaphore is not Silk.NET.Vulkan.Semaphore vkWait)
            throw new ArgumentException("The semaphore must be a Vulkan semaphore.", nameof(waitSemaphore));

        var commandBuffer = _commandBufferPool.CreateCommandBuffer();
        commandBuffer.BeginRecording();

        TransitionLayout(ImageLayout.TransferSrcOptimal, AccessFlags.TransferWriteBit);

        commandBuffer.EndRecording();
        commandBuffer.Submit(null, null, new [] { vkSemaphore });
    }

    public void BlitFrom(ITexture texture)
    {
        BlitFrom(texture, null, null);
    }

    public void BlitFrom(ITexture texture, object? renderFinishedSemaphore, object? blitSignalSemaphore)
    {
        if (texture is not IVkTexture vkTexture)
            throw new ArgumentException("The texture must be a VulkanTexture.", nameof(texture));

        var commandBuffer = _commandBufferPool.CreateCommandBuffer();
        commandBuffer.BeginRecording();

        Image from = new Image(vkTexture.ImageHandle);
        vkTexture.TransitionLayout(commandBuffer.Handle, (ulong)ImageLayout.TransferSrcOptimal,
            (ulong)AccessFlags.TransferReadBit);

        TransitionLayout(commandBuffer.InternalHandle,
            ImageLayout.TransferDstOptimal, AccessFlags.TransferWriteBit);

        var srcBlitRegion = new ImageBlit()
        {
            SrcOffsets =
                new ImageBlit.SrcOffsetsBuffer
                {
                    Element0 = new Offset3D(0, 0, 0),
                    Element1 = new Offset3D(vkTexture.Size.X, vkTexture.Size.Y, 1),
                },
            DstOffsets = new ImageBlit.DstOffsetsBuffer
            {
                Element0 = new Offset3D(0, 0, 0), Element1 = new Offset3D(Size.X, Size.Y, 1),
            },
            SrcSubresource =
                new ImageSubresourceLayers
                {
                    AspectMask = ImageAspectFlags.ColorBit, BaseArrayLayer = 0, LayerCount = 1, MipLevel = 0
                },
            DstSubresource = new ImageSubresourceLayers
            {
                AspectMask = ImageAspectFlags.ColorBit, BaseArrayLayer = 0, LayerCount = 1, MipLevel = 0
            }
        };

        Api.CmdBlitImage(commandBuffer.InternalHandle,
            from, ImageLayout.TransferSrcOptimal,
            InternalHandle, ImageLayout.TransferDstOptimal,
            1, srcBlitRegion, Filter.Linear);

        if (renderFinishedSemaphore != null && renderFinishedSemaphore is not Silk.NET.Vulkan.Semaphore)
            throw new ArgumentException("The semaphore must be a Vulkan semaphore.", nameof(renderFinishedSemaphore));

        if (blitSignalSemaphore != null && blitSignalSemaphore is not Silk.NET.Vulkan.Semaphore)
            throw new ArgumentException("The semaphore must be a Vulkan semaphore.", nameof(blitSignalSemaphore));

        commandBuffer.EndRecording();

        Semaphore renderFinishedSemaphoreVk = renderFinishedSemaphore != null
            ? (Semaphore)renderFinishedSemaphore
            : default;

        Semaphore blitSignalSemaphoreVk = blitSignalSemaphore != null
            ? (Semaphore)blitSignalSemaphore
            : default;


        Silk.NET.Vulkan.Semaphore[] waitSems = null;
        PipelineStageFlags[] waitStages = null;
        if (renderFinishedSemaphore != null)
        {
            waitSems = new[] { renderFinishedSemaphoreVk };
        }

        Silk.NET.Vulkan.Semaphore[] signalSems = null;
        if (blitSignalSemaphore != null)
            signalSems = new[] { blitSignalSemaphoreVk };

        commandBuffer.Submit(waitSems, waitStages, signalSems);

        vkTexture.TransitionLayout((uint)ImageLayout.ColorAttachmentOptimal, (uint)AccessFlags.ColorAttachmentReadBit);
    }

    public int ExportFd()
    {
        if (!Api.TryGetDeviceExtension<KhrExternalMemoryFd>(_instance, _device, out var ext))
            throw new InvalidOperationException();
        var info = new MemoryGetFdInfoKHR
        {
            Memory = _imageMemory,
            SType = StructureType.MemoryGetFDInfoKhr,
            HandleType = ExternalMemoryHandleTypeFlags.OpaqueFDBit
        };
        ext.GetMemoryF(_device, info, out var fd).ThrowOnError("Failed to get memory fd");
        return fd;
    }

    public IntPtr ExportOpaqueNtHandle()
    {
        if (!Api.TryGetDeviceExtension<KhrExternalMemoryWin32>(_instance, _device, out var ext))
            throw new InvalidOperationException();
        var info = new MemoryGetWin32HandleInfoKHR()
        {
            Memory = _imageMemory,
            SType = StructureType.MemoryGetWin32HandleInfoKhr,
            HandleType = ExternalMemoryHandleTypeFlags.OpaqueWin32Bit
        };
        ext.GetMemoryWin32Handle(_device, info, out var fd).ThrowOnError("Failed to get memory handle");
        return fd;
    }

    public IPlatformHandle Export()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            /*if (_d3dTexture2D != null)
            {
                using var dxgi = _d3dTexture2D!.QueryInterface<Resource1>();
                return new PlatformHandle(
                    dxgi.CreateSharedHandle(null, SharedResourceFlags.Read | SharedResourceFlags.Write),
                    KnownPlatformGraphicsExternalImageHandleTypes.D3D11TextureNtHandle);
            }*/

            return new PlatformHandle(ExportOpaqueNtHandle(),
                KnownPlatformGraphicsExternalImageHandleTypes.VulkanOpaqueNtHandle);
        }
        else
            return new PlatformHandle(new IntPtr(ExportFd()),
                KnownPlatformGraphicsExternalImageHandleTypes.VulkanOpaquePosixFileDescriptor);
    }

    public ImageTiling Tiling => ImageTiling.Optimal;

    //public bool IsDirectXBacked => _d3dTexture2D != null;

    internal void TransitionLayout(CommandBuffer commandBuffer,
        ImageLayout fromLayout, AccessFlags fromAccessFlags,
        ImageLayout destinationLayout, AccessFlags destinationAccessFlags)
    {
        VulkanMemoryHelper.TransitionLayout(Api, commandBuffer, InternalHandle,
            fromLayout,
            fromAccessFlags,
            destinationLayout, destinationAccessFlags,
            MipLevels);

        _currentLayout = destinationLayout;
        _currentAccessFlags = destinationAccessFlags;
    }

    internal void TransitionLayout(Image imageHandle, CommandBuffer commandBuffer,
        ImageLayout fromLayout, AccessFlags fromAccessFlags,
        ImageLayout destinationLayout, AccessFlags destinationAccessFlags)
    {
        VulkanMemoryHelper.TransitionLayout(Api, commandBuffer, imageHandle,
            fromLayout,
            fromAccessFlags,
            destinationLayout, destinationAccessFlags,
            MipLevels);
    }

    internal void TransitionLayout(CommandBuffer commandBuffer,
        ImageLayout destinationLayout, AccessFlags destinationAccessFlags)
        => TransitionLayout(commandBuffer, _currentLayout, _currentAccessFlags, destinationLayout,
            destinationAccessFlags);


    internal void TransitionLayout(ImageLayout destinationLayout, AccessFlags destinationAccessFlags)
    {
        var commandBuffer = _commandBufferPool.CreateCommandBuffer();
        commandBuffer.BeginRecording();
        TransitionLayout(commandBuffer.InternalHandle, destinationLayout, destinationAccessFlags);
        commandBuffer.EndRecording();
        commandBuffer.Submit();
    }

    public void TransitionLayout(uint destinationLayout, uint destinationAccessFlags)
    {
        TransitionLayout((ImageLayout)destinationLayout, (AccessFlags)destinationAccessFlags);
    }

    public unsafe void Dispose()
    {
        Api.DestroyImageView(_device, _imageView, null);
        Api.DestroyImage(_device, InternalHandle, null);
        Api.FreeMemory(_device, _imageMemory, null);

        _imageView = default;
        InternalHandle = default;
        _imageMemory = default;
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }
}
