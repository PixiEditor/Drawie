using Drawie.Numerics;

namespace Drawie.Backend.Core;

public interface IDrawieControl
{
    public bool NeedsRedraw { get; }
    public void BeginDraw(VecI size);

}
