using Drawie.Backend.Arco.RenderingOps;
using Drawie.Backend.Core.Surfaces;

namespace Drawie.Backend.Arco;

public struct RecordedOperation
{
    public RenderOpType RenderOp { get; set; }
    public BlendMode BlendMode { get; set; }
    public RectDrawInstance RecordedInstance { get; set; }
}