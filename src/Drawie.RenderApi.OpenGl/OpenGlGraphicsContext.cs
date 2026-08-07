using Silk.NET.Core.Contexts;
using Silk.NET.OpenGL;

namespace Drawie.RenderApi.OpenGL;

public class OpenGlGraphicsContext(GL api, IGLContext glContext) : IGraphicsContext
{
    public readonly IGLContext GlContext = glContext;
    public readonly GL Api = api;
    private HashSet<uint> ownedTextures = new HashSet<uint>();

    public bool OwnsTexture(ITexture nativeTexture)
    {
        return nativeTexture is OpenGlTexture glTexture && ownedTextures.Contains(glTexture.TextureId);
    }

    public void MakeCurrent()
    {
        GlContext.MakeCurrent();
    }

    public OpenGlTexture CreateTexture(uint id)
    {
        ownedTextures.Add(id);
        return new OpenGlTexture(id, Api);
    }
}