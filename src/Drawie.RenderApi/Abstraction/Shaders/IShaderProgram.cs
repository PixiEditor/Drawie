namespace Drawie.RenderApi.Abstraction.Shaders;

public interface IShaderProgram
{
    public void Use();
    void UpdateUniforms(List<UniformBlock> blocks);
}