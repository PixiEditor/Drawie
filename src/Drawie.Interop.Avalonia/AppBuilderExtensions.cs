using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.OpenGL;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using Avalonia.Vulkan;
using Drawie.Interop.Avalonia.Core;
using Drawie.Interop.Avalonia.OpenGl;
using Drawie.Interop.Avalonia.Vulkan;
using Drawie.Interop.Avalonia.Vulkan.Vk;
using Drawie.RenderApi;
using Drawie.RenderApi.OpenGL;
using Drawie.RenderApi.Vulkan;
using Drawie.Skia;
using DrawiEngine;
using Silk.NET.Core.Contexts;
using Silk.NET.OpenGL;

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

                    IOpenGlTextureSharingRenderInterfaceContextFeature? sharingFeature = null;
                    bool isOpenGl = openglFeature is IOpenGlTextureSharingRenderInterfaceContextFeature;
                    sharingFeature = openglFeature as IOpenGlTextureSharingRenderInterfaceContextFeature;

                    IRenderApi renderApi = null;
                    IDisposable? disposableContext = null;
                    IDisposable? ctxDisposablePostRun = null;
                    if (isOpenGl)
                    {
                        var ctx = sharingFeature!.CreateSharedContext();
                        OpenGlInteropContext context = new OpenGlInteropContext(ctx);
                        ctxDisposablePostRun = ctx.MakeCurrent();

                        renderApi = new OpenGlRenderApi(context);

                        IDrawieInteropContext.SetCurrent(context);
                    }
                    else
                    {
                        AvaloniaInteropContextInfo contextInfo = new AvaloniaInteropContextInfo();

                        VulkanInteropContext context = new VulkanInteropContext(interop);
                        context.Initialize(contextInfo);

                        renderApi = new VulkanRenderApi(context);
                        DrawieInterop.VulkanInteropContext = context;
                        IDrawieInteropContext.SetCurrent(context);
                        disposableContext = context;
                    }

                    SkiaDrawingBackend drawingBackend = new SkiaDrawingBackend();
                    DrawingEngine drawingEngine =
                        new DrawingEngine(renderApi, null, drawingBackend, new AvaloniaRenderingDispatcher());

                    if (c.Instance.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    {
                        desktop.Exit += (sender, args) =>
                        {
                            var mainWindow = (sender as IClassicDesktopStyleApplicationLifetime).MainWindow;
                            if (!mainWindow.IsLoaded)
                            {
                                drawingEngine.Dispose();
                                disposableContext?.Dispose();
                            }
                            else
                            {
                                mainWindow.Unloaded += (o, eventArgs) =>
                                {
                                    drawingEngine.Dispose();
                                    disposableContext?.Dispose();
                                };
                            }
                        };
                    }

                    drawingEngine.Run();

                    Console.WriteLine("\t- Using GPU: " +
                                      IDrawieInteropContext.Current.GetGpuDiagnostics().ActiveGpuInfo);
                    ctxDisposablePostRun?.Dispose();
                }, DispatcherPriority.Loaded);
        });

        return builder;
    }
}
