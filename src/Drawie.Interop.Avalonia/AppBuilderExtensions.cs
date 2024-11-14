using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.OpenGL;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using Drawie.Interop.Avalonia.Core;
using Drawie.Interop.Avalonia.Vulkan;
using Drawie.Interop.Avalonia.Vulkan.Vk;
using Drawie.RenderApi;
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
            Dispatcher.UIThread.Post(
                () =>
                {
                    ICompositionGpuInterop interop =
                        Compositor.TryGetDefaultCompositor().TryGetCompositionGpuInterop().Result;

                    var openglFeature = Compositor.TryGetDefaultCompositor()
                        .TryGetRenderInterfaceFeature(typeof(IOpenGlTextureSharingRenderInterfaceContextFeature))
                        .Result;

                    bool isOpenGl = openglFeature != null;

                    IRenderApi renderApi = null;
                    if (isOpenGl)
                    {
                        //renderApi = new OpenGLRenderApi();
                    }
                    else
                    {
                        AvaloniaInteropContextInfo contextInfo = new AvaloniaInteropContextInfo();

                        VulkanInteropContext context = new VulkanInteropContext(interop);
                        context.Initialize(contextInfo);

                        renderApi = new VulkanRenderApi(context);
                    }

                    SkiaDrawingBackend drawingBackend = new SkiaDrawingBackend();
                    DrawingEngine drawingEngine =
                        new DrawingEngine(renderApi, null, drawingBackend, new AvaloniaRenderingDispatcher());

                    DrawieInterop.VulkanInteropContext = context;

                    if (c.Instance.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    {
                        desktop.Exit += (sender, args) =>
                        {
                            var mainWindow = (sender as IClassicDesktopStyleApplicationLifetime).MainWindow;
                            if (!mainWindow.IsLoaded)
                            {
                                drawingEngine.Dispose();
                                context.Dispose();
                            }
                            else
                            {
                                mainWindow.Unloaded += (o, eventArgs) =>
                                {
                                    drawingEngine.Dispose();
                                    context.Dispose();
                                };
                            }
                        };
                    }

                    drawingEngine.Run();
                }, DispatcherPriority.Loaded);
        });

        return builder;
    }
}
