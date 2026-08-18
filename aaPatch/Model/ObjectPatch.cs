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
    /// Gets the type of the patch operation represented by this instance.
    /// </summary>
    /// <remarks>
    /// The patch type is determined based on the combination of values in the
    /// <see cref="Attribute"/> and <see cref="Find"/> properties.
    /// </remarks>
    public PatchType Type => DetermineType();

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

    /// <summary>
    /// Determines the type of patch operation based on the values of the <see cref="Attribute"/> and <see cref="Find"/> properties.
    /// </summary>
    /// <returns>
    /// A <see cref="PatchType"/> value indicating the type of the patch operation:
    /// <see cref="PatchType.Assign"/> if there is a valid attribute with no find value,
    /// <see cref="PatchType.Replace"/> if both attribute and find values are present,
    /// or <see cref="PatchType.ReplaceAll"/> if the attribute is empty and only the find value is present.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the combination of <see cref="Attribute"/> and <see cref="Find"/> values does not conform to a valid patch type.
    /// </exception>
    private PatchType DetermineType()
    {
        if (string.IsNullOrWhiteSpace(Attribute) && !string.IsNullOrEmpty(Find))
            return PatchType.ReplaceAll;

        if (!string.IsNullOrWhiteSpace(Attribute) && !string.IsNullOrEmpty(Find))
            return PatchType.Replace;

        if (!string.IsNullOrWhiteSpace(Attribute) && string.IsNullOrEmpty(Find))
            return PatchType.Assign;
        
        throw new InvalidOperationException("Unable to determine patch type: invalid combination of Attribute and Find values.");
    }
}

/// <summary>
/// Enum representing the type of patch operation that can be performed.
/// </summary>
/// <remarks>
/// The available patch operation types are:
/// - Assign: Represents a direct assignment operation where an attribute is set to a specified value.
/// - Replace: Represents a find-and-replace operation where a specific substring within an attribute is replaced.
/// - ReplaceAll: Represents a global replacement operation where a pattern is replaced across all relevant attributes.
/// </remarks>
public enum PatchType
{
    Assign,
    Replace,
    ReplaceAll
}