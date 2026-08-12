using Drawie.RenderApi.Abstraction.Textures;
using Silk.NET.Core.Contexts;
using Silk.NET.OpenGL;

namespace Drawie.RenderApi.OpenGL;

public class OpenGlGraphicsContext(GL api, IGLContext glContext) : IGraphicsContext
{
    public readonly IGLContext GlContext = glContext;
    public readonly GL Api = api;
    private HashSet<ulong> ownedTextures = new HashSet<ulong>();

    public bool OwnsTexture(ITexture nativeTexture)
    {
        return nativeTexture is OpenGlTexture glTexture && ownedTextures.Contains(glTexture.TextureId);
    }

    public void MakeCurrent()
    {
        GlContext.MakeCurrent();
    }

    public OpenGlTexture CreateTexture(uint id, int width, int height)
    {
        ownedTextures.Add(id);
        return new OpenGlTexture(id, Api, width, height);
    }
}