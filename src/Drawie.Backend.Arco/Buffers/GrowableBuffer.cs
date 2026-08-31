using Drawie.RenderApi.Abstraction;
using Drawie.RenderApi.Abstraction.Buffers;

namespace Drawie.Backend.Arco.Buffers;

public class GrowableBuffer<T> where T : unmanaged
{
    public IBuffer<T> Buffer
    {
        get
        {
            EnsureCapacity(Count);
            return buffer;
        }
    }

    public int Count { get; private set; }
    public BufferUsage BufferUsage { get; }

    private T[] items;
    private IBuffer<T>? buffer;

    private IGraphicsDevice GraphicsDevice { get; }

    public GrowableBuffer(
        IGraphicsDevice graphicsDevice,
        BufferUsage bufferUsage = BufferUsage.Storage,
        int initialCapacity = 256)
    {
        GraphicsDevice = graphicsDevice;
        BufferUsage = bufferUsage;
        items = new T[initialCapacity];
    }

    public void SetData(T[] data)
    {
        EnsureCapacity(data.Length);

        data.CopyTo(items, 0);
        Count = data.Length;

        buffer.SetData(data);
    }

    private void EnsureCapacity(int required)
    {
        if (buffer == null)
        {
            buffer = GraphicsDevice.CreateBuffer(BufferUsage, items);
            return;
        }
        
        if (required <= items.Length)
            return;

        int newCapacity = Math.Max(items.Length * 2, required);

        Array.Resize(ref items, newCapacity);

        (buffer as IDisposable)?.Dispose(); // Possibly a bad idea, buffer might be in use
        buffer = GraphicsDevice.CreateBuffer(BufferUsage, items);
    }
}