using Drawie.Numerics;
using Drawie.RenderApi.Vulkan.Exceptions;
using Drawie.RenderApi.Vulkan.Helpers;
using Silk.NET.Vulkan;
using Buffer = Silk.NET.Vulkan.Buffer;
using Image = Silk.NET.Vulkan.Image;

namespace Drawie.RenderApi.Vulkan.Buffers;

public class VulkanTexture : IDisposable, IVkTexture
{
    public ImageView ImageView { get; private set; }
    public Sampler Sampler => sampler;
    public Image VkImage => textureImage;
    private Vk Vk { get; }
    private Device LogicalDevice { get; }
    private PhysicalDevice PhysicalDevice { get; }

    private CommandPool CommandPool { get; }

    private Queue GraphicsQueue { get; }
    public uint QueueFamily { get; } = 0;
    public uint ImageFormat { get; private set; }
    public ulong ImageHandle => textureImage.Handle;
    public uint Tiling { get; }
    public uint UsageFlags { get; set; }
    public uint Layout => ColorAttachmentOptimal;
    public uint TargetSharingMode { get; } = (uint)SharingMode.Exclusive;
    public static uint ColorAttachmentOptimal => (uint)ImageLayout.ColorAttachmentOptimal;
    public static uint ShaderReadOnlyOptimal => (uint)ImageLayout.ShaderReadOnlyOptimal;

    private Image textureImage;
    private DeviceMemory textureImageMemory;
    private Sampler sampler;
    
    public unsafe VulkanTexture(Vk vk, Device logicalDevice, PhysicalDevice physicalDevice, CommandPool commandPool,
        Queue graphicsQueue, uint queueFamily, VecI size)
    {
        Vk = vk;
        LogicalDevice = logicalDevice;
        PhysicalDevice = physicalDevice;
        CommandPool = commandPool;
        GraphicsQueue = graphicsQueue;
        QueueFamily = queueFamily;

        /*var imageSize = (ulong)size.X * (ulong)size.Y * 4;

        using var stagingBuffer = new StagingBuffer(vk, logicalDevice, physicalDevice, imageSize);

        void* data;
        vk!.MapMemory(LogicalDevice, stagingBuffer.VkBufferMemory, 0, imageSize, 0, &data);
        image.CopyPixelDataTo(new Span<byte>(data, (int)imageSize));
        vk!.UnmapMemory(LogicalDevice, stagingBuffer.VkBufferMemory);*/

        ImageFormat = (uint)Format.R8G8B8A8Unorm;
        Tiling = (uint)ImageTiling.Optimal;
        UsageFlags = (uint)(ImageUsageFlags.SampledBit | ImageUsageFlags.TransferSrcBit | ImageUsageFlags.TransferDstBit | ImageUsageFlags.ColorAttachmentBit);

        CreateImage((uint)size.X, (uint)size.Y, (Format)ImageFormat, (ImageTiling)Tiling,
            (ImageUsageFlags)UsageFlags, MemoryPropertyFlags.DeviceLocalBit);

        /*TransitionImageLayout(textureImage, Format.R8G8B8A8Srgb, ImageLayout.Undefined, ImageLayout.TransferDstOptimal);
        CopyBufferToImage(stagingBuffer.VkBuffer, textureImage, (uint)size.X, (uint)size.Y);
        TransitionImageLayout(textureImage, Format.R8G8B8A8Srgb, ImageLayout.TransferDstOptimal,
            ImageLayout.ShaderReadOnlyOptimal);*/
        
        TransitionImageLayout(textureImage, (Format)ImageFormat, ImageLayout.Undefined, ImageLayout.ColorAttachmentOptimal);

        ImageView = ImageUtility.CreateViewForImage(Vk, LogicalDevice, textureImage, Format.R8G8B8A8Unorm);
        
        CreateSampler();
    }
    
    
    public void MakeReadOnly()
    {
        TransitionLayoutTo(ColorAttachmentOptimal, ShaderReadOnlyOptimal);
    }

