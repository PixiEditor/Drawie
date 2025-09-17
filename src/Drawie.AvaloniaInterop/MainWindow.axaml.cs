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
using Drawie.Interop.Avalonia.Core;
using Drawie.Interop.Avalonia.Core.Controls;
using Drawie.Interop.Avalonia.Vulkan.Vk;
using Drawie.Numerics;
using Drawie.RenderApi;
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
        Texture skiaTexture = new Texture(new VecI(128));
        Texture skiaTexture2 = new Texture(new VecI(128));

        if (DrawingBackendApi.HasBackend)
        {
            DrawingBackendApi.Current.RenderingDispatcher.Enqueue(() =>
            {
                UpdateDraw(skiaTexture);
                UpdateDraw(skiaTexture2);
            });
        }

        DrawieControl.Texture = skiaTexture;
        DrawieControl2.Texture = skiaTexture2;
        base.OnLoaded(e);
    }

    private void UpdateDraw(Texture texture)
    {
        using Paint paint = new Paint();
        int time = Environment.TickCount;

        byte red = (byte)(Math.Sin(time / 1000.0) * 127 + 128);
        byte green = (byte)(Math.Sin(time / 1000.0 + 2) * 127 + 128);
        byte blue = (byte)(Math.Sin(time / 1000.0 + 4) * 127 + 128);

        texture?.DrawingSurface.Canvas.DrawRect(0, 0, 128, 128,
            new Paint() { Color = new Color(red, green, blue, 255), Style = PaintStyle.StrokeAndFill });

        // test transparency

        texture?.DrawingSurface.Canvas.DrawCircle(64, 64, 64,
            new Paint() { Color = new Color(255, 255, 255, 128), Style = PaintStyle.Fill });
        DrawieControl.QueueNextFrame();
        DrawingBackendApi.Current.RenderingDispatcher.Enqueue(() =>
        {
            UpdateDraw(texture);
        });
    }
}
