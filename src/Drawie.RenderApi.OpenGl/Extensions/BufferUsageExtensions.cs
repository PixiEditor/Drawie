using Drawie.RenderApi.Abstraction.Buffers;
using Silk.NET.OpenGL;

namespace Drawie.RenderApi.OpenGL.Extensions;

public static class BufferUsageExtensions
{
    public static BufferTargetARB ToOpenGlTargetARB(this BufferUsage usage)
    {
        return usage switch
        {
            BufferUsage.Vertex => BufferTargetARB.ArrayBuffer,
            BufferUsage.Index => BufferTargetARB.ElementArrayBuffer,
            BufferUsage.Uniform => BufferTargetARB.UniformBuffer,
            BufferUsage.Storage => BufferTargetARB.ShaderStorageBuffer,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}