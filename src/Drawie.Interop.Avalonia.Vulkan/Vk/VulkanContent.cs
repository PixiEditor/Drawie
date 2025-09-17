using Avalonia;
using Drawie.Numerics;
using Drawie.RenderApi;
using Drawie.RenderApi.Vulkan.Buffers;
using Silk.NET.Vulkan;

namespace Drawie.Interop.Avalonia.Vulkan.Vk;

public class VulkanContent : IDisposable
{
    private readonly VulkanInteropContext context;

    private VecI? _previousImageSize = VecI.Zero;

    public VulkanTexture texture;

    public VulkanContent(VulkanInteropContext context)
    {
        this.context = context;
    }

    public void Render(VulkanImage image, IVkTexture? blit = null)
    {
        var api = context.Api;
        blit ??= texture;

        if (image.Size != _previousImageSize)
            CreateTemporalObjects(image.Size);

        _previousImageSize = image.Size;

        var commandBuffer = context.Pool.CreateCommandBuffer();
        commandBuffer.BeginRecording();

        blit.TransitionLayout(commandBuffer.InternalHandle.Handle, /*(uint)ImageLayout.ColorAttachmentOptimal*/
            (uint)ImageLayout.TransferSrcOptimal, (uint)AccessFlags.MemoryReadBit);

        image.TransitionLayout(commandBuffer.InternalHandle, ImageLayout.TransferDstOptimal,
            AccessFlags.TransferWriteBit);

        var srcBlitRegion = new ImageBlit
        {
            SrcOffsets =
                new ImageBlit.SrcOffsetsBuffer
                {
                    Element0 = new Offset3D(0, 0, 0),
                    Element1 = new Offset3D(image.Size.X, image.Size.Y, 1),
                },
            DstOffsets = new ImageBlit.DstOffsetsBuffer
            {
                Element0 = new Offset3D(0, 0, 0), Element1 = new Offset3D(image.Size.X, image.Size.Y, 1),
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

        api.CmdBlitImage(commandBuffer.InternalHandle, new Image(blit.ImageHandle),
            ImageLayout.TransferSrcOptimal,
            image.InternalHandle, ImageLayout.TransferDstOptimal, 1, srcBlitRegion, Filter.Linear);

        commandBuffer.Submit();

        blit.TransitionLayout(/*(uint)ImageLayout.TransferSrcOptimal,*/
            (uint)ImageLayout.ColorAttachmentOptimal, (uint)AccessFlags.MemoryReadBit);
    }

    public void CreateTextureImage(VecI size)
    {
        texture = new VulkanTexture(context.Api!, context.LogicalDevice.Device, context.PhysicalDevice,
            context.Pool.CommandPool,
            context.GraphicsQueue, context.GraphicsQueueFamilyIndex, size);
    }

    public void Dispose()
    {
        DestroyTemporalObjects();
    }

    public void DestroyTemporalObjects()
    {
        texture?.Dispose();
        _previousImageSize = VecI.Zero;
    }

    public void CreateTemporalObjects(VecI size)
    {
        DestroyTemporalObjects();

        VecI vecSize = new VecI(size.X, size.Y);

        CreateTextureImage(vecSize);

        _previousImageSize = size;
    }
}
