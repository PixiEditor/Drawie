using Drawie.Backend.Vertie.Core;

namespace Drawie.RenderApi.Abstraction.Pipeline;

public record struct RasterizerDesc
{
    public RenderMode RenderMode { get; set; }
    public int Samples { get; set; }
    public CullMode CullMode { get; set; }

    public RasterizerDesc()
    {
        CullMode = CullMode.None;
        RenderMode = RenderMode.Default;
        Samples = 1;
    }
}