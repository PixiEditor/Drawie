namespace Drawie.Interop.Avalonia.Core.Utils;

static class Disposable
{
    internal sealed class AnonymousDisposable : IDisposable
    {
        private volatile Action? _dispose;
        public AnonymousDisposable(Action dispose)
        {
            _dispose = dispose;
        }
        public bool IsDisposed => _dispose == null;
        public void Dispose()
        {
            Interlocked.Exchange(ref _dispose, null)?.Invoke();
        }
    }

    public static IDisposable Create(Action dispose)
    {
        if (dispose == null)
        {
            throw new ArgumentNullException(nameof(dispose));
        }

        return new AnonymousDisposable(dispose);
    }
}
