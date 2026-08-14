using Drawie.JSInterop;
using Drawie.RenderApi.Abstraction.Textures;
using Drawie.RenderApi.WebGl.Enums;

namespace Drawie.RenderApi.WebGl;

public class WebGlGraphicsContext(int Gl) : IGraphicsContext
{
    private HashSet<ulong> ownedTextures = new HashSet<ulong>();

    public bool OwnsTexture(ITexture nativeTexture)
    {
        return ownedTextures.Contains(nativeTexture.TextureId);
    }

    public void MakeCurrent()
    {
        JSRuntime.MakeContextCurrent(Gl);
    }

    public WebGlRenderTarget CreateRenderTarget(int handle, int width, int height)
    {
        var texture = new WebGlRenderTarget(handle, width, height, DepthFormat.NoDepth);
        ownedTextures.Add(texture.TextureId);
        return texture;
    }

    public void DisposeTexture(WebGlRenderTarget texture)
    {
        if (ownedTextures.Contains(texture.TextureId))
        {
            texture.Dispose();
            ownedTextures.Remove(texture.TextureId);
        }
    }

    public override string ToString()
    {
        return string.Join(", ", ownedTextures);
    }
}