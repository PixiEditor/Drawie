using Drawie.Backend.Core.Bridge;
using Drawie.RenderApi;

namespace Drawie.Rendering;

/// <summary>
///     Represents a context for rendering operations, allowing modifications to textures and other graphics resources.
/// </summary>
public class RenderingContext : IDisposable
{
    public IGraphicsContext? GraphicsContext { get; }
    public bool IsOpen { get; private set; } = false;
    private List<TextureFramebuffer> ownedFramebuffers = new List<TextureFramebuffer>();

    public RenderingContext(IGraphicsContext? ctx)
    {
        GraphicsContext = ctx;
    }
    
    public TextureFramebuffer Edit(Texture texture)
    {
        if (!IsOpen) throw new InvalidOperationException("Rendering Context is not open.");
        var fbo = new TextureFramebuffer(texture);
        fbo.Open();
        ownedFramebuffers.Add(fbo);
        return ownedFramebuffers[^1];
    }

    public IDisposable Open()
    {
        if (IsOpen) throw new InvalidOperationException("Rendering Context is already open.");
        IsOpen = true;
        GraphicsContext?.MakeCurrent();
        return this;
    }

    public void Dispose()
    {
        foreach (var textureFramebuffer in ownedFramebuffers)
        {
            if (textureFramebuffer.IsOpen) throw new InvalidOperationException("Framebuffer is in use");
        }

        IsOpen = false;
        ownedFramebuffers.Clear();
    }
}