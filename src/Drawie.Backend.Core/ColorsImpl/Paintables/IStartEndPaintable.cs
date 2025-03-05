using Drawie.Numerics;

namespace Drawie.Backend.Core.ColorsImpl.Paintables;

public interface IStartEndPaintable
{
    public void UpdateWithStartEnd(VecD start, VecD end);
}
