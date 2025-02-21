namespace Drawie.Numerics;

public struct Vec3D
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }

    public VecD XY => new(X, Y);

    public double TaxicabLength => Math.Abs(X) + Math.Abs(Y) + Math.Abs(Z);
    public double Length => Math.Sqrt(LengthSquared);
    public double LengthSquared => X * X + Y * Y + Z * Z;

    public static Vec3D Zero { get; } = new(0, 0, 0);

    public Vec3D(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public Vec3D(VecD xy, double z) : this(xy.X, xy.Y, z)
    {
    }

    public Vec3D(double bothAxesValue) : this(bothAxesValue, bothAxesValue, bothAxesValue)
    {
    }

    public Vec3D Round()
    {
        return new Vec3D(Math.Round(X), Math.Round(Y), Math.Round(Z));
    }

    public Vec3D Ceiling()
    {
        return new Vec3D(Math.Ceiling(X), Math.Ceiling(Y), Math.Ceiling(Z));
    }

    public Vec3D Floor()
    {
        return new Vec3D(Math.Floor(X), Math.Floor(Y), Math.Floor(Z));
    }

    public Vec3D Lerp(Vec3D other, double factor)
    {
        return (other - this) * factor + this;
    }

    public Vec3D Normalize()
    {
        return new Vec3D(X / Length, Y / Length, Z / Length);
    }

    public Vec3D Abs()
    {
        return new Vec3D(Math.Abs(X), Math.Abs(Y), Math.Abs(Z));
    }

    public Vec3D Signs()
    {
        return new Vec3D(X >= 0 ? 1 : -1, Y >= 0 ? 1 : -1, Z >= 0 ? 1 : -1);
    }

    public double Dot(Vec3D other) => (X * other.X) + (Y * other.Y) + (Z * other.Z);

    public Vec3D Multiply(Vec3D other)
    {
        return new Vec3D(X * other.X, Y * other.Y, Z * other.Z);
    }

    public Vec3D Divide(Vec3D other)
    {
        return new Vec3D(X / other.X, Y / other.Y, Z / other.Z);
    }

    public static Vec3D operator +(Vec3D a, Vec3D b)
    {
        return new Vec3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    }

    public static Vec3D operator -(Vec3D a, Vec3D b)
    {
        return new Vec3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    }

    public static Vec3D operator -(Vec3D a)
    {
        return new Vec3D(-a.X, -a.Y, -a.Z);
    }

    public static Vec3D operator *(double b, Vec3D a)
    {
        return new Vec3D(a.X * b, a.Y * b, a.Z * b);
    }

    public static double operator *(Vec3D a, Vec3D b) => a.Dot(b);

    public static Vec3D operator *(Vec3D a, double b)
    {
        return new Vec3D(a.X * b, a.Y * b, a.Z * b);
    }

    public static Vec3D operator /(Vec3D a, double b)
    {
        return new Vec3D(a.X / b, a.Y / b, a.Z / b);
    }

    public static Vec3D operator %(Vec3D a, double b)
    {
        return new Vec3D(a.X % b, a.Y % b, a.Z % b);
    }

    public static bool operator ==(Vec3D a, Vec3D b)
    {
        return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
    }

    public static bool operator !=(Vec3D a, Vec3D b)
    {
        return !(a.X == b.X && a.Y == b.Y && a.Z == b.Z);
    }

    public double Sum() => X + Y + Z;

    public static implicit operator Vec3D((double, double, double, double) tuple)
    {
        return new Vec3D(tuple.Item1, tuple.Item2, tuple.Item3);
    }

    public void Deconstruct(out double x, out double y, out double z)
    {
        x = X;
        y = Y;
        z = Z;
    }

    public bool IsNaNOrInfinity()
    {
        return double.IsNaN(X) || double.IsNaN(Y) || double.IsInfinity(X) || double.IsInfinity(Y) || double.IsNaN(Z) ||
               double.IsInfinity(Z);
    }

    public override string ToString()
    {
        return $"({X}; {Y}; {Z})";
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Vec3D item)
            return false;

        return this == (Vec3D?)item;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }

    public bool Equals(Vec3D other)
    {
        return other.X == X && other.Y == Y && other.Z == Z;
    }

    public bool AlmostEquals(Vec3D other, double axisEpsilon = 0.001)
    {
        double dX = Math.Abs(X - other.X);
        double dY = Math.Abs(Y - other.Y);
        double dZ = Math.Abs(Z - other.Z);
        return dX < axisEpsilon && dY < axisEpsilon && dZ < axisEpsilon;
    }
}
