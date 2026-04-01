using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace ModelLoader;

/// <summary>
/// Representa un conjunto de vértices, índices y texturas listo para renderizar.
/// Equivalente a la clase Mesh de LearnOpenGL.
/// </summary>
public class Mesh : IDisposable
{
    // ── Estructura de vértice ────────────────────────────────────────────────
    public struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TexCoords;
    }

    public struct TextureInfo
    {
        public int    Id;
        public string Type;   // "texture_diffuse1", etc.
        public string Path;
    }

    // ── Datos ────────────────────────────────────────────────────────────────
    private readonly Vertex[]      _vertices;
    private readonly uint[]        _indices;
    private readonly TextureInfo[] _textures;

    private int _vao, _vbo, _ebo;
    private bool _disposed;

    public Mesh(Vertex[] vertices, uint[] indices, TextureInfo[] textures)
    {
        _vertices = vertices;
        _indices  = indices;
        _textures = textures;
        SetupMesh();
    }

    private void SetupMesh()
    {
        _vao = GL.GenVertexArray();
        _vbo = GL.GenBuffer();
        _ebo = GL.GenBuffer();

        GL.BindVertexArray(_vao);

        // VBO
        GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);
        GL.BufferData(BufferTarget.ArrayBuffer,
                      _vertices.Length * System.Runtime.InteropServices.Marshal.SizeOf<Vertex>(),
                      _vertices,
                      BufferUsageHint.StaticDraw);

        // EBO
        GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
        GL.BufferData(BufferTarget.ElementArrayBuffer,
                      _indices.Length * sizeof(uint),
                      _indices,
                      BufferUsageHint.StaticDraw);

        int stride = System.Runtime.InteropServices.Marshal.SizeOf<Vertex>();

        // Atributo 0: Position
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);

        // Atributo 1: Normal
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, stride,
                               System.Runtime.InteropServices.Marshal.OffsetOf<Vertex>("Normal").ToInt32());

        // Atributo 2: TexCoords
        GL.EnableVertexAttribArray(2);
        GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, stride,
                               System.Runtime.InteropServices.Marshal.OffsetOf<Vertex>("TexCoords").ToInt32());

        GL.BindVertexArray(0);
    }

    public void Draw(Shader shader)
    {
        int diffuseN  = 1;
        int specularN = 1;

        for (int i = 0; i < _textures.Length; i++)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + i);

            string number = _textures[i].Type switch
            {
                "texture_diffuse"  => (diffuseN++).ToString(),
                "texture_specular" => (specularN++).ToString(),
                _                  => "1"
            };

            shader.SetInt(_textures[i].Type + number, i);
            GL.BindTexture(TextureTarget.Texture2D, _textures[i].Id);
        }

        // Dibujar
        GL.BindVertexArray(_vao);
        GL.DrawElements(PrimitiveType.Triangles, _indices.Length,
                        DrawElementsType.UnsignedInt, 0);
        GL.BindVertexArray(0);

        GL.ActiveTexture(TextureUnit.Texture0);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            GL.DeleteVertexArray(_vao);
            GL.DeleteBuffer(_vbo);
            GL.DeleteBuffer(_ebo);
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
