namespace aaPatch.Model;

/// <summary>
/// Represents a patch operation to be applied to Galaxy dump object attributes.
/// Parses patch strings in the format "Attribute=Value" for direct assignment or "Attribute:Find=Replace" for find-replace operations.
/// </summary>
public sealed record ObjectPatch
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectPatch"/> class by parsing a patch expression.
    /// </summary>
    /// <param name="input">
    /// The patch expression string in the format "Attribute=Value" for direct assignment or "Attribute:Find=Replace" for find-replace operations.
    /// </param>
    /// <exception cref="ArgumentException">Thrown when the input is null, empty, whitespace, or has an invalid format.</exception>
    public ObjectPatch(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Patch expression cannot be null, empty, or whitespace.", nameof(input));

        var parts = input.Split([":", "="], StringSplitOptions.None);

        if (parts.Length is < 2 or > 3)
            throw new ArgumentException("Invalid patch pattern...");

        Attribute = parts[0];
        Find = parts.Length == 3 ? parts[1] : null;
        Replacement = parts.Length == 3 ? parts[2] : parts[1];
    }

    /// <summary>
    /// Gets the name of the attribute to be patched.
    /// </summary>
    public string Attribute { get; }

    /// <summary>
    /// Gets the text to find for replacement operations.
    /// Returns null when using direct assignment format ("Attribute=Value").
    /// </summary>
    public string? Find { get; }

    /// <summary>
    /// Gets the replacement value or text to use in the patch operation.
    /// For direct assignment, this is the new value. For find-replace, this is the replacement text.
    /// </summary>
    public string Replacement { get; }

    /// <summary>
    /// Implicitly converts a string to an instance of the <see cref="ObjectPatch"/> class.
    /// </summary>
    /// <param name="text">
    /// The patch expression string in the format "Attribute=Value" for direct assignment or "Attribute:Find=Replace" for find-replace operations.
    /// </param>
    /// <returns>
    /// A new <see cref="ObjectPatch"/> instance initialized with the specified patch expression string.
    /// </returns>
    public static implicit operator ObjectPatch(string text) => new(text);
}