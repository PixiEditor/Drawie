namespace Drawie.RenderApi;

public interface IOpenGlContext
{
    public IntPtr GetGlInterface(string name);
    public bool IsGlViaAngle { get; }
}
