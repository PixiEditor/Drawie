using Drawie.Backend.Core.Surfaces;
using SkiaSharp;

namespace Drawie.Skia;

public class SkiaFramebufferInfo : IFramebufferInfo
{
    private GRBackendRenderTarget target;

    public SkiaFramebufferInfo(GRBackendRenderTarget target)
    {
        this.target = target;
    }

    public uint FramebufferId => target.GetGlFramebufferInfo().FramebufferObjectId;
}