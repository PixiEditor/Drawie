using Drawie.Numerics;
using Drawie.RenderApi;

namespace Drawie.Interop.Avalonia.Core;

public struct Frame
{
    public ITexture? Texture { get; set; }
    public Action<VecI>? PresentFrame { get; set; }
    public IDisposable? ReturnFrame { get; set; }
    public VecI Size { get; set; }
}
