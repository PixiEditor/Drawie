using Silk.NET.Core.Contexts;
using Silk.NET.OpenGL;

namespace Drawie.RenderApi.OpenGL;

public class OpenGlGraphicsContext(GL Api, IGLContext GlContext) : IGraphicsContext
{
    private HashSet<uint> ownedTextures = new HashSet<uint>();

    public bool OwnsTexture(ITexture nativeTexture)
    {
        return nativeTexture is OpenGlTexture glTexture && ownedTextures.Contains(glTexture.TextureId);
    }
    
    public OpenGlTexture CreateTexture(uint id)
    {
        ownedTextures.Add(id);
        return new OpenGlTexture(id, Api);
    }
}