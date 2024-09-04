using Drawie.RenderApi.Vulkan.Exceptions;
using Drawie.RenderApi.Vulkan.Helpers;
using Silk.NET.Vulkan;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Buffer = Silk.NET.Vulkan.Buffer;
using Image = Silk.NET.Vulkan.Image;

namespace Drawie.RenderApi.Vulkan.Buffers;

public class VulkanTexture : IDisposable
{
    public ImageView ImageView { get; private set; }
    public Sampler Sampler => sampler;
    private Vk Vk { get; }
    private Device LogicalDevice { get; }
    private PhysicalDevice PhysicalDevice { get; }

    private CommandPool CommandPool { get; }

    private Queue GraphicsQueue { get; }

    private Image textureImage;
    private DeviceMemory textureImageMemory;
    private Sampler sampler;

    public unsafe VulkanTexture(Vk vk, Device logicalDevice, PhysicalDevice physicalDevice, CommandPool commandPool,
        Queue graphicsQueue, string path)
    {
        Vk = vk;
        LogicalDevice = logicalDevice;
        PhysicalDevice = physicalDevice;
        CommandPool = commandPool;
        GraphicsQueue = graphicsQueue;

        using var image = SixLabors.ImageSharp.Image.Load<Rgba32>(path);

        var imageSize = (ulong)image.Width * (ulong)image.Height * 4;

        using var stagingBuffer = new StagingBuffer(vk, logicalDevice, physicalDevice, imageSize);

        void* data;
        vk!.MapMemory(LogicalDevice, stagingBuffer.VkBufferMemory, 0, imageSize, 0, &data);
        image.CopyPixelDataTo(new Span<byte>(data, (int)imageSize));
        vk!.UnmapMemory(LogicalDevice, stagingBuffer.VkBufferMemory);

        CreateImage((uint)image.Width, (uint)image.Height, Format.R8G8B8A8Srgb, ImageTiling.Optimal,
            ImageUsageFlags.TransferDstBit | ImageUsageFlags.SampledBit, MemoryPropertyFlags.DeviceLocalBit);

        TransitionImageLayout(textureImage, Format.R8G8B8A8Srgb, ImageLayout.Undefined, ImageLayout.TransferDstOptimal);
        CopyBufferToImage(stagingBuffer.VkBuffer, textureImage, (uint)image.Width, (uint)image.Height);
        TransitionImageLayout(textureImage, Format.R8G8B8A8Srgb, ImageLayout.TransferDstOptimal,
            ImageLayout.ShaderReadOnlyOptimal);

        ImageView = ImageUtility.CreateViewForImage(Vk, LogicalDevice, textureImage, Format.R8G8B8A8Srgb);
        
        CreateSampler();
    }

    private unsafe void CreateSampler()
    {
        Vk.GetPhysicalDeviceProperties(PhysicalDevice, out var features);
        
        SamplerCreateInfo samplerCreateInfo = new()
        {
            SType = StructureType.SamplerCreateInfo,
            MagFilter = Filter.Linear,
            MinFilter = Filter.Linear,
            AddressModeU = SamplerAddressMode.Repeat,
            AddressModeV = SamplerAddressMode.Repeat,
            AddressModeW = SamplerAddressMode.Repeat,
            AnisotropyEnable = true,
            MaxAnisotropy = features.Limits.MaxSamplerAnisotropy,
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

        if (oldLayout == ImageLayout.Undefined && newLayout == ImageLayout.TransferDstOptimal)
        {
            barrier.SrcAccessMask = 0;
            barrier.DstAccessMask = AccessFlags.TransferWriteBit;

            sourceStage = PipelineStageFlags.TopOfPipeBit;
            destinationStage = PipelineStageFlags.TransferBit;
        }
        else if (oldLayout == ImageLayout.TransferDstOptimal && newLayout == ImageLayout.ShaderReadOnlyOptimal)
        {
            barrier.SrcAccessMask = AccessFlags.TransferWriteBit;
            barrier.DstAccessMask = AccessFlags.ShaderReadBit;

            sourceStage = PipelineStageFlags.TransferBit;
            destinationStage = PipelineStageFlags.FragmentShaderBit;
        }
        else
        {
            throw new InvalidOperationException("Unsupported layout transition.");
        }

        Vk.CmdPipelineBarrier(commandBuffer.CommandBuffer, sourceStage, destinationStage, 0, 0, null, 0, null, 1,
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
}