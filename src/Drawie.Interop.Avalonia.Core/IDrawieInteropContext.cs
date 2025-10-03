using Avalonia.Rendering.Composition;
using Drawie.Backend.Core.Debug;

namespace Drawie.Interop.Avalonia.Core;

public interface IDrawieInteropContext
{
    public static IDrawieInteropContext Current { get; private set; }
    public RenderApiResources CreateResources(InteropData data);

    public static void SetCurrent(IDrawieInteropContext context)
    {
        if (Current != null)
        {
            throw new System.InvalidOperationException("Context already set");
        }
        
        Current = context;
    }

    public GpuDiagnostics GetGpuDiagnostics();
    public IDisposable EnsureContext();
}
