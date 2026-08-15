using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Vertie.Core;
using Drawie.Backend.Vertie.Rendering;
using Drawie.Numerics;
using Drawie.RenderApi.Abstraction;
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

    private static Dictionary<Scene, IShaderProgram> cachedShaderPrograms =
        new Dictionary<Scene, IShaderProgram>();

    private static IRenderTarget? cachedRenderTarget;

    private static ICommandList? cmdList;

    public static void DrawScene(this TextureFramebuffer fb, Scene scene, Camera camera,
        RenderOptions options = default)
    {
        IGraphicsDevice device = DrawingBackendApi.Current.ActiveRenderApi.GraphicsDevice;

        fb.Canvas?.Flush();
        DrawScene(fb, scene, camera, device, options);
    }

    public static void DrawScene(IRenderTarget fb, Scene scene, Camera camera,
        IGraphicsDevice device,
        RenderOptions options)
    {
        if (cachedRenderTarget == null || cachedRenderTarget.Size != fb.Size)
        {
            (cachedRenderTarget as IDisposable)?.Dispose();

            cachedRenderTarget = device.CreateRenderTarget(new TextureDesc()
            {
                Width = fb.Size.X,
                Height = fb.Size.Y,
                Format = TextureFormat.RGBA8_Unorm,
                Depth = DepthFormat.Depth24Stencil8
            });
        }

        var shader = GetOrCreateShaderProgram(device, scene);

        var pipeline = device.CreatePipeline(new PipelineDesc()
        {
            Depth = new DepthDesc()
            {
                Enabled = true,
                DepthCompare = DepthCompareType.Less,
                Format = DepthFormat.Depth24Stencil8
            },
            Rasterizer = new RasterizerDesc()
            {
                RenderMode = options.RenderMode
            },
            ShaderProgram = shader,
            Viewport = new RectI(0, 0, fb.Size.X, fb.Size.Y),
        });

        cmdList ??= device.CreateCommandList();
        cmdList.BeginRenderPass(cachedRenderTarget);

        cmdList.SetPipeline(pipeline);
        foreach (var mesh in scene.Meshes)
        {
            if (!mesh.BuffersInitialized)
            {
                mesh.GenerateBuffers(device);
            }

            cmdList.SetBuffers(mesh.Buffers);

            var material = mesh.Material;
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
        }

        var recordedRenderPass = cmdList.EndRenderPass(fb);

        device.Submit(recordedRenderPass);
        DrawingBackendApi.Current.ResetContext();
    }

    private static IShaderProgram GetOrCreateShaderProgram(IGraphicsDevice graphicsDevice, Scene scene)
    {
        if (cachedShaderPrograms.ContainsKey(scene))
        {
            return cachedShaderPrograms[scene];
        }

        var program =
            graphicsDevice.CreateShaderProgram(new ShaderProgramDesc(
                scene.Meshes.SelectMany(x => x.Material.Shaders).Select(x => new ShaderDesc(x.EntryName, x.ShaderBytes, x.ShaderType))));
        cachedShaderPrograms.Add(scene, program);

        return program;
    }
}