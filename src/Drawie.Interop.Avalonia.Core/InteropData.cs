using Avalonia.Media.Imaging;
using Avalonia.Rendering.Composition;

namespace Drawie.Interop.Avalonia.Core;

public class InteropData
{
    public CompositionDrawingSurface? Surface { get; set; }
    public ICompositionGpuInterop? GpuInterop { get; set; }

    public InteropData(CompositionDrawingSurface surface, ICompositionGpuInterop gpuInterop)
    {
        Surface = surface;
        GpuInterop = gpuInterop;
    }

    public InteropData()
    {
        Surface = null;
        GpuInterop = null;
    }
}
