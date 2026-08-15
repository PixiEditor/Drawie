using System.Numerics;
using System.Reflection.Emit;
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
using Label = Drawie.Layer.UI.MiniUi.Controls.Label;

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
        if (CollapsableGroup.Begin("debug", "Debug"))
        {
            Panel.BeginColumn();

            Panel.BeginRow();
                Label.Show($"FPS: {1f / dt:F1}");
            Panel.EndRow();

            string text = renderOptions.RenderMode == RenderMode.Default ? "Enable wireframe" : "Enable solid fill";
            if (Button.Show(text))
            {
                renderOptions.RenderMode = renderOptions.RenderMode == RenderMode.Default
                    ? RenderMode.Wireframe
                    : RenderMode.Default;
            }

            Panel.EndColumn();
            CollapsableGroup.End();
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
        handleMovement = true;

        window.InputController.PrimaryPointer.Cursor.State = CursorState.Disabled;
        window.InputController.PrimaryKeyboard.KeyPressed += (keyboard, key, code) =>
        {
            if (key == Key.Escape)
            {
                window.InputController.PrimaryPointer.Cursor.State =
                    window.InputController.PrimaryPointer.Cursor.State == CursorState.Disabled
                        ? CursorState.Normal
                        : CursorState.Disabled;
                handleMovement = !handleMovement;
            }
        };

        camera = new Camera(new Vector3(0, 0, 5), Vector3.UnitZ, Vector3.UnitY, (float)window.Size.X / window.Size.Y);

        //"Shiba" (https://skfb.ly/6WxVW) by zixisun02 is licensed under Creative Commons Attribution (http://creativecommons.org/licenses/by/4.0/).
        Scene scene = new Scene("Assets/shiba.fbx", Path.Combine("Assets", "textures"));

        foreach (var sceneMesh in scene.Meshes)
        {
            sceneMesh.Transform.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, float.DegreesToRadians(-90));
        }

        RegisterMouse(window.InputController);

        window.Update += d =>
        {
            camera.AspectRatio = (float)window.Size.X / window.Size.Y;
            HandleMovement((float)d, camera, window.InputController.PrimaryKeyboard);
        };

        window.Render += (targetTexture, deltaTime) =>
        {
            targetTexture.Clear();
            targetTexture.DrawScene(scene, camera, renderOptions);
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