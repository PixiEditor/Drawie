namespace Drawie.Backend.Core.Surfaces;

public readonly record struct CubicResampler(float B, float C)
{
    public static readonly CubicResampler Mitchell   = new(1f / 3f, 1f / 3f);
    public static readonly CubicResampler CatmullRom = new(0f, 0.5f);
}
