using System.Collections.Concurrent;
using System.Diagnostics;
using Drawie.Backend.Core.Bridge;

namespace Drawie.Backend.Core.Rendering;

public sealed class RenderThread : IDisposable
{
    public double RefreshRate { get; set; } = 60.0;
    private readonly Thread _thread;
    private readonly CancellationTokenSource _cts = new();

    private double targetFrameMs => 1000.0 / RefreshRate;

    private readonly ConcurrentDictionary<Priority, ConcurrentQueue<Action>> _renderQueue = new();

    public RenderThread(double targetFps)
    {
        RefreshRate = targetFps;
        _thread = new Thread(Run) { IsBackground = true, Name = "Drawie Render Thread" };
    }


    public void Start()
    {
        _thread.Start();
    }


    public void Enqueue(Action renderAction, Priority priority)
    {
        if (renderAction == null) return;

        _renderQueue.GetOrAdd(priority, _ => new ConcurrentQueue<Action>()).Enqueue(renderAction);
    }

    private void Run()
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();

        while (!_cts.IsCancellationRequested)
        {
            var frameStart = sw.Elapsed.TotalMilliseconds;

            ProcessQueue(Priority.Render);
            DrawingBackendApi.Current.Flush();
            ProcessQueue(Priority.BackbufferUpdate);
            ProcessQueue(Priority.UI);

            var elapsed = sw.Elapsed.TotalMilliseconds - frameStart;
            var wait = targetFrameMs - elapsed;
            if (wait > 1.0)
                Thread.Sleep((int)wait);
            else
                Thread.Yield();
        }
    }

    private void ProcessQueue(Priority priority)
    {
        var queueToProcess = PopQueueToProcess(priority);

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

    private Queue<Action>? PopQueueToProcess(Priority priority)
    {
        if (_renderQueue.TryGetValue(priority, out var queue))
        {
            var toProcess = new Queue<Action>(queue);
            _renderQueue[priority].Clear();
            return toProcess;
        }

        return null;
    }

    public void Dispose()
    {
        _cts.Cancel();
        _thread.Join();
        _cts.Dispose();
    }
}
