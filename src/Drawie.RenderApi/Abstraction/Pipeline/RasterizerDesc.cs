using Drawie.Backend.Vertie.Core;

namespace Drawie.RenderApi.Abstraction.Pipeline;

public record struct RasterizerDesc
{
    public RenderMode RenderMode { get; set; }
}