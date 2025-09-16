using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.OpenGL;
using Avalonia.Rendering.Composition;
using Avalonia.Threading;
using Drawie.Interop.Avalonia.Core;
using Drawie.Interop.Avalonia.OpenGl;
using Drawie.Interop.Avalonia.Vulkan;
using Drawie.Interop.Avalonia.Vulkan.Vk;
using Drawie.RenderApi;
using Drawie.RenderApi.OpenGL;
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

                    Compositor compositor = Compositor.TryGetDefaultCompositor();

                    var openglFeature = compositor
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
                        if (interop == null)
                        {
                            string configPath = Path.Combine(
                                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                "PixiEditor",
                                "render_api.config");

                            Console.WriteLine(
                                $"Vulkan support was reported as available, but no interop was found. Pass --opengl command line argument to use OpenGL instead or enter \"OpenGL\" inside a file: '{configPath}'.");
                            throw new InvalidOperationException(
                                "Vulkan support was reported as available, but no interop was found. Pass --opengl command line argument to use OpenGL instead or enter \"OpenGL\" inside a file: '" +
                                configPath + "'.");
                        }

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
                        new DrawingEngine(renderApi, null, drawingBackend, new DrawieRenderingDispatcher(compositor.RequestCompositionUpdate, a => Dispatcher.UIThread.Post(a, DispatcherPriority.Render)));

                    // It's very likely that this is not needed and may cause issues when reopening main window without
                    // proper reinitialization.
                    // But leaving in case it is needed for some reason
                    /*if (c.Instance.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
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
                    }*/

                    drawingEngine.Run();

                    Console.WriteLine("\t- Using GPU: " +
                                      IDrawieInteropContext.Current.GetGpuDiagnostics().ActiveGpuInfo);
                    ctxDisposablePostRun?.Dispose();
                }, DispatcherPriority.Loaded);
        });

        return builder;
    }
}
