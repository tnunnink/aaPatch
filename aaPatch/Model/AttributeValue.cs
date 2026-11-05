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
    /// Provides a standardized culture setting for operations within the <see cref="AttributeValue"/> class.
    /// This culture setting is based on <see cref="CultureInfo.InvariantCulture"/> to ensure consistent
    /// formatting and parsing behaviors, particularly for numeric and string conversions.
    /// </summary>
    private static CultureInfo Culture => CultureInfo.InvariantCulture;

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
    /// Indicates whether the underlying value of the <see cref="AttributeValue"/> instance is null.
    /// Returns <c>true</c> if the internal value is null; otherwise, <c>false</c>.
    /// </summary>
    public bool IsNull => _value is null;

    /// <summary>
    /// Represents a predefined, immutable instance of <see cref="AttributeValue"/> with a value of <c>null</c>.
    /// This property can be used to explicitly signify a null or unset state within the <see cref="AttributeValue"/> context.
    /// </summary>
    public static AttributeValue Null { get; } = new(null);

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
    /// Extracts a captured group from the first match of a regular expression against the value's string representation.
    /// </summary>
    /// <param name="pattern">The regular expression pattern to match.</param>
    /// <param name="group">The zero-based capture group index. Group zero is the entire match.</param>
    /// <returns>The captured group, or <see cref="Null"/> when the value is null, the pattern does not match,
    /// or the requested group does not exist or capture a value.</returns>
    public AttributeValue Extract(string pattern, int group)
    {
        if (_value is null || group < 0)
            return Null;

        var match = Regex.Match(ToString(), pattern);
        if (!match.Success || group >= match.Groups.Count || !match.Groups[group].Success)
            return Null;

        return new AttributeValue(match.Groups[group].Value);
    }

    /// <summary>
    /// Extracts the first capture group from the first regular expression match.
    /// </summary>
    /// <param name="pattern">The regular expression pattern to match.</param>
    /// <returns>The first captured group, or <see cref="Null"/> when there is no match or capture.</returns>
    public AttributeValue Extract(string pattern) => Extract(pattern, 1);

    /// <summary>
    /// Splits the value's string representation by a separator and returns the token at the specified index.
    /// Negative indices count from the end of the token list.
    /// </summary>
    /// <param name="separator">The literal separator used to split the value.</param>
    /// <param name="index">The zero-based token index, or a negative index counted from the end.</param>
    /// <returns>The selected token, or <see cref="Null"/> when the value is null, the separator is null,
    /// or the index is outside the token list.</returns>
    public AttributeValue Split(string? separator, int index)
    {
        if (_value is null || separator is null)
            return Null;

        var text = ToString();
        var tokens = separator.Length == 0
            ? [text]
            : text.Split(separator);
        var resolvedIndex = index < 0 ? (long)tokens.Length + index : index;

        return resolvedIndex < 0 || resolvedIndex >= tokens.Length
            ? Null
            : new AttributeValue(tokens[(int)resolvedIndex]);
    }

    /// <summary>
    /// Splits the value's string representation by a separator and returns the first token.
    /// </summary>
    /// <param name="separator">The literal separator used to split the value.</param>
    /// <returns>The first token, or <see cref="Null"/> when the value is null or the separator is null.</returns>
    public AttributeValue Split(string? separator) => Split(separator, 0);

    /// <summary>
    /// Returns a bounds-safe substring, interpreting a negative start index as an offset from the end.
    /// </summary>
    /// <param name="start">The zero-based start index, or a negative offset from the end of the value.</param>
    /// <param name="length">The requested substring length. Negative lengths are treated as zero.</param>
    /// <returns>The substring with the start and length clamped to the value's bounds, or <see cref="Null"/>
    /// when the underlying value is null.</returns>
    public AttributeValue Slice(int start, int length)
    {
        if (_value is null)
            return Null;

        var text = ToString();
        var resolvedStart = start < 0 ? text.Length + (long)start : start;
        var clampedStart = (int)Math.Clamp(resolvedStart, 0L, text.Length);
        var clampedLength = Math.Clamp(length, 0, text.Length - clampedStart);

        return new AttributeValue(text.Substring(clampedStart, clampedLength));
    }

    /// <summary>
    /// Returns the substring from the specified start index to the end of the value.
    /// Negative start indices count backwards from the end.
    /// </summary>
    /// <param name="start">The zero-based start index, or a negative offset from the end of the value.</param>
    /// <returns>The substring with the start index clamped to the value's bounds, or <see cref="Null"/>
    /// when the underlying value is null.</returns>
    public AttributeValue Slice(int start) => Slice(start, int.MaxValue);

    /// <summary>
    /// Returns the text before the first occurrence of a delimiter.
    /// </summary>
    /// <param name="delimiter">The literal delimiter to search for.</param>
    /// <returns>The text before the delimiter, or <see cref="Null"/> if the value is null or the delimiter is not found.</returns>
    public AttributeValue Before(string? delimiter)
    {
        if (_value is null || delimiter is null)
            return Null;

        var text = ToString();
        var index = text.IndexOf(delimiter, StringComparison.Ordinal);
        return index < 0 ? Null : new AttributeValue(text[..index]);
    }

    /// <summary>
    /// Returns the text after the first occurrence of a delimiter.
    /// </summary>
    /// <param name="delimiter">The literal delimiter to search for.</param>
    /// <returns>The text after the delimiter, or <see cref="Null"/> if the value is null or the delimiter is not found.</returns>
    public AttributeValue After(string? delimiter)
    {
        if (_value is null || delimiter is null)
            return Null;

        var text = ToString();
        var index = text.IndexOf(delimiter, StringComparison.Ordinal);
        return index < 0 ? Null : new AttributeValue(text[(index + delimiter.Length)..]);
    }

    /// <summary>
    /// Returns the text before the last occurrence of a delimiter.
    /// </summary>
    /// <param name="delimiter">The literal delimiter to search for.</param>
    /// <returns>The text before the last delimiter, or <see cref="Null"/> if the value is null or the delimiter is not found.</returns>
    public AttributeValue BeforeLast(string? delimiter)
    {
        if (_value is null || delimiter is null)
            return Null;

        var text = ToString();
        var index = text.LastIndexOf(delimiter, StringComparison.Ordinal);
        return index < 0 ? Null : new AttributeValue(text[..index]);
    }

    /// <summary>
    /// Returns the text after the last occurrence of a delimiter.
    /// </summary>
    /// <param name="delimiter">The literal delimiter to search for.</param>
    /// <returns>The text after the last delimiter, or <see cref="Null"/> if the value is null or the delimiter is not found.</returns>
    public AttributeValue AfterLast(string? delimiter)
    {
        if (_value is null || delimiter is null)
            return Null;

        var text = ToString();
        var index = text.LastIndexOf(delimiter, StringComparison.Ordinal);
        return index < 0 ? Null : new AttributeValue(text[(index + delimiter.Length)..]);
    }

    /// <summary>
    /// Left-pads the value's string representation to a minimum width.
    /// </summary>
    /// <param name="totalWidth">The minimum resulting width.</param>
    /// <param name="paddingChar">The string repeated to fill the left side. Defaults to <c>"0"</c>.</param>
    /// <returns>The padded value, or <see cref="Null"/> when the underlying value is null.</returns>
    public AttributeValue PadLeft(int totalWidth, string paddingChar = "0")
    {
        if (_value is null)
            return Null;

        var text = ToString();
        var requiredPaddingLength = totalWidth - text.Length;
        if (requiredPaddingLength <= 0 || string.IsNullOrEmpty(paddingChar))
            return new AttributeValue(text);

        var repeatCount = (requiredPaddingLength - 1) / paddingChar.Length + 1;
        var padding = string.Concat(Enumerable.Repeat(paddingChar, repeatCount));
        return new AttributeValue(padding[^requiredPaddingLength..] + text);
    }

    /// <summary>
    /// Removes leading and trailing whitespace from the value's string representation.
    /// </summary>
    /// <returns>The trimmed value, or <see cref="Null"/> when the underlying value is null.</returns>
    public AttributeValue Trim() => _value is null ? Null : new AttributeValue(ToString().Trim());

    /// <summary>
    /// Converts the value's string representation to invariant uppercase.
    /// </summary>
    /// <returns>The uppercase value, or <see cref="Null"/> when the underlying value is null.</returns>
    public AttributeValue ToUpper() => _value is null ? Null : new AttributeValue(ToString().ToUpperInvariant());

    /// <summary>
    /// Converts the value's string representation to invariant lowercase.
    /// </summary>
    /// <returns>The lowercase value, or <see cref="Null"/> when the underlying value is null.</returns>
    public AttributeValue ToLower() => _value is null ? Null : new AttributeValue(ToString().ToLowerInvariant());

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
                var x = Convert.ToDecimal(left, CultureInfo.InvariantCulture);
                var y = Convert.ToDecimal(right, CultureInfo.InvariantCulture);
                return x.CompareTo(y);
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
    public static bool operator ==(AttributeValue? left, AttributeValue? right) => Equals(left ?? Null, right ?? Null);

    /// <summary>
    /// Determines whether two <see cref="AttributeValue"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first <see cref="AttributeValue"/> instance to compare. Can be null.</param>
    /// <param name="right">The second <see cref="AttributeValue"/> instance to compare. Can be null.</param>
    /// <returns><c>true</c> if the two instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(AttributeValue? left, AttributeValue? right) => !Equals(left ?? Null, right ?? Null);

    /// <summary>
    /// Determines whether one <see cref="AttributeValue"/> instance is less than another.
    /// </summary>
    /// <param name="left">The first <see cref="AttributeValue"/> instance to compare. Can be null.</param>
    /// <param name="right">The second <see cref="AttributeValue"/> instance to compare. Can be null.</param>
    /// <returns><c>true</c> if the left instance is less than the right instance; otherwise, <c>false</c>.</returns>
    public static bool operator <(AttributeValue? left, AttributeValue? right) =>
        Comparable(left, right) && left!.CompareTo(right) < 0;

    /// <summary>
    /// Determines whether one <see cref="AttributeValue"/> instance is greater than another.
    /// </summary>
    /// <param name="left">The first <see cref="AttributeValue"/> instance to compare. Can be null.</param>
    /// <param name="right">The second <see cref="AttributeValue"/> instance to compare. Can be null.</param>
    /// <returns><c>true</c> if the left instance is greater than the right instance; otherwise, <c>false</c>.</returns>
    public static bool operator >(AttributeValue? left, AttributeValue? right) =>
        Comparable(left, right) && left!.CompareTo(right) > 0;

    /// <summary>
    /// Determines whether one <see cref="AttributeValue"/> instance is less than or equal to another.
    /// </summary>
    /// <param name="left">The first <see cref="AttributeValue"/> instance to compare. Can be null.</param>
    /// <param name="right">The second <see cref="AttributeValue"/> instance to compare. Can be null.</param>
    /// <returns><c>true</c> if the left instance is less than or equal to the right instance; otherwise, <c>false</c>.</returns>
    public static bool operator <=(AttributeValue? left, AttributeValue? right) =>
        Comparable(left, right) && left!.CompareTo(right) <= 0;

    /// <summary>
    /// Determines whether one <see cref="AttributeValue"/> instance is greater than or equal to another.
    /// </summary>
    /// <param name="left">The first <see cref="AttributeValue"/> instance to compare. Can be null.</param>
    /// <param name="right">The second <see cref="AttributeValue"/> instance to compare. Can be null.</param>
    /// <returns><c>true</c> if the left instance is greater than or equal to the right instance; otherwise, <c>false</c>.</returns>
    public static bool operator >=(AttributeValue? left, AttributeValue? right) =>
        Comparable(left, right) && left!.CompareTo(right) >= 0;

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
    /// Implicitly converts an <see cref="AttributeValue"/> instance to a boolean value.
    /// </summary>
    /// <param name="value">The <see cref="AttributeValue"/> instance to convert.</param>
    /// <returns>
    /// <c>true</c> if the underlying value is a boolean <c>true</c>; otherwise, <c>false</c>.
    /// </returns>
    public static implicit operator bool(AttributeValue value) => value._value is true;

    /// <summary>
    /// Implicitly converts an <see cref="AttributeValue"/> instance to a 32-bit signed integer.
    /// </summary>
    /// <param name="value">The <see cref="AttributeValue"/> instance to convert.</param>
    /// <returns>A 32-bit signed integer representation of the underlying value.</returns>
    public static implicit operator int(AttributeValue value) => Convert.ToInt32(value._value, Culture);

    /// <summary>
    /// Implicitly converts an <see cref="AttributeValue"/> instance to a 64-bit signed integer.
    /// </summary>
    /// <param name="value">The <see cref="AttributeValue"/> instance to convert.</param>
    /// <returns>A 64-bit signed integer representation of the underlying value.</returns>
    public static implicit operator long(AttributeValue value) => Convert.ToInt64(value._value, Culture);

    /// <summary>
    /// Implicitly converts an <see cref="AttributeValue"/> instance to a single-precision floating-point number.
    /// </summary>
    /// <param name="value">The <see cref="AttributeValue"/> instance to convert.</param>
    /// <returns>A single-precision floating-point number representation of the underlying value.</returns>
    public static implicit operator float(AttributeValue value) => Convert.ToSingle(value._value, Culture);

    /// <summary>
    /// Implicitly converts an <see cref="AttributeValue"/> instance to a double-precision floating-point number.
    /// </summary>
    /// <param name="value">The <see cref="AttributeValue"/> instance to convert.</param>
    /// <returns>A double-precision floating-point number representation of the underlying value.</returns>
    public static implicit operator double(AttributeValue value) => Convert.ToDouble(value._value, Culture);

    /// <summary>
    /// Implicitly converts an <see cref="AttributeValue"/> instance to a decimal number.
    /// </summary>
    /// <param name="value">The <see cref="AttributeValue"/> instance to convert.</param>
    /// <returns>A decimal number representation of the underlying value.</returns>
    public static implicit operator decimal(AttributeValue value) => Convert.ToDecimal(value._value, Culture);

    /// <summary>
    /// Implicitly converts an <see cref="AttributeValue"/> instance to a string.
    /// </summary>
    /// <param name="value">The <see cref="AttributeValue"/> instance to convert.</param>
    /// <returns>A string representation of the underlying value.</returns>
    public static implicit operator string(AttributeValue value) => value.ToString();

    /// <summary>
    /// Determines whether two <see cref="AttributeValue"/> instances can be compared.
    /// Both instances must have non-null underlying values to be considered comparable.
    /// </summary>
    /// <param name="left">The first <see cref="AttributeValue"/> instance to check. Can be null.</param>
    /// <param name="right">The second <see cref="AttributeValue"/> instance to check. Can be null.</param>
    /// <returns>
    /// <c>true</c> if both <see cref="AttributeValue"/> instances have non-null underlying values;
    /// otherwise, <c>false</c>.
    /// </returns>
    private static bool Comparable(AttributeValue? left, AttributeValue? right) =>
        left?._value is not null && right?._value is not null;

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