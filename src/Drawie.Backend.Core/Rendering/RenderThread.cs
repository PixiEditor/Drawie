using System.Collections.Concurrent;
using System.Diagnostics;
using Drawie.Backend.Core.Bridge;

namespace Drawie.Backend.Core.Rendering;

public sealed class RenderThread : IDisposable
{
    private readonly Thread _thread;
    private readonly CancellationTokenSource _cts = new();


    private readonly ConcurrentQueue<Action> _renderQueue = new();


    private readonly ConcurrentDictionary<object, Action> _uiQueue = new();
    private ConcurrentQueue<Action> swapQueue = new();


    private readonly Action<Action> _uiPost;
    private readonly Action<Action> _requestCompositionUpdate;


    public RenderThread(Action<Action> requestCompositionUpdate, Action<Action> uiPost)
    {
        _requestCompositionUpdate = requestCompositionUpdate ??
                                    throw new ArgumentNullException(nameof(requestCompositionUpdate));
        _uiPost = uiPost ?? throw new ArgumentNullException(nameof(uiPost));
        _thread = new Thread(Run) { IsBackground = true, Name = "Drawie Render Thread" };
    }


    public void Start()
    {
        _thread.Start();
    }


    public void EnqueueRender(Action renderAction)
    {
        if (renderAction == null) return;
        _renderQueue.Enqueue(renderAction);
    }


    public void EnqueueUiPresent(object sender, Action uiPresentAction, Action swapAction)
    {
        if (uiPresentAction == null) return;
        _uiQueue[sender] = uiPresentAction;
        swapQueue.Enqueue(swapAction);

        _uiPost(() => _requestCompositionUpdate(ProcessUiQueue));
    }


    private void ProcessUiQueue()
    {
        var queue = new Queue<Action>(_uiQueue.Values);

        while (queue.TryDequeue(out var action))
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UI present error: {ex}");
            }
        }
    }


    private void Run()
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        const double targetFrameMs = 1000.0 / 60.0;


        while (!_cts.IsCancellationRequested)
        {
            var frameStart = sw.Elapsed.TotalMilliseconds;

            var queueToProcess = new Queue<Action>(_renderQueue);
            _renderQueue.Clear();

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

            var swapToProcess = new Queue<Action>(swapQueue);
            swapQueue.Clear();
            while (swapToProcess.TryDequeue(out var swap))
            {
                try
                {
                    swap();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Swap action threw: {ex}");
                }
            }

            //DrawingBackendApi.Current.Flush();

            var elapsed = sw.Elapsed.TotalMilliseconds - frameStart;
            var wait = targetFrameMs - elapsed;
            if (wait > 1.0)
                Thread.Sleep((int)wait);
            else
                Thread.Yield();
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _thread.Join();
        _cts.Dispose();
    }
}
