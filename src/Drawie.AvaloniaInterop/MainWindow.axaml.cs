using System;
using System.Collections.Generic;
using System.Timers;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Interop.Avalonia.Core.Controls;
using Drawie.Numerics;
using Color = Drawie.Backend.Core.ColorsImpl.Color;
using Colors = Drawie.Backend.Core.ColorsImpl.Colors;

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

        var control = DrawieControl;
        DrawingBackendApi.Current.RenderingDispatcher.Enqueue(() =>
        {
            using Paint paint = new Paint();
            paint.Color = Colors.Red;

            texture.DrawingSurface.Canvas.DrawRect(0, 0, 128, 128, paint);
            paint.Color = Colors.Blue;
            texture.DrawingSurface.Canvas.DrawCircle(64, 64, 64, paint);
            DrawingBackendApi.Current.RenderingDispatcher.Enqueue(() =>
            {
                control.UpdateTexture();
            }, Priority.UI);
        });

        DrawieControl.Texture = texture;
        base.OnLoaded(e);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        /*int time = Environment.TickCount;

        byte red = (byte)(Math.Sin(time / 1000.0) * 127 + 128);
        byte green = (byte)(Math.Sin(time / 1000.0 + 2) * 127 + 128);
        byte blue = (byte)(Math.Sin(time / 1000.0 + 4) * 127 + 128);

        if (DrawingBackendApi.HasBackend)
        {
            var texture = DrawieControl.Texture;
            var control = DrawieControl;
            DrawingBackendApi.Current.RenderingDispatcher.Enqueue(() =>
            {
                texture?.DrawingSurface.Canvas.DrawRect(0, 0, 128, 128,
                    new Paint() { Color = new Color(red, green, blue, 255), Style = PaintStyle.StrokeAndFill });

                // test transparency

                texture?.DrawingSurface.Canvas.DrawCircle(64, 64, 64,
                    new Paint() { Color = new Color(255, 255, 255, 128), Style = PaintStyle.Fill });

            });
        }*/

        Dispatcher.UIThread.Post(InvalidateVisual);

        DrawieControl.QueueNextFrame();
    }
}
