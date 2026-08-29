using System.Linq.Dynamic.Core;
using System.Text.RegularExpressions;

namespace aaPatch.Model;

/// <summary>
/// Represents an expression that references object properties using curly brace notation and provides
/// compilation to indexer-based syntax for dynamic evaluation.
/// </summary>
/// <remarks>
/// This class parses expressions containing property references in the format {propertyName} and
/// transforms them into indexer syntax it["propertyName"] for use with dynamic property access.
/// The expression must not be null, empty, or whitespace.
/// </remarks>
public partial class ObjectExpression
{
    /// <summary>
    /// Defines the configuration settings used for parsing dynamic LINQ expressions.
    /// </summary>
    /// <remarks>
    /// This variable is initialized with specific settings to enable context keywords, such as "it",
    /// which are required for the correct parsing and evaluation of expressions in the dynamic LINQ context.
    /// </remarks>
    private static readonly ParsingConfig ParsingConfig = new() { AreContextKeywordsEnabled = true };

    /// <summary>
    /// Gets a compiled regular expression pattern that matches object property references enclosed in curly braces.
    /// </summary>
    /// <returns>A <see cref="Regex"/> instance that matches property reference patterns.</returns>
    /// <remarks>
    /// The pattern {([^{}]+)} matches any text enclosed in curly braces that does not contain nested braces.
    /// The captured group contains the property name without the surrounding braces.
    /// </remarks>
    [GeneratedRegex("{([^{}]+)}")]
    private static partial Regex ReferencePattern();

    /// <summary>
    /// Stores the original expression string containing property references in curly brace notation.
    /// </summary>
    private readonly string _expression;

    /// <summary>
    /// Initializes a new instance of the <see cref="ObjectExpression"/> class with the specified expression string.
    /// </summary>
    /// <param name="expression">The expression string containing property references in curly brace notation. Cannot be null, empty, or whitespace.</param>
    /// <exception cref="ArgumentException">Thrown when the expression is null, empty, or whitespace.</exception>
    public ObjectExpression(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("Expression cannot be null, empty, or whitespace.", nameof(expression));

        //todo is there any other validation to perform?

        _expression = expression;
    }

    /// <summary>
    /// Compiles the object expression into a strongly typed delegate function that can be invoked
    /// with instances of <typeparamref name="TValue"/>. The expression is transformed from curly brace
    /// notation {propertyName} to indexer syntax it["propertyName"] before compilation.
    /// </summary>
    /// <typeparam name="TValue">The type of the input object that the compiled expression will operate on.</typeparam>
    /// <typeparam name="TResult">The return type of the compiled expression result.</typeparam>
    /// <returns>A compiled delegate function that accepts a <typeparamref name="TValue"/> instance and returns a <typeparamref name="TResult"/>.</returns>
    /// <exception cref="ArgumentException">Thrown when the expression cannot be compiled due to invalid syntax or type incompatibility.</exception>
    public Func<TValue, TResult> Compile<TValue, TResult>()
    {
        var expression = ReferencePattern().Replace(_expression, "it[\"$1\"]");

        var lambda = DynamicExpressionParser.ParseLambda<TValue, TResult>(
            ParsingConfig,
            createParameterCtor: false,
            expression);

        try
        {
            return lambda.Compile();
        }
        catch (Exception exception)
        {
            throw new ArgumentException(
                $"Invalid expression '{_expression}': {exception.Message}",
                nameof(_expression),
                exception);
        }
    }

    /// <summary>
    /// Returns the original expression string in its uncompiled form.
    /// </summary>
    /// <returns>The original expression string with property references in curly brace notation.</returns>
    public override string ToString() => _expression;

    /// <summary>
    /// Defines an implicit conversion from a string to an <see cref="ObjectExpression"/> instance.
    /// </summary>
    /// <param name="text">The string representation of the object expression. Cannot be null, empty, or whitespace.</param>
    /// <returns>An <see cref="ObjectExpression"/> instance created from the provided string.</returns>
    /// <exception cref="ArgumentException">Thrown when the input string is null, empty, or whitespace.</exception>
    public static implicit operator ObjectExpression(string text) => new(text);
}