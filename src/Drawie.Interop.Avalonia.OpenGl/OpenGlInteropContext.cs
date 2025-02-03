using Avalonia.OpenGL;
using Avalonia.Rendering.Composition;
using Drawie.Backend.Core.Debug;
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
        return Context.GlInterface.GetProcAddress(name);
    }

    public RenderApiResources CreateResources(CompositionDrawingSurface surface, ICompositionGpuInterop interop)
    {
        return new OpenGlRenderApiResources(surface, interop);
    }

    public GpuDiagnostics GetGpuDiagnostics()
    {
        Dictionary<string, string> details = new Dictionary<string, string>();
        details.Add("Extensions", string.Join(", ", Context.GlInterface.ContextInfo.Extensions));
        details.Add("Version", Context.GlInterface.ContextInfo.Version.ToString());

        return new GpuDiagnostics(
            true,
            new GpuInfo(Context.GlInterface.Renderer, Context.GlInterface.Vendor),
            $"OpenGL", details);
    }
}
