using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Drawie.RenderApi.Vulkan.Exceptions;
using PixiEditor.Numerics;
using Silk.NET.Core;
using Silk.NET.Core.Contexts;
using Silk.NET.Core.Native;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using Semaphore = Silk.NET.Vulkan.Semaphore;

namespace Drawie.RenderApi.Vulkan;

public class VulkanWindowRenderApi : IWindowRenderApi
{
    public Vk? Vk { get; private set; }


    private const int MAX_FRAMES_IN_FLIGHT = 2;

    public Instance Instance
    {
        get => instance;
        set => instance = value;
    }

    public bool EnableValidationLayers { get; set; } = true;
    public PhysicalDevice PhysicalDevice { get; private set; }

    public Device LogicalDevice
    {
        get => logicalDevice;
        set => logicalDevice = value;
    }

    private readonly string[] validationLayers =
    {
        "VK_LAYER_KHRONOS_validation"
    };

    private readonly string[] deviceExtensions =
    {
        KhrSwapchain.ExtensionName
    };

    private ExtDebugUtils extDebugUtils;
    private DebugUtilsMessengerEXT debugMessenger;
    private Instance instance;

    private Device logicalDevice;

    private KhrSurface? khrSurface;
    private SurfaceKHR surface;

    private Queue graphicsQueue;
    private Queue presentQueue;

    private KhrSwapchain? khrSwapchain;
    private SwapchainKHR swapChain;
    private Image[] swapChainImages;
    private Format swapChainImageFormat;
    private Extent2D swapChainExtent;
    private ImageView[] swapChainImageViews;
    private Framebuffer[] swapChainFramebuffers;

    private VecI framebufferSize;
    private VecI lastFramebufferSize;

    private RenderPass renderPass;

    private PipelineLayout pipelineLayout;
    private Pipeline graphicsPipeline;

    private CommandPool commandPool;
    private CommandBuffer[]? commandBuffers;

    private Semaphore[]? imageAvailableSemaphores;
    private Semaphore[]? renderFinishedSemaphores;
    private Fence[]? inFlightFences;
    private Fence[]? imagesInFlight;
    private int currentFrame = 0;

    public GraphicsApi GraphicsApi => GraphicsApi.Vulkan;

    public void UpdateFramebufferSize(int width, int height)
    {
        framebufferSize = new VecI(width, height);
    }

    public unsafe void CreateInstance(object surfaceObject, VecI framebufferSize)
    {
        if (surfaceObject is not IVkSurface vkSurface) throw new VulkanNotSupportedException();

        this.framebufferSize = framebufferSize;

        SetupInstance(vkSurface);
        SetupDebugMessenger();
        CreateSurface(vkSurface);
        GpuInfo selectedGpu = PickPhysicalDevice();
        
        Console.WriteLine($"Selected GPU: {selectedGpu.Name}");
        
        CreateLogicalDevice();
        CreateSwapChain();
        CreateImageViews();
        CreateRenderPass();
        CreateGraphicsPipeline();
        CreateFramebuffers();
        CreateCommandPool();
        CreateCommandBuffers();
        CreateSyncObjects();
    }

    public unsafe void DestroyInstance()
    {
        Vk!.DeviceWaitIdle(LogicalDevice);
        
        CleanupSwapchain();

        for (var i = 0; i < MAX_FRAMES_IN_FLIGHT; i++)
        {
            Vk!.DestroySemaphore(logicalDevice, renderFinishedSemaphores![i], null);
            Vk!.DestroySemaphore(logicalDevice, imageAvailableSemaphores![i], null);
            Vk!.DestroyFence(logicalDevice, inFlightFences![i], null);
        }

        Vk!.DestroyCommandPool(LogicalDevice, commandPool, null);

        foreach (var framebuffer in swapChainFramebuffers) Vk!.DestroyFramebuffer(LogicalDevice, framebuffer, null);

        Vk!.DestroyPipeline(LogicalDevice, graphicsPipeline, null);
        Vk!.DestroyPipelineLayout(LogicalDevice, pipelineLayout, null);
        Vk!.DestroyRenderPass(LogicalDevice, renderPass, null);

        foreach (var view in swapChainImageViews) Vk!.DestroyImageView(LogicalDevice, view, null);

        khrSwapchain!.DestroySwapchain(LogicalDevice, swapChain, null);
        Vk!.DestroyDevice(LogicalDevice, null);
        if (EnableValidationLayers) extDebugUtils!.DestroyDebugUtilsMessenger(Instance, debugMessenger, null);

        khrSurface!.DestroySurface(instance, surface, null);
        Vk!.DestroyInstance(Instance, null);
        Vk!.Dispose();
    }

