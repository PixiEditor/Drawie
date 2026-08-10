using System.Drawing;
using System.Numerics;
using Drawie.Numerics;
using Drawie.RenderApi;
using Drawie.RenderApi.OpenGL;
using Drawie.Windowing;
using Drawie.Windowing.Input;
using Silk.NET.OpenGL;
using SilkNet;
using SilkNet.Geometry;
using SilkNet.Geometry.Primitives;
using SilkNet.Optimization;
using SilkNet.Rendering;
using Texture = SilkNet.Rendering.Texture;

namespace Drawie.Layer.ThreeD.Test;

public class ThreeDTest : ILayer
{
    private static MatShader _lampShader;
    private static MatShader _lightingShader;
    private static Texture _diffuseMap;
    private static Texture _specularMap;
    private static Gif _dogGif;
    private static Vector3 LampPosition = new Vector3(1.2f, 1.0f, 2.0f);

    private static List<GeometryObject> _objects = new List<GeometryObject>();
    private static List<GeometryObject> _gizmos = new List<GeometryObject>();
    private static List<Material> _materials = new List<Material>();

    private static Camera _camera;

    private static VecD _lastMousePosition;
    private static IKeyboard _primaryKeyboard;

    private static Vector3 _lightColor;
    private const int GifSpeed = 1;
    private static float _normalizedTime;
    private static int _currentFrame;
    private static double _lastTime;

    //Track when the window started so we can use the time elapsed to rotate the cube
    private static DateTime _startTime;

    private static MaterialBatcher _materialBatcher;

    private IWindow window;
    
    private OpenGlGraphicsContext openglContext;

    public void Initialize(IWindow window)
    {
        window.Resize += WindowOnResize;
        this.window = window;
        OnLoad(window.RenderApi.GraphicsContext);
        window.Update += OnUpdate;
        window.SubscribeToRender("3DTest.Render", "Init", OnRender);
    }

    private void WindowOnResize(VecI size)
    {
        openglContext.Api.Viewport(0, 0, (uint)size.X, (uint)size.Y);
        if (_camera != null)
            _camera.AspectRatio = (float)size.X / size.Y;
    }

    private void OnLoad(IGraphicsContext renderApiGraphicsContext)
    {
        if (renderApiGraphicsContext is not OpenGlGraphicsContext openGlGraphicsContext)
        {
            throw new ArgumentException("Only OpenGL backend is supported", nameof(renderApiGraphicsContext));
        }

        var GlContext = openGlGraphicsContext.Api;
        openglContext = openGlGraphicsContext;
        _startTime = DateTime.UtcNow;
        RegisterMouse(window.InputController);
        _primaryKeyboard = window.InputController.PrimaryKeyboard;

        ShaderLoader.BasicVertexShader = ShaderLoader.LoadRaw("BasicVertexShader");
        ShaderLoader.VertexShader = ShaderLoader.LoadRaw("VertexShader");
        ShaderLoader.UnlitShader = ShaderLoader.LoadRaw("UnlitShader");
        ShaderLoader.LitShader = ShaderLoader.LoadRaw("LitShader");

        //The lighting shader will give our main cube its colour multiplied by the lights intensity
        _lightingShader = new MatShader(GlContext, ShaderLoader.VertexShader, ShaderLoader.LitShader);
        _lampShader = new MatShader(GlContext, ShaderLoader.BasicVertexShader, ShaderLoader.UnlitShader);

        _diffuseMap = new Texture(GlContext, "Images/silkBoxed.png");
        _specularMap = new Texture(GlContext, "Images/silkSpecular.png");
        _dogGif = new Gif(GlContext, "Images/dancing-dog.gif");

        _camera = new Camera(Vector3.Zero, Vector3.UnitZ, Vector3.UnitY, (float)window.Size.X / window.Size.Y);

        Material cubeMat = new Material("BasicMat", _lightingShader);
        cubeMat.AddProperty("material.diffuse", 1f);
        cubeMat.AddProperty("material.specular", 1f);
        cubeMat.AddProperty("material.shininess", 32f);

        cubeMat.AddProperty<Vector3>("light.specular", Vector3.One);
        cubeMat.AddProperty<Vector3>("light.ambient", Vector3.One);
        cubeMat.AddProperty<Vector3>("light.diffuse", Vector3.One);
        cubeMat.AddProperty<Vector3>("light.position", LampPosition);

        Material unlitMat = new Material("UnlitMat", _lampShader);
        unlitMat.AddProperty("uColor", Vector3.One);

        _materials.Add(cubeMat);
        _materials.Add(unlitMat);

        SpawnCubes(10, 10, 10);

        _materialBatcher = new MaterialBatcher(_objects);
    }

    public static void InstantiateObject(GeometryObject obj)
    {
        _objects.Add(obj);
        _materialBatcher.AddObject(obj, _objects.Count - 1);
    }

