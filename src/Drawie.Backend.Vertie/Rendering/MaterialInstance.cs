using Drawie.Backend.Shaders.Common;
using Drawie.Backend.Vertie.Core;
using Drawie.RenderApi.Abstraction.Shaders;
using Drawie.RenderApi.Abstraction.Textures;

namespace Drawie.Backend.Vertie.Rendering;

public class MaterialInstance
{
    public Material Original { get; }

    public Guid InstanceId { get; } = Guid.NewGuid();

    public Dictionary<string, UniformBlock> Properties { get; set; }
    public List<ITexture> Textures { get; }

    public MaterialInstance(Material from)
    {
        Original = from;
        Properties = CreateFromDefinitions(from.Properties);
        Textures = new List<ITexture>(Original.Textures);
    }
    
    public void Use(Camera camera)
    {
        Properties["Transform"].SetProperty("uView", camera.ViewMatrix);
        Properties["Transform"].SetProperty("uProjection", camera.ProjectionMatrix);
    }

    public void PrepareForObject(Transform transform)
    {
        Properties["Transform"].SetProperty("uModel", transform.ViewMatrix);
    }
    
    private Dictionary<string, UniformBlock> CreateFromDefinitions(Dictionary<string, PropertyGroupDefinition> fromProperties)
    {
        var dict = new Dictionary<string, UniformBlock>();
        foreach (var propertyGroupDefinition in fromProperties)
        {
             var block = new UniformBlock(propertyGroupDefinition.Key) { ShaderLayout = propertyGroupDefinition.Value.ShaderLayout };
             foreach (var prop in propertyGroupDefinition.Value.Properties)
             {
                 block.AddProperty(new ShaderProperty(prop.Name) { ObjValue = prop.DefaultValue, Type =  prop.ValueType });
             }
             
             dict.Add(propertyGroupDefinition.Key, block);
        }

        return dict;
    }
}