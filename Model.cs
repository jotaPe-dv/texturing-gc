using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Assimp;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using StbImageSharp;

namespace ModelLoader;

public class Model : IDisposable
{
    private List<Mesh>          _meshes         = new();
    private List<Mesh.TextureInfo> _texturesLoaded = new();
    private string              _directory      = "";
    private bool                _disposed;

    public Model(string path)
    {
        LoadModel(path);
    }

    public void Draw(Shader shader)
    {
        foreach (var mesh in _meshes)
            mesh.Draw(shader);
    }

    // ── Carga del modelo ─────────────────────────────────────────────────────

    private void LoadModel(string path)
    {
        using var ctx = new AssimpContext();

        // Flags equivalentes a aiProcess_Triangulate | aiProcess_FlipUVs
        var flags = PostProcessSteps.Triangulate
                  | PostProcessSteps.FlipUVs
                  | PostProcessSteps.GenerateNormals
                  | PostProcessSteps.CalculateTangentSpace;

        Scene scene = ctx.ImportFile(path, flags);

        if (scene == null || scene.RootNode == null)
            throw new Exception($"Assimp: no se pudo cargar '{path}'");

        _directory = Path.GetDirectoryName(path) ?? "";

        ProcessNode(scene.RootNode, scene);
    }

    // Recorre el árbol de nodos recursivamente (igual que processNode en C++)
    private void ProcessNode(Node node, Scene scene)
    {
        foreach (int meshIndex in node.MeshIndices)
        {
            Assimp.Mesh aiMesh = scene.Meshes[meshIndex];
            _meshes.Add(ProcessMesh(aiMesh, scene));
        }

        foreach (Node child in node.Children)
            ProcessNode(child, scene);
    }

    // Convierte un aiMesh → nuestra clase Mesh
    private Mesh ProcessMesh(Assimp.Mesh aiMesh, Scene scene)
    {
        var vertices = new List<Mesh.Vertex>();
        var indices  = new List<uint>();
        var textures = new List<Mesh.TextureInfo>();

        // ── Vértices ──────────────────────────────────────────────────────────
        for (int i = 0; i < aiMesh.VertexCount; i++)
        {
            var v = new Mesh.Vertex();

            v.Position = new Vector3(
                aiMesh.Vertices[i].X,
                aiMesh.Vertices[i].Y,
                aiMesh.Vertices[i].Z);

            if (aiMesh.HasNormals)
                v.Normal = new Vector3(
                    aiMesh.Normals[i].X,
                    aiMesh.Normals[i].Y,
                    aiMesh.Normals[i].Z);

            if (aiMesh.HasTextureCoords(0))
                v.TexCoords = new Vector2(
                    aiMesh.TextureCoordinateChannels[0][i].X,
                    aiMesh.TextureCoordinateChannels[0][i].Y);

            vertices.Add(v);
        }

        // ── Índices ───────────────────────────────────────────────────────────
        foreach (Face face in aiMesh.Faces)
            foreach (int idx in face.Indices)
                indices.Add((uint)idx);

        // ── Material / Texturas difusas ───────────────────────────────────────
        if (aiMesh.MaterialIndex >= 0)
        {
            Material mat = scene.Materials[aiMesh.MaterialIndex];

            // Solo cargamos texturas difusas (requisito del trabajo)
            var diffuseMaps = LoadMaterialTextures(mat, TextureType.Diffuse, "texture_diffuse");
            textures.AddRange(diffuseMaps);
        }

        return new Mesh(vertices.ToArray(), indices.ToArray(), textures.ToArray());
    }

    // ── Carga de texturas ─────────────────────────────────────────────────────

    private List<Mesh.TextureInfo> LoadMaterialTextures(
        Material mat, TextureType type, string typeName)
    {
        var result = new List<Mesh.TextureInfo>();

        for (int i = 0; i < mat.GetMaterialTextureCount(type); i++)
        {
            mat.GetMaterialTexture(type, i, out TextureSlot slot);
            string texPath = slot.FilePath;

            // Evitar cargar la misma textura dos veces
            var already = _texturesLoaded.FirstOrDefault(t => t.Path == texPath);
            if (already.Id != 0)
            {
                result.Add(already);
                continue;
            }

            var info = new Mesh.TextureInfo
            {
                Id   = LoadTextureFromFile(texPath, _directory),
                Type = typeName,
                Path = texPath
            };

            result.Add(info);
            _texturesLoaded.Add(info);
        }

        return result;
    }


    public static int LoadTextureFromFile(string relativePath, string directory)
    {
        // Construir ruta absoluta
        string fullPath = Path.Combine(directory, relativePath);

        // Si no existe, buscar solo el nombre de archivo (modelos con paths absolutos)
        if (!File.Exists(fullPath))
            fullPath = Path.Combine(directory, Path.GetFileName(relativePath));

        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"Textura no encontrada: {fullPath}");

        // StbImageSharp carga la imagen
        StbImage.stbi_set_flip_vertically_on_load(1);
        using var stream = File.OpenRead(fullPath);
        var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);

        int id = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, id);

        GL.TexImage2D(TextureTarget.Texture2D,
                      0,
                      PixelInternalFormat.Rgba,
                      image.Width, image.Height,
                      0,
                      PixelFormat.Rgba,
                      PixelType.UnsignedByte,
                      image.Data);

        GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

        // Parámetros de wrapping y filtrado
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)OpenTK.Graphics.OpenGL4.TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)OpenTK.Graphics.OpenGL4.TextureWrapMode.Repeat);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        return id;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            foreach (var m in _meshes) m.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
