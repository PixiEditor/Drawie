namespace Drawie.Backend.Core.Surfaces;

public readonly struct SamplingOptions : IEquatable<SamplingOptions>
{
    public static readonly SamplingOptions Default = new(FilterMode.Nearest, MipmapMode.None);
    public static readonly SamplingOptions Bilinear = new(FilterMode.Linear, MipmapMode.Linear);

    public int MaxAnisotropy { get; }
    public bool UseCubic { get; }
    public CubicResampler Cubic { get; }
    public FilterMode Filter { get; }
    public MipmapMode Mipmap { get; }

    public bool IsAnisotropic => MaxAnisotropy > 0;

    public SamplingOptions(FilterMode filter, MipmapMode mipmap = MipmapMode.None)
    {
        MaxAnisotropy = 0;
        UseCubic = false;
        Cubic = new CubicResampler();
        Filter = filter;
        Mipmap = mipmap;
    }

    public SamplingOptions(CubicResampler resampler)
    {
        MaxAnisotropy = 0;
        UseCubic = true;
        Cubic = resampler;
        Filter = FilterMode.Nearest;
        Mipmap = MipmapMode.None;
    }

    public SamplingOptions(int maxAnisotropy)
    {
        MaxAnisotropy = Math.Max(1, maxAnisotropy);
        UseCubic = false;
        Cubic = new CubicResampler();
        Filter = FilterMode.Nearest;
        Mipmap = MipmapMode.None;
    }

    public bool Equals(SamplingOptions other) =>
        MaxAnisotropy == other.MaxAnisotropy &&
        UseCubic == other.UseCubic &&
        Cubic == other.Cubic &&
        Filter == other.Filter &&
        Mipmap == other.Mipmap;

    public override bool Equals(object obj) =>
        obj is SamplingOptions other && Equals(other);

    public static bool operator ==(SamplingOptions left, SamplingOptions right) => left.Equals(right);
    public static bool operator !=(SamplingOptions left, SamplingOptions right) => !left.Equals(right);

    public override int GetHashCode() =>
        HashCode.Combine(MaxAnisotropy, UseCubic, Cubic, Filter, Mipmap);
}
