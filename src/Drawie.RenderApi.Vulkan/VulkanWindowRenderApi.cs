using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Drawie.Numerics;
using Drawie.RenderApi.Vulkan.Buffers;
using Drawie.RenderApi.Vulkan.Exceptions;
using Drawie.RenderApi.Vulkan.Helpers;
using Drawie.RenderApi.Vulkan.Stages;
using Drawie.RenderApi.Vulkan.Stages.Builders;
using Drawie.RenderApi.Vulkan.Structs;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.KHR;
using Buffer = Silk.NET.Vulkan.Buffer;
using Semaphore = Silk.NET.Vulkan.Semaphore;

namespace Drawie.RenderApi.Vulkan;

public class VulkanWindowRenderApi : IVulkanWindowRenderApi
{
    private const int MAX_FRAMES_IN_FLIGHT = 2;

    private VulkanWindowContext context;

    private KhrSwapchain? khrSwapchain;
    private SwapchainKHR swapChain;
    private Image[] swapChainImages;
    private Format swapChainImageFormat;
    private Extent2D swapChainExtent;
    private ImageView[] swapChainImageViews;
    private Framebuffer[] swapChainFramebuffers;

    private VecI framebufferSize;
    private VecI lastFramebufferSize;

    private DescriptorPool descriptorPool;
    private DescriptorSetLayout descriptorSetLayout;
    private GraphicsPipeline graphicsPipeline;

    private CommandPool commandPool;

    private VertexBuffer vertexBuffer;

    private IndexBuffer indexBuffer;

    public VulkanTexture texture;

    private DescriptorSet[] descriptorSets;

    private CommandBuffer[]? commandBuffers;

    private Semaphore[]? imageAvailableSemaphores;
    private Semaphore[]? renderFinishedSemaphores;
    private Fence[]? inFlightFences;
    private Fence[]? imagesInFlight;
    private int currentFrame = 0;
    public event Action? FramebufferResized;
    ITexture IWindowRenderApi.RenderTexture => texture;

    public VulkanWindowContext Context => context; 
    
    IVulkanContext IVulkanWindowRenderApi.Context => context;

    public VulkanWindowRenderApi(VulkanWindowContext context)
    {
        this.context = context;
    }

    public void UpdateFramebufferSize(int width, int height)
    {
        framebufferSize = new VecI(width, height);
    }

    public void PrepareTextureToWrite()
    {
        texture.TransitionLayoutTo(VulkanTexture.ShaderReadOnlyOptimal, VulkanTexture.ColorAttachmentOptimal);
    }

    public void CreateInstance(object contextObject, VecI framebufferSize)
    {
        if (contextObject is not IVulkanContextInfo vkContext) throw new VulkanNotSupportedException();

        this.framebufferSize = framebufferSize;

        if (context == null)
        {
            context = new VulkanWindowContext();
        }

        context.Initialize(vkContext);

        Console.WriteLine($"Selected GPU: {context.GpuInfo.Name}");

        CreateSwapChain();
        CreateImageViews();
        CreateDescriptorSetLayout();
        CreateGraphicsPipeline();
        CreateFramebuffers();
        CreateCommandPool();
        CreateTextureImage();
        CreateVertexBuffer();
        CreateIndexBuffer();
        CreateDescriptorPool();
        CreateDescriptorSets();
        CreateCommandBuffers();
        CreateSyncObjects();

        lastFramebufferSize = framebufferSize;
    }

    public unsafe void DestroyInstance()
    {
        context.LogicalDevice.WaitIdle();

        CleanupSwapchain();

        texture.Dispose();

        context.Api!.DestroyDescriptorSetLayout(context.LogicalDevice.Device, descriptorSetLayout, null);

        indexBuffer.Dispose();
        vertexBuffer.Dispose();

        for (var i = 0; i < MAX_FRAMES_IN_FLIGHT; i++)
        {
            context.Api!.DestroySemaphore(context.LogicalDevice.Device, renderFinishedSemaphores![i], null);
            context.Api!.DestroySemaphore(context.LogicalDevice.Device, imageAvailableSemaphores![i], null);
            context.Api!.DestroyFence(context.LogicalDevice.Device, inFlightFences![i], null);
        }

        context.Api!.DestroyCommandPool(context.LogicalDevice.Device, commandPool, null);

        foreach (var framebuffer in swapChainFramebuffers)
            context.Api!.DestroyFramebuffer(context.LogicalDevice.Device, framebuffer, null);

        graphicsPipeline.Dispose();

        foreach (var view in swapChainImageViews)
            context.Api!.DestroyImageView(context.LogicalDevice.Device, view, null);

        khrSwapchain!.DestroySwapchain(context.LogicalDevice.Device, swapChain, null);
        context.Dispose();
    }