    public void MakeWriteable()
    {
        TransitionLayoutTo(ShaderReadOnlyOptimal, ColorAttachmentOptimal); 
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

    private unsafe void CreateImage(uint width, uint height, Format format, ImageTiling tiling, ImageUsageFlags usage,
        MemoryPropertyFlags properties)
    {
        ImageCreateInfo imageInfo = new()
        {
            SType = StructureType.ImageCreateInfo,
            ImageType = ImageType.Type2D,
            Extent = new Extent3D(width, height, 1),
            MipLevels = 1,
            ArrayLayers = 1,
            Format = format,
            Tiling = tiling,
            InitialLayout = ImageLayout.Undefined,
            Usage = usage,
            Samples = SampleCountFlags.Count1Bit,
            SharingMode = SharingMode.Exclusive
        };

        fixed (Image* imagePtr = &textureImage)
        {
            if (Vk.CreateImage(LogicalDevice, &imageInfo, null, imagePtr) != Result.Success)
                throw new VulkanException("Failed to create an image.");
        }

        Vk.GetImageMemoryRequirements(LogicalDevice, textureImage, out var memRequirements);

        MemoryAllocateInfo allocInfo = new()
        {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = memRequirements.Size,
            MemoryTypeIndex =
                BufferObject.FindMemoryType(Vk, PhysicalDevice, memRequirements.MemoryTypeBits, properties)
        };

        fixed (DeviceMemory* memoryPtr = &textureImageMemory)
        {
            if (Vk.AllocateMemory(LogicalDevice, &allocInfo, null, memoryPtr) != Result.Success)
                throw new VulkanException("Failed to allocate image memory.");
        }

        Vk.BindImageMemory(LogicalDevice, textureImage, textureImageMemory, 0);
    }

    private unsafe void TransitionImageLayout(Image image, Format format, ImageLayout oldLayout, ImageLayout newLayout)
    {
        using var commandBuffer = new SingleTimeCommandBufferSession(Vk, CommandPool, LogicalDevice, GraphicsQueue);

        TransitionImageLayout(image, oldLayout, newLayout, commandBuffer.CommandBuffer);
    }

    private unsafe void TransitionImageLayout(Image image, ImageLayout oldLayout, ImageLayout newLayout,
        CommandBuffer commandBuffer)
    {
        var barrier = new ImageMemoryBarrier()
        {
            SType = StructureType.ImageMemoryBarrier,
            OldLayout = oldLayout,
            NewLayout = newLayout,
            SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
            DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
            Image = image,
            SubresourceRange = new ImageSubresourceRange()
            {
                AspectMask = ImageAspectFlags.ColorBit,
                BaseMipLevel = 0,
                LevelCount = 1,
                BaseArrayLayer = 0,
                LayerCount = 1
            }
        };

        PipelineStageFlags sourceStage;
        PipelineStageFlags destinationStage;

        if (oldLayout == ImageLayout.Undefined)
        {
            barrier.SrcAccessMask = 0;
            sourceStage = PipelineStageFlags.TopOfPipeBit;
        }
        else if (oldLayout == ImageLayout.ColorAttachmentOptimal)
        {
            barrier.SrcAccessMask = AccessFlags.ColorAttachmentWriteBit;
            sourceStage = PipelineStageFlags.ColorAttachmentOutputBit;
        }
        else if (oldLayout == ImageLayout.ShaderReadOnlyOptimal)
        {
            barrier.SrcAccessMask = AccessFlags.ShaderReadBit;
            sourceStage = PipelineStageFlags.FragmentShaderBit;
        }
        else
        {
            barrier.SrcAccessMask = AccessFlags.MemoryReadBit;
            sourceStage = PipelineStageFlags.BottomOfPipeBit;
        }
        
        if (newLayout == ImageLayout.ColorAttachmentOptimal)
        {
            barrier.DstAccessMask = AccessFlags.ColorAttachmentWriteBit;
            destinationStage = PipelineStageFlags.ColorAttachmentOutputBit;
        }
        else if (newLayout == ImageLayout.ShaderReadOnlyOptimal)
        {
            barrier.DstAccessMask = AccessFlags.ShaderReadBit;
            destinationStage = PipelineStageFlags.FragmentShaderBit;
        }
        else
        {
            barrier.DstAccessMask = AccessFlags.MemoryReadBit;
            destinationStage = PipelineStageFlags.BottomOfPipeBit;
        }

        Vk.CmdPipelineBarrier(commandBuffer, sourceStage, destinationStage, 0, 0, null, 0, null, 1,
            barrier);
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
        Vk.DestroySampler(LogicalDevice, sampler, null);
        Vk.DestroyImageView(LogicalDevice, ImageView, null);

        Vk.DestroyImage(LogicalDevice, textureImage, null);
        Vk.FreeMemory(LogicalDevice, textureImageMemory, null);
    }

    public void TransitionLayoutTo(uint from, uint to)
    {
        TransitionImageLayout(textureImage, (Format)ImageFormat, (ImageLayout)from, (ImageLayout)to); 
    }

    public void TransitionLayoutTo(CommandBuffer buffer, ImageLayout from, ImageLayout to)
    {
        TransitionImageLayout(textureImage, from, to, buffer);
    }
}
