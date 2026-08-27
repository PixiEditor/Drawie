using System.Runtime.CompilerServices;
using Drawie.Numerics;
using Drawie.RenderApi.Abstraction;
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
using IAbstractionTexture = Drawie.RenderApi.Abstraction.Textures.ITexture;

namespace Drawie.RenderApi.Vulkan;

public sealed class VulkanGraphicsDevice : IGraphicsDevice
{
    private readonly VulkanContext context;
    private readonly CommandPool commandPool;
    private VulkanPipeline? pipeline;
    private VulkanSampler globalSampler;

    private Dictionary<Guid, UniformBuffer> bufferCache = new Dictionary<Guid, UniformBuffer>();

    private List<IDisposable> disposables = new List<IDisposable>();

    public VulkanGraphicsDevice(VulkanContext context)
    {
        if (context.Api is null)
            throw new InvalidOperationException(
                "Vulkan context must be initialized before creating a graphics device.");

        this.context = context;
        commandPool = CreateCommandPool();
        globalSampler = new VulkanSampler(context, new SamplerDesc());
    }

    private unsafe CommandPool CreateCommandPool()
    {
        var queueFamilyIndices = SetupUtility.FindQueueFamilies(context.Api, context.PhysicalDevice, null, null);

        CommandPoolCreateInfo poolInfo = new()
        {
            SType = StructureType.CommandPoolCreateInfo,
            QueueFamilyIndex = queueFamilyIndices.GraphicsFamily!.Value
        };

        if (context.Api!.CreateCommandPool(context.LogicalDevice.Device, in poolInfo, null, out var cmdPool) !=
            Result.Success)
            throw new VulkanException("Failed to create command pool.");

        return cmdPool;
    }

    public IBuffer<TData> CreateBuffer<TData>(
        BufferUsage usage,
        TData[]? data)
        where TData : unmanaged
    {
        var buffer = new VulkanBuffer<TData>(
            context,
            commandPool,
            usage,
            data);
        disposables.Add(buffer);

        return buffer;
    }

    public IAbstractionTexture CreateTexture(TextureDesc desc)
    {
        var vkTex = new VulkanTexture(
            context.Api!,
            context.LogicalDevice.Device,
            context.PhysicalDevice,
            commandPool,
            context.GraphicsQueue,
            context.GraphicsQueueFamilyIndex,
            desc, globalSampler.VkSampler);

        context.AddManagedTexture(vkTex, vkTex.ImageHandle);
        disposables.Add(vkTex);
        return vkTex;
    }

    public IPipeline CreatePipeline(PipelineDesc desc)
    {
        context.Api!.DeviceWaitIdle(context.LogicalDevice.Device);

        if (pipeline == null || !pipeline.Description.Equals(desc))
        {
            if (pipeline != null)
            {
                disposables.Remove(pipeline);
            }

            pipeline?.Dispose();
            pipeline = new VulkanPipeline(context, desc);
            disposables.Add(pipeline);
        }

        return pipeline;
    }

    public ICommandList CreateCommandList()
    {
        var cmdList = new VulkanCommandList(context, commandPool) { BufferCache = bufferCache };
        disposables.Add(cmdList);
        return cmdList;
    }

    public ISampler CreateSampler(SamplerDesc desc)
    {
        var sampler = new VulkanSampler(context, desc);
        disposables.Add(sampler);
        return sampler;
    }

    public void Submit(RecordedRenderPass cmdList)
    {
        cmdList.Execute.Invoke();
    }

    public IShaderProgram CreateShaderProgram(ShaderProgramDesc desc)
    {
        var program = new VulkanShaderProgram(context, desc);
        disposables.Add(program);
        return program;
    }

    public IRenderTarget CreateRenderTarget(TextureDesc textureDesc)
    {
        var texture = (VulkanTexture)CreateTexture(textureDesc);
        return new VulkanRenderTarget(context, texture, new VecI(textureDesc.Width, textureDesc.Height));
    }

    public IBufferGroup CreateBufferGroup()
    {
        return new VulkanBufferGroup();
    }

    public unsafe void Dispose()
    {
        foreach (var uniformBuffer in bufferCache)
        {
            uniformBuffer.Value.Dispose();
        }

        globalSampler?.Dispose();

        context.Api!.DestroyCommandPool(
            context.LogicalDevice.Device,
            commandPool,
            null);

        foreach (var disposable in disposables)
        {
            disposable.Dispose();
        }
    }
}

internal interface IVkBuffer : IBuffer
{
    public BufferObject NativeBuffer { get; }
}

internal sealed class VulkanBuffer<T> : IVkBuffer, IBuffer<T>, IDisposable where T : unmanaged
{
    private readonly VulkanContext context;
    private readonly CommandPool commandPool;

    public BufferUsage Usage { get; }
    public ulong Size { get; }
    public BufferObject NativeBuffer { get; }

    public VulkanBuffer(VulkanContext context, CommandPool commandPool, BufferUsage usage, T[] data)
    {
        this.context = context;
        this.commandPool = commandPool;
        Usage = usage;
        Size = (ulong)Unsafe.SizeOf<T>() * (ulong)data.Length;
        NativeBuffer = CreateNativeBuffer(Size, usage);

        if (data is { Length: > 0 })
            Upload(data);
    }

    public unsafe void Dispose()
    {
        NativeBuffer.Dispose();
    }

    private BufferObject CreateNativeBuffer(ulong size, BufferUsage usage)
    {
        return usage switch
        {
            BufferUsage.Vertex => new VertexBuffer(context, size),
            BufferUsage.Index => new IndexBuffer(context, size),
            BufferUsage.Uniform => new UniformBuffer(context.Api!, context.LogicalDevice.Device, context.PhysicalDevice,
                size),
            BufferUsage.Storage => new VulkanStorageBuffer(context, size),
            _ => throw new ArgumentOutOfRangeException(nameof(usage), usage, null)
        };
    }

    private void Upload(T[] data)
    {
        switch (Usage)
        {
            case BufferUsage.Vertex:
            case BufferUsage.Index:
            {
                using var stagingBuffer = new StagingBuffer(context, Size);
                stagingBuffer.SetData(data);
                CopyBuffer(stagingBuffer, NativeBuffer, Size);
                break;
            }
            case BufferUsage.Uniform:
            case BufferUsage.Storage:
                NativeBuffer.SetData(data);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private unsafe void CopyBuffer(BufferObject srcBuffer, BufferObject dstBuffer, ulong size)
    {
        using var session = new SingleTimeCommandBufferSession(context, commandPool);

        BufferCopy copyRegion = new() { Size = size };
        context.Api!.CmdCopyBuffer(session.CommandBuffer, srcBuffer.VkBuffer, dstBuffer.VkBuffer, 1, copyRegion);
    }
}
