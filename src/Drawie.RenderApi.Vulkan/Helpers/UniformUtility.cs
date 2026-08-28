using System.Numerics;
using System.Runtime.InteropServices;
using Drawie.RenderApi.Abstraction.Shaders;
using Drawie.RenderApi.Vulkan.Buffers;
using Silk.NET.Vulkan;

namespace Drawie.RenderApi.Vulkan.Helpers;

public static class UniformUtility
{
    public static void SerializeToBuffer(UniformBlock block, UniformBuffer buffer)
    {
        var data = new byte[block.ShaderLayout.Size];
        foreach (var property in block.Properties)
        {
            var layout = block.ShaderLayout.UniformProperties
                .FirstOrDefault(x => x.Name == property.UniformName);

            Write(
                data,
                layout.Offset,
                property.ObjValue);
        }

        buffer.SetData(data);
    }

    private static void Write(
        byte[] destination,
        int offset,
        object value)
    {
        switch (value)
        {
            case float v:
                BitConverter.TryWriteBytes(
                    destination.AsSpan(offset, sizeof(float)),
                    v);
                break;
            case int v:
                BitConverter.TryWriteBytes(
                    destination.AsSpan(offset, sizeof(int)),
                    v);
                break;
            case uint v:
                BitConverter.TryWriteBytes(
                    destination.AsSpan(offset, sizeof(uint)),
                    v);
                break;
            case Vector2 v:
                MemoryMarshal.Write(
                    destination.AsSpan(offset),
                    in v);
                break;
            case Vector3 v:
                MemoryMarshal.Write(
                    destination.AsSpan(offset),
                    in v);
                break;
            case Vector4 v:
                MemoryMarshal.Write(
                    destination.AsSpan(offset),
                    in v);
                break;
            case Matrix4x4 v:
                MemoryMarshal.Write(
                    destination.AsSpan(offset),
                    in v);
                break;
            default:
                throw new NotSupportedException(
                    $"Cannot serialize uniform value of type {value.GetType()}.");
        }
    }
}