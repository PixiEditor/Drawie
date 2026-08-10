using Drawie.RenderApi.Abstraction.Textures;
using Silk.NET.OpenGL;

namespace Drawie.RenderApi.OpenGL;

public sealed class OpenGlDepthBuffer : IDisposable
{
    public uint RenderbufferId { get; }

    public int Width { get; }
    public int Height { get; }

    private GL Api { get; }

    public OpenGlDepthBuffer(GL api, int width, int height, DepthFormat depth)
    {
        Api = api;

        Width = width;
        Height = height;

        RenderbufferId = Api.GenRenderbuffer();

        Api.BindRenderbuffer(
            RenderbufferTarget.Renderbuffer,
            RenderbufferId);

        Api.RenderbufferStorage(
            RenderbufferTarget.Renderbuffer,
            ToOpenglDepth(depth),
            (uint)width,
            (uint)height);

        Api.BindRenderbuffer(
            RenderbufferTarget.Renderbuffer,
            0);
    }

    private InternalFormat ToOpenglDepth(DepthFormat depth)
    {
        switch (depth)
        {
            case DepthFormat.NoDepth:
                throw new ArgumentException("Cannot create depth with NoDepth format");
            case DepthFormat.Depth24Stencil8:
                return  InternalFormat.Depth24Stencil8;
            default:
                throw new ArgumentOutOfRangeException(nameof(depth), depth, null);
        }
    }

    public void Dispose()
    {
        Api.DeleteRenderbuffer(RenderbufferId);
    }
}