namespace aaPatch.Model;

/// <summary>
/// Represents a field selection expression used to specify which attributes to include in the output.
/// Parses selection strings in the format "Attribute=Alias" or just "Attribute" (where Alias remains empty).
/// Allows renaming attributes in the output by specifying an alias.
/// </summary>
public sealed record FieldSelection
{
    /// <summary>
    /// Initializes a new instance of the <see cref="FieldSelection"/> class by parsing a selection expression.
    /// </summary>
    /// <param name="input">
    /// The selection expression string in the format "Attribute=Alias" or just "Attribute" (no alias).
    /// </param>
    /// <remarks>This constructor is how CliFx is parsing the command option.</remarks>
    public FieldSelection(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Selection expression cannot be null, empty, or whitespace.", nameof(input));

        var index = input.IndexOf('=');
        Attribute = index > 0 ? input[..index] : input;
        Alias = index > 0 ? input[(index + 1)..] : string.Empty;
    }

    /// <summary>
    /// Gets the name of the attribute to include in the output.
    /// </summary>
    public string Attribute { get; }

    /// <summary>
    /// Gets the alias name to use for the attribute in the output.
    /// Returns an empty string if no alias was specified in the selection expression.
    /// </summary>
    public string Alias { get; }

    /// <summary>
    /// Defines an implicit conversion from a string to a <see cref="FieldSelection"/> instance.
    /// </summary>
    /// <param name="text">
    /// The selection expression string in the format "Attribute=Alias" or just "Attribute" (no alias).
    /// </param>
    /// <returns>
    /// A new <see cref="FieldSelection"/> instance based on the provided selection expression.
    /// </returns>
    public static implicit operator FieldSelection(string text) => new(text);
}