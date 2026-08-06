using Drawie.JSInterop;
using Drawie.RenderApi.WebGl.Enums;

namespace Drawie.RenderApi.WebGl;

public class WebGlGraphicsContext(int Gl) : IGraphicsContext
{
    private HashSet<int> ownedTextures = new HashSet<int>();

    public bool OwnsTexture(ITexture nativeTexture)
    {
        return nativeTexture is WebGlTexture wglTexture && ownedTextures.Contains(wglTexture.TextureId);
    }

    public void MakeCurrent()
    {
        JSRuntime.MakeContextCurrent(Gl);
    }

    public WebGlTexture CreateTexture(int handle, int width, int height)
    {
        var texture = new WebGlTexture(Gl, JSRuntime.CreateTexture(handle));
        JSRuntime.BindTexture(handle, (int)WebGlTextureType.Texture2D, texture.TextureId);
        JSRuntime.TexImage2D(handle, (int)WebGlTextureType.Texture2D, 0, (int)WebGlTextureFormat.Rgba, width, height, 0, (int)WebGlTextureFormat.Rgba, (int)WebGlArrayType.UnsignedByte, 0);
        JSRuntime.TexParameteri(handle, (int)WebGlTextureType.Texture2D, (int)WebGlTextureParameterName.TextureMinFilter, (int)WebGlTextureFilter.Nearest);
        JSRuntime.TexParameteri(handle, (int)WebGlTextureType.Texture2D, (int)WebGlTextureParameterName.TextureMagFilter, (int)WebGlTextureFilter.Nearest);
        JSRuntime.TexParameteri(handle, (int)WebGlTextureType.Texture2D, (int)WebGlTextureParameterName.TextureWrapS, (int)WebGlTextureWrap.ClampToEdge);
        JSRuntime.TexParameteri(handle, (int)WebGlTextureType.Texture2D, (int)WebGlTextureParameterName.TextureWrapT, (int)WebGlTextureWrap.ClampToEdge);
        ownedTextures.Add(texture.TextureId);
        return texture;
    }

    public void DisposeTexture(WebGlTexture texture)
    {
        if (ownedTextures.Contains(texture.TextureId))
        {
            JSRuntime.DeleteTexture(Gl, texture.TextureId);
            ownedTextures.Remove(texture.TextureId);
        }
    }
}