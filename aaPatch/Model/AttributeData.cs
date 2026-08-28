namespace aaPatch.Model;

/// <summary>
/// Represents attribute data consisting of a name and an associated typed value.
/// </summary>
/// <remarks>
/// The <see cref="AttributeData"/> class stores attribute information with a name identifier
/// and a value wrapped in an <see cref="AttributeValue"/> for type safety. The class provides
/// immutable operations for renaming, duplicating, and updating values while preserving type information.
/// </remarks>
public class AttributeData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AttributeData"/> class with the specified name and value.
    /// </summary>
    /// <param name="name">The name of the attribute. Cannot be null or empty.</param>
    /// <param name="value">The value associated with the attribute. Can be null.</param>
    /// <exception cref="ArgumentException">Thrown when the name parameter is null or empty.</exception>
    public AttributeData(string name, object? value)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null, empty, or whitespace.", nameof(name));

        Name = name;
        Value = new AttributeValue(value);
    }

    /// <summary>
    /// Gets the name of the attribute.
    /// </summary>
    /// <remarks>
    /// The name serves as a unique identifier for the attribute and cannot be null or whitespace.
    /// </remarks>
    public string Name { get; }

    /// <summary>
    /// Gets the typed value associated with the attribute data.
    /// </summary>
    /// <remarks>
    /// The value is wrapped in an <see cref="AttributeValue"/> which preserves type information
    /// and provides type conversion capabilities. The underlying value can be of various types
    /// (e.g., string, int, double, bool, float) or null.
    /// </remarks>
    public AttributeValue Value { get; }

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
    /// A new <see cref="AttributeData"/> instance with the same name and a deep copy of the value.
    /// </returns>
    public AttributeData Duplicate() => new(Name, new AttributeValue(Value));

    /// <summary>
    /// Creates a new instance of the <see cref="AttributeData"/> class with the same name and an updated value.
    /// The new value is converted to match the type of the current value using <see cref="AttributeValue.As"/>.
    /// </summary>
    /// <param name="value">The new value to update. Can be null or a value convertible to the type of the current value.</param>
    /// <returns>A new instance of the <see cref="AttributeData"/> class with the updated value converted to the original type.</returns>
    /// <exception cref="InvalidCastException">Thrown if the value cannot be converted to the type of the current value.</exception>
    public AttributeData Update(AttributeValue value) => new(Name, value.As(Value.Type));

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
    public override string ToString() => Value.ToString();
}