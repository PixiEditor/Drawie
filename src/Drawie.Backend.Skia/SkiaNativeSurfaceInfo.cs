using Drawie.Backend.Core.Surfaces;
using SkiaSharp;

namespace Drawie.Skia;

public class SkiaNativeSurfaceInfo : IFramebufferInfo
{
    public GRVkImageInfo? VkImageInfo { get; }
    public GRGlFramebufferInfo? GlFramebufferInfo { get; }
    public GRGlTextureInfo? GlTextureInfo { get; }

    private GRBackendTexture texture;
    private GRBackendRenderTarget target;

    public SkiaNativeSurfaceInfo(GRBackendRenderTarget target, GRVkImageInfo imageInfo)
    {
        this.target = target;
        VkImageInfo = imageInfo;
    }

    public SkiaNativeSurfaceInfo(GRBackendRenderTarget backendRenderTarget, GRGlFramebufferInfo grGlFramebufferInfo)
    {
        this.target = backendRenderTarget;
        GlFramebufferInfo = grGlFramebufferInfo;
    }

    public SkiaNativeSurfaceInfo(GRBackendTexture backendRenderTarget, GRGlTextureInfo grGlFramebufferInfo)
    {
        GlTextureInfo = grGlFramebufferInfo;
        texture = backendRenderTarget;
    }


    public ulong SurfaceId => VkImageInfo?.Image ?? target?.GetGlFramebufferInfo().FramebufferObjectId ?? GlTextureInfo.Value.Id;
}