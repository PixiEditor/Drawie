using Drawie.RenderApi;

namespace Drawie.Interop.Avalonia.Core;

public class PendingFrame
{
    public FrameHandle Handle { get; set; }
    public IExportableTexture NativeTexture { get; set; }
    public ISemaphorePair SemaphorePair { get; set; }
}
