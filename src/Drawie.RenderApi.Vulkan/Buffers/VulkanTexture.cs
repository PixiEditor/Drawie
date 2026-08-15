using Drawie.Numerics;
using Drawie.RenderApi.Abstraction.Textures;
using Drawie.RenderApi.Vulkan.Exceptions;
using Drawie.RenderApi.Vulkan.Helpers;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;
using Image = Silk.NET.Vulkan.Image;

namespace Drawie.RenderApi.Vulkan.Buffers;

public class VulkanTexture : IDisposable, IVkTexture
{
    public Sampler Sampler => sampler;
    public Image VkImage => colorAttachment.Image;
    private Vk Vk { get; }
    private Device LogicalDevice { get; }
    private PhysicalDevice PhysicalDevice { get; }

    private CommandPool CommandPool { get; }

    private Queue GraphicsQueue { get; }
    public uint QueueFamily { get; } = 0;
    public uint ImageFormat { get; private set; }
    public ulong ImageHandle => colorAttachment.Image.Handle;
    public uint Tiling { get; }
    public uint UsageFlags { get; set; }
    public uint Layout => (uint)ImageLayout.ColorAttachmentOptimal;
    public uint TargetSharingMode { get; } = (uint)SharingMode.Exclusive;
    ulong ITexture.TextureId => ImageHandle;
    public uint Width { get; }
    public uint Height { get; }

    public VulkanImageAttachment ColorAttachment => colorAttachment;
    public VulkanImageAttachment? DepthAttachment => depthAttachment;

    private VulkanImageAttachment colorAttachment;
    private VulkanImageAttachment? depthAttachment;
    private Sampler sampler;
    
    public VulkanTexture(Vk vk, Device logicalDevice, PhysicalDevice physicalDevice, CommandPool commandPool,
        Queue graphicsQueue, uint queueFamily, VecI size) : this(vk, logicalDevice, physicalDevice, commandPool, graphicsQueue, queueFamily, new TextureDesc()
    {
        Format = TextureFormat.RGBA8_Unorm, Depth = DepthFormat.NoDepth, Width = size.X, Height = size.Y,
    })
    {
    }

    public VulkanTexture(Vk vk, Device logicalDevice, PhysicalDevice physicalDevice, CommandPool commandPool,
        Queue graphicsQueue, uint queueFamily, TextureDesc desc)
    {
        Vk = vk;
        LogicalDevice = logicalDevice;
        PhysicalDevice = physicalDevice;
        CommandPool = commandPool;
        GraphicsQueue = graphicsQueue;
        QueueFamily = queueFamily;
        ImageFormat = (uint)ToVkFormat(desc.Format);
        Tiling = (uint)ImageTiling.Optimal;
        UsageFlags = (uint)(ImageUsageFlags.SampledBit | ImageUsageFlags.TransferSrcBit | ImageUsageFlags.TransferDstBit | ImageUsageFlags.ColorAttachmentBit);
        Width = (uint)desc.Width;
        Height = (uint)desc.Height;

        colorAttachment = new VulkanImageAttachment(
            Vk,
            LogicalDevice,
            PhysicalDevice,
            CommandPool,
            GraphicsQueue,
            (uint)desc.Width,
            (uint)desc.Height,
            ToVkFormat(desc.Format),
            ImageUsageFlags.SampledBit |
            ImageUsageFlags.TransferSrcBit |
            ImageUsageFlags.TransferDstBit |
            ImageUsageFlags.ColorAttachmentBit,
            ImageAspectFlags.ColorBit);

        colorAttachment.TransitionLayout(
            ImageLayout.ColorAttachmentOptimal);

        if (desc.Depth != DepthFormat.NoDepth)
        {
            var depthFormat = ToVkDepthFormat(desc.Depth);

            depthAttachment = new VulkanImageAttachment(
                Vk,
                LogicalDevice,
                PhysicalDevice,
                CommandPool,
                GraphicsQueue,
                (uint)desc.Width,
                (uint)desc.Height,
                depthFormat,
                ImageUsageFlags.DepthStencilAttachmentBit,
                ImageAspectFlags.DepthBit | ImageAspectFlags.StencilBit);

            depthAttachment.TransitionLayout(ImageLayout.DepthStencilAttachmentOptimal);
        }
        
        CreateSampler();
    }

