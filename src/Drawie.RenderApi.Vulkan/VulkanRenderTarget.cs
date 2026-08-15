using Drawie.Numerics;
using Drawie.RenderApi.Abstraction.RenderTargets;

namespace Drawie.RenderApi.Vulkan;

internal sealed class VulkanRenderTarget(Buffers.VulkanTexture texture, VecI size) : IRenderTarget, IDisposable
{
    //TODO: It's not exactly framebuffer id, validate if ImageHandle is correct.
    public ulong SurfaceId => Texture.ImageHandle;
    public VecI Size { get; } = size;
    public Buffers.VulkanTexture Texture { get; } = texture;

    public void Dispose()
    {
        Texture.Dispose();
    }
}