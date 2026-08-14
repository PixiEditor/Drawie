using Drawie.JSInterop;
using Drawie.RenderApi.WebGl.Enums;

namespace Drawie.RenderApi.WebGl;

public class WebGlTexture : IWebGlTexture, IDisposable
{
    public int Gl { get; private set; }
    public ulong TextureId { get; private set; }
    
    uint IWebGlTexture.TextureId => (uint)TextureId;
    
    public WebGlTexture(int gl, ulong textureId)
    {
        TextureId = textureId;
        Gl = gl;
    }

    public WebGlTexture(int gl, int width, int height)
    {
        Gl = gl;
        TextureId = (ulong)JSRuntime.CreateTexture(gl);
        
        JSRuntime.BindTexture(gl, (int)WebGlTextureType.Texture2D, (int)TextureId);
        JSRuntime.TexImage2D(gl, (int)WebGlTextureType.Texture2D, 0, (int)WebGlTextureFormat.Rgba, width, height, 0, (int)WebGlTextureFormat.Rgba, (int)WebGlArrayType.UnsignedByte, 0);
        JSRuntime.TexParameteri(gl, (int)WebGlTextureType.Texture2D, (int)WebGlTextureParameterName.TextureMinFilter, (int)WebGlTextureFilter.Nearest);
        JSRuntime.TexParameteri(gl, (int)WebGlTextureType.Texture2D, (int)WebGlTextureParameterName.TextureMagFilter, (int)WebGlTextureFilter.Nearest);
        JSRuntime.TexParameteri(gl, (int)WebGlTextureType.Texture2D, (int)WebGlTextureParameterName.TextureWrapS, (int)WebGlTextureWrap.ClampToEdge);
        JSRuntime.TexParameteri(gl, (int)WebGlTextureType.Texture2D, (int)WebGlTextureParameterName.TextureWrapT, (int)WebGlTextureWrap.ClampToEdge);
    }

    public void Dispose()
    {
        JSRuntime.DeleteTexture(Gl, (int)TextureId);
    }
}
