namespace Drawie.Numerics;

/// <summary>
///     Represents an ordered pair of floating-point x- and y-coordinates that defines a point in a two-dimensional
///     plane.
/// </summary>
public struct VecF : IEquatable<VecF>
{
    public static VecF Zero = new VecF(0, 0);

    /// <summary>Represents a new instance of the <see cref="VecF" /> class with member data left uninitialized.</summary>
    public static readonly VecF Empty;

    public VecF(float x, float y)
    {
        X = x;
        Y = y;
    }

    public VecF(VecI pointPos)
    {
        X = pointPos.X;
        Y = pointPos.Y;
    }
    

    /// <summary>Gets a value indicating whether this point is empty.</summary>
    /// <value />
    public readonly bool IsEmpty => this == Empty;

    /// <summary>Gets the Euclidean distance from the origin (0, 0).</summary>
    /// <value />
    /// <remarks />
    public readonly float Length => (float)Math.Sqrt((X * (double)X) + (Y * (double)Y));

    /// <summary>Gets the Euclidean distance squared from the origin (0, 0).</summary>
    /// <value />
    /// <remarks />
    public readonly float LengthSquared => (float)((X * (double)X) + (Y * (double)Y));

    /// <param name="p">The offset value.</param>
    /// <summary>Translates a given point by a specified offset.</summary>
    /// <remarks />
    public void Offset(VecF p)
    {
        X += p.X;
        Y += p.Y;
    }

    /// <param name="dx">The offset in the x-direction.</param>
    /// <param name="dy">The offset in the y-direction.</param>
    /// <summary>Translates a given point by a specified offset.</summary>
    public void Offset(float dx, float dy)
    {
        X += dx;
        Y += dy;
    }

    /// <summary>Converts this <see cref="VecF" /> to a human readable string.</summary>
    /// <returns>A string that represents this <see cref="VecF" />.</returns>
    public override readonly string ToString()
    {
        return string.Format("{{X={0}, Y={1}}}", X, Y);
    }

    /// <param name="vecF">The point to normalize.</param>
    /// <summary>Returns a point with the same direction as the specified point, but with a length of one.</summary>
    /// <returns>Returns a point with a length of one.</returns>
    public static VecF Normalize(VecF vecF)
    {
        var num = 1.0 / Math.Sqrt((vecF.X * (double)vecF.X) + (vecF.Y * (double)vecF.Y));
        return new VecF(vecF.X * (float)num, vecF.Y * (float)num);
    }

    /// <param name="vecF">The first point.</param>
    /// <param name="other">The second point.</param>
    /// <summary>Calculate the Euclidean distance between two points.</summary>
    /// <returns>Returns the Euclidean distance between two points.</returns>
    public static float Distance(VecF vecF, VecF other)
    {
        var num1 = vecF.X - other.X;
        var num2 = vecF.Y - other.Y;
        return (float)Math.Sqrt((num1 * (double)num1) + (num2 * (double)num2));
    }

    /// <param name="vecF">The first point.</param>
    /// <param name="other">The second point.</param>
    /// <summary>Calculate the Euclidean distance squared between two points.</summary>
    /// <returns>Returns the Euclidean distance squared between two points.</returns>
    public static float DistanceSquared(VecF vecF, VecF other)
    {
        var num1 = vecF.X - other.X;
        var num2 = vecF.Y - other.Y;
        return (float)((num1 * (double)num1) + (num2 * (double)num2));
    }

    /// <param name="vecF">The point to reflect.</param>
    /// <param name="normal">The normal.</param>
    /// <summary>Returns the reflection of a point off a surface that has the specified normal.</summary>
    /// <returns>Returns the reflection of a point.</returns>
    public static VecF Reflect(VecF vecF, VecF normal)
    {
        var num = (float)((vecF.X * (double)vecF.X) + (vecF.Y * (double)vecF.Y));
        return new VecF(vecF.X - (2f * num * normal.X), vecF.Y - (2f * num * normal.Y));
    }

    /// <param name="pt">The point to translate</param>
    /// <param name="sz">The offset size.</param>
    /// <summary>Translates a given point by a specified size.</summary>
    /// <returns>Returns the translated point.</returns>
    public static VecF Add(VecF pt, VecI sz)
    {
        return pt + sz;
    }

