using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.OpenGL;
using Avalonia.OpenGL.Egl;
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
    public static AppBuilder WithDrawie(this AppBuilder builder, Action gpuTimeoutFallback)
    {
        builder.AfterSetup(_ =>
        {
            Dispatcher.UIThread.InvokeAsync(
                async () => await InitializeDrawie(gpuTimeoutFallback),
                DispatcherPriority.Loaded);
        });

        return builder;
    }

    private static async Task InitializeDrawie(Action gpuFailedCallback)
    {
        try
        {
            Console.WriteLine("Initializing Drawie...");

            var compositor = Compositor.TryGetDefaultCompositor();

            if (compositor == null)
            {
                Console.WriteLine("No compositor available.");
                gpuFailedCallback();
                return;
            }

            var interopTask = compositor.TryGetCompositionGpuInterop().AsTask();

            var completedTask = await Task.WhenAny(interopTask, Task.Delay(TimeSpan.FromSeconds(10)));

            if (completedTask != interopTask)
            {
                Console.WriteLine("Timed out waiting for GPU interop.");
                gpuFailedCallback();
                return;
            }

            var interop = await (completedTask as Task<ICompositionGpuInterop>);

            var openglFeature = await compositor.TryGetRenderInterfaceFeature(
                typeof(IOpenGlTextureSharingRenderInterfaceContextFeature));

            bool isOpenGl = openglFeature is IOpenGlTextureSharingRenderInterfaceContextFeature;

            IRenderApi renderApi;
            IDisposable? disposableContext = null;
            IDisposable? ctxDisposablePostRun = null;

            if (isOpenGl)
            {
                var sharingFeature = (IOpenGlTextureSharingRenderInterfaceContextFeature)openglFeature!;

                var ctx = sharingFeature.CreateSharedContext();
                var context = new OpenGlInteropContext(ctx, ctx is EglContext);

                ctxDisposablePostRun = ctx.MakeCurrent();

                renderApi = new OpenGlRenderApi(context);
                IDrawieInteropContext.SetCurrent(context);
            }
            else
            {
                if (interop == null)
                {
                    throw new InvalidOperationException("No composition GPU interop available.");
                }

                var contextInfo = new AvaloniaInteropContextInfo();

                Console.WriteLine("Initializing Vulkan context...");

                var context = new VulkanInteropContext(interop);
                context.Initialize(contextInfo);

                renderApi = new VulkanRenderApi(context);
                DrawieInterop.VulkanInteropContext = context;
                IDrawieInteropContext.SetCurrent(context);

                disposableContext = context;
            }

            var drawingBackend = new SkiaDrawingBackend();

            var drawingEngine = new DrawingEngine(
                renderApi,
                null,
                drawingBackend,
                new AvaloniaRenderingDispatcher());

            Console.WriteLine("Starting Drawie...");

            drawingEngine.Run();

            Console.WriteLine(
                "\t- Using GPU: " +
                IDrawieInteropContext.Current.GetGpuDiagnostics().ActiveGpuInfo);

            ctxDisposablePostRun?.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Drawie initialization failed: {ex}");
            gpuFailedCallback();
        }
    }
}
