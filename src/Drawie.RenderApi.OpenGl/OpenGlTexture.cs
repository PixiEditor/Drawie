using Drawie.Numerics;
using Silk.NET.OpenGL;

namespace Drawie.RenderApi.OpenGL;

public class OpenGlTexture : IOpenGlTexture, IDisposable
{
    public uint TextureId { get; }

    private GL Api { get; set; }

    public VecI Size { get; }

    public OpenGlTexture(uint textureId, GL api)
    {
        TextureId = textureId;
        Api = api;
    }

    public unsafe OpenGlTexture(GL api, int width, int height)
    {
        TextureId = api.GenTexture();
        Size = new VecI(width, height);

        Api = api;
        Activate(0);
        Bind();

        Api.TexImage2D(TextureTarget.Texture2D, 0, InternalFormat.Rgba, (uint)width, (uint)height, 0,
            PixelFormat.Rgba,
            PixelType.UnsignedByte, null);

        Api.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GLEnum.Repeat);
        Api.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GLEnum.Repeat);
        Api.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GLEnum.Nearest);
        Api.TexParameterI(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GLEnum.Nearest);
    }

    public void BlitFrom(ITexture texture)
    {
        if (texture is not OpenGlTexture glTexture)
            throw new ArgumentException("Texture must be of type OpenGlTexture", nameof(texture));

        Api.BindFramebuffer(FramebufferTarget.ReadFramebuffer, 0);
        Api.BindFramebuffer(FramebufferTarget.DrawFramebuffer, 0);

        Api.BlitFramebuffer(0, 0, glTexture.Size.X, glTexture.Size.Y, 0, 0, Size.X, Size.Y,
            ClearBufferMask.ColorBufferBit, BlitFramebufferFilter.Nearest);
    }

    public void BlitFrom(ITexture backingBackbufferTexture, object? renderFinishedSemaphore,
        object? blitSignalSemaphore)
    {
        BlitFrom(backingBackbufferTexture);
    }

    public void Bind()
    {
        Api.BindTexture(TextureTarget.Texture2D, TextureId);
    }

    public void Activate(int textureUnit)
    {
        Api.ActiveTexture(TextureUnit.Texture0 + textureUnit);
    }

    public void Dispose()
    {
        Api.DeleteTexture(TextureId);
    }

    public ValueTask DisposeAsync()
    {
        throw new NotImplementedException();
    }
}
