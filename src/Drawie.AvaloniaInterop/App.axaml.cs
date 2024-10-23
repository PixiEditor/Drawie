using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Rendering.Composition;
using Drawie.AvaloniaGraphics.Interop;
using Drawie.RenderApi.Vulkan;
using Drawie.Skia;
using DrawiEngine;

namespace Drawie.AvaloniaGraphics;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    internal static VulkanInteropContext InteropContext { get; set; }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void MainWindowOnLoaded(object? sender, EventArgs eventArgs)
    {
        ICompositionGpuInterop interop = GetInterop();
       
    }

    private ICompositionGpuInterop GetInterop()
    {
        return Compositor.TryGetDefaultCompositor().TryGetCompositionGpuInterop().Result;
    }
}