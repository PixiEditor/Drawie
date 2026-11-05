using Drawie.RenderApi;
using Drawie.Rendering;

namespace Drawie.Host;

public interface ILayer
{
    public bool IsRenderApiSupported(IHostViewRenderApi api);
    void Initialize(IHost host);
}