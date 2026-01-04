using Avalonia.Threading;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;

namespace Drawie.Interop.Avalonia.Core;

public class AvaloniaRenderingDispatcher : IRenderingDispatcher
{
    public Action<Action> Invoke { get; } = action =>
    {
        if (action == null) return;

        if(Dispatcher.UIThread.CheckAccess())
        {
            using var _ = IDrawieInteropContext.Current.EnsureContext();
            action();
            return;
        }

        Dispatcher.UIThread.Invoke(() =>
        {
            using var _ = IDrawieInteropContext.Current.EnsureContext();
            action();
        });
    };

    public async Task<TResult> InvokeAsync<TResult>(Func<TResult> func)
    {
        return await Dispatcher.UIThread.InvokeAsync(() =>
        {
            using var _ = IDrawieInteropContext.Current.EnsureContext();
            return func();
        });
    }

    public async Task<TResult> InvokeInBackgroundAsync<TResult>(Func<TResult> function)
    {
        return await Dispatcher.UIThread.InvokeAsync(() =>
        {
            using var _ = IDrawieInteropContext.Current.EnsureContext();
            return function();
        }, DispatcherPriority.Background);
    }

    public async Task InvokeInBackgroundAsync(Action function)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            using var _ = IDrawieInteropContext.Current.EnsureContext();
            function();
        }, DispatcherPriority.Background);
    }

    public IDisposable EnsureContext()
    {
        return IDrawieInteropContext.Current.EnsureContext();
    }
}
