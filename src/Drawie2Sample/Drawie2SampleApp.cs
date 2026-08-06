using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Numerics;
using Drawie.Windowing;
using DrawiEngine;

namespace Drawie2Sample;

public class Drawie2SampleApp : DrawieApp
{
    private IWindow window;

    public override IWindow CreateMainWindow()
    {
        window = Engine.WindowingPlatform.CreateWindow("Drawie 2 Sample", new VecI(800, 600));
        return window;
    }

    protected override void OnInitialize()
    {
        window.Render += (targetTexture, deltaTime) =>
        {
            targetTexture.DrawRectangle(0, 0, targetTexture.Size.X, targetTexture.Size.Y,
                new ColorPaintable(Colors.Green));
        };
    }
}

