namespace Drawie.Backend.Core.Shaders.Generation.Expressions;

public class SurfaceSampler : ShaderExpressionVariable<Texture>
{
    public SurfaceSampler(string name, ColorSampleMode sampleMode) : base(name)
    { 
        SampleMode = sampleMode;
    }

    public override string ConstantValueString { get; } = "";
    public override Expression? OverrideExpression { get; set; }
    public ColorSampleMode SampleMode { get; set; }
}
