using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using Drawie.RenderApi.WebGpu.Extensions;
using Evergine.Bindings.WebGPU;
using PixiEditor.Numerics;
using Silk.NET.Core.Contexts;
using static Evergine.Bindings.WebGPU.WebGPUNative;

namespace Drawie.RenderApi.WebGpu;

/*
 * TODO:
 * [] - Add texture support
 * [] - Draw rectangle with texture
 * [] - Convert rectangle into big clipped triangle
 */
public class WebGpuWindowRenderApi : IWindowRenderApi
{
    public event Action? FramebufferResized;

    private WGPUInstance Instance;
    private WGPUSurface Surface;
    private WGPUAdapter Adapter;
    private WGPUAdapterProperties AdapterProperties;
    private WGPUSupportedLimits AdapterLimits;
    private WGPUDevice Device;
    private WGPUTextureFormat SwapChainFormat;
    private WGPUQueue Queue;

    private TextureBuffer texture;
    private WGPUSampler sampler;
    private WGPUBindGroup textureBindGroup;

    private WGPUPipelineLayout pipelineLayout;
    private WGPURenderPipeline pipeline;
    private WGPUBuffer vertexBuffer;

    private VecI framebufferSize;

    public void CreateInstance(object surface, VecI size)
    {
        if (surface is not INativeWindow nativeWindow)
        {
            throw new ArgumentException("Surface must be a window handle");
        }

        framebufferSize = size;

        InitWebGpu(size, nativeWindow);
        InitResources();
    }

    public unsafe void Render(double deltaTime)
    {
        WGPUSurfaceTexture surfaceTexture = default;
        wgpuSurfaceGetCurrentTexture(Surface, &surfaceTexture);

        if (surfaceTexture.status == WGPUSurfaceGetCurrentTextureStatus.Outdated || surfaceTexture.suboptimal)
        {
            ReconfigureSwapchain();
            return;
        }

        WGPUTextureView nextView = wgpuTextureCreateView(surfaceTexture.texture, null);

        WGPUCommandEncoderDescriptor encoderDescriptor = new WGPUCommandEncoderDescriptor()
        {
            nextInChain = null,
        };
        WGPUCommandEncoder encoder = wgpuDeviceCreateCommandEncoder(Device, &encoderDescriptor);

        WGPURenderPassColorAttachment renderPassColorAttachment = new WGPURenderPassColorAttachment()
        {
            view = nextView,
            resolveTarget = WGPUTextureView.Null,
            loadOp = WGPULoadOp.Clear,
            storeOp = WGPUStoreOp.Store,
            clearValue = new WGPUColor() { a = 1.0f },
        };

        WGPURenderPassDescriptor renderPassDescriptor = new WGPURenderPassDescriptor()
        {
            nextInChain = null,
            colorAttachmentCount = 1,
            colorAttachments = &renderPassColorAttachment,
            depthStencilAttachment = null,
            timestampWrites = null,
        };

        WGPURenderPassEncoder renderPass = wgpuCommandEncoderBeginRenderPass(encoder, &renderPassDescriptor);

        wgpuRenderPassEncoderSetPipeline(renderPass, pipeline);
        wgpuRenderPassEncoderSetVertexBuffer(renderPass, 0, vertexBuffer, 0, WGPU_WHOLE_MAP_SIZE);
        wgpuRenderPassEncoderSetBindGroup(renderPass, 0, textureBindGroup, 0, null);
        wgpuRenderPassEncoderDraw(renderPass, 6, 1, 0, 0);
        wgpuRenderPassEncoderEnd(renderPass);

        wgpuTextureViewRelease(nextView);

        WGPUCommandBufferDescriptor commandBufferDescriptor = new WGPUCommandBufferDescriptor()
        {
            nextInChain = null,
        };

        WGPUCommandBuffer command = wgpuCommandEncoderFinish(encoder, &commandBufferDescriptor);
        wgpuQueueSubmit(Queue, 1, &command);

        wgpuCommandEncoderRelease(encoder);

        wgpuSurfacePresent(Surface);
    }

    private void ReconfigureSwapchain()
    {
        ConfigureSwapchain(framebufferSize);
        FramebufferResized?.Invoke();
    }

    public void DestroyInstance()
    {
        wgpuBindGroupRelease(textureBindGroup);
        wgpuSamplerRelease(sampler);
        texture.Dispose();
        wgpuSurfaceRelease(Surface);
        wgpuDeviceDestroy(Device);
        wgpuDeviceRelease(Device);
        wgpuAdapterRelease(Adapter);
        wgpuInstanceRelease(Instance);
    }

