namespace aaPatch.Model;

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
    /// Represents the default template name used for comparison in attribute name validation.
    /// </summary>
    private const string Template = "Template";

    /// <summary>
    /// Represents the default template name used for comparison in attribute name validation.
    /// </summary>
    private const string TagName = "TagName";

    /// <summary>
    /// Stores the optional raw string value associated with the attribute.
    /// </summary>
    /// <remarks>
    /// This field contains the attribute's value as a string, which may correspond to a specific type
    /// based on the type information parsed from the header. It can be null if no value is provided
    /// during initialization.
    /// The value is immutable and is set during the construction of the <see cref="AttributeData"/> instance.
    /// </remarks>
    private string? _value;

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
        _value = value;
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
    public string Name => ParseName();

    /// <summary>
    /// Gets the type information extracted from the header string.
    /// </summary>
    /// <remarks>
    /// This property parses the type portion from the header string to determine the data type
    /// associated with the attribute. The type is derived by analyzing the format of the header,
    /// typically in the structure "Name(TypeName)", where "TypeName" specifies the type.
    /// If the type is not explicitly defined in the header, it defaults to <see cref="string"/>.
    /// The property ensures that the type information is consistently resolved and is immutable
    /// after initialization.
    /// </remarks>
    public Type Type => ParseType();

    /// <summary>
    /// Retrieves the parsed value associated with the attribute data.
    /// </summary>
    /// <remarks>
    /// The value is derived by interpreting the raw input string based on the
    /// type specified in the header. If the raw value is null, the result will
    /// also be null. The type of the returned object can vary and depends on
    /// the type specified in the header (e.g., string, int, bool, etc.).
    /// </remarks>
    public object? Value => ParseValue();

    /// <summary>
    /// Indicates whether the attribute name is considered special based on predefined criteria.
    /// </summary>
    /// <remarks>
    /// An attribute is deemed special if its name matches specific default values
    /// such as "Template" or "TagName", using a case-insensitive string comparison.
    /// </remarks>
    public bool IsIdentity =>
        StringComparer.OrdinalIgnoreCase.Equals(Name, Template) ||
        StringComparer.OrdinalIgnoreCase.Equals(Name, TagName);

    /// <summary>
    /// Creates a new <see cref="AttributeData"/> instance with the specified name while preserving the current value.
    /// </summary>
    /// <param name="name">The new name to set for the attribute. Cannot be null or empty.</param>
    /// <returns>A new <see cref="AttributeData"/> instance with the updated name and the existing value.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided name parameter is null or empty.</exception>
    public AttributeData Rename(string name)
    {
        var current = ParseName();
        var header = name + Header[current.Length..];
        return new AttributeData(header, _value);
    }

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
    public AttributeData Update(string? value)
    {
        _value = value;
        return this;
    }

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
    /// Extracts the attribute name from the header string.
    /// </summary>
    /// <remarks>
    /// The name is determined by parsing the header string. If the header contains a type declaration
    /// in the format "Name(TypeName)", the name is extracted before the opening parenthesis.
    /// If no parenthesis is found, the entire header string is returned as the name.
    /// </remarks>
    /// <returns>
    /// A string representing the attribute name parsed from the header. Cannot be null or empty.
    /// </returns>
    private string ParseName()
    {
        var typeStart = Header.IndexOf('(');
        return typeStart > 0 ? Header[..typeStart] : Header;
    }

    /// <summary>
    /// Parses the type information from the header string and returns the corresponding <see cref="Type"/>.
    /// </summary>
    /// <remarks>
    /// The header string is expected to specify the type in the format "Name(TypeName)", where TypeName is optional.
    /// If TypeName is not provided, the method defaults to returning the <see cref="string"/> type.
    /// Valid TypeName values include "MxBoolean", "MxInteger", "MxFloat", and "MxDouble".
    /// </remarks>
    /// <returns>
    /// A <see cref="Type"/> that corresponds to the type information extracted from the header.
    /// Returns <see cref="string"/> if no type information is specified.
    /// </returns>
    private Type ParseType()
    {
        var typeStart = Header.IndexOf('(') + 1;
        var typeName = typeStart > 0 ? Header[typeStart..].TrimEnd(')') : string.Empty;

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
    /// Parses the raw string value associated with the attribute and converts it to the expected type.
    /// </summary>
    /// <remarks>
    /// This method interprets the attribute's type parsed from the header and attempts to
    /// convert the stored value into the corresponding .NET type. If the value is null or consists only
    /// of whitespace, the method returns null. Conversion supports types such as bool, int, float,
    /// and double. If the type does not match any known types, the raw string value is returned as-is.
    /// </remarks>
    /// <returns>
    /// The parsed value as an object of the type determined by the header, or null if the value is
    /// null or empty.
    /// </returns>
    /// <exception cref="FormatException">
    /// Thrown if the value cannot be converted to the expected type (e.g., invalid number format).
    /// </exception>
    private object? ParseValue()
    {
        if (string.IsNullOrWhiteSpace(_value))
            return null;

        var type = ParseType();

        return type switch
        {
            _ when type == typeof(bool) => bool.Parse(_value),
            _ when type == typeof(int) => int.Parse(_value),
            _ when type == typeof(float) => float.Parse(_value),
            _ when type == typeof(double) => double.Parse(_value),
            _ => _value
        };
    }
}