namespace aaPatch.Model;

/// <summary>
/// Represents a filter expression used to select Galaxy dump objects based on attribute pattern matching.
/// Parses filter strings in the format "Attribute=Pattern" or just "Pattern" (where Pattern defaults to matching the TagName attribute).
/// Supports wildcard patterns for flexible object selection.
/// </summary>
public sealed record ObjectFilter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectFilter"/> class by parsing a filter expression.
    /// </summary>
    /// <param name="input">
    /// The filter expression string in the format "Attribute=Pattern" or just "Pattern" (defaults to TagName).
    /// </param>
    /// <remarks>This constructor is how CliFx is parsing the command option.</remarks>
    public ObjectFilter(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Filter expression cannot be null, empty, or whitespace.", nameof(input));

        var index = input.IndexOf('=');
        Attribute = index > 0 ? input[..index] : "TagName";
        Pattern = index > 0 ? input[(index + 1)..] : input;
    }

    /// <summary>
    /// Gets the name of the attribute to match against.
    /// Defaults to "TagName" if not specified in the filter expression.
    /// </summary>
    public string Attribute { get; }

    /// <summary>
    /// Gets the pattern used to match attribute values.
    /// Supports wildcard patterns for flexible matching.
    /// </summary>
    public string Pattern { get; }

    /// <summary>
    /// Implicitly converts a string to an instance of the <see cref="ObjectFilter"/> class.
    /// </summary>
    /// <param name="text">
    /// The filter expression string in the format "Attribute=Pattern" or just "Pattern" (defaults to TagName).
    /// </param>
    /// <returns>
    /// A new <see cref="ObjectFilter"/> instance initialized with the specified filter expression string.
    /// </returns>
    public static implicit operator ObjectFilter(string text) => new(text);
}