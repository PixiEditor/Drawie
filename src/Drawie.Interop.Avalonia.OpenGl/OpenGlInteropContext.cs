using Avalonia.OpenGL;
using Avalonia.Rendering.Composition;
using Drawie.Backend.Core.Debug;
using Drawie.Backend.Core.Exceptions;
using Drawie.Interop.Avalonia.Core;
using Drawie.Numerics;
using Drawie.RenderApi;
using Drawie.RenderApi.OpenGL;
using Silk.NET.OpenGL;

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
        details.Add("Version", Context.GlInterface.ContextInfo.Version.ToString());
        details.Add("Extensions", string.Join(", ", Context.GlInterface.ContextInfo.Extensions));

        return new GpuDiagnostics(
            true,
            new GpuInfo(Context.GlInterface.Renderer, Context.GlInterface.Vendor),
            $"OpenGL", details);
    }

    public IDisposable EnsureContext()
    {
        return Context.MakeCurrent();
    }

    public ITexture CreateTexture(VecI vecI)
    {
        return new OpenGlTexture(GL.GetApi(Context.GlInterface.GetProcAddress), vecI.X, vecI.Y);
    }
}
