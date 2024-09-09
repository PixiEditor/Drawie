using System.Numerics;
using System.Runtime.InteropServices;
using Drawie.RenderApi.WebGpu.Extensions;
using PixiEditor.Numerics;
using Silk.NET.Core.Contexts;
using Silk.NET.WebGPU;
using Silk.NET.WebGPU.Extensions.Dawn;
using Silk.NET.WebGPU.Extensions.WGPU;
using Buffer = Silk.NET.WebGPU.Buffer;

namespace Drawie.RenderApi.WebGpu;

public class WebGpuWindowRenderApi : IWindowRenderApi
{
    public GraphicsApi GraphicsApi { get; }

    public event Action? FramebufferResized;

    private WebGPU wgpu;
    private unsafe Instance* instance;
    private unsafe Adapter* adapter;
    private AdapterProperties adapterProperties;
    private SupportedLimits adapterLimits;
    private unsafe Device* device;
    private unsafe Queue* queue;
    private unsafe Surface* wgpuSurface;
    private TextureFormat swapChainFormat;

    private unsafe PipelineLayout* pipelineLayout;
    private unsafe RenderPipeline* renderPipeline;
    private unsafe Buffer* vertexBuffer;

    private VecI framebufferSize;

    private const int DeviceExtrasFlag = 0x00030001;

    public unsafe void CreateInstance(object surface, VecI size)
    {
        if (surface is not INativeWindow nativeWindow)
        {
            throw new ArgumentException("Surface must be a window handle");
        }

        this.framebufferSize = size;

        wgpu = WebGPU.GetApi();

        InitWebGpu(size, nativeWindow);
        InitResources();
    }

    public unsafe void Render(double deltaTime)
    {
        SurfaceTexture surfaceTexture = default;
        wgpu.SurfaceGetCurrentTexture(wgpuSurface, &surfaceTexture);

        if (surfaceTexture.Status != SurfaceGetCurrentTextureStatus.Success)
        {
            // recreate surface
            Console.WriteLine("Failed to get the texture!");
        }

        TextureView* nextView = wgpu.TextureCreateView(surfaceTexture.Texture, null);

        CommandEncoderDescriptor encoderDescriptor = new CommandEncoderDescriptor()
        {
            NextInChain = null
        };
        
        CommandEncoder* encoder = wgpu.DeviceCreateCommandEncoder(device, &encoderDescriptor);

        RenderPassColorAttachment colorAttachment = new RenderPassColorAttachment()
        {
            View = nextView,
            ResolveTarget = null,
            LoadOp = LoadOp.Clear,
            StoreOp = StoreOp.Store,
            ClearValue = new Color() { A = 1 }
        };

        RenderPassDescriptor renderPassDescriptor = new RenderPassDescriptor()
        {
            NextInChain = null,
            ColorAttachmentCount = 1,
            ColorAttachments = &colorAttachment,
            DepthStencilAttachment = null,
            TimestampWrites = null
        };

        RenderPassEncoder* renderPass = wgpu.CommandEncoderBeginRenderPass(encoder, &renderPassDescriptor);

        wgpu.RenderPassEncoderSetPipeline(renderPass, renderPipeline);
        wgpu.RenderPassEncoderSetVertexBuffer(renderPass, 0, vertexBuffer, 0, WebGPU.WholeMapSize);
        wgpu.RenderPassEncoderDraw(renderPass, 3, 1, 0, 0);
        wgpu.RenderPassEncoderEnd(renderPass);

        wgpu.TextureViewRelease(nextView);

        CommandBufferDescriptor commandBufferDescriptor = new CommandBufferDescriptor()
        {
            NextInChain = null
        };

        CommandBuffer* command = wgpu.CommandEncoderFinish(encoder, &commandBufferDescriptor);
        wgpu.QueueSubmit(queue, 1, &command);

        wgpu.CommandEncoderRelease(encoder);

        wgpu.SurfacePresent(wgpuSurface);
    }

    public unsafe void DestroyInstance()
    {
        wgpu.SurfaceRelease(wgpuSurface);
        wgpu.DeviceDestroy(device);
        wgpu.DeviceRelease(device);
        wgpu.AdapterRelease(adapter);
        wgpu.InstanceRelease(instance);
    }

    public void UpdateFramebufferSize(int width, int height)
    {
        throw new NotImplementedException();
    }

    public void PrepareTextureToWrite()
    {
        throw new NotImplementedException();
    }

