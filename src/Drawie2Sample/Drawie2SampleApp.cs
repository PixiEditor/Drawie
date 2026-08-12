using System.Numerics;
using System.Text;
using Drawie.Backend.Core;
using Drawie.Backend.Core.Bridge;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Backend.Vertie.Core;
using Drawie.Backend.Vertie.Helpers;
using Drawie.Backend.Vertie.Rendering;
using Drawie.Numerics;
using Drawie.RenderApi.Abstraction.Textures;
using Drawie.RenderApi.OpenGL;
using Drawie.Rendering;
using Drawie.Windowing.Input;
using DrawiEngine;
using Silk.NET.OpenGL;
using IWindow = Drawie.Windowing.IWindow;

namespace Drawie2Sample;

public class Drawie2SampleApp : DrawieApp
{
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
        Material mat = new Material("Basic", [BuiltInShaders.BasicVertexShader, BuiltInShaders.UnlitFragmentShader]);
        var tex = DrawingBackendApi.Current.ActiveRenderApi.GraphicsDevice.CreateTexture(new TextureDesc(){
            Width = 512,
            Height = 512,
            Format = TextureFormat.RGBA8_Unorm,
            Depth = DepthFormat.NoDepth,
        });

        var surf = DrawingBackendApi.Current.CreateRenderSurface(new VecI(512, 512), tex, SurfaceOrigin.BottomLeft);
        surf.Canvas.DrawCircle(256, 256, 128, new Paint(){Color = Colors.Red});
        
        var pixelSpan = Surface.Load("Images/silkBoxed.png").DrawingSurface.PeekPixels().GetPixelSpan<byte>();
        mat.AddTexture(new OpenGlTexture(OpenGlDevice.DEBUG_API, 359, 359, pixelSpan, PixelFormat.Bgra));
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