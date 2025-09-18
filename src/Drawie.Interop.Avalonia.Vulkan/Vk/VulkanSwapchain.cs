using Avalonia;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using Drawie.Interop.Avalonia.Core;
using Drawie.Numerics;
using Drawie.RenderApi;
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

    public override VulkanSwapchainImage CreateImage(VecI size)
    {
        return new VulkanSwapchainImage(_vk, size, Interop, Target);
    }

    public (Action<VecI> present, IDisposable returnToPool) BeginDraw(VecI size, out VulkanImage image)
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
    private ICompositionImportedGpuSemaphore? _availableSemaphore, _renderCompletedSemaphore;
    private ICompositionImportedGpuImage? _importedImage;
    private Task? _lastPresent;
    public VulkanImage Image => _image;
    private bool _initial = true;

    public VulkanSwapchainImage(VulkanInteropContext vk, VecI size, ICompositionGpuInterop interop,
        CompositionDrawingSurface target)
    {
        _vk = vk;
        _interop = interop;
        _target = target;
        Size = size;
        _image = new VulkanImage(vk, (uint)Format.R8G8B8A8Unorm, size, true, interop.SupportedImageHandleTypes);
        _semaphorePair = new VulkanSemaphorePair(vk, interop.SupportedImageHandleTypes, true);
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
        _image.Dispose();
    }

    public VecI Size { get; }

    public Task? LastPresent => _lastPresent;

    public void BeginDraw()
    {
        var buffer = _vk.Pool.CreateCommandBuffer();
        buffer.BeginRecording();

        _image.TransitionLayout(buffer.InternalHandle,
            ImageLayout.Undefined, AccessFlags.None,
            ImageLayout.ColorAttachmentOptimal, AccessFlags.ColorAttachmentReadBit);

        if (_initial)
        {
            _initial = false;
            buffer.Submit();
        }
        else
            buffer.Submit(new[] { _semaphorePair.ImageAvailableSemaphore },
                new[] { PipelineStageFlags.AllGraphicsBit });

        PrepareForPresent();
    }

    private void PrepareForPresent()
    {
        var buffer = _vk.Pool.CreateCommandBuffer();
        buffer.BeginRecording();
        _image.TransitionLayout(buffer.InternalHandle, ImageLayout.TransferSrcOptimal, AccessFlags.TransferWriteBit);

        buffer.Submit(null, null, new[] { _semaphorePair.RenderFinishedSemaphore });
    }


    public Task Present(VecI size)
    {
        /*var buffer = _vk.Pool.CreateCommandBuffer();
        buffer.BeginRecording();
        _image.TransitionLayout(buffer.InternalHandle, ImageLayout.TransferSrcOptimal, AccessFlags.TransferWriteBit);

        buffer.Submit(null, null, new[] { _semaphorePair.RenderFinishedSemaphore });*/


        /*var exportable = _vk.CreateExportableTexture(size);*/
        //exportable.BlitFrom(_image, _semaphorePair.RenderFinishedSemaphore, _semaphorePair.ImageAvailableSemaphore);
        _availableSemaphore ??= _interop.ImportSemaphore(_semaphorePair.Export(false));

        _renderCompletedSemaphore ??= _interop.ImportSemaphore(_semaphorePair.Export(true));

        _importedImage ??= _interop.ImportImage(_image.Export(),
            new PlatformGraphicsExternalImageProperties
            {
                Format = PlatformGraphicsExternalImageFormat.R8G8B8A8UNorm,
                Width = Size.X,
                Height = Size.Y,
                MemorySize = _image.MemorySize
            });

        _lastPresent =
            _target.UpdateWithSemaphoresAsync(_importedImage, _renderCompletedSemaphore!, _availableSemaphore!)
                .ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        Console.WriteLine("Failed to present frame: " + t.Exception?.Message);
                    }
                });

        return _lastPresent;
    }

    public FrameHandle ExportFrame()
    {
        return new FrameHandle
        {
            ImageHandle = _image.Export(),
            AvailableSemaphore = _semaphorePair.Export(false),
            RenderCompletedSemaphore = _semaphorePair.Export(true),
            MemorySize = _image.MemorySize,
            Size = Size
        };
    }

    public uint QueueFamily => _vk.GraphicsQueueFamilyIndex;
    public uint ImageFormat => (uint)Format.R8G8B8A8Unorm;
    public ulong ImageHandle => _image.InternalHandle.Handle;
    public uint UsageFlags => _image.UsageFlags;
    public uint Layout => _image.CurrentLayout;
    public uint TargetSharingMode => (uint)SharingMode.Exclusive;
    public uint Tiling => (uint)ImageTiling.Optimal;

    public void MakeReadOnly()
    {
        throw new NotImplementedException();
    }

    public void MakeWriteable()
    {
        throw new NotImplementedException();
    }
}
