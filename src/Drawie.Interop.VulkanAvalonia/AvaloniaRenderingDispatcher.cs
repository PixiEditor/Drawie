using Avalonia.Threading;
using Drawie.Backend.Core;

namespace Drawie.Interop.VulkanAvalonia;

public class AvaloniaRenderingDispatcher : IRenderingDispatcher
{
    public Action<Action> Invoke { get; } = action => Dispatcher.UIThread.Invoke(action);
}
