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
    private static Dictionary<ITexture, ISampler> cachedSamplers = new Dictionary<ITexture, ISampler>();

    private static Dictionary<Material, IShaderProgram> cachedShaderPrograms =
        new Dictionary<Material, IShaderProgram>();

    private static ICommandList? cmdList;

    public static void DrawMesh(this TextureFramebuffer fb, Mesh mesh, Material material, Camera camera)
    {
        IGraphicsDevice device = DrawingBackendApi.Current.ActiveRenderApi.GraphicsDevice;

        fb.Canvas?.Flush();
        DrawMesh(fb, mesh, material, camera, device);
    }

    public static void DrawMesh(IRenderTarget fb, Mesh mesh, Material material, Camera camera, IGraphicsDevice device)
    {
        var sceneTarget = device.CreateRenderTarget(new TextureDesc()
        {
            Width = fb.Size.X,
            Height = fb.Size.Y,
            Format = TextureFormat.RGBA8_Unorm,
            Depth = DepthFormat.Depth24Stencil8
        });

        var shader = GetOrCreateShaderProgram(device, material);
        
        var pipeline = device.CreatePipeline(new PipelineDesc()
        {
            Depth = new DepthDesc()
            {
                Enabled = true,
                DepthCompare = DepthCompareType.Less,
            },
            ShaderProgram = shader,
            Viewport = new RectI(0, 0, fb.Size.X, fb.Size.Y),
        });

        cmdList ??= device.CreateCommandList();
        cmdList.BeginRenderPass(sceneTarget);
        cmdList.SetPipeline(pipeline);

        if (!mesh.BuffersInitialized)
        {
            mesh.GenerateBuffers(device);
        }
        
        cmdList.SetBuffers(mesh.Buffers);
        
        material.Use(camera);
        material.PrepareForObject(mesh.Transform);
        
        shader.UpdateUniforms(material.Properties.Values.ToList());

        for (var i = 0; i < material.Textures.Count; i++)
        {
            var materialTexture = material.Textures[i];
            var sampler = cachedSamplers.GetValueOrDefault(materialTexture);
            if (sampler == null)
            {
                sampler = device.CreateSampler(new SamplerDesc());
                cachedSamplers[materialTexture] = sampler;
            }

            cmdList.BindTexture(materialTexture, sampler);
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
        if (cachedShaderPrograms.ContainsKey(material))
        {
            return cachedShaderPrograms[material];
        }

        var program =
            graphicsDevice.CreateShaderProgram(new ShaderProgramDesc(material.Shaders.Select(x => new ShaderDesc(x.EntryName, x.ShaderBytes, x.ShaderType))));
        cachedShaderPrograms.Add(material, program);

        return program;
    }
}