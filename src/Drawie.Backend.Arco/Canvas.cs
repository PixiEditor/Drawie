using System.Numerics;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Shaders.Common;
using Drawie.Backend.Vertie.Core;
using Drawie.Numerics;
using Drawie.RenderApi.Abstraction;
using Drawie.RenderApi.Abstraction.Buffers;
using Drawie.RenderApi.Abstraction.CommandRecording;
using Drawie.RenderApi.Abstraction.Pipeline;
using Drawie.RenderApi.Abstraction.RenderTargets;
using Drawie.RenderApi.Abstraction.Shaders;
using Drawie.RenderApi.Abstraction.Textures;

namespace Drawie.Backend.Arco;

public class Canvas
{
    public IGraphicsDevice GraphicsDevice { get; }
    public IRenderTarget renderTarget;

    private IShaderProgram shaderProgram;
    private ICommandList commandList;
    private IPipeline pipeline;
    private IBuffer<DrawInstance> instancesBuffer;
    private List<NamedBuffer> uniformBlocks;
    private IRenderTarget? cachedRenderTarget;

    public Canvas(IGraphicsDevice device, IRenderTarget renderTarget, VecI size)
    {
        this.renderTarget = renderTarget;
        GraphicsDevice  = device;
        var instancedRectVertex = ShaderLoader.LoadShader("RectInstancedVertex");
        var fillFragment = ShaderLoader.LoadShader("FillFragment");

        if (instancedRectVertex == null || fillFragment == null) throw new Exception("Unable to load shaders");

        shaderProgram =
            GraphicsDevice.CreateShaderProgram(new ShaderProgramDesc([
                instancedRectVertex, fillFragment
            ]));

        pipeline = GraphicsDevice.CreatePipeline(new PipelineDesc()
        {
            Depth = new DepthDesc()
            {
                Enabled = false,
            },
            Rasterizer = new RasterizerDesc()
            {
                RenderMode = RenderMode.Default,
                Samples = 1,
                CullMode = CullMode.None
            },
            ShaderProgram = shaderProgram,
            Viewport = new RectI(0, 0, size.X, size.Y),
        });

        instancesBuffer = GraphicsDevice.CreateBuffer<DrawInstance>(BufferUsage.Storage, new DrawInstance[1]);
        uniformBlocks = new List<NamedBuffer>()
        {
            new NamedBuffer("instances", instancesBuffer),
        };

    }

    public void DrawRect(float x, float y, float width, float height, Color fill)
    {
        if (cachedRenderTarget == null || cachedRenderTarget.Size != renderTarget.Size)
        {
            (cachedRenderTarget as IDisposable)?.Dispose();

            cachedRenderTarget ??= GraphicsDevice.CreateRenderTarget(new TextureDesc()
            {
                Width = this.renderTarget.Size.X,
                Height = this.renderTarget.Size.Y,
                Samples = 1,
                Depth = DepthFormat.NoDepth,
                Format = TextureFormat.RGBA8_Unorm
            });
        }

        commandList = GraphicsDevice.CreateCommandList();
        
        instancesBuffer.SetData([
            new DrawInstance()
                { Color = new Vector4(fill.R / 255f, fill.G / 255f, fill.B / 255f, fill.A / 255f), Position = new Vector2(x, y), Size = new Vector2(width, height) }
        ]);
        
        commandList.SetPipeline(pipeline);
        
        commandList.BeginRenderPass(cachedRenderTarget);
        commandList.BindPipeline();
        commandList.UpdateUniforms(uniformBlocks);
        commandList.Draw(6, 1);

        var recordedRenderPass = commandList.EndRenderPass(renderTarget);
        GraphicsDevice.Submit(recordedRenderPass);
    }
}