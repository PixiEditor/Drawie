using System.Diagnostics;
using Drawie.Backend.Core.Bridge;

namespace Drawie.Backend.Core;

public class RenderThread
{
    private Thread renderThread;
    private Stopwatch renderTimer;
    private bool running;

    private Queue<Action> renderQueue = new();
    private readonly object renderQueueLock = new();

    public RenderThread()
    {
        renderTimer = new Stopwatch();
        renderThread = new Thread(RenderLoop) { IsBackground = true, Name = "Drawie Render Thread" };
    }

    public void RenderLoop()
    {
        renderTimer = Stopwatch.StartNew();
        while (running)
        {
            var frameStart = renderTimer.ElapsedMilliseconds;
            DispatchRenders();

            var elapsed = renderTimer.ElapsedMilliseconds - frameStart;
            var sleep = TimeSpan.FromMilliseconds(16 - elapsed);
            if (sleep > TimeSpan.Zero)
            {
                Thread.Sleep(sleep);
            }
        }
    }

    public void Start()
    {
        if (running)
            return;

        running = true;
        renderThread.Start();
    }

    private void DispatchRenders()
    {
        using var ctx = DrawingBackendApi.Current.RenderingDispatcher.EnsureContext();
        Queue<Action> toRender;
        lock (renderQueueLock)
        {
            toRender = new Queue<Action>(renderQueue);
            renderQueue.Clear();
        }

        while (toRender.Count > 0)
        {
            var action = toRender.Dequeue();
            try
            {
                action();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine($"Exception during render action: {e}");
            }
        }

        DrawingBackendApi.Current.Flush();
    }

    public void QueueRender(Action renderAction)
    {
        lock (renderQueueLock)
        {
            renderQueue.Enqueue(renderAction);
        }
    }
}
