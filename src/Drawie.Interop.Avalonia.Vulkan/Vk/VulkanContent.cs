using Avalonia;
using Drawie.Numerics;
using Drawie.RenderApi.Vulkan.Buffers;
using Silk.NET.Vulkan;

namespace Drawie.Interop.Avalonia.Vulkan.Vk;

public class VulkanContent : IDisposable
{
    private readonly VulkanInteropContext context;

    private PixelSize? _previousImageSize = PixelSize.Empty;

    public VulkanTexture texture;

    public VulkanContent(VulkanInteropContext context)
    {
        this.context = context;
    }

    public void Render(VulkanImage image)
    {
        var api = context.Api;

        if (image.Size != _previousImageSize)
            CreateTemporalObjects(image.Size);

        _previousImageSize = image.Size;

        var commandBuffer = context.Pool.CreateCommandBuffer();
        commandBuffer.BeginRecording();

        texture.TransitionLayoutTo(commandBuffer.InternalHandle, ImageLayout.ColorAttachmentOptimal,
            ImageLayout.TransferSrcOptimal);

        image.TransitionLayout(commandBuffer.InternalHandle, ImageLayout.TransferDstOptimal,
            AccessFlags.TransferWriteBit);

        var srcBlitRegion = new ImageBlit
        {
            SrcOffsets =
                new ImageBlit.SrcOffsetsBuffer
                {
                    Element0 = new Offset3D(0, 0, 0),
                    Element1 = new Offset3D(image.Size.Width, image.Size.Height, 1),
                },
            DstOffsets = new ImageBlit.DstOffsetsBuffer
            {
                Element0 = new Offset3D(0, 0, 0), Element1 = new Offset3D(image.Size.Width, image.Size.Height, 1),
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

        api.CmdBlitImage(commandBuffer.InternalHandle, texture.VkImage,
            ImageLayout.TransferSrcOptimal,
            image.InternalHandle, ImageLayout.TransferDstOptimal, 1, srcBlitRegion, Filter.Linear);

        commandBuffer.Submit();

        texture.TransitionLayoutTo((uint)ImageLayout.TransferSrcOptimal,
            (uint)ImageLayout.ColorAttachmentOptimal);
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
        _previousImageSize = PixelSize.Empty;
    }

    public void CreateTemporalObjects(PixelSize size)
    {
        DestroyTemporalObjects();

        VecI vecSize = new VecI(size.Width, size.Height);

        CreateTextureImage(vecSize);

        _previousImageSize = size;
    }
}