    /// <param name="pt">The point to translate.</param>
    /// <param name="sz">The offset size.</param>
    /// <summary>Translates a given point by a specified size.</summary>
    /// <returns>Returns the translated point.</returns>
    public static VecF Add(VecF pt, VecD sz)
    {
        return pt + sz;
    }

    /// <param name="pt">The point to translate.</param>
    /// <param name="sz">The offset value.</param>
    /// <summary>Translates a given point by a specified offset.</summary>
    /// <returns>Returns the translated point.</returns>
    public static VecF Add(VecF pt, VecF sz)
    {
        return pt + sz;
    }

    /// <param name="pt">The <see cref="VecF" /> to translate.</param>
    /// <param name="sz">
    ///     The <see cref="VecD" /> that specifies the numbers to subtract from the coordinates of
    ///     <paramref name="pt" />.
    /// </param>
    /// <summary>Translates a <see cref="VecF" /> by the negative of a specified size.</summary>
    /// <returns>The translated <see cref="VecF" />.</returns>
    public static VecF Subtract(VecF pt, VecI sz)
    {
        return pt - sz;
    }

    /// <param name="pt">The <see cref="VecF" /> to translate.</param>
    /// <param name="sz">
    ///     The <see cref="VecD" /> that specifies the numbers to subtract from the coordinates of
    ///     <paramref name="pt" />.
    /// </param>
    /// <summary>Translates a <see cref="VecF" /> by the negative of a specified size.</summary>
    /// <returns>The translated <see cref="VecF" />.</returns>
    public static VecF Subtract(VecF pt, VecD sz)
    {
        return pt - sz;
    }

    /// <param name="pt">The <see cref="VecF" /> to translate.</param>
    /// <param name="sz">The offset that specifies the numbers to subtract from the coordinates of <paramref name="pt" />.</param>
    /// <summary>Translates a given point by the negative of a specified offset.</summary>
    /// <returns>The translated <see cref="VecF" />.</returns>
    public static VecF Subtract(VecF pt, VecF sz)
    {
        return pt - sz;
    }

    /// <param name="pt">The point to translate.</param>
    /// <param name="sz">The offset size.</param>
    /// <summary>Translates a given point by a specified size.</summary>
    /// <returns>Returns the translated point.</returns>
    public static VecF operator +(VecF pt, VecI sz)
    {
        return new VecF(pt.X + sz.X, pt.Y + sz.Y);
    }

    /// <param name="pt">The point to translate.</param>
    /// <param name="sz">The offset size.</param>
    /// <summary>Translates a given point by a specified size.</summary>
    /// <returns>Returns the translated point.</returns>
    public static VecF operator +(VecF pt, VecD sz)
    {
        return new VecF(pt.X + (float)sz.X, pt.Y + (float)sz.Y);
    }

    /// <param name="pt">The point to translate.</param>
    /// <param name="sz">The offset value.</param>
    /// <summary>Translates a given point by a specified offset.</summary>
    /// <returns>Returns the translated point.</returns>
    public static VecF operator +(VecF pt, VecF sz)
    {
        return new VecF(pt.X + sz.X, pt.Y + sz.Y);
    }

    /// <param name="pt">The <see cref="VecF" /> to translate.</param>
    /// <param name="sz">
    ///     The <see cref="VecI" /> that specifies the numbers to subtract from the coordinates of
    ///     <paramref name="pt" />.
    /// </param>
    /// <summary>Translates a <see cref="VecF" /> by the negative of a given <see cref="VecI" />.</summary>
    /// <returns>The translated <see cref="VecF" />.</returns>
    public static VecF operator -(VecF pt, VecI sz)
    {
        return new VecF(pt.X - sz.X, pt.Y - sz.Y);
    }

    /// <param name="pt">The <see cref="VecF" /> to translate.</param>
    /// <param name="sz">
    ///     The <see cref="VecD" /> that specifies the numbers to subtract from the coordinates of
    ///     <paramref name="pt" />.
    /// </param>
    /// <summary>Translates a <see cref="VecF" /> by the negative of a given <see cref="VecD" />.</summary>
    /// <returns>The translated <see cref="VecF" />.</returns>
    public static VecF operator -(VecF pt, VecD sz)
    {
        return new VecF(pt.X - (float)sz.X, pt.Y - (float)sz.Y);
    }

