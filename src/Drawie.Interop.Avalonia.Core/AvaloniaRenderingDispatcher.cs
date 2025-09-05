using Avalonia.Threading;
using Drawie.Backend.Core;

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

    public async Task<TResult> InvokeAsync<TResult>(Func<TResult> function)
    {
        return await Dispatcher.UIThread.InvokeAsync(function, DispatcherPriority.Background);
    }

    public async Task InvokeAsync(Action function)
    {
        await Dispatcher.UIThread.InvokeAsync(function, DispatcherPriority.Background);
    }

    public IDisposable EnsureContext()
    {
        return IDrawieInteropContext.Current.EnsureContext();
    }
}
