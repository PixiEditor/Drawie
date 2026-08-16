using Drawie.Backend.Vertie.Core;

namespace Drawie.RenderApi.Abstraction.Pipeline;

public record struct RasterizerDesc
{
    public RenderMode RenderMode { get; set; }
    public int Samples { get; set; }

    public RasterizerDesc()
    {
        RenderMode = RenderMode.Default;
        Samples = 1;
    }
}