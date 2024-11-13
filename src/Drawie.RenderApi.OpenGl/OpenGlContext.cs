namespace Drawie.RenderApi.OpenGL;

public class OpenGlContext : IOpenGlContext
{
    private Func<string, IntPtr> getGlInterface;

    public OpenGlContext(Func<string, IntPtr> getGlInterface)
    {
        this.getGlInterface = getGlInterface;
    }

    IntPtr IOpenGlContext.GetGlInterface(string name)
    {
        return getGlInterface(name);
    }
}
