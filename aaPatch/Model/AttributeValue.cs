using System.Globalization;

namespace aaPatch.Model;

/// <summary>
/// Represents an immutable wrapper for a value, allowing for comparison and operations
/// with various data types. This class is designed to encapsulate and unify different
/// primitive and object types into a single model.
/// </summary>
public sealed class AttributeValue : IEquatable<AttributeValue>, IComparable
{
    /// <summary>
    /// Holds the underlying value encapsulated by the <see cref="AttributeValue"/> class.
    /// This value can represent various data types, including primitives and objects.
    /// It is immutable and serves as the core data stored in an <see cref="AttributeValue"/> instance.
    /// </summary>
    private readonly object? _value;

    /// <summary>
    /// Initializes a new instance of the <see cref="AttributeValue"/> class with the specified value.
    /// </summary>
    /// <param name="value">The value to wrap. If the value is already an <see cref="AttributeValue"/>,
    /// its underlying value is extracted to avoid double-wrapping. Can be null.</param>
    public AttributeValue(object? value)
    {
        _value = value is AttributeValue wrapped ? wrapped._value : value;
    }

    /// <summary>
    /// Indicates whether the encapsulated value within the <see cref="AttributeValue"/> instance is null.
    /// Returns true if the underlying value is null, otherwise false.
    /// </summary>
    public bool IsNull => _value is null;

    /// <summary>
    /// Gets the type of the underlying value encapsulated by the <see cref="AttributeValue"/> instance.
    /// This property returns the runtime type of the value stored within, or null if no value is set.
    /// It enables type inspection for the encapsulated data consistently.
    /// </summary>
    public Type ValueType => _value?.GetType() ?? typeof(object);

    /// <summary>
    /// Converts the wrapped value of the current <see cref="AttributeValue"/> instance to the specified type
    /// and returns a new <see cref="AttributeValue"/> instance containing the converted value.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> to which the current value should be cast.
    /// Must be a valid type compatible with the current value.</param>
    /// <returns>A new <see cref="AttributeValue"/> instance containing the value cast to the specified type.</returns>
    public AttributeValue CastTo(Type type)
    {
        var typed = Convert.ChangeType(_value, type);
        return new AttributeValue(typed);
    }