    /// <param name="pt">The <see cref="VecF" /> to translate.</param>
    /// <param name="sz">The point that specifies the numbers to subtract from the coordinates of <paramref name="pt" />.</param>
    /// <summary>Translates a given point by the negative of a specified offset.</summary>
    /// <returns>The translated <see cref="VecF" />.</returns>
    public static VecF operator -(VecF pt, VecF sz)
    {
        return new VecF(pt.X - sz.X, pt.Y - sz.Y);
    }

    /// <summary>Gets or sets the x-coordinate of this <see cref="VecF" />.</summary>
    /// <value />
    public float X { get; set; }

    /// <summary>Gets or sets the x-coordinate of this <see cref="VecF" />.</summary>
    /// <value />
    public float Y { get; set; }

    /// <param name="obj">The <see cref="VecF" /> to test.</param>
    /// <summary>
    ///     Specifies whether this <see cref="VecF" /> contains the same coordinates as the specified
    ///     <see cref="VecF" />.
    /// </summary>
    /// <returns>
    ///     This method returns true if <paramref name="obj" /> has the same coordinates as this
    ///     <see cref="VecF" />.
    /// </returns>
    public readonly bool Equals(VecF obj)
    {
        return X == (double)obj.X && Y == (double)obj.Y;
    }

    /// <param name="obj">The <see cref="T:System.Object" /> to test.</param>
    /// <summary>
    ///     Specifies whether this <see cref="VecF" /> contains the same coordinates as the specified
    ///     <see cref="T:System.Object" />.
    /// </summary>
    /// <returns>
    ///     This method returns true if <paramref name="obj" /> is a <see cref="VecF" /> and has the same
    ///     coordinates as this <see cref="VecF" />.
    /// </returns>
    /// <remarks />
    public override readonly bool Equals(object obj)
    {
        return obj is VecF Point && Equals(Point);
    }

    /// <param name="left">A <see cref="VecF" /> to compare.</param>
    /// <param name="right">A <see cref="VecF" /> to compare with.</param>
    /// <summary>
    ///     Compares two <see cref="VecF" /> structures. The result specifies whether the values of the
    ///     <see cref="P:.Point.X" /> and <see cref="P:.Point.Y" /> properties of the two
    ///     <see cref="VecF" /> structures are equal.
    /// </summary>
    /// <returns>
    ///     true if the <see cref="P:.Point.X" /> and <see cref="P:.Point.Y" /> values of the left and
    ///     right <see cref="VecF" /> structures are equal; otherwise, false.
    /// </returns>
    public static bool operator ==(VecF left, VecF right)
    {
        return left.Equals(right);
    }

    /// <param name="left">A <see cref="VecF" /> to compare.</param>
    /// <param name="right">A <see cref="VecF" /> to compare with.</param>
    /// <summary>Determines whether the coordinates of the specified points are not equal.</summary>
    /// <returns>
    ///     true if the <see cref="VecF.X" /> and <see cref="VecF.Y" /> values of the left and
    ///     right <see cref="VecF" /> structures differ; otherwise, false.
    /// </returns>
    public static bool operator !=(VecF left, VecF right)
    {
        return !left.Equals(right);
    }
    
    public static explicit operator VecF(VecI vec)
    {
        return new VecF(vec.X, vec.Y);
    }
    
    public static explicit operator VecF(VecD vec)
    {
        return new VecF((float)vec.X, (float)vec.Y);
    }
    
    public static explicit operator VecI(VecF vec)
    {
        return new VecI((int)vec.X, (int)vec.Y);
    }
    
    public static explicit operator VecD(VecF vec)
    {
        return new VecD(vec.X, vec.Y);
    }

    /// <summary>Calculates the hashcode for this point.</summary>
    /// <returns>Returns the hashcode for this point.</returns>
    /// <remarks>
    ///     You should avoid depending on GetHashCode for unique values, as two <see cref="T:System.Drawing.Point" />
    ///     objects with the same values for their X and Y properties may return the same hash code. This behavior could change
    ///     in a future release.
    /// </remarks>
    public override readonly int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(X);
        hashCode.Add(Y);
        return hashCode.ToHashCode();
    }

    public VecF Divide(float by)
    {
        return new VecF(X / by, Y / by);
    }
}
