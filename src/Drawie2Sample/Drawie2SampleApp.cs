using System.Numerics;
using System.Text;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Backend.Vertie.Core;
using Drawie.Backend.Vertie.Helpers;
using Drawie.Backend.Vertie.Rendering;
using Drawie.Numerics;
using Drawie.Rendering;
using Drawie.Windowing.Input;
using DrawiEngine;
using IWindow = Drawie.Windowing.IWindow;

namespace Drawie2Sample;

public class Drawie2SampleApp : DrawieApp
{
    private string shader = """
                            struct VertexInput
                            {
                                float3 vPos : POSITION;
                            };
                            
                            struct VertexOutput
                            {
                                float4 position : SV_Position;
                                float3 fPos : TEXCOORD0;
                            };
                            
                            
                            [[vk::binding(0, 0)]]
                            cbuffer Transform
                            {
                                float4x4 uModel;
                                float4x4 uView;
                                float4x4 uProjection;
                            };
                            
                            [shader("vertex")]
                            VertexOutput VSMain(VertexInput input)
                            {
                                VertexOutput output;
                            
                              output.position = mul(mul(mul(uProjection, uView), uModel), float4(input.vPos, 1.0));
                            
                               //We want to know the fragment's position in World space, so we multiply ONLY by uModel and not uView or uProjection
                               output.fPos = mul(uModel, float4(input.vPos, 1.0)).xyz;
                            
                                return output;
                            }
                            
                            [shader("fragment")]
                            float4 FSMain() : SV_Target
                            {
                                return float4(1, 0, 0, 1);
                            }
                            """;

    private IWindow window;
    private Material material;

    private static Camera camera;
    private static VecD lastMousePosition;
    
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
        var matShader = new ShaderDefinition(this.shader);
        var compiled = matShader.Compile();
        Material mat = new Material("Basic", compiled);
        camera = new Camera(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY, (float)window.Size.X / window.Size.Y);
        Cube cube = new Cube();
        cube.Transform.Position = new Vector3(0, 0, 0);

        RegisterMouse(window.InputController);
        
        window.Update += d =>
        {
            HandleMovement((float)d, camera, window.InputController.PrimaryKeyboard);
        };
        
        window.Render += (targetTexture, deltaTime) =>
        {
            targetTexture.Clear();

            targetTexture.DrawMesh(cube , mat, camera);
            
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
            
        };
    }
    
    
    private void RegisterMouse(InputController input)
    {
        for (int i = 0; i < input.Pointers.Count; i++)
        {
            var mouse = input.Pointers[i];
            mouse.PointerMoved += OnMouseMove;
            mouse.PointerScrolled += OnScroll;
        }
    }

    private static void OnScroll(IPointer pointer, VecD scrollDelta)
    {
        camera.Zoom = (float)scrollDelta.Y;
    }

    private static void OnMouseMove(IPointer pointer, VecD position)
    {
        float lookSensitivity = 0.1f;
        if (lastMousePosition == default)
        {
            lastMousePosition = position;
        }
        else
        {
            double offsetX = (position.X - lastMousePosition.X) * lookSensitivity;
            double offsetY = (position.Y - lastMousePosition.Y) * lookSensitivity;
            lastMousePosition = position;

            camera.SetDirection((float)offsetX, (float)offsetY);
        }
    }
    
    private static void HandleMovement(float deltaTime, Camera camera, IKeyboard primaryKeyboard)
    {
        float moveSpeed = 5f * (float)deltaTime;
        if (primaryKeyboard.IsKeyPressed(Key.W))
        {
            camera.Position += moveSpeed * camera.Forward;
        }

        if (primaryKeyboard.IsKeyPressed(Key.S))
        {
            camera.Position -= moveSpeed * camera.Forward;
        }

        if (primaryKeyboard.IsKeyPressed(Key.A))
        {
            camera.Position -= Vector3.Normalize(Vector3.Cross(camera.Forward, camera.Up)) * moveSpeed;
        }

        if (primaryKeyboard.IsKeyPressed(Key.D))
        {
            camera.Position += Vector3.Normalize(Vector3.Cross(camera.Forward, camera.Up)) * moveSpeed;
        }
    }
}