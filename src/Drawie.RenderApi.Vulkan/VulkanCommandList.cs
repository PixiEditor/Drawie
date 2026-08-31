using Drawie.Backend.Shaders.Common;
using Drawie.RenderApi.Abstraction.Buffers;
using Drawie.RenderApi.Abstraction.CommandRecording;
using Drawie.RenderApi.Abstraction.Pipeline;
using Drawie.RenderApi.Abstraction.RenderTargets;
using Drawie.RenderApi.Abstraction.Shaders;
using Drawie.RenderApi.Abstraction.Textures;
using Drawie.RenderApi.Vulkan.Buffers;
using Drawie.RenderApi.Vulkan.Exceptions;
using Drawie.RenderApi.Vulkan.Helpers;
using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan;

internal sealed class VulkanCommandList : CommandList, IDisposable
{
    public CommandBuffer CommandBuffer => commandBuffer;
    public Dictionary<Guid, UniformBuffer> BufferCache { get; set; }

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
        commandBuffer = AllocateCommandBuffer();
        BeginCommandBuffer(commandBuffer);
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

        renderTarget = target;

        renderTarget.CreateFramebufferFor(pipeline.GraphicsPipeline.VkRenderPass);

        recording = true;

        BeginRendering(target);
    }

    public override void SetPipeline(IPipeline pipeline)
    {
        if (pipeline is not VulkanPipeline vkPipeline) throw new ArgumentException("Only VulkanPipeline is supported");
        this.pipeline = vkPipeline;
        //vkPipeline.DescriptorPool.Reset();
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

    public override void BindPipeline()
    {
        if (!recording)
            throw new InvalidOperationException(
                "BeginRenderPass must be called first.");

        if (pipeline == null) throw new NullReferenceException("Pipeline is was not set");

        pipeline.Apply(this);
    }

    public override PreparedTexture PrepareTexture(ITexture texture)
    {
        var vkTex = context.ManagedTextures[texture.TextureId];

        if (vkTex is not VulkanTexture vkTexture) throw new ArgumentException("Only IVkTexture's are valid");
        vkTexture.MakeReadOnly(commandBuffer);

        return new PreparedTexture(texture.TextureId);
    }

    public override void UpdateUniforms(IEnumerable<UniformBlock> blocks, IEnumerable<PreparedTexture> textures,
        IEnumerable<ISampler> samplers)
    {
        foreach (var block in blocks)
        {
            UniformBuffer? buffer = null;
            if (BufferCache.TryGetValue(block.UniformBlockId, out var uniformBuffer))
            {
                buffer = uniformBuffer;
            }
            else
            {
                buffer = new UniformBuffer(context.Api, context.LogicalDevice.Device, context.PhysicalDevice,
                    (ulong)block.ShaderLayout.Size);
                BufferCache[block.UniformBlockId] = buffer;
            }

            UniformUtility.SerializeToBuffer(block, buffer);
            // TODO first texture is temporary as shader only supports single texture
            UpdateUniformDescriptor(buffer, (ulong)block.ShaderLayout.Size, textures.FirstOrDefault(),
                samplers.FirstOrDefault());
        }
    }

    public override void UpdateUniforms(IEnumerable<NamedBuffer> properties)
    {
        foreach (var namedBuffer in properties)
        {
            if (namedBuffer.Buffer is IVkBuffer vkBuffer)
            {
                int binding = pipeline.Program.DescriptorSetLayout.Bindings.IndexOf(namedBuffer.Name);
                if (binding == -1)
                    throw new ArgumentException($"Could not find {namedBuffer.Name} inside current shader program.");
                if (binding < 0) 
                    throw new ArgumentOutOfRangeException(nameof(binding));
                
                UpdateDescriptor(0, (uint)binding, vkBuffer);
            }
            else
            {
                throw new ArgumentException("Only IVkBuffer is valid buffer type");
            }
        }
    }

    public override void RestoreTexture(PreparedTexture preparedTextureValue)
    {
        var target = context.ManagedTextures[preparedTextureValue.Handle];
        if (target is not VulkanTexture vkTex) throw new ArgumentException("Only VulkanTexture's are valid");
        vkTex.MakeReadOnly(commandBuffer);
    }

    private unsafe void UpdateDescriptor(int setIndex, uint binding, IVkBuffer buffer)
    {
        var set = pipeline.DescriptorPool.GetOrAllocateDescriptorSet(setIndex, (ulong)setIndex);
        DescriptorBufferInfo bufferInfo = new()
        {
            Buffer = buffer.NativeBuffer.VkBuffer,
            Offset = 0,
            Range = buffer.NativeBuffer.Size
        };

        WriteDescriptorSet write = new()
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = set,
            DstBinding = binding,
            DstArrayElement = 0,
            DescriptorCount = 1,
            DescriptorType = UsageToDescriptor(buffer.Usage),
            PBufferInfo = &bufferInfo
        };
        
        context.Api.UpdateDescriptorSets(context.LogicalDevice.Device, 1, &write, 0, null);

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

    private DescriptorType UsageToDescriptor(BufferUsage bufferUsage)
    {
        return bufferUsage switch
        {
            BufferUsage.Uniform => DescriptorType.UniformBuffer,
            BufferUsage.Storage => DescriptorType.StorageBuffer,
            _ => throw new ArgumentException("Invalid buffer type: " + bufferUsage)
        };
    }

    private unsafe void UpdateUniformDescriptor(UniformBuffer buffer, ulong size, PreparedTexture texture,
        ISampler sampler)
    {
        if (texture.Handle == 0 || sampler == default)
            return;

        var vkTexture = context.ManagedTextures[texture.Handle] as VulkanTexture;
        var vkSampler = sampler as VulkanSampler;
        // TODO: better id
        var set = pipeline.DescriptorPool.GetOrAllocateDescriptorSet(0, buffer.VkBuffer.Handle);
        DescriptorBufferInfo bufferInfo = new()
        {
            Buffer = buffer.VkBuffer,
            Offset = 0,
            Range = size
        };

        WriteDescriptorSet write = new()
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = set,
            DstBinding = 0,
            DstArrayElement = 0,
            DescriptorCount = 1,
            DescriptorType = DescriptorType.UniformBuffer,
            PBufferInfo = &bufferInfo
        };

        var imageInfo = new DescriptorImageInfo
        {
            Sampler = vkSampler.VkSampler,
            ImageView = vkTexture.ColorAttachment.View,
            ImageLayout = ImageLayout.ShaderReadOnlyOptimal
        };

        var writeImgSampler = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = set,
            DstBinding = 1,
            DstArrayElement = 0,
            DescriptorCount = 1,
            DescriptorType = DescriptorType.CombinedImageSampler,
            PImageInfo = &imageInfo
        };

        WriteDescriptorSet* sets = stackalloc WriteDescriptorSet[2];
        sets[0] = write;
        sets[1] = writeImgSampler;

        context.Api.UpdateDescriptorSets(
            context.LogicalDevice.Device,
            2,
            sets,
            0,
            null);

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

    public override void BindTexture(
        PreparedTexture texture,
        ISampler sampler)
    {
        if (!recording)
            throw new InvalidOperationException(
                "BeginRenderPass must be called first.");

        if (sampler is not VulkanSampler vkSampler)
            throw new ArgumentException(
                "Sampler must be a Vulkan sampler.",
                nameof(sampler));

        var vkTex = context.ManagedTextures[texture.Handle];

        if (vkTex is not VulkanTexture vkTexture) throw new ArgumentException("Only IVkTexture's are valid");

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

    public override void Draw(int vertexCount, int instanceCount)
    {
        if (!recording)
            throw new InvalidOperationException(
                "BeginRenderPass must be called first.");

        context.Api!.CmdDraw(commandBuffer, (uint)vertexCount, (uint)instanceCount, 0, 0);
    }

    public override RecordedRenderPass EndRenderPass()
    {
        EnsureRecording();

        //context.DynamicRendering.CmdEndRendering(commandBuffer);
        context.Api!.CmdEndRenderPass(commandBuffer);

        EndCommandBuffer(commandBuffer);

        recording = false;
        pipeline = null;

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

        context.Api!.CmdEndRenderPass(commandBuffer);

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

    private unsafe void BindTextureDescriptor(
        VulkanTexture texture,
        VulkanSampler sampler)
    {
        var set = pipeline.DescriptorPool.GetOrAllocateDescriptorSet(1, texture.ImageHandle);
        var imageInfo = new DescriptorImageInfo
        {
            Sampler = sampler.VkSampler,
            ImageView = texture.ColorAttachment.View,
            ImageLayout = ImageLayout.ShaderReadOnlyOptimal
        };

        var write = new WriteDescriptorSet
        {
            SType = StructureType.WriteDescriptorSet,
            DstSet = set,
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

        context.Api!.CmdBindDescriptorSets(
            commandBuffer,
            PipelineBindPoint.Graphics,
            pipeline.GraphicsPipeline.VkPipelineLayout,
            1,
            1,
            in set,
            0,
            null);
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
            Flags = CommandBufferUsageFlags.OneTimeSubmitBit,
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

    private void BeginRendering(VulkanRenderTarget target)
    {
        //BeginDynamicRendering(target);
        BeginRenderPass(target);
    }

    private unsafe void BeginRenderPass(VulkanRenderTarget target)
    {
        target.Texture.MakeWriteable(commandBuffer);
        RenderPassBeginInfo renderPassInfo = new()
        {
            SType = StructureType.RenderPassBeginInfo,
            RenderPass = pipeline.GraphicsPipeline.VkRenderPass,
            Framebuffer = renderTarget.Framebuffer.Value,
            RenderArea = new Rect2D
            {
                Offset = new Offset2D(0, 0),
                Extent = new Extent2D((uint)renderTarget.Size.X, (uint)renderTarget.Size.Y)
            }
        };
        ClearValue clearColor = new()
        {
            Color = new ClearColorValue() { Float32_0 = 0, Float32_1 = 0, Float32_2 = 0, Float32_3 = 1 }
        };

        ClearValue clearDepth = new()
        {
            DepthStencil = new ClearDepthStencilValue(1, 0)
        };

        ClearValue clearMsaaResolved = new()
        {
            Color = new ClearColorValue() { Float32_0 = 1, Float32_1 = 1, Float32_2 = 1, Float32_3 = 1 }
        };

        ClearValue* clearValues = stackalloc ClearValue[(int)target.Texture.Attachments];
        clearValues[0] = clearColor;
        int i = 0;
        if (target.Texture.DepthAttachment != null)
        {
            clearValues[1] = clearDepth;
            i++;
        }

        if (target.Texture.MsaaResolvedColorAttachment != null)
        {
            clearValues[i] = clearMsaaResolved;
        }

        renderPassInfo.ClearValueCount = target.Texture.Attachments;
        renderPassInfo.PClearValues = clearValues;

        context.Api!.CmdBeginRenderPass(commandBuffer, &renderPassInfo, SubpassContents.Inline);
    }

    /*private unsafe void BeginDynamicRendering(VulkanRenderTarget target)
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

        if (target.Texture.MsaaResolvedColorAttachment != null)
        {
            colorAttachment.ResolveMode = ResolveModeFlags.AverageBit;
            colorAttachment.ResolveImageView = target.Texture.MsaaResolvedColorAttachment.View;
            colorAttachment.ResolveImageLayout = ImageLayout.ColorAttachmentOptimal;
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
            PColorAttachments = &colorAttachment,
        };

        if (target.Texture.DepthAttachment is not null)
            renderingInfo.PDepthAttachment = &depthAttachment;

        context.DynamicRendering?.CmdBeginRendering(commandBuffer, &renderingInfo);
    }*/

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

    private void Blit(
        VulkanTexture source,
        VulkanTexture destination)
    {
        var sourceAttachment = source.MsaaResolvedColorAttachment ?? source.ColorAttachment;
        sourceAttachment.TransitionLayout(ImageLayout.TransferSrcOptimal, commandBuffer);
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
            sourceAttachment.Image,
            ImageLayout.TransferSrcOptimal,
            destination.VkImage,
            ImageLayout.TransferDstOptimal,
            1,
            in region,
            Filter.Nearest);

        destination.ColorAttachment.TransitionLayout(ImageLayout.ColorAttachmentOptimal, commandBuffer);
    }

    public unsafe void Dispose()
    {
        context.Api.DestroyCommandPool(context.LogicalDevice.Device, commandPool, null);
    }
}