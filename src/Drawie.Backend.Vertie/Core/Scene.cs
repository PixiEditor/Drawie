using System.Numerics;
using Assimp;
using Drawie.Backend.Core;
using Drawie.Backend.Vertie.Rendering;
using Material = Drawie.Backend.Vertie.Rendering.Material;

namespace Drawie.Backend.Vertie.Core;

public class Scene
{
    public List<Mesh> Meshes { get; } = new List<Mesh>();


    public Scene(string path, Texture texture)
    {
        AssimpContext ctx = new AssimpContext();
        var scene = ctx.ImportFile(path, PostProcessPreset.TargetRealTimeMaximumQuality);
        if (!scene.HasMeshes) return;

        Rendering.Material[] materials = new Material[scene.Meshes.Count];
        for (var index = 0; index < scene.Materials.Count; index++)
        {
            var sceneMaterial = scene.Materials[index];
            materials[index] = new Material(sceneMaterial.Name,
                [BuiltInShaders.BasicVertexShader, BuiltInShaders.UnlitFragmentShader]);
        }

        foreach (var mesh in scene.Meshes)
        {
            var vertices = mesh!.Vertices.Select(x => new Vector3(x.X, x.Y, x.Z)).ToArray();
            var indicies = mesh.GetUnsignedIndices().ToArray();
            var normals = mesh.Normals.Select(x => new Vector3(x.X, x.Y, x.Z)).ToArray();
            var texCoords = mesh.TextureCoordinateChannels[0].Select(x => new Vector2(x.X, x.Y)).ToArray();
            Meshes.Add(new Mesh(vertices, indicies, normals, texCoords, materials[mesh.MaterialIndex]));
        }
    }

    public Scene(string path, string assetsRoot = "")
    {
        AssimpContext ctx = new AssimpContext();
        var scene = ctx.ImportFile(path, PostProcessPreset.TargetRealTimeMaximumQuality);
        if (!scene.HasMeshes) return;

        Rendering.Material[] materials = new Material[scene.Meshes.Count];
        for (var index = 0; index < scene.Materials.Count; index++)
        {
            var sceneMaterial = scene.Materials[index];
            materials[index] = new Material(sceneMaterial.Name,
                [BuiltInShaders.BasicVertexShader, BuiltInShaders.UnlitFragmentShader]);
            string texPath = Path.Combine(assetsRoot,
                Path.GetFileName(sceneMaterial.TextureDiffuse.FilePath?.Replace("\\", "/") ?? ""));

            //TODO It's a temp solution
            if (!sceneMaterial.HasTextureDiffuse)
            {
                texPath = Path.Combine(assetsRoot, "diffuse.png");
                if(!File.Exists(texPath)) continue;
            }
            
            materials[index].AddTexture(Texture.Load(texPath));
        }

        foreach (var mesh in scene.Meshes)
        {
            var vertices = mesh!.Vertices.Select(x => new Vector3(x.X, x.Y, x.Z)).ToArray();
            var indicies = mesh.GetUnsignedIndices().ToArray();
            var normals = mesh.Normals.Select(x => new Vector3(x.X, x.Y, x.Z)).ToArray();
            var texCoords = mesh.TextureCoordinateChannels[0].Select(x => new Vector2(x.X, x.Y)).ToArray();
            Meshes.Add(new Mesh(vertices, indicies, normals, texCoords, materials[mesh.MaterialIndex]));
        }
    }
}
