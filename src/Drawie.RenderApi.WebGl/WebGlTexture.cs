using Drawie.JSInterop;

namespace Drawie.RenderApi.WebGl;

public class WebGlTexture : IWebGlTexture, IDisposable
{
    public int Gl { get; private set; }
    public int TextureId { get; private set; }
    
    uint IWebGlTexture.TextureId => (uint)TextureId;
    
    public WebGlTexture(int gl, int textureId)
    {
        TextureId = textureId;
        Gl = gl;
    }

    public void Dispose()
    {
        JSRuntime.DeleteTexture(Gl, TextureId);
    }
}