    public void UpdateFramebufferSize(int width, int height)
    {
        framebufferSize = new VecI(width, height);
    }

    public void PrepareTextureToWrite()
    {
        // Not needed
    }

    private void InitWebGpu(VecI size, INativeWindow nativeWindow)
    {
        CreateInstance();
        CreateSurface(nativeWindow);
        CreateDevice();
        ConfigureSwapchain(size);
    }

    private unsafe void ConfigureSwapchain(VecI size)
    {
        SwapChainFormat = wgpuSurfaceGetPreferredFormat(Surface, Adapter);

        WGPUSurfaceConfiguration surfaceConfiguration = new WGPUSurfaceConfiguration()
        {
            nextInChain = null,
            device = Device,
            format = SwapChainFormat,
            usage = WGPUTextureUsage.RenderAttachment,
            width = (uint)size.X,
            height = (uint)size.Y,
            presentMode = WGPUPresentMode.Fifo,
        };

        wgpuSurfaceConfigure(Surface, &surfaceConfiguration);
    }

    private unsafe void CreateDevice()
    {
        WGPURequestAdapterOptions options = new WGPURequestAdapterOptions()
        {
            nextInChain = null,
            compatibleSurface = Surface,
            powerPreference = WGPUPowerPreference.HighPerformance
        };

        wgpuInstanceRequestAdapter(Instance, &options, OnAdapterRequestEnded, (void*)0);

        WGPUDeviceDescriptor deviceDescriptor = new WGPUDeviceDescriptor()
        {
            nextInChain = null,
            label = null,
            requiredFeatures = (WGPUFeatureName*)0,
            requiredLimits = null,
        };

        wgpuAdapterRequestDevice(Adapter, &deviceDescriptor, OnDeviceRequestEnded, (void*)0);

        wgpuDeviceSetUncapturedErrorCallback(Device, HandleUncapturedErrorCallback, (void*)0);

        Queue = wgpuDeviceGetQueue(Device);
    }

    private unsafe void CreateSurface(INativeWindow nativeWindow)
    {
        var window = nativeWindow.Win32;

        WGPUSurfaceDescriptorFromWindowsHWND windowsSurface = new WGPUSurfaceDescriptorFromWindowsHWND()
        {
            chain = new WGPUChainedStruct()
            {
                sType = WGPUSType.SurfaceDescriptorFromWindowsHWND,
            },
            hinstance = window.Value.HInstance.ToPointer(),
            hwnd = window.Value.Hwnd.ToPointer(),
        };

        WGPUSurfaceDescriptor surfaceDescriptor = new WGPUSurfaceDescriptor()
        {
            nextInChain = &windowsSurface.chain,
        };

        Surface = wgpuInstanceCreateSurface(Instance, &surfaceDescriptor);
    }

    private unsafe void CreateInstance()
    {
        WGPUInstanceExtras instanceExtras = new WGPUInstanceExtras()
        {
            chain = new WGPUChainedStruct()
            {
                sType = (WGPUSType)WGPUNativeSType.InstanceExtras,
            },
            backends = WGPUInstanceBackend.Vulkan,
        };

        WGPUInstanceDescriptor instanceDescriptor = new WGPUInstanceDescriptor()
        {
            nextInChain = &instanceExtras.chain,
        };
        Instance = wgpuCreateInstance(&instanceDescriptor);
    }

