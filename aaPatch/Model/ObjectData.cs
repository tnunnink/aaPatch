using System.Collections;
using System.Linq.Dynamic.Core.CustomTypeProviders;

namespace aaPatch.Model;

/// <summary>
/// Represents an exported object instance from a galaxy dump file. This record contains the parent template name and
/// tag name reference, along with the dynamic collection of attribute key/value pairs.
/// </summary>
[DynamicLinqType]
public class ObjectData : IReadOnlyCollection<AttributeData>
{
    /// <summary>
    /// Provides a predefined string comparer that performs case-insensitive string comparisons.
    /// This comparer is commonly used when string operations need to ignore the case sensitivity
    /// of characters, such as in dictionary keys or grouping operations.
    /// </summary>
    private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

    /// <summary>
    /// Maintains a dictionary mapping attribute headers to their corresponding <see cref="AttributeData"/> instances,
    /// enabling efficient lookup and management of attributes by their header values.
    /// </summary>
    private readonly Dictionary<string, AttributeData> _attributes;

    /// <summary>
    /// Represents an exported object instance from a galaxy dump file. This record contains the parent template name and
    /// tag name reference, along with the dynamic collection of attribute key/value pairs.
    /// </summary>
    public ObjectData(IEnumerable<AttributeData> attributes)
    {
        _attributes = attributes.ToDictionary(a => a.Name, Comparer);
    }

    /// <summary>
    /// Gets the total number of attributes associated with the object.
    /// </summary>
    public int Count => _attributes.Count;

    /// <summary>
    /// Provides an indexer for accessing object data attributes by name. The indexer allows retrieval of the
    /// value associated with a specific attribute, including special cases for "Template" and "TagName".
    /// </summary>
    /// <param name="name">The name of the attribute to retrieve. Use "Template" or "TagName" to access their corresponding values,
    /// or the name of a specific object attribute.</param>
    /// <returns>The value of the requested attribute if it exists, or null if the attribute is not defined.</returns>
    public AttributeValue? this[string name] => TryResolveAttribute(name, out var attribute) ? attribute.Value : null;

    /// <summary>
    /// Checks if the object contains all specified attributes.
    /// </summary>
    /// <param name="attributes">An array of attribute names to check for existence.</param>
    /// <returns>True if all specified attributes exist; otherwise, false.</returns>
    public bool Has(IReadOnlyCollection<string> attributes)
    {
        if (attributes.Count == 0)
            return true;

        foreach (var attribute in attributes)
        {
            if (!TryResolveAttribute(attribute, out _))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Returns a string representation of the object data by concatenating the values
    /// of all attributes, separated by commas.
    /// </summary>
    /// <returns>
    /// A comma-separated string that represents the values of the object's attributes.
    /// </returns>
    public override string ToString() => string.Join(",", _attributes.Select(a => a.ToString()));

    /// <inheritdoc />
    public IEnumerator<AttributeData> GetEnumerator() => _attributes.Values.AsEnumerable().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Attempts to resolve an attribute by its name from the collection of attributes.
    /// If an explicit match is found, it is returned. If no explicit match exists, attempts
    /// to find a single attribute whose name starts with the provided input.
    /// </summary>
    /// <param name="name">The name of the attribute to resolve. Can be a full name or the starting portion of the name.</param>
    /// <param name="attribute">When the method returns, contains the resolved attribute if the resolution succeeds,
    /// or null if it fails.</param>
    /// <returns>
    /// True if the resolution succeeds and an attribute is found; otherwise, false if no matching or ambiguous matches exist.
    /// </returns>
    private bool TryResolveAttribute(string name, out AttributeData attribute)
    {
        // Explicit match to the attribute name wins first
        if (_attributes.TryGetValue(name, out var matched))
        {
            attribute = matched;
            return true;
        }

        // Otherwise, help the user and try to find the name starting with the provided text.
        // This is designed to help with accessing AVEVA columns that have type metadata (e.g., MyColumn(MxInteger))
        var prefix = $"{name}(";
        var matches = _attributes.Keys.Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList();

        switch (matches.Count)
        {
            case > 1:
                throw new ArgumentException(
                    $"Ambiguous attribute name '{name}'. Multiple attributes match: {string.Join(", ", matches)}. " +
                    "Please specify the full attribute name including type suffix.");
            case 1:
                attribute = _attributes[matches[0]];
                return true;
            default:
                attribute = null!;
                return false;
        }
    }
}