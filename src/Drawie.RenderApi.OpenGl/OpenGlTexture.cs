using Silk.NET.OpenGL;

namespace Drawie.RenderApi.OpenGL;

public class OpenGlTexture : IOpenGlTexture, IDisposable
{
    public ulong TextureId { get; }

    public int Width { get; }
    public int Height { get; }

    private GL Api { get; }

    public OpenGlTexture(uint textureId, GL api, int width, int height)
    {
        TextureId = textureId;
        Api = api;
        Width = width;
        Height = height;
    }

    public unsafe OpenGlTexture(GL api, int width, int height)
    {
        Api = api;

        Width = width;
        Height = height;

        TextureId = Api.GenTexture();

        Activate(0);
        Bind();

        Api.TexImage2D(
            TextureTarget.Texture2D,
            0,
            InternalFormat.Rgba8,
            (uint)width,
            (uint)height,
            0,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            null);

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
        Api.BindTexture(
            TextureTarget.Texture2D,
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