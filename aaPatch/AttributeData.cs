namespace aaPatch;

/// <summary>
/// Represents attribute data consisting of a header, a type, a name, and an associated value.
/// </summary>
/// <remarks>
/// The <see cref="AttributeData"/> class is designed to parse attribute-related information
/// from a given header string and maintain an optional value associated with the attribute.
/// The header is expected to follow a specific format for accurate parsing of the name
/// and type.
/// </remarks>
public class AttributeData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AttributeData"/> class with the specified header and optional value.
    /// </summary>
    /// <remarks>
    /// The header string is parsed to extract the attribute's name and type information.
    /// The header is expected to follow the format "Name(TypeName)" where TypeName is a valid type identifier
    /// such as MxBoolean, MxInteger, MxFloat, MxDouble, or defaults to string if no type is specified.
    /// </remarks>
    /// <param name="header">The header string containing the attribute name and optional type declaration. Cannot be null or empty.</param>
    /// <param name="value">The optional raw string value associated with the attribute. Can be null.</param>
    /// <exception cref="ArgumentException">Thrown when the header parameter is null or empty.</exception>
    public AttributeData(string header, string? value)
    {
        if (string.IsNullOrEmpty(header))
            throw new ArgumentException("Header cannot be null or empty.", nameof(header));

        Header = header;
        Value = ParseValue(ParseType(Header), value);
    }

    /// <summary>
    /// Gets the header string that defines the attribute's name and optional type.
    /// </summary>
    /// <remarks>
    /// The header string serves as the primary input used to extract the attribute's name and type information.
    /// It is expected to follow a specific format, typically "Name(TypeName)", where the TypeName is optional.
    /// If no type is explicitly defined in the header, it defaults to string.
    /// This property is immutable and set during the initialization of the <see cref="AttributeData"/> instance.
    /// </remarks>
    public string Header { get; }

    /// <summary>
    /// Gets the name derived from the header field.
    /// </summary>
    /// <remarks>
    /// The name is extracted from the header string by isolating the portion
    /// preceding the first occurrence of a type definition enclosed in parentheses.
    /// If no such portion exists, the full header string is returned as the name.
    /// </remarks>
    public string Name => ParseName(Header);

    /// <summary>
    /// Retrieves the parsed value associated with the attribute data.
    /// </summary>
    /// <remarks>
    /// The value is derived by interpreting the raw input string based on the
    /// type specified in the header. If the raw value is null, the result will
    /// also be null. The type of the returned object can vary and depends on
    /// the type specified in the header (e.g., string, int, bool, etc.).
    /// </remarks>
    public object? Value { get; }

    /// <summary>
    /// Updates the value of the current <see cref="AttributeData"/> instance with the specified string value.
    /// </summary>
    /// <param name="value">
    /// The new raw string value to be associated with this <see cref="AttributeData"/> instance.
    /// Can be null, in which case the value will be cleared or reset based on the type.
    /// </param>
    /// <returns>
    /// The updated <see cref="AttributeData"/> instance with the new value applied.
    /// </returns>
    public AttributeData With(string? value) => new(Header, value);

    /// <summary>
    /// Returns a string representation of the current <see cref="AttributeData"/> instance.
    /// </summary>
    /// <remarks>
    /// If the underlying value is a boolean, the method returns "True" or "False" accordingly.
    /// For other types, the method returns the string representation of the value.
    /// If the value is null, an empty string is returned.
    /// </remarks>
    /// <returns>
    /// A string representation of the attribute's value, or an empty string if the value is null.
    /// </returns>
    public override string ToString()
    {
        if (Value is bool b)
        {
            return b ? "true" : "false";
        }

        return Value?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// Extracts and returns the name portion from the provided header string.
    /// </summary>
    /// <param name="header">The header string from which the name portion is to be extracted.</param>
    /// <returns>The name portion extracted from the header string.</returns>
    private static string ParseName(string header)
    {
        var typeStart = header.IndexOf('(');
        return typeStart > 0 ? header[..typeStart] : header;
    }

    /// <summary>
    /// Extracts and returns the name portion from the provided header string.
    /// </summary>
    /// <param name="header">The header string from which the name portion is to be extracted.</param>
    /// <returns>The name portion extracted from the header string.</returns>
    private static Type ParseType(string header)
    {
        var typeStart = header.IndexOf('(') + 1;
        var typeName = typeStart > 0 ? header[typeStart..].TrimEnd(')') : string.Empty;

        return typeName switch
        {
            "MxBoolean" => typeof(bool),
            "MxInteger" => typeof(int),
            "MxFloat" => typeof(float),
            "MxDouble" => typeof(double),
            _ => typeof(string)
        };
    }

    /// <summary>
    /// Parses the provided value string and converts it into an object of the specified type.
    /// </summary>
    /// <param name="type">The target type to which the value should be converted.</param>
    /// <param name="value">The string representation of the value to be parsed.</param>
    /// <returns>An object of the specified type that represents the parsed value. Returns null if the input value is null.</returns>
    private static object? ParseValue(Type type, string? value)
    {
        if (value is null) return null;

        return type switch
        {
            _ when type == typeof(bool) => bool.Parse(value),
            _ when type == typeof(int) => int.Parse(value),
            _ when type == typeof(float) => float.Parse(value),
            _ when type == typeof(double) => double.Parse(value),
            _ => value
        };
    }
}