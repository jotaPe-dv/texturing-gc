# OpenTK – Model Loading + Diffuse Texture

Trabajo de recuperación – Computación Gráfica  
Basado en:
- [LearnOpenGL / Model-Loading / Model](https://learnopengl.com/Model-Loading/Model)
- [OpenTK Tutorial 2 – Iluminación](https://opentk.net/learn/chapter2/index.html)

---

## ¿Qué hace el programa?

1. **Carga un modelo 3D** (`.obj`, `.fbx`, `.gltf`, etc.) usando la biblioteca Assimp.
2. **Aplica iluminación Phong** (ambiente + difusa + especular) — cubierta en el Tutorial 2 de OpenTK.
3. **Renderiza una única textura difusa** sobre el modelo, tal como pide el enunciado.

---

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- GPU con soporte OpenGL 3.3+

Las siguientes bibliotecas se instalan automáticamente con `dotnet restore`:

| Paquete | Uso |
|---|---|
| `OpenTK 4.8.2` | Ventana, contexto GL, matemáticas |
| `Assimp.NET 5.0.0` | Carga del modelo 3D |
| `StbImageSharp` | Carga de imágenes / texturas |

---

## Modelo de prueba

Descarga el backpack del tutorial de LearnOpenGL:  
👉 https://learnopengl.com/data/models/backpack.zip

Descomprime y coloca la carpeta en:

```
ModelLoader/
└── Resources/
    └── backpack/
        ├── backpack.obj
        ├── backpack.mtl
        ├── diffuse.jpg   ← textura difusa
        └── ...
```

Si usas otro modelo, cambia la constante `ModelPath` en `MainWindow.cs`.

---

## Cómo ejecutar

```bash
# 1. Clonar / abrir el repositorio
cd ModelLoader

# 2. Restaurar dependencias
dotnet restore

# 3. Compilar y ejecutar
dotnet run
```

---

## Controles

| Tecla / Acción | Función |
|---|---|
| `W A S D` | Mover cámara |
| Mover ratón | Rotar cámara (modo FPS) |
| `Escape` | Cerrar |

---

## Estructura del proyecto

```
ModelLoader/
├── Program.cs          ← Punto de entrada
├── MainWindow.cs       ← GameWindow: carga, render loop, input
├── Shader.cs           ← Compilación y uso de shaders GLSL
├── Mesh.cs             ← VAO/VBO/EBO + Draw por mesh
├── Model.cs            ← Carga con Assimp, genera lista de Mesh
├── Camera.cs           ← Cámara FPS (Tutorial 2 OpenTK)
├── Shaders/
│   ├── model.vert      ← Vertex shader
│   └── model.frag      ← Fragment shader (Phong + textura difusa)
└── Resources/
    └── backpack/       ← Modelo de prueba (no incluido, ver arriba)
```

---

## Relación con los tutoriales

### Tutorial 2 de OpenTK (iluminación)
El fragment shader implementa el modelo de iluminación **Phong**:
- **Ambiente** – luz base que evita sombras completamente negras
- **Difusa** – `max(dot(normal, lightDir), 0)` — componente principal
- **Especular** – reflexión brillante usando el vector de vista

Esto cubre el contenido de `Basic Lighting` del tutorial.

### LearnOpenGL – Model Loading
La cadena `Model → ProcessNode → ProcessMesh → LoadMaterialTextures` replica exactamente la estructura del tutorial en C#:
- `Model.cs` usa `AssimpContext.ImportFile()` con flags `Triangulate | FlipUVs`
- `ProcessNode` recorre el árbol de nodos recursivamente
- `LoadMaterialTextures` evita cargar la misma textura dos veces (optimización del tutorial)
- Solo se cargan texturas de tipo `Diffuse` (`aiTextureType_DIFFUSE`)

---

## Resultado esperado

El modelo se renderiza con iluminación Phong y su textura difusa, permitiendo rotación libre con el ratón.

![Resultado esperado](https://learnopengl.com/img/model_loading/model_diffuse.png)