    private unsafe void InitResources()
    {
        WGPUSamplerBindingLayout samplerBindingLayout = new WGPUSamplerBindingLayout()
        {
            type = WGPUSamplerBindingType.Filtering
        };
        
        WGPUTextureBindingLayout textureBindingLayout = new WGPUTextureBindingLayout()
        {
            sampleType = WGPUTextureSampleType.Float,
            viewDimension = WGPUTextureViewDimension._2D,
            multisampled = false,
        };
        
        WGPUBindGroupLayoutEntry* bindGroupLayoutEntries = stackalloc WGPUBindGroupLayoutEntry[2]
        {
            new WGPUBindGroupLayoutEntry()
            {
                visibility = WGPUShaderStage.Fragment,
                binding = 0,
                sampler = samplerBindingLayout
            },
            new WGPUBindGroupLayoutEntry()
            {
                visibility = WGPUShaderStage.Fragment,
                binding = 1,
                texture = textureBindingLayout
            }
        };
        var bindGroupLayoutDescriptor = new WGPUBindGroupLayoutDescriptor()
        {
            entryCount = 2,
            entries = bindGroupLayoutEntries,
        };
        
        var bindGroupLayout = wgpuDeviceCreateBindGroupLayout(Device, &bindGroupLayoutDescriptor); 

        WGPUPipelineLayoutDescriptor layoutDescription = new()
        {
            nextInChain = null,
            bindGroupLayoutCount = 1,
            bindGroupLayouts = &bindGroupLayout,
        };

        pipelineLayout = wgpuDeviceCreatePipelineLayout(Device, &layoutDescription);

        string shaderSource = File.ReadAllText(Path.Combine("Shaders", "wgpu_shader.wgsl"));

        WGPUShaderModuleWGSLDescriptor shaderCodeDescriptor = new()
        {
            chain = new WGPUChainedStruct()
            {
                next = null,
                sType = WGPUSType.ShaderModuleWGSLDescriptor,
            },
            code = shaderSource.ToPointer(),
        };

        WGPUShaderModuleDescriptor moduleDescriptor = new()
        {
            nextInChain = &shaderCodeDescriptor.chain,
            hintCount = 0,
            hints = null,
        };

        WGPUShaderModule shaderModule = wgpuDeviceCreateShaderModule(Device, &moduleDescriptor);

        WGPUVertexAttribute* vertexAttributes = stackalloc WGPUVertexAttribute[2]
        {
            new WGPUVertexAttribute()
            {
                format = WGPUVertexFormat.Float32x2,
                offset = 0,
                shaderLocation = 0,
            },
            new WGPUVertexAttribute()
            {
                format = WGPUVertexFormat.Float32x2,
                offset = 8,
                shaderLocation = 1,
            },
        };

        WGPUVertexBufferLayout vertexLayout = new WGPUVertexBufferLayout()
        {
            attributeCount = 2,
            attributes = vertexAttributes,
            arrayStride = (ulong)sizeof(Vector2) * 2,
            stepMode = WGPUVertexStepMode.Vertex,
        };

        WGPUBlendState blendState = new WGPUBlendState()
        {
            color = new WGPUBlendComponent()
            {
                srcFactor = WGPUBlendFactor.One,
                dstFactor = WGPUBlendFactor.Zero,
                operation = WGPUBlendOperation.Add,
            },
            alpha = new WGPUBlendComponent()
            {
                srcFactor = WGPUBlendFactor.One,
                dstFactor = WGPUBlendFactor.Zero,
                operation = WGPUBlendOperation.Add,
            }
        };

        WGPUColorTargetState colorTargetState = new WGPUColorTargetState()
        {
            nextInChain = null,
            format = SwapChainFormat,
            blend = &blendState,
            writeMask = WGPUColorWriteMask.All,
        };

        WGPUFragmentState fragmentState = new WGPUFragmentState()
        {
            nextInChain = null,
            module = shaderModule,
            entryPoint = "fragmentMain".ToPointer(),
            constantCount = 0,
            constants = null,
            targetCount = 1,
            targets = &colorTargetState,
        };

        WGPURenderPipelineDescriptor pipelineDescriptor = new WGPURenderPipelineDescriptor()
        {
            layout = pipelineLayout,
            vertex = new WGPUVertexState()
            {
                bufferCount = 1,
                buffers = &vertexLayout,

                module = shaderModule,
                entryPoint = "vertexMain".ToPointer(),
                constantCount = 0,
                constants = null,
            },
            primitive = new WGPUPrimitiveState()
            {
                topology = WGPUPrimitiveTopology.TriangleList,
                stripIndexFormat = WGPUIndexFormat.Undefined,
                frontFace = WGPUFrontFace.CCW,
                cullMode = WGPUCullMode.None,
            },
            fragment = &fragmentState,
            depthStencil = null,
            multisample = new WGPUMultisampleState()
            {
                count = 1,
                mask = ~0u,
                alphaToCoverageEnabled = false,
            }
        };

        pipeline = wgpuDeviceCreateRenderPipeline(Device, &pipelineDescriptor);

        wgpuShaderModuleRelease(shaderModule);

        // triangle
        Vector2* vertexData = stackalloc Vector2[]
        {
            new Vector2(-1.0f, -1f), // bottom left pos
            new Vector2(0.0f, 1.0f), // texture coords bottom left (flipped from 0.0f to 1.0f)

            new Vector2(1.0f, -1.0f), // top right pos
            new Vector2(1.0f, 1.0f), // texture coords top right (flipped from 0.0f to 1.0f)

            new Vector2(-1.0f, 1.0f), // bottom left pos
            new Vector2(0.0f, 0.0f), // texture coords bottom left (flipped from 1.0f to 0.0f)

            new Vector2(-1.0f, 1.0f), // bottom left pos
            new Vector2(0.0f, 0.0f), // texture coords bottom left (flipped from 1.0f to 0.0f)

            new Vector2(1.0f, -1.0f), // top right pos
            new Vector2(1.0f, 1.0f), // texture coords top right (flipped from 0.0f to 1.0f)

            new Vector2(1.0f, 1.0f), // bottom right pos
            new Vector2(1.0f, 0.0f), // texture coords bottom right (flipped from 1.0f to 0.0f)

        };

        ulong size = (ulong)(12 * sizeof(Vector2));
        WGPUBufferDescriptor bufferDescriptor = new WGPUBufferDescriptor()
        {
            nextInChain = null,
            usage = WGPUBufferUsage.Vertex | WGPUBufferUsage.CopyDst,
            size = size,
            mappedAtCreation = false,
        };

        vertexBuffer = wgpuDeviceCreateBuffer(Device, &bufferDescriptor);
        wgpuQueueWriteBuffer(Queue, vertexBuffer, 0, vertexData, size);

        CreateTexture();
    }

