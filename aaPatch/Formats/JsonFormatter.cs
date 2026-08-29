using System.Text.Json;
using aaPatch.Model;

namespace aaPatch.Formats;

/// <summary>
/// A formatter for handling JSON data. This class implements the <see cref="IObjectFormater"/> interface,
/// providing methods to serialize a collection of <see cref="ObjectData"/> objects to a JSON string,
/// and to deserialize a JSON string into a collection of <see cref="ObjectData"/> instances.
/// Ensures compliance with JSON's requirement for unique attribute names within objects.
/// </summary>
public class JsonFormatter : IObjectFormater
{
    /// <summary>
    /// Defines the JSON serialization options used for customizing the behavior of JSON output,
    /// such as enabling indented formatting for better readability of the serialized data.
    /// </summary> 
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    /// <summary>
    /// Deserializes a JSON string into a collection of <see cref="ObjectData"/> instances.
    /// Ensures that the input text is properly formatted and not null or empty.
    /// </summary>
    /// <param name="text">The JSON string to deserialize into <see cref="ObjectData"/> objects.</param>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="ObjectData"/> representing the deserialized data.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the provided <paramref name="text"/> is null or empty.
    /// </exception>
    public IEnumerable<ObjectData> Read(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("The text parameter cannot be null or empty.", nameof(text));

        if (!text.Trim().StartsWith('['))
            throw new ArgumentException("The input JSON must be an array starting with '['.", nameof(text));

        var records = JsonSerializer.Deserialize<Dictionary<string, object>[]>(text) ?? [];

        return records.Select(d =>
        {
            var attributes = d.Select(kvp => new AttributeData(kvp.Key, ParseValue(kvp.Value)));
            return new ObjectData(attributes);
        });
    }

    /// <summary>
    /// Serializes a collection of <see cref="ObjectData"/> objects into a JSON string representation.
    /// Ensures that attribute names within each object are unique, as required by the JSON format.
    /// </summary>
    /// <param name="data">The collection of <see cref="ObjectData"/> objects to be serialized.</param>
    /// <returns>A JSON string representing the serialized collection.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when one or more objects in the collection contain duplicate attribute names.
    /// JSON format requires all attribute names within an object to be unique.
    /// </exception>
    public string Write(IEnumerable<ObjectData> data)
    {
        var dictionary = data.Select(x => x.ToDictionary(
            a => a.Name,
            a => a.Value.GetValue(),
            StringComparer.OrdinalIgnoreCase)
        ).ToArray();

        return JsonSerializer.Serialize(dictionary, JsonOptions);
    }

    /// <summary>
    /// Parses a JSON value into a corresponding .NET object, handling various JSON value kinds such as strings,
    /// numbers, booleans, nulls, and complex types (objects or arrays).
    /// </summary>
    /// <param name="value">The JSON value to parse, represented as an <see cref="object"/> or <see cref="JsonElement"/>.</param>
    /// <returns>
    /// A .NET object representing the parsed value:
    /// - <see langword="null"/> if the value is <see cref="JsonValueKind.Null"/> or <see cref="JsonValueKind.Undefined"/>.
    /// - A .NET primitive or string for simple JSON types (e.g., string, number, boolean).
    /// - A JSON-encoded string for complex types (objects or arrays).
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the provided <paramref name="value"/> contains an unsupported JSON value kind.
    /// </exception>
    private static object? ParseValue(object? value)
    {
        if (value is not JsonElement element)
            return value;

        return element.ValueKind switch
        {
            JsonValueKind.Null => null,
            JsonValueKind.Undefined => null,
            JsonValueKind.Object or JsonValueKind.Array => element.GetRawText(),
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number when element.TryGetInt32(out var number) => number,
            JsonValueKind.Number when element.TryGetInt64(out var number) => number,
            JsonValueKind.Number when element.TryGetDouble(out var number) => number,
            JsonValueKind.True or JsonValueKind.False => element.GetBoolean(),
            _ => throw new ArgumentOutOfRangeException(nameof(value), element.ValueKind, "Unsupported JSON value kind.")
        };
    }
}