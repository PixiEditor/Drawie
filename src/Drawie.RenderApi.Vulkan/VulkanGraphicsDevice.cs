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
using Buffer = Silk.NET.Vulkan.Buffer;
using IAbstractionTexture = Drawie.RenderApi.Abstraction.Textures.ITexture;

namespace Drawie.RenderApi.Vulkan;

public sealed class VulkanGraphicsDevice : IGraphicsDevice, IDisposable
{
    private readonly VulkanContext context;
    private readonly CommandPool commandPool;
    
    public VulkanGraphicsDevice(VulkanContext context)
    {
        if (context.Api is null)
            throw new InvalidOperationException(
                "Vulkan context must be initialized before creating a graphics device.");

        this.context = context;
        commandPool = CreateCommandPool();
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
        return new VulkanBuffer<TData>(
            context,
            commandPool,
            usage,
            data);
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
            desc);

        context.AddManagedTexture(vkTex, vkTex.ImageHandle);
        return vkTex;
    }

    private VulkanPipeline? pipeline;
    public IPipeline CreatePipeline(PipelineDesc desc)
    {
        context.Api!.DeviceWaitIdle(context.LogicalDevice.Device);
        
        return pipeline ??= new VulkanPipeline(context, desc);
    }

    public ICommandList CreateCommandList()
    {
        return new VulkanCommandList(context, commandPool);
    }

    public ISampler CreateSampler(SamplerDesc desc)
    {
        return new VulkanSampler(context, desc);
    }

    public void Submit(RecordedRenderPass cmdList)
    {
        cmdList.Execute.Invoke();
    }

    public IShaderProgram CreateShaderProgram(ShaderProgramDesc desc)
    {
        return new VulkanShaderProgram(context, desc);
    }

    public IRenderTarget CreateRenderTarget(TextureDesc textureDesc)
    {
        var texture = (VulkanTexture)CreateTexture(textureDesc);

        return new VulkanRenderTarget(texture, new VecI(textureDesc.Width, textureDesc.Height));
    }

    public IBufferGroup CreateBufferGroup()
    {
        return new VulkanBufferGroup();
    }

    public unsafe void Dispose()
    {
        context.Api!.DestroyCommandPool(
            context.LogicalDevice.Device,
            commandPool,
            null);
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
            BufferUsage.Uniform => new UniformBuffer(context.Api!, context.LogicalDevice.Device, context.PhysicalDevice, size),
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