    private unsafe void InitWebGpu(VecI size, INativeWindow nativeWindow)
    {
        InstanceExtras instanceExtras = new InstanceExtras()
        {
            Backends = InstanceBackend.Vulkan
        };

        InstanceDescriptor instanceDescriptor = new InstanceDescriptor()
        {
            NextInChain = &instanceExtras.Chain
        };

        instance = wgpu.CreateInstance(instanceDescriptor);

        var window = nativeWindow.Win32;

        SurfaceDescriptorFromWindowsHWND windowSurfaceDescriptor =
            new SurfaceDescriptorFromWindowsHWND()
            {
                Chain = new ChainedStruct()
                {
                    SType = SType.SurfaceDescriptorFromWindowsHwnd,
                },

                Hwnd = window.Value.Hwnd.ToPointer(),
                Hinstance = window.Value.HInstance.ToPointer()
            };

        SurfaceDescriptor surfaceDescriptor = new SurfaceDescriptor()
        {
            NextInChain = &windowSurfaceDescriptor.Chain
        };

        wgpuSurface = wgpu.InstanceCreateSurface(instance, &surfaceDescriptor);

        var adapterOptions = new RequestAdapterOptions()
        {
            NextInChain = null,
            PowerPreference = PowerPreference.HighPerformance,
            CompatibleSurface = wgpuSurface,
        };

        var requestAdapterCallback = new PfnRequestAdapterCallback(AdapterRequestEndedCallback);

        wgpu.InstanceRequestAdapter(instance, &adapterOptions, requestAdapterCallback, (void*)0);

        DeviceDescriptor deviceDescriptor = new DeviceDescriptor()
        {
            NextInChain = null,
            Label = null,
            RequiredFeatures = (FeatureName*)0,
            RequiredLimits = null,
        };

        var deviceCallback = new PfnRequestDeviceCallback(OnDeviceRequestEnded);

        wgpu.AdapterRequestDevice(adapter, &deviceDescriptor, deviceCallback, (void*)0);
        wgpu.DeviceSetUncapturedErrorCallback(device, new PfnErrorCallback(OnDeviceError), (void*)0);
        queue = wgpu.DeviceGetQueue(device);

        swapChainFormat = wgpu.SurfaceGetPreferredFormat(wgpuSurface, adapter);

        var surfaceConfig = new SurfaceConfiguration()
        {
            NextInChain = null,
            Device = device,
            Usage = TextureUsage.RenderAttachment,
            Format = swapChainFormat,
            Width = (uint)size.X,
            Height = (uint)size.Y,
            PresentMode = PresentMode.Fifo,
        };

        wgpu.SurfaceConfigure(wgpuSurface, &surfaceConfig);
    }

