using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace ModelLoader;

/// <summary>
/// Ventana principal OpenTK.
/// - Carga un modelo 3D con Assimp
/// - Aplica iluminación difusa Phong (Tutorial 2 OpenTK)
/// - Renderiza con una única textura difusa
/// </summary>
public class MainWindow : GameWindow
{
    // ── Rutas ─────────────────────────────────────────────────────────────────
    // Cambia esta ruta por la de tu modelo .obj / .fbx / .gltf
    private const string ModelPath  = "Resources/backpack/backpack.obj";

    // ── Objetos OpenGL ────────────────────────────────────────────────────────
    private Shader? _shader;
    private Model?  _model;
    private Camera? _camera;

    // ── Input ─────────────────────────────────────────────────────────────────
    private bool  _firstMove = true;
    private float _sensitivity = 0.2f;
    private float _speed       = 2.5f;
    private Vector2 _lastMousePos;

    // ── Luz ───────────────────────────────────────────────────────────────────
    private Vector3 _lightPos   = new(3f, 3f, 3f);
    private Vector3 _lightColor = Vector3.One;

    public MainWindow()
        : base(GameWindowSettings.Default,
               new NativeWindowSettings
               {
                   ClientSize = new Vector2i(1280, 720),
                   Title      = "OpenTK – Model Loading + Diffuse Texture",
                   APIVersion = new Version(3, 3),
               })
    { }

    // ─────────────────────────────────────────────────────────────────────────
    protected override void OnLoad()
    {
        base.OnLoad();

        GL.ClearColor(0.1f, 0.1f, 0.1f, 1f);
        GL.Enable(EnableCap.DepthTest);

        // Compilar shaders
        _shader = new Shader("model.vert", "model.frag");

        // Cargar modelo con Assimp
        Console.WriteLine($"Cargando modelo: {ModelPath}");
        _model = new Model(ModelPath);
        Console.WriteLine("Modelo cargado correctamente.");

        // Cámara inicial
        _camera = new Camera(new Vector3(0f, 0f, 5f),
                             (float)ClientSize.X / ClientSize.Y);

        // Ocultar cursor para modo FPS
        CursorState = CursorState.Grabbed;
    }

    // ─────────────────────────────────────────────────────────────────────────
    protected override void OnRenderFrame(FrameEventArgs e)
    {
        base.OnRenderFrame(e);

        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        _shader!.Use();

        // Matrices MVP
        var model = Matrix4.Identity;
        // Centrar y escalar el modelo si es necesario:
        model = Matrix4.CreateScale(1f) * model;

        _shader.SetMatrix4("model",      model);
        _shader.SetMatrix4("view",       _camera!.GetViewMatrix());
        _shader.SetMatrix4("projection", _camera.GetProjectionMatrix());

        // Parámetros de iluminación (Tutorial 2 – Basic Lighting)
        _shader.SetVector3("lightPos",   _lightPos);
        _shader.SetVector3("lightColor", _lightColor);
        _shader.SetVector3("viewPos",    _camera.Position);

        // Dibujar modelo
        _model!.Draw(_shader);

        SwapBuffers();
    }

    // ─────────────────────────────────────────────────────────────────────────
    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        base.OnUpdateFrame(e);
        if (!IsFocused) return;

        float dt = (float)e.Time;

        // ── Teclado ────────────────────────────────────────────────────────
        var kb = KeyboardState;
        if (kb.IsKeyDown(Keys.Escape)) Close();

        if (kb.IsKeyDown(Keys.W)) _camera!.Position += _camera.GetViewMatrix().Row2.Xyz * -_speed * dt;
        if (kb.IsKeyDown(Keys.S)) _camera!.Position -= _camera.GetViewMatrix().Row2.Xyz * -_speed * dt;
        if (kb.IsKeyDown(Keys.A)) _camera!.Position -= Vector3.Normalize(
                                      Vector3.Cross(-_camera.GetViewMatrix().Row2.Xyz, Vector3.UnitY)) * _speed * dt;
        if (kb.IsKeyDown(Keys.D)) _camera!.Position += Vector3.Normalize(
                                      Vector3.Cross(-_camera.GetViewMatrix().Row2.Xyz, Vector3.UnitY)) * _speed * dt;

        // ── Ratón ──────────────────────────────────────────────────────────
        var mouse = MouseState;
        if (_firstMove)
        {
            _lastMousePos = new Vector2(mouse.X, mouse.Y);
            _firstMove    = false;
        }
        else
        {
            float dx = mouse.X - _lastMousePos.X;
            float dy = mouse.Y - _lastMousePos.Y;
            _lastMousePos = new Vector2(mouse.X, mouse.Y);

            _camera!.Yaw   += dx * _sensitivity;
            _camera!.Pitch -= dy * _sensitivity;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        GL.Viewport(0, 0, e.Width, e.Height);
        if (_camera != null)
            _camera.AspectRatio = (float)e.Width / e.Height;
    }

    protected override void OnUnload()
    {
        base.OnUnload();
        _shader?.Dispose();
        _model?.Dispose();
    }
}
