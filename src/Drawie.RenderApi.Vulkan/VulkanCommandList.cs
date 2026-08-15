using Drawie.RenderApi.Abstraction.Buffers;
using Drawie.RenderApi.Abstraction.CommandRecording;
using Drawie.RenderApi.Abstraction.Pipeline;
using Drawie.RenderApi.Abstraction.RenderTargets;
using Drawie.RenderApi.Abstraction.Textures;
using Drawie.RenderApi.Vulkan.Buffers;
using Drawie.RenderApi.Vulkan.Exceptions;
using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan;

internal sealed class VulkanCommandList : CommandList, IDisposable
{
    public CommandBuffer CommandBuffer => commandBuffer;
    private readonly VulkanContext context;
    private readonly CommandPool commandPool;

    private CommandBuffer commandBuffer;
    private bool recording;

    private VulkanRenderTarget? renderTarget;
    private VulkanPipeline? pipeline;

    public VulkanCommandList(
        VulkanContext context,
        CommandPool commandPool)
    {
        this.context = context;
        this.commandPool = commandPool;
    }

    public override void BeginRenderPass(IRenderTarget fb)
    {
        if (recording)
            throw new InvalidOperationException(
                "A Vulkan render pass is already being recorded.");

        if (fb is not VulkanRenderTarget target)
            throw new ArgumentException(
                "Render target must be a Vulkan render target.",
                nameof(fb));

        pipeline = null;
        renderTarget = target;

        commandBuffer = AllocateCommandBuffer();

        BeginCommandBuffer(commandBuffer);

        recording = true;

        BeginRendering(target);
    }

    public override void SetPipeline(IPipeline pipeline)
    {
        if (!recording)
            throw new InvalidOperationException(
                "BeginRenderPass must be called first.");

        if (pipeline is not VulkanPipeline vkPipeline) throw new ArgumentException("Only VulkanPipeline is supported");
        
        pipeline.Apply(this);
        this.pipeline = vkPipeline;
    }

    public override void SetBuffers(IBufferGroup bufferGroup)
    {
        if (!recording)
            throw new InvalidOperationException(
                "BeginRenderPass must be called first.");

        if (bufferGroup is not VulkanBufferGroup vkBuffers)
            throw new ArgumentException(
                "Buffer group must be a Vulkan buffer group.",
                nameof(bufferGroup));

        BindBuffers(vkBuffers);
    }

    public override void BindTexture(
        ITexture texture,
        ISampler sampler)
    {
        if (!recording)
            throw new InvalidOperationException(
                "BeginRenderPass must be called first.");

        if (sampler is not VulkanSampler vkSampler)
            throw new ArgumentException(
                "Sampler must be a Vulkan sampler.",
                nameof(sampler));

        var vkTex = context.ManagedTextures[texture.TextureId];

        if (vkTex is not VulkanTexture vkTexture) throw new ArgumentException("Only IVkTexture's are valid");
        
        vkTexture.MakeReadOnly(commandBuffer);

        BindTextureDescriptor(
            vkTexture,
            vkSampler);
    }

    public override void DrawIndexed(int indexCount)
    {
        if (!recording)
            throw new InvalidOperationException(
                "BeginRenderPass must be called first.");

        context.Api!.CmdDrawIndexed(commandBuffer, (uint)indexCount, 1, 0, 0, 0);
    }

    public override RecordedRenderPass EndRenderPass()
    {
        EnsureRecording();

        context.Api!.CmdEndRendering(commandBuffer);

        EndCommandBuffer(commandBuffer);

        recording = false;

        return new VulkanRecordedRenderPass(context, commandBuffer, commandPool);
    }

    public override RecordedRenderPass EndRenderPass(
        IRenderTarget blitTo)
    {
        EnsureRecording();

        var vkTex = context.ManagedTextures[blitTo.SurfaceId];

        if (vkTex is not VulkanTexture destination)
            throw new ArgumentException(
                "Destination must be a Vulkan render target.",
                nameof(blitTo));

        context.Api!.CmdEndRendering(commandBuffer);

        Blit(renderTarget!.Texture, destination);

        EndCommandBuffer(commandBuffer);

        recording = false;

        return new VulkanRecordedRenderPass(context, commandBuffer, commandPool);
    }

    private void EnsureRecording()
    {
        if (!recording)
            throw new InvalidOperationException(
                "No Vulkan render pass is currently being recorded.");
    }

    private unsafe CommandBuffer AllocateCommandBuffer()
    {
        CommandBufferAllocateInfo info = new()
        {
            SType = StructureType.CommandBufferAllocateInfo,
            CommandPool = commandPool,
            Level = CommandBufferLevel.Primary,
            CommandBufferCount = 1
        };

        if (context.Api!.AllocateCommandBuffers(
                context.LogicalDevice.Device,
                in info,
                out var result) != Result.Success)
        {
            throw new VulkanException(
                "Failed to allocate Vulkan command buffer.");
        }

        return result;
    }

    private void BeginCommandBuffer(CommandBuffer buffer)
    {
        CommandBufferBeginInfo info = new()
        {
            SType = StructureType.CommandBufferBeginInfo,
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit
        };

        if (context.Api!.BeginCommandBuffer(
                buffer,
                in info) != Result.Success)
        {
            throw new VulkanException(
                "Failed to begin Vulkan command buffer.");
        }
    }

    private void EndCommandBuffer(CommandBuffer buffer)
    {
        if (context.Api!.EndCommandBuffer(buffer) != Result.Success)
            throw new VulkanException(
                "Failed to end Vulkan command buffer.");
    }