    private unsafe void InitResources()
    {
        PipelineLayoutDescriptor layoutDescriptor = new PipelineLayoutDescriptor()
        {
            NextInChain = null,
            BindGroupLayouts = null,
            BindGroupLayoutCount = 0,
        };

        pipelineLayout = wgpu.DeviceCreatePipelineLayout(device, &layoutDescriptor);

        string shaderSource = File.ReadAllText(Path.Combine("Shaders", "wgpu_shader.wgsl"));

        ShaderModuleWGSLDescriptor shaderModuleDescriptor = new ShaderModuleWGSLDescriptor()
        {
            Chain = new ChainedStruct()
            {
                SType = SType.ShaderModuleWgslDescriptor,
                Next = null
            },
            
            Code = shaderSource.ToPointer()
        };

        ShaderModuleDescriptor shaderModule = new ShaderModuleDescriptor()
        {
            NextInChain = &shaderModuleDescriptor.Chain,
            HintCount = 0,
            Hints = null,
        };

        var shaderModulePtr = wgpu.DeviceCreateShaderModule(device, &shaderModule);

        VertexAttribute* vertexAttributes = stackalloc VertexAttribute[2];

        vertexAttributes[0] = new VertexAttribute()
        {
            Format = VertexFormat.Float32x4,
            Offset = 0,
            ShaderLocation = 0,
        };

        vertexAttributes[1] = new VertexAttribute()
        {
            Format = VertexFormat.Float32x4,
            Offset = 16,
            ShaderLocation = 1,
        };

        VertexBufferLayout vertexBufferLayout = new VertexBufferLayout()
        {
            ArrayStride = (ulong)sizeof(Vector4) * 2,
            AttributeCount = 2,
            Attributes = vertexAttributes,
            StepMode = VertexStepMode.Vertex
        };

        BlendState blendState = new BlendState()
        {
            Color = new BlendComponent()
            {
                Operation = BlendOperation.Add,
                SrcFactor = BlendFactor.One,
                DstFactor = BlendFactor.Zero,
            },
            Alpha = new BlendComponent()
            {
                Operation = BlendOperation.Add,
                SrcFactor = BlendFactor.One,
                DstFactor = BlendFactor.Zero,
            },
        };

        ColorTargetState colorTargetState = new ColorTargetState()
        {
            NextInChain = null,
            Format = swapChainFormat,
            Blend = &blendState,
            WriteMask = ColorWriteMask.All,
        };

        FragmentState fragmentState = new FragmentState()
        {
            NextInChain = null,
            Module = shaderModulePtr,
            EntryPoint = "fragmentMain".ToPointer(),
            TargetCount = 1,
            Targets = &colorTargetState,
            Constants = null,
            ConstantCount = 0
        };

        RenderPipelineDescriptor renderPipelineDescriptor = new RenderPipelineDescriptor()
        {
            Layout = pipelineLayout,
            Vertex = new VertexState()
            {
                Module = shaderModulePtr,
                EntryPoint = "vertexMain".ToPointer(),
                BufferCount = 1,
                Buffers = &vertexBufferLayout,
                Constants = null,
                ConstantCount = 0
            },
            Primitive = new PrimitiveState()
            {
                Topology = PrimitiveTopology.TriangleList,
                StripIndexFormat = IndexFormat.Undefined,
                FrontFace = FrontFace.Ccw,
                CullMode = CullMode.None,
            },
            Fragment = &fragmentState,
            DepthStencil = null,
            Multisample = new MultisampleState()
            {
                Count = 1,
                Mask = ~0u,
                AlphaToCoverageEnabled = false,
            },
        };

        renderPipeline = wgpu.DeviceCreateRenderPipeline(device, &renderPipelineDescriptor);

        wgpu.ShaderModuleRelease(shaderModulePtr);

        Vector4* vertices = stackalloc Vector4[]
        {
            new Vector4(0.0f, 0.5f, 0.5f, 1.0f),
            new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
            new Vector4(0.5f, -0.5f, 0.5f, 1.0f),
            new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
            new Vector4(-0.5f, -0.5f, 0.5f, 1.0f),
            new Vector4(0.0f, 0.0f, 1.0f, 1.0f)
        };

        ulong size = (ulong)(sizeof(Vector4) * 6);
        BufferDescriptor bufferDescriptor = new BufferDescriptor()
        {
            NextInChain = null,
            Usage = BufferUsage.Vertex | BufferUsage.CopyDst,
            Size = size,
            MappedAtCreation = false,
        };

        vertexBuffer = wgpu.DeviceCreateBuffer(device, &bufferDescriptor);
        wgpu.QueueWriteBuffer(queue, vertexBuffer, 0, vertices, (nuint)size);
    }

    private unsafe void AdapterRequestEndedCallback(RequestAdapterStatus status, Adapter* adapter, byte* message,
        void* userData)
    {
        if (status == RequestAdapterStatus.Success)
        {
            this.adapter = adapter;

            AdapterProperties fetchedAdapterProperties = default;

            wgpu.AdapterGetProperties(adapter, &fetchedAdapterProperties);

            this.adapterProperties = fetchedAdapterProperties;

            SupportedLimits limits;
            wgpu.AdapterGetLimits(adapter, &limits);

            this.adapterLimits = limits;

            string name = Marshal.PtrToStringAnsi((IntPtr)adapterProperties.Name);

            Console.WriteLine($"Adapter name: {name}");
        }
        else
        {
            throw new Exception("Failed to get adapter");
        }
    }

    private unsafe void OnDeviceRequestEnded(RequestDeviceStatus status, Device* device, byte* message, void* userData)
    {
        if (status == RequestDeviceStatus.Success)
        {
            this.device = device;
        }
        else
        {
            throw new Exception("Failed to get device");
        }
    }

    private unsafe void OnDeviceError(ErrorType errorType, byte* message, void* userData)
    {
        throw new Exception($"Device error of type {errorType}: '{Marshal.PtrToStringAnsi((IntPtr)message)}'");
    }
}