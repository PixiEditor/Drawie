using Drawie.RenderApi.Vulkan.Exceptions;
using Drawie.RenderApi.Vulkan.Helpers;
using Silk.NET.Vulkan;
using Image = Silk.NET.Vulkan.Image;

namespace Drawie.RenderApi.Vulkan.Buffers;

public sealed unsafe class VulkanImageAttachment : IDisposable
{
    private readonly Vk vk;
    private readonly Device device;
    private readonly PhysicalDevice physicalDevice;
    private readonly CommandPool commandPool;
    private readonly Queue graphicsQueue;

    public Image Image { get; private set; }
    public DeviceMemory Memory { get; private set; }
    public ImageView View { get; private set; }

    public Format Format { get; }
    public uint Width { get; }
    public uint Height { get; }
    public ImageAspectFlags AspectMask { get; }
    public ImageUsageFlags Usage { get; }
    public ImageLayout Layout { get; private set; } = ImageLayout.Undefined;
    public SampleCountFlags Samples { get; private set; }
    

    public VulkanImageAttachment(
        Vk vk,
        Device device,
        PhysicalDevice physicalDevice,
        CommandPool commandPool,
        Queue graphicsQueue,
        uint width,
        uint height,
        Format format,
        ImageUsageFlags usage,
        ImageAspectFlags aspectMask, SampleCountFlags samples)
    {
        this.vk = vk;
        this.device = device;
        this.physicalDevice = physicalDevice;
        this.commandPool = commandPool;
        this.graphicsQueue = graphicsQueue;

        Width = width;
        Height = height;
        Format = format;
        Usage = usage;
        AspectMask = aspectMask;
        Samples = samples;

        CreateImage();
        AllocateMemory();
        CreateView();
    }

    private void CreateImage()
    {
        ImageCreateInfo imageInfo = new()
        {
            SType = StructureType.ImageCreateInfo,
            ImageType = ImageType.Type2D,
            Extent = new Extent3D(Width, Height, 1),
            MipLevels = 1,
            ArrayLayers = 1,
            Format = Format,
            Tiling = ImageTiling.Optimal,
            InitialLayout = ImageLayout.Undefined,
            Usage = Usage,
            Samples = Samples,
            SharingMode = SharingMode.Exclusive
        };

        if (vk.CreateImage(device, &imageInfo, null, out var img) != Result.Success)
            throw new VulkanException("Failed to create image attachment.");
        
        Image = img;
    }

    private void AllocateMemory()
    {
        vk.GetImageMemoryRequirements(device, Image, out var requirements);

        MemoryAllocateInfo allocInfo = new()
        {
            SType = StructureType.MemoryAllocateInfo,
            AllocationSize = requirements.Size,
            MemoryTypeIndex = BufferObject.FindMemoryType(
                vk,
                physicalDevice,
                requirements.MemoryTypeBits,
                MemoryPropertyFlags.DeviceLocalBit)
        };

        if (vk.AllocateMemory(device, &allocInfo, null, out var memory) != Result.Success)
            throw new VulkanException("Failed to allocate image attachment memory.");

        Memory = memory;
        
        if (vk.BindImageMemory(device, Image, Memory, 0) != Result.Success)
            throw new VulkanException("Failed to bind image attachment memory.");
    }

    private void CreateView()
    {
        ImageViewCreateInfo viewInfo = new()
        {
            SType = StructureType.ImageViewCreateInfo,
            Image = Image,
            ViewType = ImageViewType.Type2D,
            Format = Format,
            SubresourceRange = new ImageSubresourceRange
            {
                AspectMask = AspectMask,
                BaseMipLevel = 0,
                LevelCount = 1,
                BaseArrayLayer = 0,
                LayerCount = 1
            }
        };

        if (vk.CreateImageView(device, &viewInfo, null, out var view) != Result.Success)
            throw new VulkanException("Failed to create image attachment view.");

        View = view;
    }

    public void TransitionLayout(
        ImageLayout newLayout,
        CommandBuffer? commandBuffer = null)
    {
        if (commandBuffer.HasValue)
        {
            TransitionLayout(
                commandBuffer.Value,
                Layout,
                newLayout);

            Layout = newLayout;
            return;
        }

        using var session = new SingleTimeCommandBufferSession(
            vk,
            commandPool,
            device,
            graphicsQueue);

        TransitionLayout(
            session.CommandBuffer,
            Layout,
            newLayout);

        Layout = newLayout;
    }
    
