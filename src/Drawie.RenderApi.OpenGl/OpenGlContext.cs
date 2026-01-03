namespace Drawie.RenderApi.OpenGL;

public class OpenGlContext : IOpenGlContext
{
    private Func<string, IntPtr> getGlInterface;
    public bool IsGlViaAngle { get; }

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