    private void SpawnCubes(int rows, int columns, int depth)
    {
        for (int y = 0; y < columns; y++)
        {
            for (int x = 0; x < rows; x++)
            {
                for (int z = 0; z < depth; z++)
                {
                    _objects.Add(new Cube(openglContext.Api, 0)
                    {
                        Transform = { Position = new Vector3(x * 2.5f, y * 2.5f, z * 2.5f) }
                    });
                }
            }
        }
    }

    private static void UpdateBasicMaterial()
    {
        Material cubeMat = _materials[0];

        var difference = (float)(DateTime.UtcNow - _startTime).TotalSeconds;
        _lightColor = Vector3.Zero;
        _lightColor.X = MathF.Sin(difference * 2f);
        _lightColor.Y = MathF.Sin(difference * 0.7f);
        _lightColor.Z = MathF.Sin(difference * 1.3f);

        var diffuseColor = _lightColor * new Vector3(0.5f);
        var ambientColor = diffuseColor * new Vector3(1f);

        cubeMat.SetProperty("light.specular", new Vector3(1f, 1f, 1f));
        cubeMat.SetProperty("light.ambient", ambientColor);
        cubeMat.SetProperty("light.diffuse", diffuseColor);
        cubeMat.SetProperty("light.position", LampPosition);
    }

    private static void OnUpdate(double deltaTime)
    {
        float moveSpeed = 2.5f * (float)deltaTime;

        HandleMovement(moveSpeed);
        _camera.RecalculateFrustum();
    }

    private static void HandleMovement(float moveSpeed)
    {
        if (_primaryKeyboard.IsKeyPressed(Key.W))
        {
            _camera.Position += moveSpeed * _camera.Forward;
        }

        if (_primaryKeyboard.IsKeyPressed(Key.S))
        {
            _camera.Position -= moveSpeed * _camera.Forward;
        }

        if (_primaryKeyboard.IsKeyPressed(Key.A))
        {
            _camera.Position -= Vector3.Normalize(Vector3.Cross(_camera.Forward, _camera.Up)) * moveSpeed;
        }

        if (_primaryKeyboard.IsKeyPressed(Key.D))
        {
            _camera.Position += Vector3.Normalize(Vector3.Cross(_camera.Forward, _camera.Up)) * moveSpeed;
        }
    }

    private void OnRender(double deltaTime)
    {
        openglContext.Api.Enable(EnableCap.DepthTest);
        openglContext.Api.Clear((uint)(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit));

        UpdateBasicMaterial();
        RenderObjects();
    }

    private void RenderObjects()
    {
        foreach (var obj in _materialBatcher.Batches)
        {
            Batch batch = obj.Value;
            Material material = _materials[obj.Key];
            material.Use(_camera);

            for (int i = 0; i < batch.ObjectsCount; i++)
            {
                GeometryObject geometryObject = _objects[batch.StartIndex + i];
                if (!geometryObject.IsInFrustum(_camera.Frustum, geometryObject.Transform)) continue;

                geometryObject.OpenDrawingContext();
                _materials[geometryObject.MaterialIndex].PrepareForObject(geometryObject.Transform);
                geometryObject.Draw(openglContext.Api);
            }
        }
    }

    private static void OnClose()
    {
        _lampShader.Dispose();
        _lightingShader.Dispose();
        _diffuseMap.Dispose();
        _specularMap.Dispose();
        _dogGif.Dispose();
    }

    private void RegisterMouse(InputController input)
    {
        for (int i = 0; i < input.Pointers.Count; i++)
        {
            var mouse = input.Pointers[i];
            mouse.PointerMoved += OnMouseMove;
            mouse.PointerScrolled += OnScroll;
            mouse.PointerClicked += OnMouseClick;
        }
    }

    private void OnMouseClick(IPointer pointer, PointerButton button, VecD position)
    {
        if (button == PointerButton.Left)
        {
            InstantiateObject(new Cube(openglContext.Api, 0)
            {
                Transform = { Position = _camera.Position + _camera.Forward * 2f }
            });
        }
    }

    private static void OnScroll(IPointer pointer, VecD scrollDelta)
    {
        _camera.Zoom = (float)scrollDelta.Y;
    }

    private static void OnMouseMove(IPointer pointer, VecD position)
    {
        float lookSensitivity = 0.1f;
        if (_lastMousePosition == default)
        {
            _lastMousePosition = position;
        }
        else
        {
            double offsetX = (position.X - _lastMousePosition.X) * lookSensitivity;
            double offsetY = (position.Y - _lastMousePosition.Y) * lookSensitivity;
            _lastMousePosition = position;

            _camera.SetDirection((float)offsetX, (float)offsetY);
        }
    }
}