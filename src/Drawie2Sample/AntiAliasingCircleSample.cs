using Drawie.Backend.Arco;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Numerics;
using Drawie.Rendering;

namespace Drawie2Sample;

public class AntiAliasingCircleSample : ArcoSample
{
    public AntiAliasingCircleSample(ArcoGraphicsContext context, VecI size) : base(context, size)
    {
    }

    public override void OnInit()
    {
        RenderSurface.Canvas.DrawCircle(100, 100, 50, new Paint()
        {
            Color = Colors.Green,
            IsAntiAliased = true
        });
    }
}