namespace Drawie.RenderApi;

public interface IOpenGlContext
{
    public IntPtr GetGlInterface(string name);
    public bool IsGlViaAngle { get; }
    void AddManagedTexture(IOpenGlTexture texture);
    IOpenGlTexture? GetManagedTexture(ulong textureId);
    void RemoveManagedTexture(ulong textureId);
}
