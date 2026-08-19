using System.Globalization;

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
    public AttributeData(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Attribute expression can not be null, empty, or white space.", nameof(input));

        var index = input.IndexOf('=');

        if (index < 0)
            throw new ArgumentException("");

        Name = input[..index];
        Value = input[(index + 1)..];
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AttributeData"/> class with the specified name and value.
    /// </summary>
    /// <param name="name">The name of the attribute. Cannot be null or empty.</param>
    /// <param name="value">The value associated with the attribute. Can be null.</param>
    /// <exception cref="ArgumentException">Thrown when the name parameter is null or empty.</exception>
    public AttributeData(string name, object? value)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Name cannot be null or empty.", nameof(name));

        Name = name;
        Value = value;
    }

    /// <summary>
    /// Gets the name derived from the header field.
    /// </summary>
    /// <remarks>
    /// The name is extracted from the header string by isolating the portion
    /// preceding the first occurrence of a type definition enclosed in parentheses.
    /// If no such portion exists, the full header string is returned as the name.
    /// </remarks>
    public string Name { get; }

    /// <summary>
    /// Retrieves the parsed value associated with the attribute data.
    /// </summary>
    /// <remarks>
    /// The value is derived by interpreting the raw input string based on the
    /// type specified in the header. If the raw value is null, the result will
    /// also be null. The type of the returned object can vary and depends on
    /// the type specified in the header (e.g., string, int, bool, etc.).
    /// </remarks>
    public object? Value { get; private set; }

    /// <summary>
    /// Creates a new <see cref="AttributeData"/> instance with the specified name while preserving the current value.
    /// </summary>
    /// <param name="name">The new name to set for the attribute. Cannot be null or empty.</param>
    /// <returns>A new <see cref="AttributeData"/> instance with the updated name and the existing value.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided name parameter is null or empty.</exception>
    public AttributeData Rename(string name) => new(name, Value);

    /// <summary>
    /// Creates a duplicate of the current <see cref="AttributeData"/> instance.
    /// </summary>
    /// <returns>
    /// A new <see cref="AttributeData"/> instance with the same header and value as the current instance.
    /// </returns>
    public AttributeData Duplicate() => new(Name, Value);

    /// <summary>
    /// Updates the value of the attribute by converting it to the current type of the attribute's value
    /// using the invariant culture.
    /// </summary>
    /// <param name="value">The new value to update the attribute with. The value is converted to the type of the existing value, or to a string type if the existing value is null.</param>
    /// <exception cref="InvalidCastException">Thrown when the value cannot be converted to the type of the existing attribute's value.</exception>
    /// <exception cref="FormatException">Thrown when the format of the value is invalid for the conversion to the target type.</exception>
    public void Update(object? value)
    {
        var type = Value?.GetType() ?? typeof(string);
        Value = Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
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
    /// Converts a string to an instance of the <see cref="AttributeData"/> class.
    /// </summary>
    /// <param name="text">The input string to be converted into an <see cref="AttributeData"/> instance. Cannot be null or empty.</param>
    /// <returns>
    /// A new instance of the <see cref="AttributeData"/> class initialized with the specified string.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when the input string is null, empty, or improperly formatted.</exception>
    public static implicit operator AttributeData(string text) => new(text);
}