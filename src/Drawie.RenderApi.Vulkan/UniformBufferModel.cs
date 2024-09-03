using Silk.NET.Maths;

namespace Drawie.RenderApi.Vulkan;

public struct UniformBufferModel
{
    public Matrix4X4<float> model;
    public Matrix4X4<float> view;
    public Matrix4X4<float> proj;
}