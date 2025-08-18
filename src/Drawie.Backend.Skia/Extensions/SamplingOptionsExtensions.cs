using Drawie.Backend.Core.Surfaces;
using SkiaSharp;

namespace Drawie.Skia.Extensions;

public static class SamplingOptionsExtensions
{
    public static SKSamplingOptions ToSkSamplingOptions(this SamplingOptions samplingOptions)
    {
        if (samplingOptions.Filter != FilterMode.Nearest && samplingOptions.Mipmap != MipmapMode.None)
        {
            return new SKSamplingOptions((SKFilterMode)samplingOptions.Filter, (SKMipmapMode)samplingOptions.Mipmap);
        }

        if (samplingOptions.MaxAnisotropy != 0)
        {
            return new SKSamplingOptions(samplingOptions.MaxAnisotropy);
        }

        if (samplingOptions.Cubic != default)
        {
            return new SKSamplingOptions(samplingOptions.Cubic.ToSkCubicResampler());
        }

        return new SKSamplingOptions((SKFilterMode)samplingOptions.Filter);
    }

    public static SKCubicResampler ToSkCubicResampler(this CubicResampler cubicResampler)
    {
        return new SKCubicResampler(cubicResampler.B, cubicResampler.C);
    }
}
