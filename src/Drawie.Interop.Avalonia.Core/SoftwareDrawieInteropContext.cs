using Avalonia.Rendering.Composition;
using Drawie.Backend.Core.Debug;
using Drawie.Backend.Core.Utils;
using Drawie.RenderApi;

namespace Drawie.Interop.Avalonia.Core;

public class SoftwareDrawieInteropContext : IDrawieInteropContext
{
    public RenderApiResources CreateResources(InteropData data)
    {
        return new SoftwareRenderApiResources(data);
    }

    public GpuDiagnostics GetGpuDiagnostics()
    {
        return new GpuDiagnostics(
            false,
            new GpuInfo("Software Rendering", "Unknown"),
            "Software Rendering",
            new Dictionary<string, string>());
    }

    public IDisposable EnsureContext()
    {
        return Disposable.Empty;
    }
}
