using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Vulkan;
using Drawie.Numerics;
using Drawie.RenderApi.Vulkan;
using Drawie.RenderApi.Vulkan.Buffers;
using Drawie.RenderApi.Vulkan.Extensions;
using Drawie.RenderApi.Vulkan.Stages;
using Drawie.RenderApi.Vulkan.Stages.Builders;
using Drawie.RenderApi.Vulkan.Structs;
using Silk.NET.Vulkan;
using Buffer = System.Buffer;

namespace Drawie.AvaloniaGraphics.Interop;

public class VulkanContent : IDisposable
{
    private readonly VulkanInteropContext context;

    private DescriptorSetLayout _descriptorSetLayout;
    private Framebuffer _framebuffer;

    private DescriptorPool descriptorPool;
    private DescriptorSetLayout descriptorSetLayout;
    private DescriptorSet descriptorSet;
    private GraphicsPipeline graphicsPipeline;

    private VertexBuffer vertexBuffer;

    private IndexBuffer indexBuffer;
    private PixelSize? _previousImageSize = PixelSize.Empty;

    private VulkanImage _colorAttachment;
    public VulkanTexture texture;

    private bool isInit;

    public unsafe VulkanContent(VulkanInteropContext context)
    {
        this.context = context;
        var api = this.context.Api;
        var device = this.context.LogicalDevice.Device;

        CreateBuffers();
    }


    public unsafe void Render(VulkanImage image)
    {
        var api = context.Api;

        if (image.Size != _previousImageSize)
            CreateTemporalObjects(image.Size);

        _previousImageSize = image.Size;

        texture.TransitionLayoutTo(VulkanTexture.ColorAttachmentOptimal, VulkanTexture.ShaderReadOnlyOptimal);

        var commandBuffer = context.Pool.CreateCommandBuffer();
        commandBuffer.BeginRecording();

        _colorAttachment!.TransitionLayout(commandBuffer.InternalHandle,
            ImageLayout.Undefined, AccessFlags.None,
            ImageLayout.ColorAttachmentOptimal, AccessFlags.ColorAttachmentWriteBit);

        var commandBufferHandle = new CommandBuffer(commandBuffer.Handle);

        api.CmdSetViewport(commandBufferHandle, 0, 1,
            new Viewport()
            {
                Width = (float)image.Size.Width,
                Height = (float)image.Size.Height,
                MaxDepth = 1,
                MinDepth = 0,
                X = 0,
                Y = 0
            });

        var scissor = new Rect2D
        {
            Extent = new Extent2D((uint?)image.Size.Width, (uint?)image.Size.Height)
        };

        api.CmdSetScissor(commandBufferHandle, 0, 1, &scissor);

        var clearValue = new ClearValue
        {
            Color = new ClearColorValue { Float32_0 = 0, Float32_1 = 0, Float32_2 = 0, Float32_3 = 1f },
        };

        var beginInfo = new RenderPassBeginInfo()
        {
            SType = StructureType.RenderPassBeginInfo,
            RenderPass = graphicsPipeline.VkRenderPass,
            Framebuffer = _framebuffer,
            RenderArea = new Rect2D(new Offset2D(0, 0),
                new Extent2D((uint?)image.Size.Width, (uint?)image.Size.Height)),
            ClearValueCount = 1,
            PClearValues = &clearValue
        };

        api.CmdBeginRenderPass(commandBufferHandle, beginInfo, SubpassContents.Inline);

        api.CmdBindPipeline(commandBufferHandle, PipelineBindPoint.Graphics, graphicsPipeline.VkPipeline);

        var vertexBuffers = new[] { vertexBuffer.VkBuffer };
        var offsets = new ulong[] { 0 };

        fixed (ulong* offsetsPtr = offsets)
        fixed (Silk.NET.Vulkan.Buffer* vertexBuffersPtr = vertexBuffers)
        {
            context.Api!.CmdBindVertexBuffers(commandBufferHandle, 0, 1, vertexBuffersPtr, offsetsPtr);
        }

        context.Api!.CmdBindIndexBuffer(commandBufferHandle, indexBuffer.VkBuffer, 0, IndexType.Uint16);

        context.Api!.CmdBindDescriptorSets(commandBufferHandle, PipelineBindPoint.Graphics,
            graphicsPipeline.VkPipelineLayout,
            0, 1, descriptorSet, 0, null);

        context.Api!.CmdDrawIndexed(commandBufferHandle, (uint)Primitives.Indices.Length, 1, 0, 0, 0);

        api.CmdEndRenderPass(commandBufferHandle);

        _colorAttachment.TransitionLayout(commandBuffer.InternalHandle, ImageLayout.TransferSrcOptimal,
            AccessFlags.TransferReadBit);
        image.TransitionLayout(commandBuffer.InternalHandle, ImageLayout.TransferDstOptimal,
            AccessFlags.TransferWriteBit);

        var srcBlitRegion = new ImageBlit
        {
            SrcOffsets = new ImageBlit.SrcOffsetsBuffer
            {
                Element0 = new Offset3D(0, 0, 0),
                Element1 = new Offset3D(image.Size.Width, image.Size.Height, 1),
            },
            DstOffsets = new ImageBlit.DstOffsetsBuffer
            {
                Element0 = new Offset3D(0, 0, 0),
                Element1 = new Offset3D(image.Size.Width, image.Size.Height, 1),
            },
            SrcSubresource =
                new ImageSubresourceLayers
                {
                    AspectMask = ImageAspectFlags.ColorBit,
                    BaseArrayLayer = 0,
                    LayerCount = 1,
                    MipLevel = 0
                },
            DstSubresource = new ImageSubresourceLayers
            {
                AspectMask = ImageAspectFlags.ColorBit,
                BaseArrayLayer = 0,
                LayerCount = 1,
                MipLevel = 0
            }
        };

        api.CmdBlitImage(commandBuffer.InternalHandle, _colorAttachment.InternalHandle,
            ImageLayout.TransferSrcOptimal,
            image.InternalHandle, ImageLayout.TransferDstOptimal, 1, srcBlitRegion, Filter.Linear);

        commandBuffer.Submit();
    }

