using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Vertie.Core;
using Drawie.Backend.Vertie.Rendering;
using Drawie.Numerics;
using Drawie.RenderApi;
using Drawie.RenderApi.Abstraction;
using Drawie.RenderApi.Abstraction.Buffers;
using Drawie.RenderApi.Abstraction.CommandRecording;
using Drawie.RenderApi.Abstraction.Pipeline;
using Drawie.RenderApi.Abstraction.RenderTargets;
using Drawie.RenderApi.Abstraction.Shaders;
using Drawie.RenderApi.Abstraction.Textures;
using Drawie.Rendering;

namespace Drawie.Backend.Vertie.Helpers;

public static class TextureFramebufferExtensions
{
    private static Dictionary<Mesh, (IBuffer, IBuffer)> cachedMeshBuffers = new Dictionary<Mesh, (IBuffer, IBuffer)>();

    private static Dictionary<CompiledShader, IShaderProgram> cachedShaderPrograms =
        new Dictionary<CompiledShader, IShaderProgram>();

    private static ICommandList? cmdList;

    public static void DrawMesh(this TextureFramebuffer fb, Mesh mesh, Material material)
    {
        IGraphicsDevice device = DrawingBackendApi.Current.ActiveRenderApi.GraphicsDevice;

        fb.Canvas?.Flush();
        DrawMesh(fb, mesh, material, device);
    }

    public static void DrawMesh(IRenderTarget fb, Mesh mesh, Material material, IGraphicsDevice device)
    {
        var sceneTarget = device.CreateRenderTarget(new TextureDesc()
        {
            Width = fb.Size.X,
            Height = fb.Size.Y,
            Format = TextureFormat.RGBA8_Unorm,
            Depth = DepthFormat.Depth24Stencil8
        });
        
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

        cmdList ??= device.CreateCommandList();
        cmdList.BeginRenderPass(sceneTarget);
        cmdList.SetPipeline(pipeline);

        (IBuffer vertex, IBuffer index) buffers = InitializeBuffers(mesh, device);
        cmdList.SetIndexBuffer(buffers.index);
        cmdList.SetVertexBuffer(buffers.vertex);

        foreach (var texture in material.Textures)
        {
            cmdList.BindTexture(texture.Name, texture.Texture);
        }

        cmdList.DrawIndexed(mesh.IndexCount);
        var recordedRenderPass = cmdList.EndRenderPass(fb);

        device.Submit(recordedRenderPass);
        DrawingBackendApi.Current.ResetContext();

        if (sceneTarget is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    private static IShaderProgram GetOrCreateShaderProgram(IGraphicsDevice graphicsDevice, Material material)
    {
        if (cachedShaderPrograms.ContainsKey(material.Shader))
        {
            return cachedShaderPrograms[material.Shader];
        }

        var program =
            graphicsDevice.CreateShaderProgram(new ShaderProgramDesc(material.Shader.Vertex, material.Shader.Fragment));
        cachedShaderPrograms.Add(material.Shader, program);

        return program;
    }

    private static (IBuffer vertex, IBuffer index) InitializeBuffers(Mesh mesh, IGraphicsDevice device)
    {
        if (cachedMeshBuffers.TryGetValue(mesh, out var buffers))
        {
            return buffers;
        }

        var vertex = device.CreateBuffer(BufferUsage.Vertex, mesh.Vertices);
        var index = device.CreateBuffer(BufferUsage.Index, mesh.Indicies);

        cachedMeshBuffers[mesh] = (vertex, index);

        return (vertex, index);
    }
}