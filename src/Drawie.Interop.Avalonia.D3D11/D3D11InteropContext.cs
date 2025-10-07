using Avalonia.Rendering.Composition;
using Drawie.Backend.Core.Debug;
using Drawie.Backend.Core.Utils;
using Drawie.Interop.Avalonia.Core;
using Drawie.RenderApi;

namespace Drawie.Interop.Avalonia.D3D11;

public class D3D11InteropContext : IDrawieInteropContext, IDirectXContext
{
    public IntPtr Adapter { get; }
    public IntPtr Device { get; }
    public IntPtr Queue { get; }

    public D3D11InteropContext(IntPtr adapter, IntPtr device, IntPtr queue)
    {
        Adapter = adapter;
        Device = device;
        Queue = queue;
    }

    public RenderApiResources CreateResources(CompositionDrawingSurface surface, ICompositionGpuInterop interop)
    {
        return new D3D11RenderApiResources(surface, interop);
    }

    public GpuDiagnostics GetGpuDiagnostics()
    {
        return new GpuDiagnostics(false, null, "D3D11", new Dictionary<string, string>());
    }

    public IDisposable EnsureContext()
    {
        return Disposable.Empty;
    }
}
