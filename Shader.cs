using System;
using System.IO;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace ModelLoader;

public class Shader : IDisposable
{
    public readonly int Handle;
    private bool _disposed;

    public Shader(string vertPath, string fragPath)
    {
        string vertSrc = File.ReadAllText(vertPath);
        string fragSrc = File.ReadAllText(fragPath);

        int vert = Compile(ShaderType.VertexShader,   vertSrc);
        int frag = Compile(ShaderType.FragmentShader, fragSrc);

        Handle = GL.CreateProgram();
        GL.AttachShader(Handle, vert);
        GL.AttachShader(Handle, frag);
        GL.LinkProgram(Handle);

        GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int success);
        if (success == 0)
            throw new Exception($"Link error:\n{GL.GetProgramInfoLog(Handle)}");

        GL.DetachShader(Handle, vert);
        GL.DetachShader(Handle, frag);
        GL.DeleteShader(vert);
        GL.DeleteShader(frag);
    }

    private static int Compile(ShaderType type, string src)
    {
        int id = GL.CreateShader(type);
        GL.ShaderSource(id, src);
        GL.CompileShader(id);
        GL.GetShader(id, ShaderParameter.CompileStatus, out int ok);
        if (ok == 0)
            throw new Exception($"Compile error ({type}):\n{GL.GetShaderInfoLog(id)}");
        return id;
    }

    public void Use() => GL.UseProgram(Handle);

    public int GetAttribLocation(string name)  => GL.GetAttribLocation(Handle, name);
    public int GetUniformLocation(string name) => GL.GetUniformLocation(Handle, name);

    public void SetInt(string name, int value)
        => GL.Uniform1(GetUniformLocation(name), value);

    public void SetFloat(string name, float value)
        => GL.Uniform1(GetUniformLocation(name), value);

    public void SetVector3(string name, Vector3 value)
        => GL.Uniform3(GetUniformLocation(name), value);

    public void SetMatrix4(string name, Matrix4 value)
        => GL.UniformMatrix4(GetUniformLocation(name), false, ref value);

    public void Dispose()
    {
        if (!_disposed)
        {
            GL.DeleteProgram(Handle);
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
