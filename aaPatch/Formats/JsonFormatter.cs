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

        var dictionaries = JsonSerializer.Deserialize<Dictionary<string, object>[]>(text);

        if (dictionaries is null)
            return [];

        return dictionaries.Select(dict =>
        {
            var attributes = dict.Select(kvp => new AttributeData(kvp.Key, kvp.Value));
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
        var collection = data.ToList();

        var duplicate = collection.SelectMany(a => a.GroupBy(x => x.Name)).FirstOrDefault(g => g.Count() > 1);

        if (duplicate is not null)
            throw new InvalidOperationException(
                $"Cannot serialize to JSON: Attribute '{duplicate.Key}' appears multiple times in an object. " +
                "Use --select with aliases to rename duplicate attributes.");

        var dictionary = collection.Select(x => x.ToDictionary(
            a => a.Name,
            a => a.Value,
            StringComparer.OrdinalIgnoreCase)
        ).ToArray();

        return JsonSerializer.Serialize(dictionary, JsonOptions);
    }
}