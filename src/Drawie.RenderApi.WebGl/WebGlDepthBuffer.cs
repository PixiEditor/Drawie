using Drawie.JSInterop;
using Drawie.RenderApi.Abstraction.Textures;
using Drawie.RenderApi.WebGl.Enums;

namespace Drawie.RenderApi.WebGl;

public class WebGlDepthBuffer : IDisposable
{
    public uint RenderbufferId { get; }

    public int Width { get; }
    public int Height { get; }

    private int Gl { get; }

    public WebGlDepthBuffer(int api, int width, int height, DepthFormat depth)
    {
        Gl = api;

        Width = width;
        Height = height;

        RenderbufferId = (uint)JSRuntime.CreateRenderbuffer(api);

        JSRuntime.BindRenderbuffer(api, (int)WebGlRenderbufferTarget.Renderbuffer, (int)RenderbufferId);

        JSRuntime.RenderbufferStorage(
            api,
            (int)WebGlRenderbufferTarget.Renderbuffer,
            (int)ToOpenglDepth(depth), width, height);

        JSRuntime.BindRenderbuffer(api, (int)WebGlRenderbufferTarget.Renderbuffer, 0);
    }

    private WebGlRenderbufferFormat ToOpenglDepth(DepthFormat depth)
    {
        switch (depth)
        {
            case DepthFormat.NoDepth:
                throw new ArgumentException("Cannot create depth with NoDepth format");
            case DepthFormat.Depth24Stencil8:
                return WebGlRenderbufferFormat.Depth24Stencil8;
            default:
                throw new ArgumentOutOfRangeException(nameof(depth), depth, null);
        }
    }

    public void Dispose()
    {
        JSRuntime.DeleteRenderbuffer(Gl, (int)RenderbufferId);
    }   
}