    /// <summary>
    /// Returns a string representation of the value wrapped by the <see cref="AttributeValue"/> class.
    /// </summary>
    /// <returns>
    /// A string representation of the underlying value. If the value is a boolean, it is converted to "true" or "false".
    /// If the value is null, an empty string is returned. Otherwise, the value's <see cref="object.ToString"/> implementation is used.
    /// </returns>
    public override string ToString()
    {
        if (_value is bool b)
        {
            return b ? "true" : "false";
        }

        return _value?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Compares the current instance of <see cref="AttributeValue"/> with another object and returns an integer indicating their relative order.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance. Must be of a comparable type or an <see cref="AttributeValue"/> instance.</param>
    /// <returns>
    /// A signed integer that indicates the relative order of the objects being compared:
    /// - A value less than zero indicates that this instance precedes <paramref name="obj"/> in the sort order.
    /// - Zero indicates that this instance occurs in the same position in the sort order as <paramref name="obj"/>.
    /// - A value greater than zero indicates that this instance follows <paramref name="obj"/> in the sort order.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown if the object is not comparable or not an <see cref="AttributeValue"/> instance.</exception>
    public int CompareTo(object? obj)
    {
        return (_value, obj) switch
        {
            _ when ReferenceEquals(this, obj) => 0,
            (_, null) => 1,
            (null, _) => -1,
            (_, AttributeValue other) => CompareTo(other._value),
            (IComparable left, IComparable right) => CompareValues(left, right),
            _ => throw new ArgumentException($"Object must be comparable. Type: {obj.GetType().Name}", nameof(obj))
        };

        static int CompareValues(IComparable left, IComparable right)
        {
            // As long as the types match, just dispatch to the correct comparison method.
            if (left.GetType() == right.GetType())
                return left.CompareTo(right);

            // To support string-typed numeric data, try and convert to match the supplied type.
            // This is specifically useful since typically CSV/AVEVA attribute values are strings.
            if (left is string text && (IsNumeric(right) || right is bool))
            {
                var converted = Convert.ChangeType(
                    text,
                    right.GetType(),
                    CultureInfo.InvariantCulture);

                return ((IComparable)converted).CompareTo(right);
            }

            // If we have different numeric types, promote both to a common numeric that can preserve both values.
            if (IsNumeric(left) && IsNumeric(right))
            {
                var leftNumber = Convert.ToDecimal(left, CultureInfo.InvariantCulture);
                var rightNumber = Convert.ToDecimal(right, CultureInfo.InvariantCulture);
                return leftNumber.CompareTo(rightNumber);
            }

            throw new InvalidOperationException(
                $"Values of types '{left.GetType().Name}' and '{right.GetType().Name}' cannot be compared.");
        }
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current <see cref="AttributeValue"/> instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance. Can be null.</param>
    /// <returns>
    /// <c>true</c> if the specified object is equal to the current instance; otherwise, <c>false</c>.
    /// Returns <c>true</c> if the object is the same reference as the current instance, or if the object
    /// is an <see cref="AttributeValue"/> with an equal underlying value, or if the object itself equals
    /// the underlying value. Returns <c>false</c> if the object is null.
    /// </returns>
    public override bool Equals(object? obj)
    {
        return obj switch
        {
            not null when ReferenceEquals(obj, this) => true,
            not null when _value is null => false,
            null => _value is null,
            AttributeValue other => Equals(other._value),
            _ => AreEqual(_value, obj)
        };

        static bool AreEqual(object left, object right)
        {
            // As long as the types match, just dispatch to the typed equality comparison.
            if (left.GetType() == right.GetType())
                return Equals(left, right);

            // To support string-typed numeric data, try and convert to match the supplied type.
            // This is specifically useful since typically CSV/AVEVA attribute values are strings.
            if (left is string text && (IsNumeric(right) || right is bool))
            {
                var converted = Convert.ChangeType(
                    text,
                    right.GetType(),
                    CultureInfo.InvariantCulture);

                return Equals(converted, right);
            }

            // If we have different numeric types, promote both to a common numeric that can preserve both values.
            if (IsNumeric(left) && IsNumeric(right))
            {
                var leftNumber = Convert.ToDecimal(left, CultureInfo.InvariantCulture);
                var rightNumber = Convert.ToDecimal(right, CultureInfo.InvariantCulture);
                return Equals(leftNumber, rightNumber);
            }

            return Equals(left, right);
        }
    }

    /// <summary>
    /// Determines whether the current <see cref="AttributeValue"/> instance is equal to a specified <see cref="AttributeData"/> instance.
    /// </summary>
    /// <param name="other">The <see cref="AttributeData"/> instance to compare with the current <see cref="AttributeValue"/>. Can be null.</param>
    /// <returns><c>true</c> if the current instance is equal to the specified <see cref="AttributeData"/> instance; otherwise, <c>false</c>.</returns>
    public bool Equals(AttributeValue? other) => Equals(other as object);

    /// <summary>
    /// Returns the hash code for the current <see cref="AttributeValue"/> instance.
    /// </summary>
    /// <returns>
    /// The hash code of the underlying value, or 0 if the underlying value is null.
    /// </returns>
    public override int GetHashCode() => _value?.GetHashCode() ?? 0;

    /// <summary>
    /// Determines whether two <see cref="AttributeValue"/> instances are equal.
    /// </summary>
    /// <param name="left">The first <see cref="AttributeValue"/> instance to compare. Can be null.</param>
    /// <param name="right">The second <see cref="AttributeValue"/> instance to compare. Can be null.</param>
    /// <returns><c>true</c> if the two instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(AttributeValue? left, AttributeValue? right) => Equals(left, right);

    /// <summary>
    /// Determines whether two <see cref="AttributeValue"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="AttributeValue"/> instance to compare. Can be null.</param>
    /// <param name="right">The second <see cref="AttributeValue"/> instance to compare. Can be null.</param>
    /// <returns><c>true</c> if the two instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(AttributeValue? left, AttributeValue? right) => !Equals(left, right);

    /// <summary>
    /// Determines whether one <see cref="AttributeValue"/> instance is less than another.
    /// </summary>
    /// <param name="left">The first <see cref="AttributeValue"/> instance to compare. Can be null.</param>
    /// <param name="right">The second <see cref="AttributeValue"/> instance to compare. Can be null.</param>
    /// <returns><c>true</c> if the left instance is less than the right instance; otherwise, <c>false</c>.</returns>
    public static bool operator <(AttributeValue? left, AttributeValue? right) =>
        left is null ? right is not null : left.CompareTo(right) < 0;

    /// <summary>
    /// Determines whether one <see cref="AttributeValue"/> instance is greater than another.
    /// </summary>
    /// <param name="left">The first <see cref="AttributeValue"/> instance to compare. Can be null.</param>
    /// <param name="right">The second <see cref="AttributeValue"/> instance to compare. Can be null.</param>
    /// <returns><c>true</c> if the left instance is greater than the right instance; otherwise, <c>false</c>.</returns>
    public static bool operator >(AttributeValue? left, AttributeValue? right) =>
        left is not null && left.CompareTo(right) > 0;

    /// <summary>
    /// Determines whether one <see cref="AttributeValue"/> instance is less than or equal to another.
    /// </summary>
    /// <param name="left">The first <see cref="AttributeValue"/> instance to compare. Can be null.</param>
    /// <param name="right">The second <see cref="AttributeValue"/> instance to compare. Can be null.</param>
    /// <returns><c>true</c> if the left instance is less than or equal to the right instance; otherwise, <c>false</c>.</returns>
    public static bool operator <=(AttributeValue? left, AttributeValue? right) =>
        left is null || left.CompareTo(right) <= 0;

    /// <summary>
    /// Determines whether one <see cref="AttributeValue"/> instance is greater than or equal to another.
    /// </summary>
    /// <param name="left">The first <see cref="AttributeValue"/> instance to compare. Can be null.</param>
    /// <param name="right">The second <see cref="AttributeValue"/> instance to compare. Can be null.</param>
    /// <returns><c>true</c> if the left instance is greater than or equal to the right instance; otherwise, <c>false</c>.</returns>
    public static bool operator >=(AttributeValue? left, AttributeValue? right) =>
        left is null ? right is null : left.CompareTo(right) >= 0;

    /// <summary>
    /// Implicitly converts a boolean value to an <see cref="AttributeValue"/> instance.
    /// </summary>
    /// <param name="value">The boolean value to convert.</param>
    /// <returns>A new <see cref="AttributeValue"/> instance wrapping the specified boolean value.</returns>
    public static implicit operator AttributeValue(bool value) => new(value);

    /// <summary>
    /// Implicitly converts an integer value to an <see cref="AttributeValue"/> instance.
    /// </summary>
    /// <param name="value">The integer value to convert.</param>
    /// <returns>A new <see cref="AttributeValue"/> instance wrapping the specified integer value.</returns>
    public static implicit operator AttributeValue(int value) => new(value);

    /// <summary>
    /// Implicitly converts a long integer value to an <see cref="AttributeValue"/> instance.
    /// </summary>
    /// <param name="value">The long integer value to convert.</param>
    /// <returns>A new <see cref="AttributeValue"/> instance wrapping the specified long integer value.</returns>
    public static implicit operator AttributeValue(long value) => new(value);

    /// <summary>
    /// Implicitly converts a double-precision floating-point value to an <see cref="AttributeValue"/> instance.
    /// </summary>
    /// <param name="value">The double value to convert.</param>
    /// <returns>A new <see cref="AttributeValue"/> instance wrapping the specified double value.</returns>
    public static implicit operator AttributeValue(double value) => new(value);

    /// <summary>
    /// Implicitly converts a string value to an <see cref="AttributeValue"/> instance.
    /// </summary>
    /// <param name="value">The string value to convert.</param>
    /// <returns>A new <see cref="AttributeValue"/> instance wrapping the specified string value.</returns>
    public static implicit operator AttributeValue(string value) => new(value);

    /// <summary>
    /// Determines whether the specified value is of a numeric type.
    /// </summary>
    /// <param name="value">The value to check. Can be of any type.</param>
    /// <returns>
    /// <c>true</c> if the specified value is a numeric type; otherwise, <c>false</c>.
    /// </returns>
    private static bool IsNumeric(object value)
    {
        return value
            is byte or sbyte
            or short or ushort
            or int or uint
            or long or ulong
            or float or double or decimal;
    }
}