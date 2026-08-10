using Silk.NET.OpenGL;

namespace Drawie.RenderApi.OpenGL;

public class OpenGlTexture : IOpenGlTexture, IDisposable
{
    public uint TextureId { get; }

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

    public void Bind()
    {
        Api.BindTexture(
            TextureTarget.Texture2D,
            TextureId);
    }

    public void Activate(int textureUnit)
    {
        Api.ActiveTexture(
            TextureUnit.Texture0 + textureUnit);
    }

    public void Dispose()
    {
        Api.DeleteTexture(TextureId);
    }
}