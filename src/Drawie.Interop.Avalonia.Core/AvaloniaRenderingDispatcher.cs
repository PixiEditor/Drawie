﻿using Avalonia.Threading;
using Drawie.Backend.Core;

namespace Drawie.Interop.Avalonia.Core;

public class AvaloniaRenderingDispatcher : IRenderingDispatcher
{
    public Action<Action> Invoke { get; } = action =>
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            using var _ = IDrawieInteropContext.Current.EnsureContext();
            action();
        });
    };

    public IDisposable EnsureContext()
    {
        return IDrawieInteropContext.Current.EnsureContext();
    }
}
