namespace Drawie.Backend.Core.Utils;

public class Disposable : IDisposable
{
    public static readonly Disposable Empty = new(() => { });
    public Action OnDispose { get; }

    public Disposable(Action onDispose)
    {
        OnDispose = onDispose;
    }

    public void Dispose()
    {
        OnDispose();
    }

    public static IDisposable Create(Action action)
    {
        return new Disposable(action);
    }
}
