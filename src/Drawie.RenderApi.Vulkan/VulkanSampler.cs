using Drawie.RenderApi.Abstraction.Textures;
using Drawie.RenderApi.Vulkan.Exceptions;
using Drawie.RenderApi.Vulkan.Extensions;
using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan;

internal sealed class VulkanSampler : ISampler, IDisposable
{
    private readonly VulkanContext context;
    private readonly Sampler _vkSampler;

    public uint Handle => unchecked((uint)_vkSampler.Handle);
    public Sampler VkSampler => _vkSampler;

    public VulkanSampler(VulkanContext context, SamplerDesc desc)
    {
        this.context = context;
        _vkSampler = CreateSampler(desc);
    }

    public unsafe void Dispose()
    {
        context.Api!.DestroySampler(
            context.LogicalDevice.Device,
            _vkSampler,
            null);
    }

    private unsafe Sampler CreateSampler(SamplerDesc desc)
    {
        SamplerCreateInfo info = new()
        {
            SType = StructureType.SamplerCreateInfo,

            MagFilter = desc.MagFilter.ToFilter(),
            MinFilter = desc.MinFilter.ToFilter(),

            AddressModeU = desc.WrapU.ToAddressMode(),
            AddressModeV = desc.WrapV.ToAddressMode(),
            AddressModeW = desc.WrapW.ToAddressMode(),

            AnisotropyEnable = false,
            MaxAnisotropy = 1,

            BorderColor = BorderColor.IntOpaqueBlack,

            UnnormalizedCoordinates = desc.UnnormalizedCoordinates,

            CompareEnable = desc.EnableCompare,
            CompareOp = desc.CompareOp.ToCompareOp(),

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