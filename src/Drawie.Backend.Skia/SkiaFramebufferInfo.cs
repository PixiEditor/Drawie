using Drawie.Backend.Core.Surfaces;
using SkiaSharp;

namespace Drawie.Skia;

public class SkiaFramebufferInfo : IFramebufferInfo
{
    public GRVkImageInfo? VkImageInfo { get; }
    public GRGlFramebufferInfo? GlFramebufferInfo { get; }
    private GRBackendRenderTarget target;

    public SkiaFramebufferInfo(GRBackendRenderTarget target, GRVkImageInfo imageInfo)
    {
        this.target = target;
        VkImageInfo = imageInfo;
    }

    public SkiaFramebufferInfo(GRBackendRenderTarget backendRenderTarget, GRGlFramebufferInfo grGlFramebufferInfo)
    {
        this.target = backendRenderTarget;
        GlFramebufferInfo = grGlFramebufferInfo;
    }

    public uint FramebufferId => target.GetGlFramebufferInfo().FramebufferObjectId;
}