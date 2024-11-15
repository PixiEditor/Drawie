using Avalonia.Controls;
using Avalonia.Interactivity;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Numerics;

namespace Drawie.AvaloniaGraphics;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        Texture texture = new Texture(new VecI(128, 128));
        using Paint paint = new Paint();
        paint.Color = Colors.Red;

        texture.DrawingSurface.Canvas.DrawRect(0, 0, 128, 128, paint);
        paint.Color = Colors.Blue;
        texture.DrawingSurface.Canvas.DrawCircle(64, 64, 64, paint);

        DrawieControl.Texture = texture;
        base.OnLoaded(e);
    }
}
