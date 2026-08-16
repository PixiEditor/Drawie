using Drawie.RenderApi.Abstraction.Textures;
using Silk.NET.OpenGL;

namespace Drawie.RenderApi.OpenGL;

public sealed class OpenGlDepthBuffer : IDisposable
{
    public uint RenderbufferId { get; }

    public int Width { get; }
    public int Height { get; }
    public int Samples { get; }

    private GL Api { get; }
    public InternalFormat Format { get; set; }

    public OpenGlDepthBuffer(GL api, int width, int height, DepthFormat depth, int samples)
    {
        Api = api;

        Width = width;
        Height = height;
        Samples = samples;

        RenderbufferId = Api.GenRenderbuffer();
        Format = ToOpenglDepth(depth);

        Api.BindRenderbuffer(
            RenderbufferTarget.Renderbuffer,
            RenderbufferId);
        

        if (samples == 1)
        {
            Api.RenderbufferStorage(
                RenderbufferTarget.Renderbuffer,
                Format,
                (uint)width,
                (uint)height);
        }
        else
        {
            Api.RenderbufferStorageMultisample(RenderbufferTarget.Renderbuffer, (uint)Samples, Format, (uint)Width, (uint)Height);
        }

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