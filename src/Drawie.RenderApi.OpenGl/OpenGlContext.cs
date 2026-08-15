namespace Drawie.RenderApi.OpenGL;

public class OpenGlContext : IOpenGlContext
{
    private Func<string, IntPtr> getGlInterface;
    public bool IsGlViaAngle { get; }
    
    private Dictionary<ulong, IOpenGlTexture> Textures { get; } = new Dictionary<ulong, IOpenGlTexture>();
    
    public void AddManagedTexture(IOpenGlTexture texture)
    {
        Textures.Add(texture.TextureId, texture);
    }

    public OpenGlContext(Func<string, IntPtr> getGlInterface, bool isGlViaAngle)
    {
        this.getGlInterface = getGlInterface;
        IsGlViaAngle = isGlViaAngle;
    }

    IntPtr IOpenGlContext.GetGlInterface(string name)
    {
        return getGlInterface(name);
    }
}