    private unsafe void CreateTexture()
    {
        texture = new TextureBuffer(Device, Queue, new VecI(128, 128));
        sampler = CreateSampler();
        textureBindGroup = CreateBindGroup();
    }

    private unsafe WGPUSampler CreateSampler()
    {
        WGPUSamplerDescriptor samplerDescriptor = new WGPUSamplerDescriptor()
        {
            nextInChain = null,
            addressModeU = WGPUAddressMode.ClampToEdge,
            addressModeV = WGPUAddressMode.ClampToEdge,
            addressModeW = WGPUAddressMode.ClampToEdge,
            magFilter = WGPUFilterMode.Linear,
            minFilter = WGPUFilterMode.Linear,
            mipmapFilter = WGPUMipmapFilterMode.Linear,
            lodMinClamp = 0,
            lodMaxClamp = 0,
            compare = WGPUCompareFunction.Undefined,
            maxAnisotropy = 1,
        };

        return wgpuDeviceCreateSampler(Device, &samplerDescriptor);
    }

    private unsafe WGPUBindGroup CreateBindGroup()
    {
        var bindLayout = wgpuRenderPipelineGetBindGroupLayout(pipeline, 0);

        WGPUBindGroupEntry* textureBindGroupEntries = stackalloc WGPUBindGroupEntry[]
        {
            new WGPUBindGroupEntry()
            {
                binding = 0,
                sampler = sampler,
            },
            new WGPUBindGroupEntry()
            {
                binding = 1,
                textureView = wgpuTextureCreateView(texture.WgpuTexture, null),
            }
        };

        WGPUBindGroupDescriptor bindGroupDescriptor = new WGPUBindGroupDescriptor()
        {
            nextInChain = null,
            layout = bindLayout,
            entryCount = 2,
            entries = textureBindGroupEntries,
        };

        return wgpuDeviceCreateBindGroup(Device, &bindGroupDescriptor);
    }

    private static unsafe void HandleUncapturedErrorCallback(WGPUErrorType type, char* pMessage, void* pUserData)
    {
        Console.WriteLine($"Uncaptured device error: type: {type} ({StringExtensions.GetString(pMessage)})");
    }

    private unsafe void OnAdapterRequestEnded(WGPURequestAdapterStatus status, WGPUAdapter candidateAdapter,
        char* message,
        void* pUserData)
    {
        if (status == WGPURequestAdapterStatus.Success)
        {
            Adapter = candidateAdapter;
            WGPUAdapterProperties properties;
            wgpuAdapterGetProperties(candidateAdapter, &properties);

            WGPUSupportedLimits limits;
            wgpuAdapterGetLimits(candidateAdapter, &limits);

            AdapterProperties = properties;
            AdapterLimits = limits;
        }
        else
        {
            Console.WriteLine($"Could not get WebGPU adapter: {StringExtensions.GetString(message)}");
        }
    }

    private unsafe void OnDeviceRequestEnded(WGPURequestDeviceStatus status, WGPUDevice device, char* message,
        void* pUserData)
    {
        if (status == WGPURequestDeviceStatus.Success)
        {
            Device = device;
        }
        else
        {
            Console.WriteLine($"Could not get WebGPU device: {StringExtensions.GetString(message)}");
        }
    }
}