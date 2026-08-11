using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

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
    
    public unsafe OpenGlTexture(GL api, int width, int height, string path)
    {
        Api = api;

        Width = width;
        Height = height;

        TextureId = Api.GenTexture();
        
        Activate(0);
        Bind();
       
        LoadTextureFromPath(path);

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
    
    private unsafe void LoadTextureFromPath(string path)
    {
        using var img = Image.Load<Rgba32>(path);
        // Reserve memory in GPU for whole image 
        Api.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, (uint)img.Width, (uint)img.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);
        
        img.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                fixed (void* data = accessor.GetRowSpan(y))
                {
                    // Load the actual image
                    Api.TexSubImage2D(TextureTarget.Texture2D, 0, 0, y, (uint)accessor.Width, 1, PixelFormat.Rgba, PixelType.UnsignedByte, data);
                }
            }
        });
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