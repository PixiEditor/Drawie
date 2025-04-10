using Drawie.Numerics;

namespace Drawie.Backend.Core.ColorsImpl.Paintables;

public interface IStartEndPaintable
{
    public VecD Start { get; set; }
    public VecD End { get; set; }
}
