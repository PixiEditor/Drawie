using Drawie.RenderApi.Vulkan.Exceptions;
using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan.Stages.Builders;

public class RenderPassBuilder : IDisposable
{
    public Vk Vk { get; set; }
    public Device LogicalDevice { get; set; }

    public RenderPassBuilder(Vk vk, Device logicalDevice)
    {
        Vk = vk;
        LogicalDevice = logicalDevice;
    }

    public unsafe RenderPass Create(Format swapChainImageFormat, ImageLayout finalLayout)
    {
        AttachmentDescription colorAttachment = new()
        {
            Format = swapChainImageFormat,
            Samples = SampleCountFlags.Count1Bit,
            LoadOp = AttachmentLoadOp.Clear,
            StoreOp = AttachmentStoreOp.Store,
            StencilLoadOp = AttachmentLoadOp.DontCare,
            StencilStoreOp = AttachmentStoreOp.DontCare,
            InitialLayout = ImageLayout.Undefined,
            FinalLayout = finalLayout
        };

        AttachmentReference colorAttachmentRef = new()
        {
            Attachment = 0,
            Layout = ImageLayout.ColorAttachmentOptimal
        };

        SubpassDescription subpass = new()
        {
            PipelineBindPoint = PipelineBindPoint.Graphics,
            ColorAttachmentCount = 1,
            PColorAttachments = &colorAttachmentRef
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

        RenderPassCreateInfo renderPassInfo = new()
        {
            SType = StructureType.RenderPassCreateInfo,
            AttachmentCount = 1,
            PAttachments = &colorAttachment,
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