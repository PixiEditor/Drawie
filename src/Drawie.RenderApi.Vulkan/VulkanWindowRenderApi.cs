using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Drawie.RenderApi.Vulkan.Buffers;
using Drawie.RenderApi.Vulkan.Exceptions;
using Drawie.RenderApi.Vulkan.Helpers;
using Drawie.RenderApi.Vulkan.Stages;
using Drawie.RenderApi.Vulkan.Stages.Builders;
using Drawie.RenderApi.Vulkan.Structs;
using PixiEditor.Numerics;
using Silk.NET.Core;
using Silk.NET.Core.Contexts;
using Silk.NET.Core.Native;
using Silk.NET.Maths;
using Silk.NET.Vulkan;
using Silk.NET.Vulkan.Extensions.EXT;
using Silk.NET.Vulkan.Extensions.KHR;
using Buffer = Silk.NET.Vulkan.Buffer;
using Semaphore = Silk.NET.Vulkan.Semaphore;

namespace Drawie.RenderApi.Vulkan;

public class VulkanWindowRenderApi : IVulkanWindowRenderApi
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

    public Queue graphicsQueue;
    public Queue presentQueue;

    private KhrSwapchain? khrSwapchain;
    private SwapchainKHR swapChain;
    private Image[] swapChainImages;
    private Format swapChainImageFormat;
    private Extent2D swapChainExtent;
    private ImageView[] swapChainImageViews;
    private Framebuffer[] swapChainFramebuffers;

    private VecI framebufferSize;
    private VecI lastFramebufferSize;

    private DescriptorSetLayout descriptorSetLayout;
    private GraphicsPipeline graphicsPipeline;

    private CommandPool commandPool;

    private VertexBuffer vertexBuffer;

    private IndexBuffer indexBuffer;

    public VulkanTexture texture;

    private DescriptorPool descriptorPool;
    private DescriptorSet[] descriptorSets;

    private CommandBuffer[]? commandBuffers;

    private Semaphore[]? imageAvailableSemaphores;
    private Semaphore[]? renderFinishedSemaphores;
    private Fence[]? inFlightFences;
    private Fence[]? imagesInFlight;
    private int currentFrame = 0;

    private Vertex[] vertices = new Vertex[]
    {
        new()
        {
            Position = new Vector2D<float>(-1f, -1f), Color = new Vector3D<float>(1.0f, 0.0f, 0.0f),
            TexCoord = new Vector2D<float>(0.0f, 0.0f)
        },
        new()
        {
            Position = new Vector2D<float>(1f, -1f), Color = new Vector3D<float>(0.0f, 1.0f, 0.0f),
            TexCoord = new Vector2D<float>(1.0f, 0.0f)
        },
        new()
        {
            Position = new Vector2D<float>(1f, 1f), Color = new Vector3D<float>(0.0f, 0.0f, 1.0f),
            TexCoord = new Vector2D<float>(1.0f, 1.0f)
        },
        new()
        {
            Position = new Vector2D<float>(-1f, 1f), Color = new Vector3D<float>(1.0f, 1.0f, 1.0f),
            TexCoord = new Vector2D<float>(0.0f, 1.0f)
        }
    };

    private ushort[] indices = new ushort[]
    {
        0, 1, 2, 2, 3, 0
    };

    public GraphicsApi GraphicsApi => GraphicsApi.Vulkan;

    public VulkanWindowRenderApi()
    {
        
    }
    
    public VulkanWindowRenderApi(Instance instance, Device logicalDevice, PhysicalDevice physicalDevice, Queue graphicsQueue, Queue presentQueue)
    {
        this.instance = instance;
        this.logicalDevice = logicalDevice;
        PhysicalDevice = physicalDevice;
        this.graphicsQueue = graphicsQueue;
        this.presentQueue = presentQueue;
    }

    public void UpdateFramebufferSize(int width, int height)
    {
        framebufferSize = new VecI(width, height);
    }

    public void PrepareTextureToWrite()
    {
        texture.TransitionLayoutTo(VulkanTexture.ShaderReadOnlyOptimal, VulkanTexture.ColorAttachmentOptimal);
    }

    public void CreateInstance(object surfaceObject, VecI framebufferSize)
    {
        if (surfaceObject is not IVkSurface vkSurface) throw new VulkanNotSupportedException();

        this.framebufferSize = framebufferSize;
        
        Vk = Vk.GetApi();

        if (instance.Handle == default)
        {
            SetupInstance(vkSurface);
            SetupDebugMessenger();
        }

        CreateSurface(vkSurface);
        if (logicalDevice.Handle == default)
        {
            var selectedGpu = PickPhysicalDevice();

            Console.WriteLine($"Selected GPU: {selectedGpu.Name}");

            CreateLogicalDevice();
        }

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
        Vk!.DeviceWaitIdle(LogicalDevice);

        CleanupSwapchain();

        texture.Dispose();

        Vk!.DestroyDescriptorSetLayout(LogicalDevice, descriptorSetLayout, null);

        indexBuffer.Dispose();
        vertexBuffer.Dispose();

        for (var i = 0; i < MAX_FRAMES_IN_FLIGHT; i++)
        {
            Vk!.DestroySemaphore(logicalDevice, renderFinishedSemaphores![i], null);
            Vk!.DestroySemaphore(logicalDevice, imageAvailableSemaphores![i], null);
            Vk!.DestroyFence(logicalDevice, inFlightFences![i], null);
        }

        Vk!.DestroyCommandPool(LogicalDevice, commandPool, null);

        foreach (var framebuffer in swapChainFramebuffers) Vk!.DestroyFramebuffer(LogicalDevice, framebuffer, null);

        graphicsPipeline.Dispose();

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
        foreach (var framebuffer in swapChainFramebuffers) Vk!.DestroyFramebuffer(LogicalDevice, framebuffer, null);

        fixed (CommandBuffer* commandBuffersPtr = commandBuffers)
        {
            Vk!.FreeCommandBuffers(LogicalDevice, commandPool, (uint)commandBuffers!.Length, commandBuffersPtr);
        }

        graphicsPipeline.Dispose();

        foreach (var imageView in swapChainImageViews) Vk!.DestroyImageView(LogicalDevice, imageView, null);

        khrSwapchain!.DestroySwapchain(LogicalDevice, swapChain, null);

        Vk!.DestroyDescriptorPool(LogicalDevice, descriptorPool, null);
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

            if (Vk!.CreateDescriptorSetLayout(LogicalDevice, layoutInfo, null, descriptorSetLayoutPtr) !=
                Result.Success)
                throw new VulkanException("Failed to create descriptor set layout.");
        }
    }

    public void CreateTextureImage()
    {
        texture = new VulkanTexture(Vk!, LogicalDevice, PhysicalDevice, commandPool, graphicsQueue, framebufferSize);
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
            if (Vk!.CreateDescriptorPool(LogicalDevice, poolInfo, null, descriptorPoolPtr) != Result.Success)
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
                if (Vk!.AllocateDescriptorSets(LogicalDevice, allocInfo, descriptorSetsPtr) != Result.Success)
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
            
            Vk!.UpdateDescriptorSets(LogicalDevice, 1, &samplerDescriptorSet, 0, null);
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

        Vk!.DeviceWaitIdle(LogicalDevice);

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
        else if (acquireResult != Result.Success && acquireResult != Result.SuboptimalKhr)
        {
            throw new VulkanException("Failed to acquire swap chain image.");
        }

        UpdateTextureLayout();

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

        if (result == Result.ErrorOutOfDateKhr || result == Result.SuboptimalKhr ||
            lastFramebufferSize != framebufferSize)
            RecreateSwapchain();
        else if (result != Result.Success) throw new VulkanException("Failed to present swap chain image.");

        currentFrame = (currentFrame + 1) % MAX_FRAMES_IN_FLIGHT;
    }

    public event Action? FramebufferResized;

    private void UpdateTextureLayout()
    {
        texture.TransitionLayoutTo(VulkanTexture.ColorAttachmentOptimal, VulkanTexture.ShaderReadOnlyOptimal);
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

            Vk!.CmdBeginRenderPass(commandBuffers[i], &renderPassInfo, SubpassContents.Inline);
            Vk!.CmdBindPipeline(commandBuffers[i], PipelineBindPoint.Graphics, graphicsPipeline.VkPipeline);

            var vertexBuffers = new[] { vertexBuffer.VkBuffer };
            var offsets = new ulong[] { 0 };

            fixed (ulong* offsetsPtr = offsets)
            fixed (Buffer* vertexBuffersPtr = vertexBuffers)
            {
                Vk!.CmdBindVertexBuffers(commandBuffers[i], 0, 1, vertexBuffersPtr, offsetsPtr);
            }

            Vk!.CmdBindIndexBuffer(commandBuffers[i], indexBuffer.VkBuffer, 0, IndexType.Uint16);

            Vk!.CmdBindDescriptorSets(commandBuffers[i], PipelineBindPoint.Graphics, graphicsPipeline.VkPipelineLayout,
                0, 1, descriptorSets[i], 0, null);

            Vk!.CmdDrawIndexed(commandBuffers[i], (uint)indices.Length, 1, 0, 0, 0);

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
                RenderPass = graphicsPipeline.VkRenderPass,
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

    private void CreateGraphicsPipeline()
    {
        GraphicsPipelineBuilder builder = new(Vk!, LogicalDevice);

        builder
            .AddStage(stage => stage.OfType(GraphicsPipelineStageType.Vertex).WithShader("shaders/vert.spv"))
            .AddStage(stage => stage.OfType(GraphicsPipelineStageType.Fragment).WithShader("shaders/frag.spv"))
            .WithRenderPass(renderPass =>
            {
                /*TODO: Add some meaningful stuff*/
            });

        graphicsPipeline = builder.Create(swapChainExtent, swapChainImageFormat, ref descriptorSetLayout);
    }

    private unsafe void CreateImageViews()
    {
        swapChainImageViews = new ImageView[swapChainImages.Length];

        for (var i = 0; i < swapChainImages.Length; i++)
            swapChainImageViews[i] = ImageUtility.CreateViewForImage(Vk!, LogicalDevice, swapChainImages[i],
                swapChainImageFormat);
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
            if (availableFormat.Format == Format.R8G8B8A8Srgb &&
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

    private unsafe void CreateIndexBuffer()
    {
        var bufferSize = (ulong)Unsafe.SizeOf<ushort>() * (ulong)indices.Length;

        using StagingBuffer stagingBuffer = new(Vk!, LogicalDevice, PhysicalDevice, bufferSize);

        stagingBuffer.SetData(indices);

        indexBuffer = new IndexBuffer(Vk!, LogicalDevice, PhysicalDevice, bufferSize);

        CopyBuffer(stagingBuffer, indexBuffer, bufferSize);
    }

    private void CreateVertexBuffer()
    {
        var bufferSize = (ulong)Marshal.SizeOf<Vertex>() * (ulong)vertices.Length;

        using StagingBuffer stagingBuffer = new(Vk!, LogicalDevice, PhysicalDevice, bufferSize);

        stagingBuffer.SetData(vertices);

        vertexBuffer = new VertexBuffer(Vk!, LogicalDevice, PhysicalDevice, bufferSize);
        CopyBuffer(stagingBuffer, vertexBuffer, bufferSize);
    }

    private void CopyBuffer(BufferObject srcBuffer, BufferObject dstBuffer, ulong size)
    {
        using var commandBuffer = new SingleTimeCommandBufferSession(Vk, commandPool, LogicalDevice, graphicsQueue);

        BufferCopy copyRegion = new()
        {
            Size = size
        };

        Vk!.CmdCopyBuffer(commandBuffer.CommandBuffer, srcBuffer.VkBuffer, dstBuffer.VkBuffer, 1, copyRegion);
    }

    private unsafe void SetupInstance(IVkSurface surface)
    {
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
            if (IsDeviceSuitable(device))
            {
                var props = Vk.GetPhysicalDeviceProperties(device);
                var name = props.DeviceName;
                var deviceName = Marshal.PtrToStringAnsi((nint)name);

                if (deviceName == null) throw new VulkanException("Failed to get device name.");

                GpuInfo gpuInfo = new(deviceName);
                PhysicalDevice = device;
                return gpuInfo;
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

        PhysicalDeviceFeatures deviceFeatures = new()
        {
            SamplerAnisotropy = false
        };

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

        var features = Vk!.GetPhysicalDeviceFeatures(device);

        return indices.IsComplete && extensionsSupported && swapChainAdequate && features.SamplerAnisotropy;
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

    public IntPtr LogicalDeviceHandle => logicalDevice.Handle; 
    public IntPtr PhysicalDeviceHandle => PhysicalDevice.Handle;
    public IntPtr InstanceHandle => Instance.Handle;
    public IntPtr GraphicsQueueHandle => graphicsQueue.Handle;
    public IVkTexture RenderTexture => texture;

    public IntPtr GetProcedureAddress(string name, IntPtr instance, IntPtr device)
    {
        return Vk!.GetInstanceProcAddr(Instance, name);
    }
}