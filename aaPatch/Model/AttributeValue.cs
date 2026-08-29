using System.Globalization;
using System.Linq.Dynamic.Core.CustomTypeProviders;
using System.Text.RegularExpressions;

namespace aaPatch.Model;

/// <summary>
/// Represents an immutable wrapper for a value, allowing for comparison and operations
/// with various data types. This class is designed to encapsulate and unify different
/// primitive and object types into a single model to be used with Dynamic LINQ expressions.
/// </summary>
[DynamicLinqType]
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
    /// Gets the type of the underlying value encapsulated by the <see cref="AttributeValue"/> instance.
    /// This property returns the runtime type of the value stored within, or null if no value is set.
    /// It enables type inspection for the encapsulated data consistently.
    /// </summary>
    public Type Type => _value?.GetType() ?? typeof(object);

    /// <summary>
    /// Indicates whether the encapsulated value within the <see cref="AttributeValue"/> instance is null.
    /// Returns true if the underlying value is null, otherwise false.
    /// </summary>
    public bool IsNull => _value is null;

    /// <summary>
    /// Retrieves the encapsulated value of the current <see cref="AttributeValue"/> instance.
    /// </summary>
    /// <returns>
    /// The underlying value stored in the <see cref="AttributeValue"/> instance,
    /// or null if no value is present.
    /// </returns>
    public object? GetValue() => _value;

    /// <summary>
    /// Converts the wrapped value of the current <see cref="AttributeValue"/> instance to the specified type
    /// and returns a new <see cref="AttributeValue"/> instance containing the converted value.
    /// </summary>
    /// <param name="type">The <see cref="System.Type"/> to which the current value should be cast.
    /// Must be a valid type compatible with the current value.</param>
    /// <returns>A new <see cref="AttributeValue"/> instance containing the value cast to the specified type.</returns>
    public AttributeValue As(Type type) => new(Convert.ChangeType(_value, type));

    /// <summary>
    /// Determines whether the current instance contains the specified text.
    /// </summary>
    /// <param name="text">The text to search for within the current instance.
    /// The comparison is case-insensitive by default.</param>
    /// <returns>True if the specified text is found; otherwise, false.</returns>
    public bool Contains(string text) => ContainsText(text, false);

    /// <summary>
    /// Determines whether the encapsulated value contains the specified text,
    /// with an option to consider case sensitivity.
    /// </summary>
    /// <param name="text">The text to search for within the encapsulated value.</param>
    /// <param name="matchCase">A boolean value indicating whether the comparison
    /// should be case-sensitive. If true, the comparison is case-sensitive;
    /// otherwise, the comparison is case-insensitive.</param>
    /// <returns>Returns true if the encapsulated value contains the specified text;
    /// otherwise, false. If the encapsulated value is null, the method returns false.</returns>
    public bool Contains(string text, bool matchCase) => ContainsText(text, matchCase);

    /// <summary>
    /// Determines if the string representation of the current value matches the specified
    /// SQL-style 'LIKE' pattern. The pattern may include '%' as a wildcard for zero or more
    /// characters and '?' as a wildcard for a single character.
    /// </summary>
    /// <param name="pattern">The SQL-style pattern to compare against the value.
    /// The pattern supports '%' as a multi-character wildcard and '?' as a single-character wildcard.</param>
    /// <returns>Returns <c>true</c> if the string representation of the current value matches
    /// the specified pattern; otherwise, <c>false</c>.</returns>
    public bool Like(string pattern) => LikePattern(pattern);

    /// <summary>
    /// Determines whether the encapsulated value matches the specified regular expression pattern.
    /// </summary>
    /// <param name="pattern">The regular expression pattern to match against the current value.</param>
    /// <returns><c>true</c> if the encapsulated value matches the specified pattern; otherwise, <c>false</c>.</returns>
    public bool Matches(string pattern) => MatchesPattern(pattern);

    /// <summary>
    /// Replaces all occurrences of the specified substring in the current value with a new string
    /// and returns a new <see cref="AttributeValue"/> instance containing the replaced value.
    /// </summary>
    /// <param name="find">The substring to locate in the current value.</param>
    /// <param name="replace">The string to replace all occurrences of <paramref name="find"/>.</param>
    /// <returns>A new <see cref="AttributeValue"/> instance containing the value after replacing
    /// occurrences of <paramref name="find"/> with <paramref name="replace"/>.</returns>
    public AttributeValue Replace(string find, string replace) => new(ReplaceText(find, replace, false));

    /// <summary>
    /// Replaces all occurrences of a specified string in the current <see cref="AttributeValue"/>
    /// with another specified string and returns a new <see cref="AttributeValue"/> containing
    /// the result.
    /// </summary>
    /// <param name="find">The string to be replaced.</param>
    /// <param name="replace">The string to replace all occurrences of <paramref name="find"/>.</param>
    /// <param name="match">If true, performs case-sensitive matching; if false, performs case-insensitive matching.</param>
    /// <returns>A new <see cref="AttributeValue"/> with the result of the replacement operation.</returns>
    public AttributeValue Replace(string find, string replace, bool match) => new(ReplaceText(find, replace, match));

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
            (_, AttributeValue other) => CompareTo(other._value),
            (null, null) => 0,
            (_, null) => 1,
            (null, _) => -1,
            (IComparable left, IComparable right) => CompareValues(left, right),
            _ => throw new ArgumentException($"Object must be comparable. Type: {obj.GetType().Name}", nameof(obj))
        };

        static int CompareValues(IComparable left, IComparable right)
        {
            // As long as the types match, just dispatch to the correct comparison method.
            if (left.GetType() == right.GetType())
                return left.CompareTo(right);

            // To support string-typed numeric data, try and convert to match the supplied type.
            // This is useful since typically CSV/AVEVA attribute values are strings.
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
            AttributeValue other => Equals(other._value),
            not null when _value is null => false,
            null => _value is null,
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
    /// Implicitly converts a short value to an <see cref="AttributeValue"/> instance.
    /// </summary>
    /// <param name="value">The short value to convert.</param>
    /// <returns>A new <see cref="AttributeValue"/> instance wrapping the specified short value.</returns>
    public static implicit operator AttributeValue(short value) => new(value);

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

    /// <summary>
    /// Determines whether the current value matches the specified pattern.
    /// The pattern supports wildcard characters '%' (matches zero or more characters)
    /// and '?' (matches a single character).
    /// </summary>
    /// <param name="pattern">The pattern to match against. Supports SQL-style wildcards.</param>
    /// <returns>True if the current value matches the specified pattern; otherwise, false.</returns>
    private bool LikePattern(string pattern)
    {
        if (_value is null)
            return false;

        var regexPattern = "^" + Regex.Escape(pattern)
            .Replace("\\?", ".")
            .Replace("%", ".*") + "$";

        return Regex.IsMatch(ToString(), regexPattern);
    }

    /// <summary>
    /// Determines whether the current value matches the specified regular expression pattern.
    /// </summary>
    /// <param name="pattern">The regular expression pattern to match against the string representation of the value.</param>
    /// <returns>True if the value matches the specified pattern; otherwise, false.</returns>
    private bool MatchesPattern(string pattern)
    {
        if (_value is null)
            return false;

        return Regex.IsMatch(ToString(), pattern);
    }

    /// <summary>
    /// Determines whether the value contains the specified text, optionally considering case sensitivity.
    /// </summary>
    /// <param name="text">The text to search for within the value.</param>
    /// <param name="matchCase">A flag indicating whether the search should be case-sensitive. If true, the search is case-sensitive; otherwise, it is case-insensitive.</param>
    /// <returns>True if the value contains the specified text; otherwise, false.</returns>
    private bool ContainsText(string text, bool matchCase)
    {
        if (_value is null)
            return false;

        var comparison = matchCase ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;
        return ToString().Contains(text, comparison);
    }

    /// <summary>
    /// Replaces occurrences of a specified substring within the current value with another substring,
    /// optionally using case sensitivity.
    /// </summary>
    /// <param name="find">The substring to locate within the current value.</param>
    /// <param name="replace">The substring to replace the located substring with.</param>
    /// <param name="matchCase">A boolean value indicating whether the search should be case-sensitive.</param>
    /// <returns>A modified object where the specified substring is replaced, preserving the original type.</returns>
    private object ReplaceText(string find, string replace, bool matchCase)
    {
        if (_value is null)
            return this;

        var comparison = matchCase ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;
        var result = ToString().Replace(find, replace, comparison);
        return Convert.ChangeType(result, _value.GetType());
    }
}