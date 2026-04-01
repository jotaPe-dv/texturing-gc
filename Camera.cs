using System;
using OpenTK.Mathematics;

namespace ModelLoader;

/// <summary>
/// Cámara tipo FPS con movimiento libre.
/// Basada en el tutorial de cámara de OpenTK / LearnOpenGL.
/// </summary>
public class Camera
{
    private Vector3 _front = -Vector3.UnitZ;
    private Vector3 _up    =  Vector3.UnitY;
    private Vector3 _right =  Vector3.UnitX;

    private float _pitch;   // radianes
    private float _yaw = -MathHelper.PiOver2;

    public Vector3 Position { get; set; }
    public float   Fov      { get; set; } = 45f;
    public float   AspectRatio { private get; set; }

    public Camera(Vector3 position, float aspectRatio)
    {
        Position    = position;
        AspectRatio = aspectRatio;
        UpdateVectors();
    }

    public float Pitch
    {
        get => MathHelper.RadiansToDegrees(_pitch);
        set
        {
            _pitch = MathHelper.DegreesToRadians(Math.Clamp(value, -89f, 89f));
            UpdateVectors();
        }
    }

    public float Yaw
    {
        get => MathHelper.RadiansToDegrees(_yaw);
        set
        {
            _yaw = MathHelper.DegreesToRadians(value);
            UpdateVectors();
        }
    }

    public Matrix4 GetViewMatrix()
        => Matrix4.LookAt(Position, Position + _front, _up);

    public Matrix4 GetProjectionMatrix()
        => Matrix4.CreatePerspectiveFieldOfView(
               MathHelper.DegreesToRadians(Fov),
               AspectRatio, 0.01f, 100f);

    private void UpdateVectors()
    {
        _front = Vector3.Normalize(new Vector3(
            MathF.Cos(_pitch) * MathF.Cos(_yaw),
            MathF.Sin(_pitch),
            MathF.Cos(_pitch) * MathF.Sin(_yaw)));

        _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
        _up    = Vector3.Normalize(Vector3.Cross(_right, _front));
    }
}
