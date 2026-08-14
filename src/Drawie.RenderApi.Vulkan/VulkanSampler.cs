using Drawie.RenderApi.Abstraction.Textures;
using Drawie.RenderApi.Vulkan.Exceptions;
using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan;

internal sealed class VulkanSampler : ISampler, IDisposable
{
    private readonly VulkanContext context;
    private readonly Sampler sampler;

    public uint Handle => unchecked((uint)sampler.Handle);

    public VulkanSampler(VulkanContext context, SamplerDesc desc)
    {
        this.context = context;
        sampler = CreateSampler(desc);
    }

    public unsafe void Dispose()
    {
        context.Api!.DestroySampler(
            context.LogicalDevice.Device,
            sampler,
            null);
    }

    private unsafe Sampler CreateSampler(SamplerDesc desc)
    {
        SamplerCreateInfo info = new()
        {
            SType = StructureType.SamplerCreateInfo,

            MagFilter = Filter.Linear,
            MinFilter = Filter.Linear,

            AddressModeU = SamplerAddressMode.Repeat,
            AddressModeV = SamplerAddressMode.Repeat,
            AddressModeW = SamplerAddressMode.Repeat,

            AnisotropyEnable = false,
            MaxAnisotropy = 1,

            BorderColor = BorderColor.IntOpaqueBlack,

            UnnormalizedCoordinates = false,

            CompareEnable = false,
            CompareOp = CompareOp.Always,

            MipmapMode = SamplerMipmapMode.Linear
        };

        if (context.Api!.CreateSampler(
                context.LogicalDevice.Device,
                &info,
                null,
                out var result) != Result.Success)
        {
            throw new VulkanException("Failed to create Vulkan sampler.");
        }

        return result;
    }
}