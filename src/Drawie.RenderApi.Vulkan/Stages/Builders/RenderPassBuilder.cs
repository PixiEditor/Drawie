using Drawie.RenderApi.Vulkan.Exceptions;
using Drawie.RenderApi.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan.Stages.Builders;

public class RenderPassBuilder : IDisposable
{
    public Vk Vk { get; set; }
    public Device LogicalDevice { get; set; }

    public bool WithDepthStencil { get; set; }
    public Format DepthStencilFormat { get; set; }
    public int Samples { get; set; } = 1;

    public RenderPassBuilder(Vk vk, Device logicalDevice)
    {
        Vk = vk;
        LogicalDevice = logicalDevice;
    }

    public RenderPassBuilder WithDepth(Format depthStencilFormat)
    {
        WithDepthStencil = true;
        DepthStencilFormat = depthStencilFormat;
        return this;
    }

    public RenderPassBuilder WithSamples(int samples)
    {
        Samples = samples;
        return this;
    }

    public unsafe RenderPass Create(Format format, ImageLayout imageLayout)
    {
        AttachmentDescription colorAttachment = new()
        {
            Format = format,
            Samples = FormatExtensions.ToSampleFlags(Samples),
            LoadOp = AttachmentLoadOp.Clear,
            StoreOp = AttachmentStoreOp.Store,
            StencilLoadOp = AttachmentLoadOp.DontCare,
            StencilStoreOp = AttachmentStoreOp.DontCare,
            InitialLayout = ImageLayout.Undefined,
            FinalLayout = Samples == 1 ? imageLayout : ImageLayout.ColorAttachmentOptimal
        };

        AttachmentDescription depthAttachment = new()
        {
            Format = DepthStencilFormat,
            Samples = FormatExtensions.ToSampleFlags(Samples),
            InitialLayout = ImageLayout.Undefined,
            FinalLayout = ImageLayout.DepthStencilAttachmentOptimal,
            LoadOp = AttachmentLoadOp.Clear,
            StoreOp = AttachmentStoreOp.DontCare,
            StencilLoadOp = AttachmentLoadOp.Clear,
            StencilStoreOp = AttachmentStoreOp.DontCare,
        };

        AttachmentReference colorAttachmentRef = new()
        {
            Attachment = 0,
            Layout = ImageLayout.ColorAttachmentOptimal
        };

        AttachmentReference depthAttachmentRef = new()
        {
            Attachment = 1,
            Layout = ImageLayout.DepthStencilAttachmentOptimal
        };

        SubpassDescription subpass = new()
        {
            PipelineBindPoint = PipelineBindPoint.Graphics,
            ColorAttachmentCount = 1,
            PColorAttachments = &colorAttachmentRef,
            PDepthStencilAttachment = WithDepthStencil ? &depthAttachmentRef : null,
        };

        AttachmentDescription colorResolveAttachment = new()
        {
            Format = format,
            Samples = SampleCountFlags.Count1Bit,
            LoadOp = AttachmentLoadOp.DontCare,
            StoreOp = AttachmentStoreOp.Store,
            StencilLoadOp = AttachmentLoadOp.DontCare,
            StencilStoreOp = AttachmentStoreOp.DontCare,
            InitialLayout = ImageLayout.Undefined,
            FinalLayout = ImageLayout.PresentSrcKhr
        };
        
        if (Samples > 1)
        {
            AttachmentReference colorResolveAttachmentRef = new()
            {
                Attachment = (uint)(WithDepthStencil ? 2 : 1),
                Layout = ImageLayout.ColorAttachmentOptimal,
            };

            subpass.PResolveAttachments = &colorResolveAttachmentRef;
        }

        SubpassDependency dependency = default;
        if (!WithDepthStencil)
        {
            dependency = new()
            {
                SrcSubpass = Vk.SubpassExternal,
                DstSubpass = 0,
                SrcStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
                SrcAccessMask = AccessFlags.None,
                DstStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
                DstAccessMask = AccessFlags.ColorAttachmentWriteBit
            };
        }
        else
        {
            dependency = new()
            {
                SrcSubpass = Vk.SubpassExternal,
                DstSubpass = 0,
                SrcStageMask = PipelineStageFlags.ColorAttachmentOutputBit | PipelineStageFlags.EarlyFragmentTestsBit | PipelineStageFlags.LateFragmentTestsBit,
                SrcAccessMask = AccessFlags.ColorAttachmentWriteBit | AccessFlags.DepthStencilAttachmentWriteBit,

                DstStageMask = PipelineStageFlags.ColorAttachmentOutputBit | PipelineStageFlags.EarlyFragmentTestsBit,
                DstAccessMask = AccessFlags.ColorAttachmentWriteBit | AccessFlags.DepthStencilAttachmentWriteBit
            };
        }

        uint attachmentCount = 1;
        if (WithDepthStencil) attachmentCount++;
        if (Samples > 1) attachmentCount++;

        AttachmentDescription* attachments = &colorAttachment;
        if (attachmentCount > 1)
        {
            var attachmentDescriptions = stackalloc AttachmentDescription[(int)attachmentCount];
            attachmentDescriptions[0] = colorAttachment;
            int i = 1;
            if (WithDepthStencil)
            {
                attachmentDescriptions[1] = depthAttachment;
                i++;
            }

            if (Samples > 1)
            {
                attachmentDescriptions[i] = colorResolveAttachment;
            }

            attachments = attachmentDescriptions;
        }


        RenderPassCreateInfo renderPassInfo = new()
        {
            SType = StructureType.RenderPassCreateInfo,
            AttachmentCount = attachmentCount,
            PAttachments = attachments,
            SubpassCount = 1,
            PSubpasses = &subpass,
            DependencyCount = 1,
            PDependencies = &dependency
        };

        if (Vk!.CreateRenderPass(LogicalDevice, in renderPassInfo, null, out var renderPass) != Result.Success)
            throw new VulkanException("Failed to create render pass.");

        return renderPass;
    }

    public void Dispose()
    {
    }
}