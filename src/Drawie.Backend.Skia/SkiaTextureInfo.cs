using Drawie.Backend.Core.Surfaces;
using Drawie.RenderApi.Abstraction.Textures;
using SkiaSharp;

namespace Drawie.Skia;

public class SkiaTextureInfo : ITexture
{
    public GRBackendTexture Target => target;
    private GRBackendTexture target;

    public SkiaTextureInfo(GRBackendTexture target)
    {
        this.target = target;
    }

    public ulong TextureId => target.GetGlTextureInfo().Id;
}