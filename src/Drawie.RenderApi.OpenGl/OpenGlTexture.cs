using Drawie.Numerics;
using Silk.NET.OpenGL;

namespace Drawie.RenderApi.OpenGL;

public class OpenGlTexture : IOpenGlTexture, IDisposable
{
    public ulong TextureId { get; }
    public VecI Size => new VecI(Width, Height);

    public int Width { get; }
    public int Height { get; }
    public int Samples { get; }

    private GL Api { get; }

    public OpenGlTexture(uint textureId, GL api, int width, int height)
    {
        TextureId = textureId;
        Api = api;
        Width = width;
        Height = height;
        Samples = 1;
    }

    public unsafe OpenGlTexture(GL api, int width, int height, int samples)
    {
        Api = api;

        Width = width;
        Height = height;
        Samples = samples;

        TextureId = Api.GenTexture();

        Activate(0);
        Bind();

        if (samples == 1)
        {
            Api.TexImage2D(
                TextureTarget.Texture2D,
                0,
                InternalFormat.Rgba,
                (uint)width,
                (uint)height,
                0,
                PixelFormat.Rgba,
                PixelType.UnsignedByte,
                null);
        }
        else
        {
            Api.TexImage2DMultisample(
                TextureTarget.Texture2DMultisample,
                (uint)samples,
                InternalFormat.Rgb,
                (uint)width,
                (uint)height, true);
        }

        ApplyParameters();
    }
    
    public unsafe OpenGlTexture(GL api, int width, int height, Span<byte> data, PixelFormat format = PixelFormat.Rgba)
    {
        Api = api;

        Width = width;
        Height = height;

        TextureId = Api.GenTexture();
        
        Activate(0);
        Bind();
       
        LoadTextureFromBytes(data, format);

        ApplyParameters();
    }

    private void ApplyParameters()
    {
        Api.TexParameterI(
            TextureTarget.Texture2D,
            TextureParameterName.TextureWrapS,
            (int)GLEnum.ClampToEdge);

        Api.TexParameterI(
            TextureTarget.Texture2D,
            TextureParameterName.TextureWrapT,
            (int)GLEnum.ClampToEdge);

        Api.TexParameterI(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMinFilter,
            (int)GLEnum.Nearest);

        Api.TexParameterI(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMagFilter,
            (int)GLEnum.Nearest);
    }

    private unsafe void LoadTextureFromBytes(Span<byte> data, PixelFormat format)
    {
        fixed (void* d = &data[0])
        {
            Api.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Rgba, (uint)Width, (uint)Height, 0, format, PixelType.UnsignedByte, d);
        }
    }

    public void Bind()
    {
        var target = Samples == 1 ? TextureTarget.Texture2D : TextureTarget.Texture2DMultisample;
        Api.BindTexture(
            target,
            (uint)TextureId);
    }

    public void Activate(int textureUnit)
    {
        Api.ActiveTexture(
            TextureUnit.Texture0 + textureUnit);
    }

    public void Dispose()
    {
        Api.DeleteTexture((uint)TextureId);
    }
}