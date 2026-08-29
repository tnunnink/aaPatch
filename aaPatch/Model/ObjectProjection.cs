namespace aaPatch.Model;

/// <summary>
/// Represents a field selection expression used to specify which attributes to include in the output.
/// Parses selection strings in the format "Attribute=Alias" or just "Attribute" (where Alias remains empty).
/// Allows renaming attributes in the output by specifying an alias.
/// </summary>
public sealed record ObjectProjection
{
    /// <summary>
    /// Gets the name of the attribute to include in the output.
    /// </summary>
    private readonly string _name;

    /// <summary>
    /// Gets the parsed expression representing the field or attribute, using curly brace notation
    /// for dynamic property references and supporting transformations to indexer-based syntax.
    /// </summary>
    private readonly ObjectExpression _expression;

    /// <summary>
    /// Caches the compiled expression to optimize repeated evaluations of object data projections.
    /// </summary>
    private Func<ObjectData, object?>? _cached;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectProjection"/> class by parsing a selection expression.
    /// </summary>
    /// <param name="input">
    /// The selection expression string in the format "Attribute=Alias" or just "Attribute" (no alias).
    /// </param>
    /// <remarks>This constructor is how CliFx is parsing the command option.</remarks>
    public ObjectProjection(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Selection expression cannot be null, empty, or whitespace.", nameof(input));

        var index = input.IndexOf(":=", StringComparison.Ordinal);
        _name = index > 0 ? input[..index].Trim() : input.Trim();
        _expression = index > 0 ? input[(index + 2)..].Trim() : $"{{{input.Trim()}}}";
    }

    /// <summary>
    /// Projects data from an <see cref="ObjectData"/> instance into a collection of <see cref="AttributeData"/>
    /// by applying the field selection and transformation defined by the current <see cref="ObjectProjection"/> instance.
    /// </summary>
    /// <param name="data">
    /// The <see cref="ObjectData"/> instance to project attributes from.
    /// </param>
    /// <returns>
    /// A collection of <see cref="AttributeData"/> representing the projected attributes
    /// based on the field selection and transformation rules.
    /// </returns>
    public IEnumerable<AttributeData> Project(ObjectData data)
    {
        if (_name == "*")
            return data.Select(a => a);

        var selection = _cached ??= _expression.Compile<ObjectData, object?>();
        var value = selection(data);
        return [new AttributeData(_name, value)];
    }

    /// <summary>
    /// Defines an implicit conversion from a string to a <see cref="ObjectProjection"/> instance.
    /// </summary>
    /// <param name="text">
    /// The selection expression string in the format "Attribute=Alias" or just "Attribute" (no alias).
    /// </param>
    /// <returns>
    /// A new <see cref="ObjectProjection"/> instance based on the provided selection expression.
    /// </returns>
    public static implicit operator ObjectProjection(string text) => new(text);
}