using Drawie.RenderApi.Vulkan.Exceptions;
using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan.Stages.Builders;

public class RenderPassBuilder : IDisposable
{
    public Vk Vk { get; set; }
    public Device LogicalDevice { get; set; }
    
    public bool WithDepthStencil { get; set; }
    public Format DepthStencilFormat { get; set; }

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
    

    public unsafe RenderPass Create(Format format, ImageLayout imageLayout)
    {
        AttachmentDescription colorAttachment = new()
        {
            Format = format,
            Samples = SampleCountFlags.Count1Bit,
            LoadOp = AttachmentLoadOp.Clear,
            StoreOp = AttachmentStoreOp.Store,
            StencilLoadOp = AttachmentLoadOp.DontCare,
            StencilStoreOp = AttachmentStoreOp.DontCare,
            InitialLayout = ImageLayout.Undefined,
            FinalLayout = imageLayout
        };

        AttachmentDescription depthAttachment = new()
        {
            Format = DepthStencilFormat,
            Samples = SampleCountFlags.Count1Bit,
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

        SubpassDependency dependency = new()
        {
            SrcSubpass = Vk.SubpassExternal,
            DstSubpass = 0,
            SrcStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
            SrcAccessMask = 0,
            DstStageMask = PipelineStageFlags.ColorAttachmentOutputBit,
            DstAccessMask = AccessFlags.ColorAttachmentWriteBit
        };

        AttachmentDescription* attachments = &colorAttachment;
        if (WithDepthStencil)
        {
            var attachmentDescriptions = stackalloc AttachmentDescription[2];
            attachmentDescriptions[0] = colorAttachment;
            attachmentDescriptions[1] = depthAttachment;
            
            attachments = attachmentDescriptions;
        }

        RenderPassCreateInfo renderPassInfo = new()
        {
            SType = StructureType.RenderPassCreateInfo,
            AttachmentCount = (uint)(WithDepthStencil ? 2 : 1),
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