    private unsafe void CleanupSwapchain()
    {
        foreach (var framebuffer in swapChainFramebuffers)
        {
            Vk!.DestroyFramebuffer(LogicalDevice, framebuffer, null);
        }
        
        fixed (CommandBuffer* commandBuffersPtr = commandBuffers)
        {
            Vk!.FreeCommandBuffers(LogicalDevice, commandPool, (uint)commandBuffers!.Length, commandBuffersPtr);
        }
        
        Vk!.DestroyPipeline(LogicalDevice, graphicsPipeline, null);
        Vk!.DestroyPipelineLayout(LogicalDevice, pipelineLayout, null);
        Vk!.DestroyRenderPass(LogicalDevice, renderPass, null);
        
        foreach (var imageView in swapChainImageViews)
        {
            Vk!.DestroyImageView(LogicalDevice, imageView, null);
        }
        
        khrSwapchain!.DestroySwapchain(LogicalDevice, swapChain, null);
    }

    private void RecreateSwapchain()
    {
        if(framebufferSize.X == 0 || framebufferSize.Y == 0)
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
        
        Vk!.DeviceWaitIdle(LogicalDevice);
        
        CleanupSwapchain();
        
        CreateSwapChain();
        CreateImageViews();
        CreateRenderPass();
        CreateGraphicsPipeline();
        CreateFramebuffers();
        CreateCommandBuffers();
        
        imagesInFlight = new Fence[swapChainImages.Length];
        
        lastFramebufferSize = framebufferSize;
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
            if (Vk!.CreateSemaphore(LogicalDevice, semaphoreInfo, null, out imageAvailableSemaphores[i]) !=
                Result.Success ||
                Vk!.CreateSemaphore(LogicalDevice, semaphoreInfo, null, out renderFinishedSemaphores[i]) !=
                Result.Success ||
                Vk!.CreateFence(LogicalDevice, fenceInfo, null, out inFlightFences[i]) != Result.Success)
                throw new VulkanException("Failed to create synchronization objects for a frame.");
    }

    public unsafe void Render(double deltaTime)
    {
        Vk!.WaitForFences(LogicalDevice, 1, inFlightFences![currentFrame], true, ulong.MaxValue);

        uint imageIndex = 0;
        var acquireResult = khrSwapchain!.AcquireNextImage(LogicalDevice, swapChain, ulong.MaxValue,
            imageAvailableSemaphores![currentFrame], default, ref imageIndex);

        if (acquireResult == Result.ErrorOutOfDateKhr)
        { 
            RecreateSwapchain();
            return;
        }

        if (imagesInFlight![imageIndex].Handle != default)
            Vk!.WaitForFences(LogicalDevice, 1, imagesInFlight[imageIndex], true, ulong.MaxValue);

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

        Vk!.ResetFences(LogicalDevice, 1, inFlightFences[currentFrame]);

        if (Vk!.QueueSubmit(graphicsQueue, 1, &submitInfo, inFlightFences[currentFrame]) != Result.Success)
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

        var result = khrSwapchain!.QueuePresent(presentQueue, presentInfo);
        
        if(result == Result.ErrorOutOfDateKhr || result == Result.SuboptimalKhr || lastFramebufferSize != framebufferSize)
        {
            RecreateSwapchain();
        }
        else if(result != Result.Success)
        {
            throw new VulkanException("Failed to present swap chain image.");
        }
        
        currentFrame = (currentFrame + 1) % MAX_FRAMES_IN_FLIGHT;
    }

