using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Vertie.Core;
using Drawie.Backend.Vertie.Rendering;
using Drawie.Numerics;
using Drawie.RenderApi;
using Drawie.RenderApi.Abstraction;
using Drawie.RenderApi.Abstraction.Buffers;
using Drawie.RenderApi.Abstraction.Pipeline;
using Drawie.RenderApi.Abstraction.Shaders;
using Drawie.Rendering;

namespace Drawie.Backend.Vertie.Helpers;

public static class TextureFramebufferExtensions
{
    private static Dictionary<Mesh, (IBuffer, IBuffer)> cachedMeshBuffers = new Dictionary<Mesh, (IBuffer, IBuffer)>();
    private static Dictionary<CompiledShader, IShaderProgram> cachedShaderPrograms = new Dictionary<CompiledShader, IShaderProgram>();
    public static void DrawMesh(this TextureFramebuffer fb, Mesh mesh, Material material)
    {
        IGraphicsDevice device = DrawingBackendApi.Current.ActiveRenderApi.GraphicsDevice;
        var pipeline = device.CreatePipeline(new PipelineDesc()
        {
            Depth = new DepthDesc()
            {
                Enabled = true,
                DepthCompare = DepthCompareType.Less,
            },
            ShaderProgram = GetOrCreateShaderProgram(device, material),
            Viewport = new RectI(0, 0, fb.Size.X, fb.Size.Y),
        });

        var cmdList = device.CreateCommandList();
        cmdList.BeginRenderPass(fb);
        cmdList.SetPipeline(pipeline);

        (IBuffer vertex, IBuffer index) buffers = InitializeBuffers(mesh, device);
        cmdList.SetVertexBuffer(buffers.vertex);
        cmdList.SetIndexBuffer(buffers.index);
        
        foreach (var texture in material.Textures)
        {
            cmdList.BindTexture(texture.Name, texture.Texture);
        }

        cmdList.DrawIndexed(mesh.IndexCount);

        var recordedRenderPass = cmdList.EndRenderPass();

        device.Submit(recordedRenderPass);
    }

    private static IShaderProgram GetOrCreateShaderProgram(IGraphicsDevice graphicsDevice, Material material)
    {
        if (cachedShaderPrograms.ContainsKey(material.Shader))
        {
            return cachedShaderPrograms[material.Shader];
        }
        
        var program = graphicsDevice.CreateShaderProgram(new ShaderProgramDesc(material.Shader.Vertex, material.Shader.Fragment));
        cachedShaderPrograms.Add(material.Shader, program);
        
        return program;
    }

    private static (IBuffer vertex, IBuffer index) InitializeBuffers(Mesh mesh, IGraphicsDevice device)
    {
        if(cachedMeshBuffers.TryGetValue(mesh, out var buffers))
        {
            return buffers;
        }
        
        var vertex = device.CreateBuffer(BufferUsage.Vertex, mesh.Vertices);
        var index = device.CreateBuffer(BufferUsage.Index, mesh.Indicies);

        cachedMeshBuffers[mesh] = (vertex, index);
        
        return (vertex, index);
    }
}