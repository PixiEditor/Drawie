using System.Text;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Backend.Vertie.Helpers;
using Drawie.Backend.Vertie.Rendering;
using Drawie.Layer.ThreeD.Test;
using Drawie.Layer.UI.ImGui;
using Drawie.Numerics;
using Drawie.Rendering;
using Drawie.Windowing;
using DrawiEngine;

namespace Drawie2Sample;

public class Drawie2SampleApp : DrawieApp
{
    private string shader = """
                            [shader("vertex")]
                            float4 VSMain(float3 position : POSITION) : SV_Position
                            {
                                return float4(position, 1.0);
                            }
                            
                            [shader("fragment")]
                            float4 FSMain() : SV_Target
                            {
                                return float4(1, 0, 0, 1);
                            }
                            """;

    private IWindow window;
    private Material material;

    public override IWindow CreateMainWindow()
    {
        window = Engine.WindowingPlatform.CreateWindow("Drawie 2 Sample", new VecI(1920, 1080));
        //window.AddLayer(new ThreeDTest());
        //window.AddLayer(new ImGuiLayer(RenderImGui));
        return window;
    }

    private void RenderImGui(double dt)
    {
        ImGuiNET.ImGui.ShowDemoWindow();
    }

    protected override void OnInitialize()
    {
        var matShader = new ShaderDefiniton(this.shader);
        var compiled = matShader.Compile();
        Material mat = new Material("Basic", compiled);
        
        window.Render += (targetTexture, deltaTime) =>
        {
            targetTexture.Clear();
            
            using Font defaultFont = Font.CreateDefault();
            Paintable color = new ColorPaintable(Colors.White);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Stores: " + GraphicsStore.AllStores.Count);
            foreach (var store in GraphicsStore.AllStores)
            {
                sb.AppendLine("Store:  " + store.GetDebugText());
            }

            RichText rt = new RichText(sb.ToString()) { Fill = true, FillPaintable = color };
            
            using Paint p = new Paint() { Paintable = color };
            
            rt.Paint(targetTexture.Canvas, new VecD(0, 20), defaultFont, p, null);
            
            targetTexture.DrawMesh(new Cube(), mat);
        };
    }
}

