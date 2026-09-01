using Drawie.Backend.Core.Surfaces;

namespace Drawie.Backend.Arco;

public struct RecordedOperation
{
    public BlendMode BlendMode { get; set; }
    public RectDrawInstance RecordedInstance { get; set; }
}