    private unsafe void CreateCommandPool()
    {
        var queueFamilyIndices = FindQueueFamilies(PhysicalDevice);

        CommandPoolCreateInfo poolInfo = new()
        {
            SType = StructureType.CommandPoolCreateInfo,
            QueueFamilyIndex = queueFamilyIndices.GraphicsFamily!.Value
        };

        if (Vk!.CreateCommandPool(LogicalDevice, in poolInfo, null, out commandPool) != Result.Success)
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
            if (Vk!.AllocateCommandBuffers(LogicalDevice, in allocInfo, commandBuffersPtr) != Result.Success)
                throw new VulkanException("Failed to allocate command buffers.");
        }

        for (var i = 0; i < commandBuffers.Length; i++)
        {
            CommandBufferBeginInfo beginInfo = new()
            {
                SType = StructureType.CommandBufferBeginInfo
            };

            if (Vk!.BeginCommandBuffer(commandBuffers[i], in beginInfo) != Result.Success)
                throw new VulkanException("Failed to begin recording command buffer.");

            RenderPassBeginInfo renderPassInfo = new()
            {
                SType = StructureType.RenderPassBeginInfo,
                RenderPass = renderPass,
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

            Vk!.CmdBeginRenderPass(commandBuffers[i], &renderPassInfo, SubpassContents.Inline);
            Vk!.CmdBindPipeline(commandBuffers[i], PipelineBindPoint.Graphics, graphicsPipeline);
            Vk!.CmdDraw(commandBuffers[i], 3, 1, 0, 0);
            Vk!.CmdEndRenderPass(commandBuffers[i]);

            if (Vk!.EndCommandBuffer(commandBuffers[i]) != Result.Success)
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
                RenderPass = renderPass,
                AttachmentCount = 1,
                PAttachments = &attachment,
                Width = swapChainExtent.Width,
                Height = swapChainExtent.Height,
                Layers = 1
            };

            if (Vk!.CreateFramebuffer(logicalDevice, framebufferInfo, null, out swapChainFramebuffers[i]) !=
                Result.Success) throw new VulkanException("Failed to create framebuffer.");
        }
    }

    private unsafe void CreateGraphicsPipeline()
    {
        var vertShaderCode = File.ReadAllBytes("shaders/vert.spv");
        var fragShaderCode = File.ReadAllBytes("shaders/frag.spv");

        var vertShaderModule = CreateShaderModule(vertShaderCode);
        var fragShaderModule = CreateShaderModule(fragShaderCode);

        PipelineShaderStageCreateInfo vertShaderStageInfo = new()
        {
            SType = StructureType.PipelineShaderStageCreateInfo,
            Stage = ShaderStageFlags.VertexBit,
            Module = vertShaderModule,
            PName = (byte*)Marshal.StringToHGlobalAnsi("main")
        };

        PipelineShaderStageCreateInfo fragShaderStageInfo = new()
        {
            SType = StructureType.PipelineShaderStageCreateInfo,
            Stage = ShaderStageFlags.FragmentBit,
            Module = fragShaderModule,
            PName = (byte*)Marshal.StringToHGlobalAnsi("main")
        };

        var shaderStages = stackalloc[]
        {
            vertShaderStageInfo,
            fragShaderStageInfo
        };

        PipelineVertexInputStateCreateInfo vertexInputInfo = new()
        {
            SType = StructureType.PipelineVertexInputStateCreateInfo,
            VertexBindingDescriptionCount = 0,
            VertexAttributeDescriptionCount = 0
        };

        PipelineInputAssemblyStateCreateInfo inputAssembly = new()
        {
            SType = StructureType.PipelineInputAssemblyStateCreateInfo,
            Topology = PrimitiveTopology.TriangleList,
            PrimitiveRestartEnable = false
        };

        Viewport viewport = new()
        {
            X = 0.0f,
            Y = 0.0f,
            Width = (float)swapChainExtent.Width,
            Height = (float)swapChainExtent.Height,
            MinDepth = 0.0f,
            MaxDepth = 1.0f
        };

        Rect2D scissor = new()
        {
            Offset = new Offset2D(0, 0),
            Extent = swapChainExtent
        };

        PipelineViewportStateCreateInfo viewportState = new()
        {
            SType = StructureType.PipelineViewportStateCreateInfo,
            ViewportCount = 1,
            PViewports = &viewport,
            ScissorCount = 1,
            PScissors = &scissor
        };

        PipelineRasterizationStateCreateInfo rasterizer = new()
        {
            SType = StructureType.PipelineRasterizationStateCreateInfo,
            DepthClampEnable = false,
            RasterizerDiscardEnable = false,
            PolygonMode = PolygonMode.Fill,
            LineWidth = 1.0f,
            CullMode = CullModeFlags.BackBit,
            FrontFace = FrontFace.Clockwise,
            DepthBiasEnable = false
        };

        PipelineMultisampleStateCreateInfo multisampling = new()
        {
            SType = StructureType.PipelineMultisampleStateCreateInfo,
            SampleShadingEnable = false,
            RasterizationSamples = SampleCountFlags.Count1Bit
        };

        PipelineColorBlendAttachmentState colorBlendAttachment = new()
        {
            ColorWriteMask = ColorComponentFlags.RBit | ColorComponentFlags.GBit | ColorComponentFlags.BBit |
                             ColorComponentFlags.ABit,
            BlendEnable = false
        };

        PipelineColorBlendStateCreateInfo colorBlending = new()
        {
            SType = StructureType.PipelineColorBlendStateCreateInfo,
            LogicOpEnable = false,
            LogicOp = LogicOp.Copy,
            AttachmentCount = 1,
            PAttachments = &colorBlendAttachment
        };

        colorBlending.BlendConstants[0] = 0.0f;
        colorBlending.BlendConstants[1] = 0.0f;
        colorBlending.BlendConstants[2] = 0.0f;
        colorBlending.BlendConstants[3] = 0.0f;

        PipelineLayoutCreateInfo pipelineLayoutInfo = new()
        {
            SType = StructureType.PipelineLayoutCreateInfo,
            SetLayoutCount = 0,
            PushConstantRangeCount = 0
        };

        if (Vk!.CreatePipelineLayout(LogicalDevice, in pipelineLayoutInfo, null, out pipelineLayout) != Result.Success)
            throw new VulkanException("Failed to create pipeline layout.");

        GraphicsPipelineCreateInfo pipelineCreateInfo = new()
        {
            SType = StructureType.GraphicsPipelineCreateInfo,
            StageCount = 2,
            PStages = shaderStages,
            PVertexInputState = &vertexInputInfo,
            PInputAssemblyState = &inputAssembly,
            PViewportState = &viewportState,
            PRasterizationState = &rasterizer,
            PMultisampleState = &multisampling,
            PColorBlendState = &colorBlending,
            Layout = pipelineLayout,
            RenderPass = renderPass,
            Subpass = 0,
            BasePipelineHandle = default
        };

        if (Vk!.CreateGraphicsPipelines(LogicalDevice, default, 1, &pipelineCreateInfo, null, out graphicsPipeline) !=
            Result.Success) throw new VulkanException("Failed to create graphics pipeline.");

        Vk!.DestroyShaderModule(LogicalDevice, vertShaderModule, null);
        Vk!.DestroyShaderModule(LogicalDevice, fragShaderModule, null);

        SilkMarshal.Free((nint)vertShaderStageInfo.PName);
        SilkMarshal.Free((nint)fragShaderStageInfo.PName);
    }

    private unsafe ShaderModule CreateShaderModule(byte[] code)
    {
        ShaderModuleCreateInfo createInfo = new()
        {
            SType = StructureType.ShaderModuleCreateInfo,
            CodeSize = (nuint)code.Length
        };

        ShaderModule shaderModule;

        fixed (byte* codePtr = code)
        {
            createInfo.PCode = (uint*)codePtr;
            if (Vk!.CreateShaderModule(LogicalDevice, in createInfo, null, out shaderModule) != Result.Success)
                throw new VulkanException("Failed to create shader module");
        }

        return shaderModule;
    }

    private unsafe void CreateImageViews()
    {
        swapChainImageViews = new ImageView[swapChainImages.Length];

        for (var i = 0; i < swapChainImages.Length; i++)
        {
            ImageViewCreateInfo createInfo = new()
            {
                SType = StructureType.ImageViewCreateInfo,
                Image = swapChainImages[i],
                ViewType = ImageViewType.Type2D,
                Format = swapChainImageFormat,
                Components = new ComponentMapping
                {
                    R = ComponentSwizzle.Identity,
                    G = ComponentSwizzle.Identity,
                    B = ComponentSwizzle.Identity,
                    A = ComponentSwizzle.Identity
                },
                SubresourceRange = new ImageSubresourceRange
                {
                    AspectMask = ImageAspectFlags.ColorBit,
                    BaseMipLevel = 0,
                    LevelCount = 1,
                    BaseArrayLayer = 0,
                    LayerCount = 1
                }
            };

            if (Vk!.CreateImageView(LogicalDevice, in createInfo, null, out swapChainImageViews[i]) != Result.Success)
                throw new VulkanException("Failed to create image views.");
        }
    }

    private unsafe void CreateRenderPass()
    {
        AttachmentDescription colorAttachment = new()
        {
            Format = swapChainImageFormat,
            Samples = SampleCountFlags.Count1Bit,
            LoadOp = AttachmentLoadOp.Clear,
            StoreOp = AttachmentStoreOp.Store,
            StencilLoadOp = AttachmentLoadOp.DontCare,
            InitialLayout = ImageLayout.Undefined,
            FinalLayout = ImageLayout.PresentSrcKhr
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

        if (Vk!.CreateRenderPass(LogicalDevice, in renderPassInfo, null, out renderPass) != Result.Success)
            throw new VulkanException("Failed to create render pass.");
    }

    private unsafe void CreateSwapChain()
    {
        var swapChainSupport = QuerySwapChainSupport(PhysicalDevice);

        var surfaceFormat = ChooseSwapSurfaceFormat(swapChainSupport.Formats);
        var presentMode = ChoosePresentMode(swapChainSupport.PresentModes);
        var extent = ChooseSwapExtent(swapChainSupport.Capabilities);

        var imageCount = swapChainSupport.Capabilities.MinImageCount + 1;
        if (swapChainSupport.Capabilities.MaxImageCount > 0 && imageCount > swapChainSupport.Capabilities.MaxImageCount)
            imageCount = swapChainSupport.Capabilities.MaxImageCount;

        SwapchainCreateInfoKHR creatInfo = new()
        {
            SType = StructureType.SwapchainCreateInfoKhr,
            Surface = surface,

            MinImageCount = imageCount,
            ImageFormat = surfaceFormat.Format,
            ImageColorSpace = surfaceFormat.ColorSpace,
            ImageExtent = extent,
            ImageArrayLayers = 1,
            ImageUsage = ImageUsageFlags.ColorAttachmentBit
        };

        var indices = FindQueueFamilies(PhysicalDevice);
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

        if (!Vk!.TryGetDeviceExtension(instance, logicalDevice, out khrSwapchain))
            throw new NotSupportedException("VK_KHR_swapchain extension not found.");

        if (khrSwapchain!.CreateSwapchain(logicalDevice, creatInfo, null, out swapChain) != Result.Success)
            throw new VulkanException("Failed to create swap chain.");

        khrSwapchain.GetSwapchainImages(logicalDevice, swapChain, ref imageCount, null);
        swapChainImages = new Image[imageCount];
        fixed (Image* swapChainImagesPtr = swapChainImages)
        {
            khrSwapchain.GetSwapchainImages(logicalDevice, swapChain, ref imageCount, swapChainImagesPtr);
        }

        swapChainImageFormat = surfaceFormat.Format;
        swapChainExtent = extent;
    }

    private SurfaceFormatKHR ChooseSwapSurfaceFormat(IReadOnlyList<SurfaceFormatKHR> availableFormats)
    {
        foreach (var availableFormat in availableFormats)
            if (availableFormat.Format == Format.B8G8R8A8Srgb &&
                availableFormat.ColorSpace == ColorSpaceKHR.SpaceSrgbNonlinearKhr)
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

    private unsafe SwapChainSupportDetails QuerySwapChainSupport(PhysicalDevice physicalDevice)
    {
        var details = new SwapChainSupportDetails();

        khrSurface!.GetPhysicalDeviceSurfaceCapabilities(physicalDevice, surface, out var capabilities);
        details.Capabilities = capabilities;

        uint formatCount = 0;
        khrSurface.GetPhysicalDeviceSurfaceFormats(physicalDevice, surface, ref formatCount, null);

        if (formatCount != 0)
        {
            details.Formats = new SurfaceFormatKHR[formatCount];
            fixed (SurfaceFormatKHR* formatsPtr = details.Formats)
            {
                khrSurface.GetPhysicalDeviceSurfaceFormats(physicalDevice, surface, ref formatCount, formatsPtr);
            }
        }
        else
        {
            details.Formats = Array.Empty<SurfaceFormatKHR>();
        }

        uint presentModeCount = 0;
        khrSurface.GetPhysicalDeviceSurfacePresentModes(physicalDevice, surface, ref presentModeCount, null);

        if (presentModeCount != 0)
        {
            details.PresentModes = new PresentModeKHR[presentModeCount];
            fixed (PresentModeKHR* formatsPtr = details.PresentModes)
            {
                khrSurface.GetPhysicalDeviceSurfacePresentModes(physicalDevice, surface, ref presentModeCount,
                    formatsPtr);
            }
        }
        else
        {
            details.PresentModes = Array.Empty<PresentModeKHR>();
        }

        return details;
    }

    private unsafe void CreateSurface(IVkSurface vkSurface)
    {
        if (!Vk!.TryGetInstanceExtension<KhrSurface>(instance, out khrSurface))
            throw new NotSupportedException("KHR_surface extension not found.");

        surface = vkSurface!.Create<AllocationCallbacks>(instance.ToHandle(), null).ToSurface();
    }

    private unsafe void SetupInstance(IVkSurface surface)
    {
        Vk = Vk.GetApi();

        ThrowIfValidationLayersNotSupported();

        ApplicationInfo appInfo = new()
        {
            SType = StructureType.ApplicationInfo,
            PApplicationName = (byte*)Marshal.StringToHGlobalAnsi("Drawie"),
            ApplicationVersion = new Version32(1, 0, 0),
            PEngineName = (byte*)Marshal.StringToHGlobalAnsi("Drawie Engine"),
            EngineVersion = new Version32(1, 0, 0),
            ApiVersion = Vk.Version12
        };

        InstanceCreateInfo createInfo = new()
        {
            SType = StructureType.InstanceCreateInfo,
            PApplicationInfo = &appInfo
        };

        var extensions = GetExtensions(surface);

        createInfo.EnabledExtensionCount = (uint)extensions.Length;
        createInfo.PpEnabledExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(extensions);

        if (EnableValidationLayers)
        {
            createInfo.EnabledLayerCount = (uint)validationLayers.Length;
            createInfo.PpEnabledLayerNames = (byte**)SilkMarshal.StringArrayToPtr(validationLayers);

            DebugUtilsMessengerCreateInfoEXT debugCreateInfo = new();
            PopulateDebugMessengerCreateInfo(ref debugCreateInfo);
            createInfo.PNext = &debugCreateInfo;
        }
        else
        {
            createInfo.EnabledLayerCount = 0;
            createInfo.PNext = null;
        }

        if (Vk.CreateInstance(&createInfo, null, out instance) != Result.Success)
            throw new VulkanException("Failed to create instance.");

        Marshal.FreeHGlobal((nint)appInfo.PApplicationName);
        Marshal.FreeHGlobal((nint)appInfo.PEngineName);
        SilkMarshal.Free((nint)createInfo.PpEnabledExtensionNames);

        if (EnableValidationLayers) SilkMarshal.Free((nint)createInfo.PpEnabledLayerNames);
    }

    private unsafe GpuInfo PickPhysicalDevice()
    {
        var devices = Vk!.GetPhysicalDevices(Instance);
        foreach (var device in devices)
        {
            if (IsDeviceSuitable(device))
            {
                var props = Vk.GetPhysicalDeviceProperties(device);
                var name = props.DeviceName;
                var deviceName = Marshal.PtrToStringAnsi((nint)name);

                if (deviceName == null)
                {
                    throw new VulkanException("Failed to get device name.");
                }
                
                GpuInfo gpuInfo = new(deviceName);
                PhysicalDevice = device;
                return gpuInfo;
            }
        }

        if (PhysicalDevice.Handle == 0) throw new VulkanException("Failed to find a suitable Vulkan GPU.");
        
        return new GpuInfo("Unknown");
    }

    private unsafe void CreateLogicalDevice()
    {
        var indices = FindQueueFamilies(PhysicalDevice);

        var uniqueQueueFamilies = new[] { indices.GraphicsFamily!.Value, indices.PresentFamily!.Value };
        uniqueQueueFamilies = uniqueQueueFamilies.Distinct().ToArray();

        using var mem = GlobalMemory.Allocate(uniqueQueueFamilies.Length * sizeof(DeviceQueueCreateInfo));
        var queueCreateInfos = (DeviceQueueCreateInfo*)Unsafe.AsPointer(ref mem.GetPinnableReference());

        var queuePriority = 1.0f;
        for (var i = 0; i < uniqueQueueFamilies.Length; i++)
            queueCreateInfos[i] = new DeviceQueueCreateInfo
            {
                SType = StructureType.DeviceQueueCreateInfo,
                QueueFamilyIndex = uniqueQueueFamilies[i],
                QueueCount = 1,
                PQueuePriorities = &queuePriority
            };

        PhysicalDeviceFeatures deviceFeatures = new();

        DeviceCreateInfo createInfo = new()
        {
            SType = StructureType.DeviceCreateInfo,
            QueueCreateInfoCount = (uint)uniqueQueueFamilies.Length,
            PQueueCreateInfos = queueCreateInfos,

            PEnabledFeatures = &deviceFeatures,

            EnabledExtensionCount = (uint)deviceExtensions.Length,
            PpEnabledExtensionNames = (byte**)SilkMarshal.StringArrayToPtr(deviceExtensions)
        };

        if (EnableValidationLayers)
        {
            createInfo.EnabledLayerCount = (uint)validationLayers.Length;
            createInfo.PpEnabledLayerNames = (byte**)SilkMarshal.StringArrayToPtr(validationLayers);
        }
        else
        {
            createInfo.EnabledLayerCount = 0;
        }

        if (Vk!.CreateDevice(PhysicalDevice, in createInfo, null, out logicalDevice) != Result.Success)
            throw new VulkanException("Failed to create logical device.");

        Vk!.GetDeviceQueue(logicalDevice, indices.GraphicsFamily!.Value, 0, out graphicsQueue);
        Vk!.GetDeviceQueue(logicalDevice, indices.PresentFamily!.Value, 0, out presentQueue);

        if (EnableValidationLayers) SilkMarshal.Free((nint)createInfo.PpEnabledLayerNames);
    }

    private bool IsDeviceSuitable(PhysicalDevice device)
    {
        var indices = FindQueueFamilies(device);

        var extensionsSupported = CheckDeviceExtensionSupport(device);

        var swapChainAdequate = false;
        if (extensionsSupported)
        {
            var swapChainSupport = QuerySwapChainSupport(device);
            swapChainAdequate = swapChainSupport.Formats.Any() && swapChainSupport.PresentModes.Any();
        }

        return indices.IsComplete && extensionsSupported && swapChainAdequate;
    }

    private unsafe bool CheckDeviceExtensionSupport(PhysicalDevice device)
    {
        uint extensionCount = 0;
        Vk!.EnumerateDeviceExtensionProperties(device, (byte*)null, ref extensionCount, null);
        var availableExtensions = new ExtensionProperties[extensionCount];
        fixed (ExtensionProperties* availableExtensionsPtr = availableExtensions)
        {
            Vk!.EnumerateDeviceExtensionProperties(device, (byte*)null, ref extensionCount, availableExtensionsPtr);
        }

        var availableExtensionNames = availableExtensions
            .Select(extension => Marshal.PtrToStringAnsi((nint)extension.ExtensionName)).ToHashSet();

        return deviceExtensions.All(availableExtensionNames.Contains);
    }

    private unsafe QueueFamilyIndices FindQueueFamilies(PhysicalDevice device)
    {
        var indices = new QueueFamilyIndices();

        uint queueFamilityCount = 0;
        Vk!.GetPhysicalDeviceQueueFamilyProperties(device, ref queueFamilityCount, null);

        var queueFamilies = new QueueFamilyProperties[queueFamilityCount];
        fixed (QueueFamilyProperties* queueFamiliesPtr = queueFamilies)
        {
            Vk!.GetPhysicalDeviceQueueFamilyProperties(device, ref queueFamilityCount, queueFamiliesPtr);
        }

        uint i = 0;
        foreach (var queueFamily in queueFamilies)
        {
            if (queueFamily.QueueFlags.HasFlag(QueueFlags.GraphicsBit)) indices.GraphicsFamily = i;

            khrSurface!.GetPhysicalDeviceSurfaceSupport(device, i, surface, out var presentSupport);

            if (presentSupport) indices.PresentFamily = i;

            if (indices.IsComplete) break;

            i++;
        }

        return indices;
    }

    private unsafe string[] GetExtensions(IVkSurface surface)
    {
        var windowExtensions = surface.GetRequiredExtensions(out var count);
        var extensions = SilkMarshal.PtrToStringArray((nint)windowExtensions, (int)count);

        if (EnableValidationLayers) return extensions.Append(ExtDebugUtils.ExtensionName).ToArray();

        return extensions;
    }

    private unsafe void PopulateDebugMessengerCreateInfo(ref DebugUtilsMessengerCreateInfoEXT createInfo)
    {
        createInfo = new DebugUtilsMessengerCreateInfoEXT
        {
            SType = StructureType.DebugUtilsMessengerCreateInfoExt,
            MessageSeverity = DebugUtilsMessageSeverityFlagsEXT.VerboseBitExt |
                              DebugUtilsMessageSeverityFlagsEXT.WarningBitExt |
                              DebugUtilsMessageSeverityFlagsEXT.ErrorBitExt,
            MessageType = DebugUtilsMessageTypeFlagsEXT.GeneralBitExt |
                          DebugUtilsMessageTypeFlagsEXT.PerformanceBitExt |
                          DebugUtilsMessageTypeFlagsEXT.ValidationBitExt,
            PfnUserCallback = (DebugUtilsMessengerCallbackFunctionEXT)DebugCallback
        };
    }

    private unsafe void SetupDebugMessenger()
    {
        if (!EnableValidationLayers) return;

        if (!Vk!.TryGetInstanceExtension(Instance, out extDebugUtils)) return;

        DebugUtilsMessengerCreateInfoEXT createInfo = new();
        PopulateDebugMessengerCreateInfo(ref createInfo);

        if (extDebugUtils!.CreateDebugUtilsMessenger(Instance, in createInfo, null, out debugMessenger) !=
            Result.Success)
            throw new Exception("failed to set up debug messenger!");
    }

    private void ThrowIfValidationLayersNotSupported()
    {
        if (EnableValidationLayers && !CheckValidationLayerSupport())
            throw new VulkanException("validation layers requested, but not available!");
    }

    private unsafe bool CheckValidationLayerSupport()
    {
        uint layerCount = 0;
        Vk!.EnumerateInstanceLayerProperties(ref layerCount, null);
        var availableLayers = new LayerProperties[layerCount];
        fixed (LayerProperties* availableLayersPtr = availableLayers)
        {
            Vk!.EnumerateInstanceLayerProperties(ref layerCount, availableLayersPtr);
        }

        var availableLayerNames = availableLayers.Select(layer => Marshal.PtrToStringAnsi((IntPtr)layer.LayerName))
            .ToHashSet();

        return validationLayers.All(availableLayerNames.Contains);
    }

    private unsafe uint DebugCallback(DebugUtilsMessageSeverityFlagsEXT messageSeverity,
        DebugUtilsMessageTypeFlagsEXT messageTypes, DebugUtilsMessengerCallbackDataEXT* pCallbackData, void* pUserData)
    {
        Console.WriteLine($"validation layer:" + Marshal.PtrToStringAnsi((nint)pCallbackData->PMessage));

        return Vk.False;
    }
}