    public void CreateTextureImage(VecI size)
    {
        texture = new VulkanTexture(context.Api!, context.LogicalDevice.Device, context.PhysicalDevice,
            context.Pool.CommandPool,
            context.GraphicsQueue, context.GraphicsQueueFamilyIndex, size);
    }

    private unsafe void CreateDescriptorSet()
    {
        var samplerLayoutBinding = new DescriptorSetLayoutBinding()
        {
            Binding = 1,
            DescriptorCount = 1,
            DescriptorType = DescriptorType.CombinedImageSampler,
            PImmutableSamplers = null,
            StageFlags = ShaderStageFlags.FragmentBit
        };

        fixed (DescriptorSetLayout* descriptorSetLayoutPtr = &descriptorSetLayout)
        {
            DescriptorSetLayoutCreateInfo layoutInfo = new()
            {
                SType = StructureType.DescriptorSetLayoutCreateInfo,
                BindingCount = 1,
                PBindings = &samplerLayoutBinding
            };

            if (context.Api!.CreateDescriptorSetLayout(context.LogicalDevice.Device, layoutInfo, null,
                    descriptorSetLayoutPtr) !=
                Result.Success)
                throw new VulkanException("Failed to create descriptor set layout.");

            var descriptorCreateInfo = new DescriptorSetAllocateInfo()
            {
                SType = StructureType.DescriptorSetAllocateInfo,
                DescriptorPool = context.DescriptorPool,
                DescriptorSetCount = 1,
                PSetLayouts = descriptorSetLayoutPtr
            };

            if (context.Api!.AllocateDescriptorSets(context.LogicalDevice.Device, descriptorCreateInfo,
                    out descriptorSet) !=
                Result.Success)
                throw new VulkanException("Failed to allocate descriptor set.");


            DescriptorImageInfo imageInfo = new()
            {
                Sampler = texture.Sampler,
                ImageView = texture.ImageView,
                ImageLayout = ImageLayout.ShaderReadOnlyOptimal
            };

            var samplerDescriptorSet = new WriteDescriptorSet()
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = descriptorSet,
                DstBinding = 1,
                DstArrayElement = 0,
                DescriptorType = DescriptorType.CombinedImageSampler,
                DescriptorCount = 1,
                PImageInfo = &imageInfo
            };

            context.Api!.UpdateDescriptorSets(context.LogicalDevice.Device, 1, &samplerDescriptorSet, 0, null);
        }
    }

    private void CreateGraphicsPipeline(VecI size)
    {
        GraphicsPipelineBuilder builder = new(context.Api!, context.LogicalDevice.Device);

        builder
            .AddStage(stage => stage.OfType(GraphicsPipelineStageType.Vertex).WithShader("shaders/vert.spv"))
            .AddStage(stage => stage.OfType(GraphicsPipelineStageType.Fragment).WithShader("shaders/frag.spv"))
            .WithRenderPass(renderPass =>
            {
                /*TODO: Add some meaningful stuff*/
            });

        Extent2D extent = new((uint)size.X, (uint)size.Y);
        Format swapChainImageFormat = Format.R8G8B8A8Unorm;
        graphicsPipeline = builder.Create(extent, swapChainImageFormat, ImageLayout.ColorAttachmentOptimal,
            ref descriptorSetLayout);
    }

    public unsafe void Dispose()
    {
        if (isInit)
        {
            DestroyTemporalObjects();

            vertexBuffer.Dispose();
            indexBuffer.Dispose();
        }

        isInit = false;
    }

    public unsafe void DestroyTemporalObjects()
    {
        if (isInit)
        {
            if (graphicsPipeline.VkRenderPass.Handle != 0)
            {
                var api = context.Api;
                var device = context.LogicalDevice.Device;
                api.FreeDescriptorSets(context.LogicalDevice.Device, context.DescriptorPool,
                    new[] { descriptorSet });

                api.DestroyFramebuffer(device, _framebuffer, null);
                graphicsPipeline.Dispose();

                api.DestroyDescriptorSetLayout(device, _descriptorSetLayout, null);

                texture.Dispose();

                _colorAttachment?.Dispose();

                _previousImageSize = PixelSize.Empty;
            }
        }
    }

    public unsafe void CreateTemporalObjects(PixelSize size)
    {
        DestroyTemporalObjects();

        _colorAttachment = new VulkanImage(context, (uint)Format.R8G8B8A8Unorm, size, false, Array.Empty<string>());

        VecI vecSize = new VecI(size.Width, size.Height);

        CreateTextureImage(vecSize);
        CreateDescriptorSet();
        CreateGraphicsPipeline(vecSize);

        // create framebuffer
        var frameBufferAttachments = new[] { new ImageView(_colorAttachment.ViewHandle) };

        fixed (ImageView* frAtPtr = frameBufferAttachments)
        {
            var framebufferCreateInfo = new FramebufferCreateInfo()
            {
                SType = StructureType.FramebufferCreateInfo,
                RenderPass = graphicsPipeline.VkRenderPass,
                AttachmentCount = (uint)frameBufferAttachments.Length,
                PAttachments = frAtPtr,
                Width = (uint)size.Width,
                Height = (uint)size.Height,
                Layers = 1
            };

            context.Api!.CreateFramebuffer(context.LogicalDevice.Device, framebufferCreateInfo, null,
                    out _framebuffer)
                .ThrowOnError();
        }

        isInit = true;
        _previousImageSize = size;
    }

    private unsafe void CreateBuffers()
    {
        ulong vertexBufferSize = (ulong)Marshal.SizeOf<Vertex>() * (ulong)Primitives.Vertices.Length;
        vertexBuffer = new VertexBuffer(context, vertexBufferSize);

        ulong indexBufferSize = (ulong)Marshal.SizeOf<ushort>() * (ulong)Primitives.Indices.Length;
        indexBuffer = new IndexBuffer(context, indexBufferSize);
    }

    public void PrepareTextureToWrite()
    {
        texture.TransitionLayoutTo(VulkanTexture.ShaderReadOnlyOptimal, VulkanTexture.ColorAttachmentOptimal);
    }
}