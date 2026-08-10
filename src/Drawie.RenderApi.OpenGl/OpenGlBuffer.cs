using Drawie.RenderApi.Abstraction;
using Drawie.RenderApi.Abstraction.Buffers;
using Drawie.RenderApi.OpenGL.Extensions;
using Silk.NET.OpenGL;

namespace Drawie.RenderApi.OpenGL;

public class OpenGlBuffer<TData> : IBuffer where TData : unmanaged
{
    public int Handle { get; }
    public uint NativeHandle => openglHandle;
    public BufferUsage Usage { get; }
    public uint Size { get; }

    private uint openglHandle;
    private GL api;

    public OpenGlBuffer(GL api, int handle, BufferUsage usage, TData[]? data = null)
    {
        Handle = handle;
        Usage = usage;
        this.api = api;
        openglHandle = api.GenBuffer();
        if (data != null)
        {
            unsafe
            {
                Size = (uint)data.Length;
                fixed (void* d = data)
                {
                    BufferTargetARB bufferType = ToBufferType();
                    api.BindBuffer(bufferType, openglHandle);
                    api.BufferData(bufferType, (nuint)(Size * (uint)sizeof(TData)), d, BufferUsageARB.StaticDraw);
                }
            }
        }
        
    }

    public void Dispose()
    {
        api.DeleteBuffer(openglHandle);
    }

    private BufferTargetARB ToBufferType()
    {
        return Usage.ToOpenGlTargetARB();
    }
}