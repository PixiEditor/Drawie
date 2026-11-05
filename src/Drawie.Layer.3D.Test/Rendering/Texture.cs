using Silk.NET.OpenGL;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace SilkNet.Rendering;

public sealed class Texture : NativeObject
{
    public Texture(GL glContext, string path) : base(glContext, glContext.GenTexture())
    {
        LoadTextureFromPath(path);
        SetParameters();
    }

    public Texture(GL glContext, Span<byte> data, uint width, uint height) : base(glContext, glContext.GenTexture())
    {
        LoadDataFromSpan(data, width, height);
    }

    private unsafe void LoadDataFromSpan(Span<byte> data, uint width, uint height)
    {
        fixed (void* d = &data[0])
        {
            GlContext.TexImage2D(TextureTarget.Texture2D, 0, (int)InternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, d);
            SetParameters();
        }
    }

    private unsafe void LoadTextureFromPath(string path)
    {
        using var img = Image.Load<Rgba32>(path);
        // Reserve memory in GPU for whole image 
        GlContext.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba8, (uint)img.Width, (uint)img.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, null);
        
        img.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                fixed (void* data = accessor.GetRowSpan(y))
                {
                    // Load the actual image
                    GlContext.TexSubImage2D(TextureTarget.Texture2D, 0, 0, y, (uint)accessor.Width, 1, PixelFormat.Rgba, PixelType.UnsignedByte, data);
                }
            }
        });
    }
    
    private void SetParameters()
    {
        GlContext.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.ClampToEdge);
        GlContext.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.ClampToEdge);
        GlContext.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.LinearMipmapLinear);
        GlContext.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Linear);
        GlContext.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureBaseLevel, 0);
        GlContext.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, 8);
        
        GlContext.GenerateMipmap(TextureTarget.Texture2D);
    }

    public void Bind(TextureUnit textureSlot = TextureUnit.Texture0)
    {
        GlContext.ActiveTexture(textureSlot);
        GlContext.BindTexture(TextureTarget.Texture2D, Handle);
    }

    public override void Dispose()
    {
        GlContext.DeleteTexture(Handle);
    }
}