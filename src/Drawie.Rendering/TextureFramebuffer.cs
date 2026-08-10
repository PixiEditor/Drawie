using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;
using Drawie.RenderApi.Abstraction.RenderTargets;
using Drawie.Rendering.Exceptions;

namespace Drawie.Rendering;

public class TextureFramebuffer : IDisposable, IRenderTarget
{
    internal Texture UnderlyingTexture { get; }
    public bool IsOpen { get; private set; }
    public VecI Size { get; }
    public Canvas? Canvas => IsOpen ? UnderlyingTexture?.NativeTexture?.DrawingSurface?.Canvas : null;


    internal TextureFramebuffer(Texture texture)
    {
        UnderlyingTexture = texture;
        Size = new VecI(texture.Width, texture.Height);
    }

    internal IDisposable Open()
    {
        IsOpen = true;
        return this;
    }

    public void Clear()
    {
        ThrowIfNotOpen();
        UnderlyingTexture.NativeTexture.DrawingSurface.Canvas.Clear();
    }
    
    public void Clear(Color color)
    {
        ThrowIfNotOpen();
        UnderlyingTexture.NativeTexture.DrawingSurface.Canvas.Clear(color);
    }
    
    public void DrawRectangle(float x, float y, float width, float height, Paintable paintable)
    {
        ThrowIfNotOpen();

        using var paint = new Paint() { Paintable = paintable };
        UnderlyingTexture.NativeTexture.DrawingSurface.Canvas.DrawRect(x, y, width, height, paint);
    }

    private void ThrowIfNotOpen()
    {
        if (!IsOpen)
        {
            throw new FramebufferNotOpenException("Cannot edit closed framebuffer");
        }
    }

    public void Dispose()
    {
        if (!IsOpen) return;
        
        IsOpen = false;
        UnderlyingTexture.NativeTexture.DrawingSurface.Flush();
    }

    uint IRenderTarget.FramebufferId => UnderlyingTexture.NativeTexture.FramebufferId;
}