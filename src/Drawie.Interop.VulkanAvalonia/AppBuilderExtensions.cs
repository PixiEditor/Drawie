using Avalonia;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using Drawie.Interop.VulkanAvalonia.Vulkan;
using Drawie.RenderApi.Vulkan;
using Drawie.Skia;
using DrawiEngine;

namespace Drawie.Interop.VulkanAvalonia;

public static class AppBuilderExtensions
{
    public static AppBuilder WithDrawie(this AppBuilder builder)
    {
        builder.AfterSetup(c =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                ICompositionGpuInterop interop =
                    Compositor.TryGetDefaultCompositor().TryGetCompositionGpuInterop().Result;

                VulkanInteropContext context = new VulkanInteropContext(interop);

                AvaloniaInteropContextInfo contextInfo = new AvaloniaInteropContextInfo();

                context.Initialize(contextInfo);

                VulkanRenderApi renderApi = new VulkanRenderApi(context);
                SkiaDrawingBackend drawingBackend = new SkiaDrawingBackend();
                DrawingEngine drawingEngine = new DrawingEngine(renderApi, null, drawingBackend);

                DrawieInterop.VulkanInteropContext = context;

                drawingEngine.Run();
            }, DispatcherPriority.Loaded);
        });

        return builder;
    }
}