using System.Numerics;
using System.Text;
using Drawie.Backend.Core.ColorsImpl;
using Drawie.Backend.Core.ColorsImpl.Paintables;
using Drawie.Backend.Core.Surfaces;
using Drawie.Backend.Core.Surfaces.PaintImpl;
using Drawie.Backend.Core.Text;
using Drawie.Backend.Vertie.Core;
using Drawie.Backend.Vertie.Helpers;
using Drawie.Backend.Vertie.Rendering;
using Drawie.Host;
using Drawie.Host.Input;
using Drawie.Layer.UI.ImGui;
using Drawie.Layer.UI.MiniUi;
using Drawie.Layer.UI.MiniUi.Controls;
using Drawie.Numerics;
using Drawie.Rendering;
using DrawiEngine;
using ImGuiNET;

namespace Drawie2Sample;

public class Drawie2SampleApp : DrawieApp
{
    private IHost window;

    private static Camera camera;
    private static VecD lastMousePosition;
    private int activeRenderMode = 0;
    private bool handleMovement;

    private string[] renderModes = new[]
    {
        "Default",
        "Wireframe",
    };

    private RenderOptions renderOptions = new RenderOptions();

    public override IHost CreateMainWindow()
    {
        window = Engine.WindowingPlatform.CreateWindow("Drawie 2 Sample", new VecI(1920, 1080));
        //window.AddLayer(new ImGuiLayer(RenderImGui));
        window.AddLayer(new MiniUILayer(RenderMiniUi));
        return window;
    }

    private void RenderMiniUi(double dt)
    {
        string text = renderOptions.RenderMode == RenderMode.Default ? "Enable wireframe" : "Enable solid fill";
        if (Button.Show(text))
        {
            renderOptions.RenderMode = renderOptions.RenderMode == RenderMode.Default ?  RenderMode.Wireframe : RenderMode.Default;
        }
    }

    private void RenderImGui(double dt)
    {
        ImGui.BeginGroup();
        if (ImGui.Combo("Render Mode", ref activeRenderMode, renderModes, renderModes.Length))
        {
            renderOptions.RenderMode = (RenderMode)activeRenderMode;
        }
        ImGui.EndGroup();
    }

    protected override void OnInitialize()
    {
        Material mat = new Material("Basic", [BuiltInShaders.BasicVertexShader, BuiltInShaders.UnlitFragmentShader]);

        var surf = GraphicsStore.Global.Create(new VecI(512, 512));
        RenderingContext context = new RenderingContext(GraphicsStore.Global.GraphicsContext);
        var ctx = context.Open();
        var fb = context.Edit(surf);
        fb.DrawRectangle(0, 0, 512, 512, new ColorPaintable(Colors.Blue));
        fb.Dispose();
        ctx.Dispose();
        
        mat.AddTexture(surf);
        handleMovement = true;
        
        window.InputController.PrimaryPointer.Cursor.State = CursorState.Disabled;
        window.InputController.PrimaryKeyboard.KeyPressed += (keyboard, key, code) =>
        {
            if (key == Key.Escape)
            {
                window.InputController.PrimaryPointer.Cursor.State = window.InputController.PrimaryPointer.Cursor.State == CursorState.Disabled ? CursorState.Normal : CursorState.Disabled;
                handleMovement = !handleMovement;
            }
        };
        
        camera = new Camera(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY, (float)window.Size.X / window.Size.Y);
        Mesh mesh = new Mesh("Assets/teapot.obj");

        RegisterMouse(window.InputController);

        window.Update += d =>
        {
            camera.AspectRatio = (float)window.Size.X / window.Size.Y;
            HandleMovement((float)d, camera, window.InputController.PrimaryKeyboard);
        };

        window.Render += (targetTexture, deltaTime) =>
        {
            targetTexture.Clear();

            targetTexture.DrawMesh(mesh, mat, camera, renderOptions);

            /*
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

            rt.Paint(targetTexture.Canvas, new VecD(0, 20), defaultFont, p, null);*/
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

    private void OnScroll(IPointer pointer, VecD scrollDelta)
    {
        if (!handleMovement) return;
        camera.Zoom = (float)scrollDelta.Y;
    }

    private void OnMouseMove(IPointer pointer, VecD position)
    {
        if (!handleMovement) return;
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

    private void HandleMovement(float deltaTime, Camera camera, IKeyboard primaryKeyboard)
    {
        if (!handleMovement) return;
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