    private unsafe void BeginRendering(VulkanRenderTarget target)
    {
        target.Texture.MakeWriteable(commandBuffer);

        RenderingAttachmentInfo colorAttachment = new()
        {
            SType = StructureType.RenderingAttachmentInfo,
            ImageView = target.Texture.ColorAttachment.View,
            ImageLayout = ImageLayout.ColorAttachmentOptimal,
            LoadOp = AttachmentLoadOp.Clear,
            StoreOp = AttachmentStoreOp.Store,
            ClearValue = new ClearValue
            {
                Color = new ClearColorValue(0f, 0f, 0f, 1f)
            },
        };

        RenderingAttachmentInfo depthAttachment = default;

        if (target.Texture.DepthAttachment is not null)
        {
            depthAttachment = new RenderingAttachmentInfo
            {
                SType = StructureType.RenderingAttachmentInfo,
                ImageView = target.Texture.DepthAttachment.View,
                ImageLayout = ImageLayout.DepthStencilAttachmentOptimal,
                LoadOp = AttachmentLoadOp.Clear,
                StoreOp = AttachmentStoreOp.Store,
                ClearValue = new ClearValue
                {
                    DepthStencil = new ClearDepthStencilValue(1f, 0)
                }
            };
        }

        Rect2D renderArea = new()
        {
            Offset = new Offset2D(0, 0),
            Extent = new Extent2D(
                (uint)target.Size.X,
                (uint)target.Size.Y)
        };

        RenderingInfo renderingInfo = new()
        {
            SType = StructureType.RenderingInfo,
            RenderArea = renderArea,
            LayerCount = 1,
            ColorAttachmentCount = 1,
            PColorAttachments = &colorAttachment
        };

        if (target.Texture.DepthAttachment is not null)
            renderingInfo.PDepthAttachment = &depthAttachment;

        context.Api!.CmdBeginRendering(
            commandBuffer,
            in renderingInfo);
    }    

    private unsafe void BindBuffers(VulkanBufferGroup group)
    {
        group.Open((list) =>
        {
            var bufList = list as VulkanBufferGroupList;
            if (bufList.VertexBuffer != null)
            {
                var buffer = bufList.VertexBuffer.NativeBuffer.VkBuffer;
                ulong offset = 0;

                context.Api!.CmdBindVertexBuffers(
                    commandBuffer,
                    0,
                    1,
                    &buffer,
                    &offset);
            }

            if (bufList.IndexBuffer is not null)
            {
                context.Api!.CmdBindIndexBuffer(
                    commandBuffer,
                    bufList.IndexBuffer.NativeBuffer.VkBuffer,
                    0,
                    IndexType.Uint32);
            }
        });
    }

    private unsafe void BindTextureDescriptor(
        VulkanTexture texture,
        VulkanSampler sampler)
    {
        var imageInfo = new DescriptorImageInfo
        {
            Sampler = sampler.VkSampler,
            ImageView = texture.ColorAttachment.View,
            ImageLayout = ImageLayout.ShaderReadOnlyOptimal
        };

        var write = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = pipeline.DescriptorSet,
            DstBinding = 1,
            DstArrayElement = 0,
            DescriptorCount = 1,
            DescriptorType = DescriptorType.CombinedImageSampler,
            PImageInfo = &imageInfo
        };

        context.Api!.UpdateDescriptorSets(
            context.LogicalDevice.Device,
            1,
            &write,
            0,
            null);

        var set = pipeline.DescriptorSet;
        context.Api!.CmdBindDescriptorSets(
            commandBuffer,
            PipelineBindPoint.Graphics,
            pipeline.GraphicsPipeline.VkPipelineLayout,
            0,
            1,
            in set,
            0,
            null);
        
    }

    private void Blit(
        VulkanTexture source,
        VulkanTexture destination)
    {
        source.ColorAttachment.TransitionLayout(ImageLayout.TransferSrcOptimal, commandBuffer);
        destination.ColorAttachment.TransitionLayout(ImageLayout.TransferDstOptimal, commandBuffer);

        ImageBlit region = new()
        {
            SrcSubresource = new ImageSubresourceLayers
            {
                AspectMask = ImageAspectFlags.ColorBit,
                MipLevel = 0,
                BaseArrayLayer = 0,
                LayerCount = 1
            },

            DstSubresource = new ImageSubresourceLayers
            {
                AspectMask = ImageAspectFlags.ColorBit,
                MipLevel = 0,
                BaseArrayLayer = 0,
                LayerCount = 1
            }
        };

        region.SrcOffsets[0] = new Offset3D(0, 0, 0);
        region.SrcOffsets[1] = new Offset3D(
            (int)source.Width,
            (int)source.Height,
            1);

        // Y is flipped at blit level
        region.DstOffsets[0] = new Offset3D(0, (int)destination.Height, 0);
        region.DstOffsets[1] = new Offset3D(
            (int)destination.Width,
            0,
            1);

        context.Api!.CmdBlitImage(
            commandBuffer,
            source.VkImage,
            ImageLayout.TransferSrcOptimal,
            destination.VkImage,
            ImageLayout.TransferDstOptimal,
            1,
            in region,
            Filter.Nearest);

        destination.ColorAttachment.TransitionLayout(ImageLayout.ColorAttachmentOptimal, commandBuffer);
    }
    
    public void Dispose()
    {
        
    }
}