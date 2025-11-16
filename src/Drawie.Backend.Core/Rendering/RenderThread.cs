using System.Collections.Concurrent;
using Drawie.Backend.Core.Bridge;

namespace Drawie.Backend.Core.Rendering;

public sealed class RenderThread : IDisposable
{
    public double RefreshRate { get; set; } = 60.0;
    private readonly Thread _thread;
    private readonly CancellationTokenSource _cts = new();

    private double targetFrameMs => 1000.0 / RefreshRate;

    private readonly ConcurrentQueue<Action> _renderQueue = new();

    public RenderThread(double targetFps)
    {
        RefreshRate = targetFps;
        _thread = new Thread(Run) { IsBackground = true, Name = "Drawie Render Thread" };
    }


    public void Start()
    {
        _thread.Start();
    }


    public void Enqueue(Action renderAction)
    {
        if (renderAction == null) return;

        _renderQueue.Enqueue(renderAction);
    }

    private void Run()
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        while (!_cts.IsCancellationRequested)
        {
            var frameStart = sw.Elapsed.TotalMilliseconds;

            ProcessQueue();
            //DrawingBackendApi.Current.Flush();

            var elapsed = sw.Elapsed.TotalMilliseconds - frameStart;
            var wait = targetFrameMs - elapsed;
            if (wait > 1.0)
                Thread.Sleep((int)wait);
            else
                Thread.Yield();
        }
    }

    private void ProcessQueue()
    {
        var queueToProcess = new Queue<Action>(_renderQueue);
        _renderQueue.Clear();

        if (queueToProcess == null) return;

        while (queueToProcess.TryDequeue(out var render))
        {
            try
            {
                render();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Render action threw: {ex}");
            }
        }
    }

    /*private Queue<Action>? PopQueueToProcess(Priority priority)
    {
        if (_renderQueue.TryGetValue(priority, out var queue))
        {
            var toProcess = new Queue<Action>(queue);
            _renderQueue[priority].Clear();
            return toProcess;
        }

        return null;
    }*/

    public void Dispose()
    {
        _cts.Cancel();
        _thread.Join();
        _cts.Dispose();
    }

    public async Task WaitForIdleAsync()
    {
        var tcs = new TaskCompletionSource();
        Enqueue(() => tcs.SetResult());
        await tcs.Task;
    }
}
