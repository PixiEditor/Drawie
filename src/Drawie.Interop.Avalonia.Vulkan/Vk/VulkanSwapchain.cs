using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using Drawie.Interop.Avalonia.Core;
using Silk.NET.Vulkan;

namespace Drawie.Interop.Avalonia.Vulkan.Vk;

public class VulkanSwapchain : SwapchainBase<VulkanSwapchainImage>
{
    private readonly VulkanInteropContext _vk;

    public VulkanSwapchain(VulkanInteropContext vk, ICompositionGpuInterop interop,
        CompositionDrawingSurface target) : base(
        interop, target)
    {
        _vk = vk;
    }

    protected override VulkanSwapchainImage CreateImage(PixelSize size)
    {
        return new VulkanSwapchainImage(_vk, size, Interop, Target);
    }

    public IDisposable BeginDraw(PixelSize size, out VulkanImage image)
    {
        _vk.Pool.FreeUsedCommandBuffers();
        var rv = BeginDrawCore(size, out var swapchainImage);
        image = swapchainImage.Image;
        return rv;
    }
}

public class VulkanSwapchainImage : ISwapchainImage
{
    private readonly VulkanInteropContext _vk;
    private readonly ICompositionGpuInterop _interop;
    private readonly CompositionDrawingSurface _target;
    private readonly VulkanImage _image;
    private readonly VulkanSemaphorePair _semaphorePair;
    private readonly VulkanTimelineSemaphore? _timelineSemaphore;
    private ICompositionImportedGpuSemaphore? _availableSemaphore, _renderCompletedSemaphore, _importedTimelineSemaphore;
    private ICompositionImportedGpuImage? _importedImage;
    private Task? _lastPresent;
    public VulkanImage Image => _image;
    private bool _initial = true;
    private ulong _timelineCounter;

    public VulkanSwapchainImage(VulkanInteropContext vk, PixelSize size, ICompositionGpuInterop interop,
        CompositionDrawingSurface target)
    {
        _vk = vk;
        _interop = interop;
        _target = target;
        Size = size;
        var format = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? Format.B8G8R8A8Unorm : Format.R8G8B8A8Unorm;
        _image = new VulkanImage(vk, (uint)format, size, true, interop.SupportedImageHandleTypes);
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            _timelineSemaphore = new(vk);
        else
            _semaphorePair = new VulkanSemaphorePair(vk, true);
    }

    public async ValueTask DisposeAsync()
    {
        // Below sometimes got stuck and resources never had a chance to be disposed
        if (LastPresent != null)
            await LastPresent;
        if (_importedImage != null)
            await _importedImage.DisposeAsync();
        if (_availableSemaphore != null)
            await _availableSemaphore.DisposeAsync();
        if (_renderCompletedSemaphore != null)
            await _renderCompletedSemaphore.DisposeAsync();

        _semaphorePair.Dispose();
        _timelineSemaphore?.Dispose();
        _image.Dispose();
    }

    public PixelSize Size { get; }

    public Task? LastPresent => _lastPresent;

    public void BeginDraw()
    {
        var buffer = _vk.Pool.CreateCommandBuffer();
        buffer.BeginRecording();

        _image.TransitionLayout(buffer.InternalHandle,
            ImageLayout.Undefined, AccessFlags.None,
            ImageLayout.ColorAttachmentOptimal, AccessFlags.ColorAttachmentReadBit);

        if (_image.IsDirectXBacked)
            buffer.Submit(null, null, null, null,
                new VulkanCommandBufferPool.VulkanCommandBuffer.KeyedMutexSubmitInfo
                {
                    AcquireKey = 0, DeviceMemory = _image.DeviceMemory
                });
        else if (_timelineSemaphore != null)
        {
            unsafe
            {
                var wait = _timelineCounter;
                var submitInfo = new TimelineSemaphoreSubmitInfo
                {
                    PWaitSemaphoreValues = &wait,
                    WaitSemaphoreValueCount = 1,
                    SType = StructureType.TimelineSemaphoreSubmitInfo
                };
                var waitSemaphores = new[] { _timelineSemaphore.Handle };

                buffer.Submit(waitSemaphores, pNext: (IntPtr)(&submitInfo));
            }
        }
        else if (_initial)
        {
            _initial = false;
            buffer.Submit();
        }
        else
            buffer.Submit(new[] { _semaphorePair!.ImageAvailableSemaphore },
                new[] { PipelineStageFlags.AllGraphicsBit });
    }


    public void Present()
    {
        var buffer = _vk.Pool.CreateCommandBuffer();
        buffer.BeginRecording();
        _image.TransitionLayout(buffer.InternalHandle, ImageLayout.TransferSrcOptimal, AccessFlags.TransferWriteBit);

        if (_image.IsDirectXBacked)
        {
            buffer.Submit(null, null, null, null,
                new VulkanCommandBufferPool.VulkanCommandBuffer.KeyedMutexSubmitInfo
                {
                    DeviceMemory = _image.DeviceMemory, ReleaseKey = 1
                });
        }
        else if (_timelineSemaphore != null)
        {
            unsafe
            {
                var signal = _timelineCounter + 1;
                var submitInfo = new TimelineSemaphoreSubmitInfo
                {
                    PSignalSemaphoreValues = &signal,
                    SignalSemaphoreValueCount = 1,
                    SType = StructureType.TimelineSemaphoreSubmitInfo
                };
                var signalSemaphores = new[] { _timelineSemaphore.Handle };

                buffer.Submit(default, signalSemaphores: signalSemaphores, pNext: (IntPtr)(&submitInfo));
            }
        }
        else
            buffer.Submit(null, null, new[] { _semaphorePair!.RenderFinishedSemaphore });

        if (_timelineSemaphore != null)
        {
            _importedTimelineSemaphore ??= _interop.ImportSemaphore(_timelineSemaphore.Export());
        }
        else if (!_image.IsDirectXBacked)
        {
            _availableSemaphore ??= _interop.ImportSemaphore(_semaphorePair!.Export(false));

            _renderCompletedSemaphore ??= _interop.ImportSemaphore(_semaphorePair!.Export(true));
        }

        _importedImage ??= _interop.ImportImage(_image.Export(),
            new PlatformGraphicsExternalImageProperties
            {
                Format = RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
                    ? PlatformGraphicsExternalImageFormat.B8G8R8A8UNorm
                    : PlatformGraphicsExternalImageFormat.R8G8B8A8UNorm,
                Width = Size.Width,
                Height = Size.Height,
                MemorySize = _image.MemorySize
            });
        if (_importedTimelineSemaphore != null)
        {
            _lastPresent = _target.UpdateWithTimelineSemaphoresAsync(_importedImage,
                _importedTimelineSemaphore, _timelineCounter + 1, _importedTimelineSemaphore, _timelineCounter + 2);
            _timelineCounter += 2;
        }
        else if (_image.IsDirectXBacked)
            _lastPresent = _target.UpdateWithKeyedMutexAsync(_importedImage, 1, 0);
        else
            _lastPresent =
                _target.UpdateWithSemaphoresAsync(_importedImage, _renderCompletedSemaphore!, _availableSemaphore!);
    }
}
