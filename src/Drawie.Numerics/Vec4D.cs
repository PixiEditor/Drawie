namespace Drawie.Numerics;

public struct Vec4D
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Z { get; set; }
    public double W { get; set; }

    public VecD XY => new(X, Y);
    public VecD XZ => new(X, Z);
    public VecD YZ => new(Y, Z);
    public VecD XW => new(X, W);
    public VecD YW => new(Y, W);
    public VecD ZW => new(Z, W);

    public Vec3D XYZ => new(X, Y, Z);
    public Vec3D XYW => new(X, Y, W);
    public Vec3D XZW => new(X, Z, W);
    public Vec3D YZW => new(Y, Z, W);

    public double TaxicabLength => Math.Abs(X) + Math.Abs(Y) + Math.Abs(Z) + Math.Abs(W);
    public double Length => Math.Sqrt(LengthSquared);
    public double LengthSquared => X * X + Y * Y + Z * Z + W * W;

    public static Vec4D Zero { get; } = new(0, 0, 0, 0);

    public Vec4D(double x, double y, double z, double w)
    {
        X = x;
        Y = y;
        Z = z;
        W = w;
    }

    public Vec4D(double allAxesValue) : this(allAxesValue, allAxesValue, allAxesValue, allAxesValue)
    {
    }

    public Vec4D Round()
    {
        return new Vec4D(Math.Round(X), Math.Round(Y), Math.Round(Z), Math.Round(W));
    }

    public Vec4D Ceiling()
    {
        return new Vec4D(Math.Ceiling(X), Math.Ceiling(Y), Math.Ceiling(Z), Math.Ceiling(W));
    }

    public Vec4D Floor()
    {
        return new Vec4D(Math.Floor(X), Math.Floor(Y), Math.Floor(Z), Math.Floor(W));
    }

    public Vec4D Lerp(Vec4D other, double factor)
    {
        return (other - this) * factor + this;
    }

    public Vec4D Normalize()
    {
        return new Vec4D(X / Length, Y / Length, Z / Length, W / Length);
    }

    public Vec4D Abs()
    {
        return new Vec4D(Math.Abs(X), Math.Abs(Y), Math.Abs(Z), Math.Abs(W));
    }

    public Vec4D Signs()
    {
        return new Vec4D(X >= 0 ? 1 : -1, Y >= 0 ? 1 : -1, Z >= 0 ? 1 : -1, W >= 0 ? 1 : -1);
    }

    public double Dot(Vec4D other) => (X * other.X) + (Y * other.Y) + (Z * other.Z) + (W * other.W);

    public Vec4D Multiply(Vec4D other)
    {
        return new Vec4D(X * other.X, Y * other.Y, Z * other.Z, W * other.W);
    }

    public Vec4D Divide(Vec4D other)
    {
        return new Vec4D(X / other.X, Y / other.Y, Z / other.Z, W / other.W);
    }

    public static Vec4D operator +(Vec4D a, Vec4D b)
    {
        return new Vec4D(a.X * b.X, a.Y * b.Y, a.Z * b.Z, a.W * b.W);
    }

    public static Vec4D operator -(Vec4D a, Vec4D b)
    {
        return new Vec4D(a.X - b.X, a.Y - b.Y, a.Z - b.Z, a.W - b.W);
    }

    public static Vec4D operator -(Vec4D a)
    {
        return new Vec4D(-a.X, -a.Y, -a.Z, -a.W);
    }

    public static Vec4D operator *(double b, Vec4D a)
    {
        return new Vec4D(a.X * b, a.Y * b, a.Z * b, a.W * b);
    }

    public static double operator *(Vec4D a, Vec4D b) => a.Dot(b);

    public static Vec4D operator *(Vec4D a, double b)
    {
        return new Vec4D(a.X * b, a.Y * b, a.Z * b, a.W * b);
    }

    public static Vec4D operator /(Vec4D a, double b)
    {
        return new Vec4D(a.X / b, a.Y / b, a.Z / b, a.W / b);
    }

    public static Vec4D operator %(Vec4D a, double b)
    {
        return new Vec4D(a.X % b, a.Y % b, a.Z % b, a.W % b);
    }

    public static bool operator ==(Vec4D a, Vec4D b)
    {
        return a.X == b.X && a.Y == b.Y && a.Z == b.Z && a.W == b.W;
    }

    public static bool operator !=(Vec4D a, Vec4D b)
    {
        return !(a.X == b.X && a.Y == b.Y && a.Z == b.Z && a.W == b.W);
    }

    public double Sum() => X + Y + Z + W;

    public static implicit operator Vec4D((double, double, double, double) tuple)
    {
        return new Vec4D(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);
    }

    public void Deconstruct(out double x, out double y, out double z, out double w)
    {
        x = X;
        y = Y;
        z = Z;
        w = W;
    }

    public bool IsNaNOrInfinity()
    {
        return double.IsNaN(X) || double.IsNaN(Y) || double.IsInfinity(X) || double.IsInfinity(Y) || double.IsNaN(Z) ||
               double.IsInfinity(Z) || double.IsNaN(W) || double.IsInfinity(W);
    }

    public override string ToString()
    {
        return $"({X}; {Y}; {Z}; {W})";
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Vec4D item)
            return false;

        return this == (Vec4D?)item;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z, W);
    }

    public bool Equals(Vec4D other)
    {
        return other.X == X && other.Y == Y && other.Z == Z && other.W == W;
    }

    public bool AlmostEquals(Vec4D other, double axisEpsilon = 0.001)
    {
        double dX = Math.Abs(X - other.X);
        double dY = Math.Abs(Y - other.Y);
        double dZ = Math.Abs(Z - other.Z);
        double dW = Math.Abs(W - other.W);
        return dX < axisEpsilon && dY < axisEpsilon && dZ < axisEpsilon && dW < axisEpsilon;
    }
}
