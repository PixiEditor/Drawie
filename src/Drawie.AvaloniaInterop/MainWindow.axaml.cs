using System;
using System.Collections.Generic;
using System.Timers;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Drawie.Backend.Core;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Interop.Avalonia.Core.Controls;
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
        Timer timer = new Timer(new TimeSpan(0, 0, 0, 0, 1));
        int i = 0;
        Texture texture = new Texture(new VecI(512, 512));
        texture.DrawingSurface.Canvas.Clear(Colors.Green);
        texture.DrawingSurface.Canvas.DrawRect(0, 0, 512, 512, new Paint(){Color = Colors.Red});
        timer.Elapsed += (s, e) =>
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                if (i % 50 == 0)
                {
                    Items.Items.Clear();
                }

                Items.Items.Add(new DrawieTextureControl() {  Width = 60, Height = 60, Texture = texture });
            });
            i++;
        };
        timer.Start();
        base.OnLoaded(e);
    }
}