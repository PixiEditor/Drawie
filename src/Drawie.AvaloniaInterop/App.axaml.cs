using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
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

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            ICompositionGpuInterop interop = GetInterop();
            VulkanInteropContext context = new VulkanInteropContext(interop);
            
            AvaloniaInteropContextInfo contextInfo = new AvaloniaInteropContextInfo();
            
            context.Initialize(contextInfo);
            
            VulkanRenderApi renderApi = new VulkanRenderApi(context);
            SkiaDrawingBackend drawingBackend = new SkiaDrawingBackend();
            DrawingEngine drawingEngine = new DrawingEngine(renderApi, null, drawingBackend);

            drawingEngine.Run();
            
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private ICompositionGpuInterop GetInterop()
    {
        return Compositor.TryGetDefaultCompositor().TryGetCompositionGpuInterop().Result;
    }
}