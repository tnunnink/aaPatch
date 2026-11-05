namespace aaPatch.Model;

/// <summary>
/// Represents a parsed patch expression that can modify object attributes. A patch can be either global
/// (applied to all objects) or targeted (applied to a specific attribute using the '{Attribute} := Expression' syntax).
/// </summary>
public sealed record ObjectPatch
{
    /// <summary>
    /// Gets the target attribute name for this patch, or null if this is a global patch.
    /// For targeted patches using '{Attribute} := Expression' syntax, this contains the attribute name without braces.
    /// </summary>
    private readonly string? _target;

    /// <summary>
    /// Gets the expression that will be evaluated to produce the patch value.
    /// This expression is parsed from either the entire input string (for global patches)
    /// or the portion after the ':=' operator (for targeted patches).
    /// </summary>
    private readonly ObjectExpression _expression;

    /// <summary>
    /// Caches the compiled patch expression delegate so it can be reused across applications of this patch.
    /// The delegate is created lazily on first use and retained to avoid recompiling the expression for later calls.
    /// </summary>
    private Func<ObjectData, object?>? _cached;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectPatch"/> class by parsing a patch expression string.
    /// Supports both global patches (e.g., "SomeExpression") and targeted patches (e.g., "{AttributeName} := SomeExpression").
    /// </summary>
    /// <param name="input">The patch expression string to parse.</param>
    /// <exception cref="ArgumentException">Thrown when the input is null, empty, whitespace, or has invalid syntax.</exception>
    public ObjectPatch(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Patch expression cannot be null, empty, or whitespace.", nameof(input));

        var index = input.IndexOf(":=", StringComparison.Ordinal);
        var target = index > 0 ? input[..index].Trim() : null;
        var expression = index > 0 ? input[(index + 2)..].Trim() : input.Trim();

        if (target is not null && (!target.StartsWith('{') || !target.EndsWith('}')))
            throw new ArgumentException("A targeted patch must use '{Attribute} := Expression'.");

        if (expression.Length == 0)
            throw new ArgumentException("Patch expression cannot be empty.");

        _target = target?[1..^1]?.Trim();
        _expression = new ObjectExpression(expression);
    }

    /// <summary>
    /// Applies the current patch to the specified attribute.
    /// The method evaluates the patch expression and updates the attribute's value
    /// if the target matches or if the patch is global.
    /// </summary>
    /// <param name="instance">The attribute data to which the patch is applied.</param>
    /// <returns>A new <see cref="AttributeData"/> instance with the updated value or a duplicate
    /// of the original attribute if no update is performed.</returns>
    public ObjectData Apply(ObjectData instance)
    {
        var patch = _cached ??= _expression.Compile<ObjectData, object?>();
        var value = patch.Invoke(instance);

        if (value is ObjectData data)
            return data;

        if (_target is not null)
            return instance.Update(_target, value);

        return instance;
    }

    /// <summary>
    /// Implicitly converts a string to an <see cref="ObjectPatch"/> instance by parsing the string as a patch expression.
    /// </summary>
    /// <param name="text">The patch expression string to convert.</param>
    /// <returns>A new <see cref="ObjectPatch"/> instance representing the parsed expression.</returns>
    public static implicit operator ObjectPatch(string text) => new(text);
}