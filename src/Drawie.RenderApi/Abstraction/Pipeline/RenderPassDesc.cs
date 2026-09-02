namespace Drawie.RenderApi.Abstraction.Pipeline;

public struct RenderPassDesc
{
    public ColorLoadOp ColorLoadOp { get; set; } = ColorLoadOp.Clear;
    
    public RenderPassDesc()
    {
    }
}

public enum ColorLoadOp
{
    Clear,
    Load
}