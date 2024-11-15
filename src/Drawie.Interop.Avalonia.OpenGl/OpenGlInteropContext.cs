using Avalonia.OpenGL;
using Avalonia.Rendering.Composition;
using Drawie.Backend.Core.Exceptions;
using Drawie.Interop.Avalonia.Core;
using Drawie.RenderApi;

namespace Drawie.Interop.Avalonia.OpenGl;

public class OpenGlInteropContext : IOpenGlContext, IDrawieInteropContext
{
    public static OpenGlInteropContext? Current { get; private set; }

    public IGlContext Context { get; }

    public OpenGlInteropContext(IGlContext context)
    {
        Context = context;

        if (Current != null)
        {
            throw new InitializationDuplicateException("OpenGL context was already initialized.");
        }

        Current = this;
    }

    public IntPtr GetGlInterface(string name)
    {
        Context.MakeCurrent();
        return Context.GlInterface.GetProcAddress(name);
    }

    public RenderApiResources CreateResources(CompositionDrawingSurface surface, ICompositionGpuInterop interop)
    {
        return new OpenGlRenderApiResources(surface, interop);
    }

}