    private Format ToVkDepthFormat(DepthFormat descDepth)
    {
        switch (descDepth)
        {
            case DepthFormat.NoDepth:   
                throw new ArgumentException("No depth is not supported depth format.");
            case DepthFormat.Depth24Stencil8:
                return Format.D24UnormS8Uint;
            default:
                throw new ArgumentOutOfRangeException(nameof(descDepth), descDepth, null);
        }
    }

    private Format ToVkFormat(TextureFormat descFormat)
    {
        return descFormat switch
        {
            TextureFormat.RGBA8_Unorm => Format.R8G8B8A8Unorm,
            _ => throw new ArgumentOutOfRangeException(nameof(descFormat), descFormat, null)
        };
    }


    public void MakeReadOnly()
    {
        colorAttachment.TransitionLayout(ImageLayout.ShaderReadOnlyOptimal);
    }
    
    public void MakeReadOnly(CommandBuffer cmdBuffer)
    {    
        colorAttachment.TransitionLayout(ImageLayout.ShaderReadOnlyOptimal, cmdBuffer);
    }

    public void MakeWriteable()
    {
        colorAttachment.TransitionLayout(ImageLayout.ColorAttachmentOptimal);
    }

    public event Action? Disposing;

    public void MakeWriteable(CommandBuffer cmdBuffer)
    {
        colorAttachment.TransitionLayout(ImageLayout.ColorAttachmentOptimal, cmdBuffer);
    }

    private unsafe void CreateSampler()
    {
        SamplerCreateInfo samplerCreateInfo = new()
        {
            SType = StructureType.SamplerCreateInfo,
            MagFilter = Filter.Linear,
            MinFilter = Filter.Linear,
            AddressModeU = SamplerAddressMode.Repeat,
            AddressModeV = SamplerAddressMode.Repeat,
            AddressModeW = SamplerAddressMode.Repeat,
            AnisotropyEnable = false,
            MaxAnisotropy = 1, 
            BorderColor = BorderColor.IntOpaqueBlack,
            UnnormalizedCoordinates = false,
            CompareEnable = false,
            CompareOp = CompareOp.Always,
            MipmapMode = SamplerMipmapMode.Linear,
            MipLodBias = 0,
            MinLod = 0,
            MaxLod = 0
        };
        
        fixed (Sampler* samplerPtr = &sampler)
        {
            if (Vk.CreateSampler(LogicalDevice, &samplerCreateInfo, null, samplerPtr) != Result.Success)
                throw new VulkanException("Failed to create a texture sampler.");
        }
    }

    private unsafe void CopyBufferToImage(Buffer buffer, Image image, uint width, uint height)
    {
        using var commandBuffer = new SingleTimeCommandBufferSession(Vk, CommandPool, LogicalDevice, GraphicsQueue);

        var region = new BufferImageCopy()
        {
            BufferOffset = 0,
            BufferRowLength = 0,
            BufferImageHeight = 0,
            ImageSubresource = new ImageSubresourceLayers()
            {
                AspectMask = ImageAspectFlags.ColorBit,
                MipLevel = 0,
                BaseArrayLayer = 0,
                LayerCount = 1
            },
            ImageOffset = new Offset3D(0, 0, 0),
            ImageExtent = new Extent3D(width, height, 1)
        };

        Vk.CmdCopyBufferToImage(commandBuffer.CommandBuffer, buffer, image, ImageLayout.TransferDstOptimal, 1, &region);
    }

    public unsafe void Dispose()
    {
        Disposing?.Invoke();
        Vk.DestroySampler(LogicalDevice, sampler, null);
        ColorAttachment.Dispose();
        DepthAttachment?.Dispose();
    }
}