    private unsafe void CleanupSwapchain()
    {
        foreach (var framebuffer in swapChainFramebuffers)
            context.Api!.DestroyFramebuffer(context.LogicalDevice.Device, framebuffer, null);

        fixed (CommandBuffer* commandBuffersPtr = commandBuffers)
        {
            context.Api!.FreeCommandBuffers(context.LogicalDevice.Device, commandPool, (uint)commandBuffers!.Length,
                commandBuffersPtr);
        }

        graphicsPipeline.Dispose();

        foreach (var imageView in swapChainImageViews)
            context.Api!.DestroyImageView(context.LogicalDevice.Device, imageView, null);

        khrSwapchain!.DestroySwapchain(context.LogicalDevice.Device, swapChain, null);

        context.Api!.DestroyDescriptorPool(context.LogicalDevice.Device, descriptorPool, null);
    }

    private unsafe void CreateDescriptorSetLayout()
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
        }
    }

    public void CreateTextureImage()
    {
        texture = new VulkanTexture(context.Api!, context.LogicalDevice.Device, context.PhysicalDevice, commandPool,
            context.GraphicsQueue, context.GraphicsQueueFamilyIndex, framebufferSize);
        texture.MakeReadOnly();
    }

    private unsafe void CreateDescriptorPool()
    {
        var poolSize = new DescriptorPoolSize()
        {
            Type = DescriptorType.CombinedImageSampler,
            DescriptorCount = (uint)swapChainImages.Length
        };

        fixed (DescriptorPool* descriptorPoolPtr = &descriptorPool)
        {
            DescriptorPoolCreateInfo poolInfo = new()
            {
                SType = StructureType.DescriptorPoolCreateInfo,
                PoolSizeCount = 1,
                PPoolSizes = &poolSize,
                MaxSets = (uint)swapChainImages.Length
            };
            if (context.Api!.CreateDescriptorPool(context.LogicalDevice.Device, poolInfo, null, descriptorPoolPtr) !=
                Result.Success)
                throw new VulkanException("Failed to create descriptor pool.");
        }
    }

    private unsafe void CreateDescriptorSets()
    {
        var layouts = new DescriptorSetLayout[swapChainImages.Length];
        Array.Fill(layouts, descriptorSetLayout);

        fixed (DescriptorSetLayout* layoutsPtr = layouts)
        {
            DescriptorSetAllocateInfo allocInfo = new()
            {
                SType = StructureType.DescriptorSetAllocateInfo,
                DescriptorPool = descriptorPool,
                DescriptorSetCount = (uint)swapChainImages.Length,
                PSetLayouts = layoutsPtr
            };

            descriptorSets = new DescriptorSet[swapChainImages.Length];
            fixed (DescriptorSet* descriptorSetsPtr = descriptorSets)
            {
                if (context.Api!.AllocateDescriptorSets(context.LogicalDevice.Device, allocInfo, descriptorSetsPtr) !=
                    Result.Success)
                    throw new VulkanException("Failed to allocate descriptor sets.");
            }
        }

        for (var i = 0; i < swapChainImages.Length; i++)
        {
            DescriptorImageInfo imageInfo = new()
            {
                Sampler = texture.Sampler,
                ImageView = texture.ImageView,
                ImageLayout = ImageLayout.ShaderReadOnlyOptimal
            };

            var samplerDescriptorSet = new WriteDescriptorSet()
            {
                SType = StructureType.WriteDescriptorSet,
                DstSet = descriptorSets[i],
                DstBinding = 1,
                DstArrayElement = 0,
                DescriptorType = DescriptorType.CombinedImageSampler,
                DescriptorCount = 1,
                PImageInfo = &imageInfo
            };

            context.Api!.UpdateDescriptorSets(context.LogicalDevice.Device, 1, &samplerDescriptorSet, 0, null);
        }
    }

    private void RecreateSwapchain()
    {
        if (framebufferSize.X == 0 || framebufferSize.Y == 0)
        {
            // Handle minimized window differently than in tutorial
            /*
             *  while (framebufferSize.X == 0 || framebufferSize.Y == 0)
                      {
                          framebufferSize = window.FramebufferSize;
                          window.DoEvents();
                      }
             */
            framebufferSize = lastFramebufferSize;
            return;
        }

        context.Api!.DeviceWaitIdle(context.LogicalDevice.Device);

        CleanupSwapchain();

        texture.Dispose();

        CreateSwapChain();
        CreateImageViews();
        CreateGraphicsPipeline();
        CreateFramebuffers();
        CreateTextureImage();
        CreateDescriptorPool();
        CreateDescriptorSets();
        CreateCommandBuffers();

        imagesInFlight = new Fence[swapChainImages.Length];

        lastFramebufferSize = framebufferSize;

        FramebufferResized?.Invoke();
    }

    private unsafe void CreateSyncObjects()
    {
        imageAvailableSemaphores = new Semaphore[MAX_FRAMES_IN_FLIGHT];
        renderFinishedSemaphores = new Semaphore[MAX_FRAMES_IN_FLIGHT];
        inFlightFences = new Fence[MAX_FRAMES_IN_FLIGHT];
        imagesInFlight = new Fence[swapChainImages.Length];

        SemaphoreCreateInfo semaphoreInfo = new()
        {
            SType = StructureType.SemaphoreCreateInfo
        };

        FenceCreateInfo fenceInfo = new()
        {
            SType = StructureType.FenceCreateInfo,
            Flags = FenceCreateFlags.SignaledBit
        };

        for (var i = 0; i < MAX_FRAMES_IN_FLIGHT; i++)
            if (context.Api!.CreateSemaphore(context.LogicalDevice.Device, semaphoreInfo, null,
                    out imageAvailableSemaphores[i]) !=
                Result.Success ||
                context.Api!.CreateSemaphore(context.LogicalDevice.Device, semaphoreInfo, null,
                    out renderFinishedSemaphores[i]) !=
                Result.Success ||
                context.Api!.CreateFence(context.LogicalDevice.Device, fenceInfo, null, out inFlightFences[i]) !=
                Result.Success)
                throw new VulkanException("Failed to create synchronization objects for a frame.");
    }

    public unsafe void Render(double deltaTime)
    {
        context.Api!.WaitForFences(context.LogicalDevice.Device, 1, inFlightFences![currentFrame], true, ulong.MaxValue);

        uint imageIndex = 0;
        var acquireResult = khrSwapchain!.AcquireNextImage(context.LogicalDevice.Device, swapChain, ulong.MaxValue,
            imageAvailableSemaphores![currentFrame], default, ref imageIndex);

        if (acquireResult == Result.ErrorOutOfDateKhr)
        {
            RecreateSwapchain();
            return;
        }
        else if (acquireResult != Result.Success && acquireResult != Result.SuboptimalKhr)
        {
            throw new VulkanException("Failed to acquire swap chain image.");
        }

        UpdateTextureLayout();

        if (imagesInFlight![imageIndex].Handle != default)
            context.Api!.WaitForFences(context.LogicalDevice.Device, 1, imagesInFlight[imageIndex], true,
                ulong.MaxValue);

        imagesInFlight[imageIndex] = inFlightFences[currentFrame];

        SubmitInfo submitInfo = new()
        {
            SType = StructureType.SubmitInfo
        };

        var waitSemaphores = stackalloc[] { imageAvailableSemaphores[currentFrame] };
        var waitStages = stackalloc[] { PipelineStageFlags.ColorAttachmentOutputBit };

        var buffer = commandBuffers![imageIndex];

        submitInfo = submitInfo with
        {
            WaitSemaphoreCount = 1,
            PWaitSemaphores = waitSemaphores,
            PWaitDstStageMask = waitStages,

            CommandBufferCount = 1,
            PCommandBuffers = &buffer
        };

        var signalSemaphores = stackalloc[] { renderFinishedSemaphores![currentFrame] };
        submitInfo = submitInfo with
        {
            SignalSemaphoreCount = 1,
            PSignalSemaphores = signalSemaphores
        };

        context.Api!.ResetFences(context.LogicalDevice.Device, 1, inFlightFences[currentFrame]);

        if (context.Api!.QueueSubmit(context.GraphicsQueue, 1, &submitInfo, inFlightFences[currentFrame]) !=
            Result.Success)
            throw new VulkanException("Failed to submit draw command buffer.");

        var swapChains = stackalloc[] { swapChain };

        PresentInfoKHR presentInfo = new()
        {
            SType = StructureType.PresentInfoKhr,
            WaitSemaphoreCount = 1,
            PWaitSemaphores = signalSemaphores,
            SwapchainCount = 1,
            PSwapchains = swapChains,
            PImageIndices = &imageIndex
        };

        var result = khrSwapchain!.QueuePresent(context.PresentQueue, presentInfo);

        if (result == Result.ErrorOutOfDateKhr || result == Result.SuboptimalKhr ||
            lastFramebufferSize != framebufferSize)
            RecreateSwapchain();
        else if (result != Result.Success) throw new VulkanException("Failed to present swap chain image.");

        currentFrame = (currentFrame + 1) % MAX_FRAMES_IN_FLIGHT;
    }


    private void UpdateTextureLayout()
    {
        texture.TransitionLayoutTo(VulkanTexture.ColorAttachmentOptimal, VulkanTexture.ShaderReadOnlyOptimal);
    }

    private unsafe void CreateCommandPool()
    {
        var queueFamilyIndices = SetupUtility.FindQueueFamilies(context);

        CommandPoolCreateInfo poolInfo = new()
        {
            SType = StructureType.CommandPoolCreateInfo,
            QueueFamilyIndex = queueFamilyIndices.GraphicsFamily!.Value
        };

        if (context.Api!.CreateCommandPool(context.LogicalDevice.Device, in poolInfo, null, out commandPool) !=
            Result.Success)
            throw new VulkanException("Failed to create command pool.");
    }

    private unsafe void CreateCommandBuffers()
    {
        commandBuffers = new CommandBuffer[swapChainFramebuffers.Length];
        CommandBufferAllocateInfo allocInfo = new()
        {
            SType = StructureType.CommandBufferAllocateInfo,
            CommandPool = commandPool,
            Level = CommandBufferLevel.Primary,
            CommandBufferCount = (uint)commandBuffers.Length
        };

        fixed (CommandBuffer* commandBuffersPtr = commandBuffers)
        {
            if (context.Api!.AllocateCommandBuffers(context.LogicalDevice.Device, in allocInfo, commandBuffersPtr) !=
                Result.Success)
                throw new VulkanException("Failed to allocate command buffers.");
        }

        for (var i = 0; i < commandBuffers.Length; i++)
        {
            CommandBufferBeginInfo beginInfo = new()
            {
                SType = StructureType.CommandBufferBeginInfo
            };

            if (context.Api!.BeginCommandBuffer(commandBuffers[i], in beginInfo) != Result.Success)
                throw new VulkanException("Failed to begin recording command buffer.");

            RenderPassBeginInfo renderPassInfo = new()
            {
                SType = StructureType.RenderPassBeginInfo,
                RenderPass = graphicsPipeline.VkRenderPass,
                Framebuffer = swapChainFramebuffers[i],
                RenderArea = new Rect2D
                {
                    Offset = new Offset2D(0, 0),
                    Extent = swapChainExtent
                }
            };

            ClearValue clearColor = new()
            {
                Color = new ClearColorValue() { Float32_0 = 0, Float32_1 = 0, Float32_2 = 0, Float32_3 = 1 }
            };

            renderPassInfo.ClearValueCount = 1;
            renderPassInfo.PClearValues = &clearColor;

            context.Api!.CmdBeginRenderPass(commandBuffers[i], &renderPassInfo, SubpassContents.Inline);
            context.Api!.CmdBindPipeline(commandBuffers[i], PipelineBindPoint.Graphics, graphicsPipeline.VkPipeline);

            var vertexBuffers = new[] { vertexBuffer.VkBuffer };
            var offsets = new ulong[] { 0 };

            fixed (ulong* offsetsPtr = offsets)
            fixed (Buffer* vertexBuffersPtr = vertexBuffers)
            {
                context.Api!.CmdBindVertexBuffers(commandBuffers[i], 0, 1, vertexBuffersPtr, offsetsPtr);
            }

            context.Api!.CmdBindIndexBuffer(commandBuffers[i], indexBuffer.VkBuffer, 0, IndexType.Uint16);

            context.Api!.CmdBindDescriptorSets(commandBuffers[i], PipelineBindPoint.Graphics,
                graphicsPipeline.VkPipelineLayout,
                0, 1, descriptorSets[i], 0, null);

            context.Api!.CmdDrawIndexed(commandBuffers[i], (uint)Primitives.Indices.Length, 1, 0, 0, 0);

            context.Api!.CmdEndRenderPass(commandBuffers[i]);

            if (context.Api!.EndCommandBuffer(commandBuffers[i]) != Result.Success)
                throw new VulkanException("Failed to record command buffer.");
        }
    }

    private unsafe void CreateFramebuffers()
    {
        swapChainFramebuffers = new Framebuffer[swapChainImageViews!.Length];

        for (var i = 0; i < swapChainImageViews.Length; i++)
        {
            var attachment = swapChainImageViews[i];

            FramebufferCreateInfo framebufferInfo = new()
            {
                SType = StructureType.FramebufferCreateInfo,
                RenderPass = graphicsPipeline.VkRenderPass,
                AttachmentCount = 1,
                PAttachments = &attachment,
                Width = swapChainExtent.Width,
                Height = swapChainExtent.Height,
                Layers = 1
            };

            if (context.Api!.CreateFramebuffer(context.LogicalDevice.Device, framebufferInfo, null,
                    out swapChainFramebuffers[i]) !=
                Result.Success) throw new VulkanException("Failed to create framebuffer.");
        }
    }

    private void CreateGraphicsPipeline()
    {
        GraphicsPipelineBuilder builder = new(context.Api!, context.LogicalDevice.Device);

        builder
            .AddStage(stage => stage.OfType(GraphicsPipelineStageType.Vertex).WithShader("Drawie.RenderApi.Vulkan.Shaders.vert.spv"))
            .AddStage(stage => stage.OfType(GraphicsPipelineStageType.Fragment).WithShader("Drawie.RenderApi.Vulkan.Shaders.frag.spv"))
            .WithRenderPass(renderPass =>
            {
                /*TODO: Add some meaningful stuff*/
            });

        graphicsPipeline = builder.Create(swapChainExtent, swapChainImageFormat, ImageLayout.PresentSrcKhr, ref descriptorSetLayout);
    }

    private unsafe void CreateImageViews()
    {
        swapChainImageViews = new ImageView[swapChainImages.Length];

        for (var i = 0; i < swapChainImages.Length; i++)
            swapChainImageViews[i] = ImageUtility.CreateViewForImage(context.Api!, context.LogicalDevice.Device,
                swapChainImages[i],
                swapChainImageFormat);
    }

    private unsafe void CreateSwapChain()
    {
        var swapChainSupport = SetupUtility.QuerySwapChainSupport(context.PhysicalDevice, context.Surface, context.KhrSurface!);

        var surfaceFormat = ChooseSwapSurfaceFormat(swapChainSupport.Formats);
        var presentMode = ChoosePresentMode(swapChainSupport.PresentModes);
        var extent = ChooseSwapExtent(swapChainSupport.Capabilities);

        var imageCount = swapChainSupport.Capabilities.MinImageCount + 1;
        if (swapChainSupport.Capabilities.MaxImageCount > 0 && imageCount > swapChainSupport.Capabilities.MaxImageCount)
            imageCount = swapChainSupport.Capabilities.MaxImageCount;

        SwapchainCreateInfoKHR creatInfo = new()
        {
            SType = StructureType.SwapchainCreateInfoKhr,
            Surface = context.Surface,

            MinImageCount = imageCount,
            ImageFormat = surfaceFormat.Format,
            ImageColorSpace = surfaceFormat.ColorSpace,
            ImageExtent = extent,
            ImageArrayLayers = 1,
            ImageUsage = ImageUsageFlags.ColorAttachmentBit
        };

        var indices = SetupUtility.FindQueueFamilies(context);
        var queueFamilyIndices = stackalloc[] { indices.GraphicsFamily!.Value, indices.PresentFamily!.Value };

        if (indices.GraphicsFamily != indices.PresentFamily)
            creatInfo = creatInfo with
            {
                ImageSharingMode = SharingMode.Concurrent,
                QueueFamilyIndexCount = 2,
                PQueueFamilyIndices = queueFamilyIndices
            };
        else
            creatInfo.ImageSharingMode = SharingMode.Exclusive;

        creatInfo = creatInfo with
        {
            PreTransform = swapChainSupport.Capabilities.CurrentTransform,
            CompositeAlpha = CompositeAlphaFlagsKHR.OpaqueBitKhr,
            PresentMode = presentMode,
            Clipped = true,

            OldSwapchain = default
        };

        if (!context.Api!.TryGetDeviceExtension(context.Instance, context.LogicalDevice.Device, out khrSwapchain))
            throw new NotSupportedException("VK_KHR_swapchain extension not found.");

        if (khrSwapchain!.CreateSwapchain(context.LogicalDevice.Device, creatInfo, null, out swapChain) !=
            Result.Success)
            throw new VulkanException("Failed to create swap chain.");

        khrSwapchain.GetSwapchainImages(context.LogicalDevice.Device, swapChain, ref imageCount, null);
        swapChainImages = new Image[imageCount];
        fixed (Image* swapChainImagesPtr = swapChainImages)
        {
            khrSwapchain.GetSwapchainImages(context.LogicalDevice.Device, swapChain, ref imageCount,
                swapChainImagesPtr);
        }

        swapChainImageFormat = surfaceFormat.Format;
        swapChainExtent = extent;
    }

    private SurfaceFormatKHR ChooseSwapSurfaceFormat(IReadOnlyList<SurfaceFormatKHR> availableFormats)
    {
        foreach (var availableFormat in availableFormats)
            if (availableFormat is { Format: Format.R8G8B8A8Unorm, ColorSpace: ColorSpaceKHR.SpaceSrgbNonlinearKhr })
                return availableFormat;

        return availableFormats[0];
    }

    private PresentModeKHR ChoosePresentMode(IReadOnlyList<PresentModeKHR> availablePresentModes)
    {
        foreach (var availablePresentMode in availablePresentModes)
            if (availablePresentMode == PresentModeKHR.MailboxKhr)
                return availablePresentMode;

        return PresentModeKHR.FifoKhr;
    }

    private Extent2D ChooseSwapExtent(SurfaceCapabilitiesKHR capabilities)
    {
        if (capabilities.CurrentExtent.Width != uint.MaxValue) return capabilities.CurrentExtent;

        Extent2D actualExtent = new()
        {
            Width = (uint)framebufferSize.X,
            Height = (uint)framebufferSize.Y
        };

        actualExtent.Width = Math.Clamp(actualExtent.Width, capabilities.MinImageExtent.Width,
            capabilities.MaxImageExtent.Width);
        actualExtent.Height = Math.Clamp(actualExtent.Height, capabilities.MinImageExtent.Height,
            capabilities.MaxImageExtent.Height);

        return actualExtent;
    }

    private unsafe void CreateIndexBuffer()
    {
        var bufferSize = (ulong)Unsafe.SizeOf<ushort>() * (ulong)Primitives.Indices.Length;

        using StagingBuffer stagingBuffer = new(context, bufferSize);

        stagingBuffer.SetData(Primitives.Indices);

        indexBuffer = new IndexBuffer(context, bufferSize);

        CopyBuffer(stagingBuffer, indexBuffer, bufferSize);
    }

    private void CreateVertexBuffer()
    {
        var bufferSize = (ulong)Marshal.SizeOf<Vertex>() * (ulong)Primitives.Vertices.Length;

        using StagingBuffer stagingBuffer = new(context, bufferSize);

        stagingBuffer.SetData(Primitives.Vertices);

        vertexBuffer = new VertexBuffer(context, bufferSize);
        CopyBuffer(stagingBuffer, vertexBuffer, bufferSize);
    }

    private void CopyBuffer(BufferObject srcBuffer, BufferObject dstBuffer, ulong size)
    {
        using var commandBuffer =
            new SingleTimeCommandBufferSession(context, commandPool);

        BufferCopy copyRegion = new()
        {
            Size = size
        };

        context.Api!.CmdCopyBuffer(commandBuffer.CommandBuffer, srcBuffer.VkBuffer, dstBuffer.VkBuffer, 1, copyRegion);
    }
}