    public void TransitionLayout(
        ImageLayout oldLayout,
        ImageLayout newLayout,
        CommandBuffer? commandBuffer = null)
    {
        if (commandBuffer.HasValue)
        {
            TransitionLayout(
                commandBuffer.Value,
                oldLayout,
                newLayout);

            Layout = newLayout;
            return;
        }

        using var session = new SingleTimeCommandBufferSession(
            vk,
            commandPool,
            device,
            graphicsQueue);

        TransitionLayout(
            session.CommandBuffer,
            oldLayout,
            newLayout);

        Layout = newLayout;
    }


    private void TransitionLayout(
        CommandBuffer commandBuffer,
        ImageLayout oldLayout,
        ImageLayout newLayout)
    {
        var barrier = new ImageMemoryBarrier
        {
            SType = StructureType.ImageMemoryBarrier,
            OldLayout = oldLayout,
            NewLayout = newLayout,
            SrcQueueFamilyIndex = Vk.QueueFamilyIgnored,
            DstQueueFamilyIndex = Vk.QueueFamilyIgnored,
            Image = Image,
            SubresourceRange = new ImageSubresourceRange
            {
                AspectMask = AspectMask,
                BaseMipLevel = 0,
                LevelCount = 1,
                BaseArrayLayer = 0,
                LayerCount = 1
            }
        };

        PipelineStageFlags sourceStage;
        PipelineStageFlags destinationStage;

        switch (oldLayout)
        {
            case ImageLayout.Undefined:
                barrier.SrcAccessMask = 0;
                sourceStage = PipelineStageFlags.TopOfPipeBit;
                break;

            case ImageLayout.ColorAttachmentOptimal:
                barrier.SrcAccessMask =
                    AccessFlags.ColorAttachmentWriteBit;
                sourceStage =
                    PipelineStageFlags.ColorAttachmentOutputBit;
                break;

            case ImageLayout.DepthStencilAttachmentOptimal:
                barrier.SrcAccessMask =
                    AccessFlags.DepthStencilAttachmentWriteBit;
                sourceStage =
                    PipelineStageFlags.EarlyFragmentTestsBit |
                    PipelineStageFlags.LateFragmentTestsBit;
                break;

            case ImageLayout.ShaderReadOnlyOptimal:
                barrier.SrcAccessMask =
                    AccessFlags.ShaderReadBit;
                sourceStage =
                    PipelineStageFlags.FragmentShaderBit;
                break;
            
            case ImageLayout.PresentSrcKhr:
                barrier.SrcAccessMask = 0;
                sourceStage =
                    PipelineStageFlags.BottomOfPipeBit;
                break;

            default:
                barrier.SrcAccessMask = AccessFlags.MemoryReadBit;
                sourceStage = PipelineStageFlags.BottomOfPipeBit;
                break;
        }

        switch (newLayout)
        {
            case ImageLayout.ColorAttachmentOptimal:
                barrier.DstAccessMask =
                    AccessFlags.ColorAttachmentWriteBit;
                destinationStage =
                    PipelineStageFlags.ColorAttachmentOutputBit;
                break;

            case ImageLayout.DepthStencilAttachmentOptimal:
                barrier.DstAccessMask =
                    AccessFlags.DepthStencilAttachmentReadBit |
                    AccessFlags.DepthStencilAttachmentWriteBit;

                destinationStage =
                    PipelineStageFlags.EarlyFragmentTestsBit |
                    PipelineStageFlags.LateFragmentTestsBit;
                break;

            case ImageLayout.ShaderReadOnlyOptimal:
                barrier.DstAccessMask =
                    AccessFlags.ShaderReadBit;
                destinationStage =
                    PipelineStageFlags.FragmentShaderBit;
                break;
            
            case ImageLayout.PresentSrcKhr:
                barrier.DstAccessMask = 0;
                destinationStage =
                    PipelineStageFlags.BottomOfPipeBit;
                break;

            default:
                barrier.DstAccessMask = AccessFlags.MemoryReadBit;
                destinationStage = PipelineStageFlags.BottomOfPipeBit;
                break;
        }

        vk.CmdPipelineBarrier(
            commandBuffer,
            sourceStage,
            destinationStage,
            0,
            0,
            null,
            0,
            null,
            1,
            &barrier);
    }

    public void Dispose()
    {
        if (View.Handle != 0)
        {
            vk.DestroyImageView(device, View, null);
            View = default;
        }

        if (Image.Handle != 0)
        {
            vk.DestroyImage(device, Image, null);
            Image = default;
        }

        if (Memory.Handle != 0)
        {
            vk.FreeMemory(device, Memory, null);
            Memory = default;
        }
    }
}