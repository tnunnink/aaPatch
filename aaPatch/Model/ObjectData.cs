using System.Linq.Dynamic.Core.CustomTypeProviders;

namespace aaPatch.Model;

/// <summary>
/// Represents an exported object instance from a galaxy dump file. This record contains the parent template name and
/// tag name reference, along with the dynamic collection of attribute key/value pairs.
/// </summary>
[DynamicLinqType]
public class ObjectData
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
    /// Gets the name of the parent template associated with this exported object instance.
    /// </summary>
    public AttributeValue Template => this[nameof(Template)];

    /// <summary>
    /// Gets the tag name associated with the exported object instance.
    /// </summary>
    public AttributeValue TagName => this[nameof(TagName)];

    /// <summary>
    /// Gets the collection of attributes associated with this object, represented as name/value metadata entries.
    /// The returned sequence contains the stored attribute data values for the object, including identity attributes
    /// such as template and tag name when present.
    /// </summary>
    public IEnumerable<AttributeData> Attributes => _attributes.Values;

    /// <summary>
    /// Retrieves the value of an attribute by its name in the context of the current object.
    /// This indexer provides access to a specific attribute's value based on the provided attribute name.
    /// </summary>
    /// <param name="name">The name of the attribute to retrieve.</param>
    /// <returns>The value of the attribute as an <see cref="AttributeValue"/> object.</returns>
    /// <exception cref="ArgumentException">Thrown when multiple attributes match the specified name.</exception>
    public AttributeValue this[string name] => ResolveAttribute(name)?.Value ?? AttributeValue.Null;

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
            if (!ContainsAttribute(attribute))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Creates a new object data instance with the specified attribute value updated. If no matching attribute exists,
    /// a new attribute with the specified name and value is added.
    /// </summary>
    /// <param name="name">The name of the attribute to update or add.</param>
    /// <param name="value">The value to assign to the attribute.</param>
    /// <returns>A new object data instance containing the updated or added attribute.</returns>
    public ObjectData Update(string name, object? value)
    {
        var matched = false;
        var attributes = new List<AttributeData>(_attributes.Count + 1);

        foreach (var attribute in _attributes.Values)
        {
            if (IsMatch(attribute, name))
            {
                attributes.Add(attribute.Update(new AttributeValue(value)));
                matched = true;
            }
            else
            {
                attributes.Add(attribute);
            }
        }

        if (!matched)
        {
            attributes.Add(new AttributeData(name, value));
        }

        return new ObjectData(attributes);
    }

    /// <summary>
    /// Creates a new object data instance with the specified text replaced across all attribute values using a case-insensitive search.
    /// </summary>
    /// <param name="find">The text to search for in each attribute value.</param>
    /// <param name="replace">The text to substitute for each matching occurrence.</param>
    /// <returns>A new object data instance containing the updated attribute values.</returns>
    public ObjectData Replace(string find, string replace) => Replace(find, replace, false);

    /// <summary>
    /// Creates a new object data instance with the specified text replaced across all attribute values.
    /// </summary>
    /// <param name="find">The text to search for in each attribute value.</param>
    /// <param name="replace">The text to substitute for each matching occurrence.</param>
    /// <param name="matchCase">A value indicating whether the search should be case-sensitive.</param>
    /// <returns>A new object data instance containing the updated attribute values.</returns>
    public ObjectData Replace(string find, string replace, bool matchCase)
    {
        var attributes = _attributes.Values.Select(a =>
        {
            var updated = a.Value.Replace(find, replace, matchCase);
            return a.Update(updated);
        });

        return new ObjectData(attributes);
    }

    /// <summary>
    /// Creates a new object containing the identity attributes from the supplied object and any attributes whose values differ
    /// from this object.
    /// </summary>
    /// <param name="other">The object to compare against this object.</param>
    /// <returns>A new object containing the differing attributes from <paramref name="other" /> and its identity attributes.</returns>
    public ObjectData Diff(ObjectData other)
    {
        var attributes = other.Attributes.Where(a => a.IsIdentity || !this[a.Name].Equals(a.Value));
        return new ObjectData(attributes);
    }

    /// <summary>
    /// Determines whether any non-identity attribute in the specified object has a value that differs from the corresponding
    /// attribute in this object.
    /// </summary>
    /// <param name="other">The object data to compare against this object.</param>
    /// <returns>True if at least one non-identity attribute has changed; otherwise, false.</returns>
    public bool HasChanges(ObjectData other)
    {
        // Has any non-identity attribute changed?
        return other.Attributes.Any(a => !a.IsIdentity && !this[a.Name].Equals(a.Value));
    }

    /// <summary>
    /// Returns a string representation of the object data by concatenating the values
    /// of all attributes, separated by commas.
    /// </summary>
    /// <returns>
    /// A comma-separated string that represents the values of the object's attributes.
    /// </returns>
    public override string ToString() => string.Join(",", _attributes.Select(a => a.ToString()));

    /// <summary>
    /// Determines whether the specified attribute name exists within the collection of attributes.
    /// This method performs an exact match first, and if no match is found, it attempts to locate an attribute
    /// that starts with the specified name followed by an opening parenthesis.
    /// </summary>
    /// <param name="name">The name of the attribute to check for existence.</param>
    /// <returns>
    /// Returns <c>true</c> if the attribute name exists in the collection, either as an exact match or with a prefix;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the provided attribute name is <c>null</c> or empty.
    /// </exception>
    private bool ContainsAttribute(string name)
    {
        // Explicit match to the attribute name wins first
        if (_attributes.ContainsKey(name))
            return true;

        // Otherwise, help the user and try to find the name starting with the provided text.
        // This is designed to help with accessing AVEVA columns that have type metadata (e.g., MyColumn(MxInteger))
        var prefix = $"{name}(";
        return _attributes.Keys.Any(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Determines whether an attribute name matches the requested name, using a case-insensitive exact comparison or a
    /// case-insensitive prefix comparison for names that include type metadata in parentheses.
    /// </summary>
    /// <param name="attribute">The attribute to compare against the requested name.</param>
    /// <param name="name">The attribute name to match.</param>
    /// <returns>True if the attribute name matches exactly or begins with the requested name followed by an opening parenthesis; otherwise, false.</returns>
    private static bool IsMatch(AttributeData attribute, string name)
    {
        // Explicit match to the attribute name passes first
        if (Comparer.Equals(attribute.Name, name))
            return true;

        // Otherwise, help the user and try to find the name starting with the provided text.
        // This is designed to help with accessing AVEVA columns that have type metadata (e.g., MyColumn(MxInteger))
        var prefix = $"{name}(";
        return attribute.Name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Resolves the value of an attribute by its name. If an exact match is not found, attempts to find an attribute
    /// whose name starts with the specified text. Throws an exception if multiple matches are found.
    /// </summary>
    /// <param name="name">The name of the attribute to resolve. This can be either the full attribute name or the prefix.</param>
    /// <returns>The resolved <see cref="AttributeValue"/>. Returns <see cref="AttributeValue.Null"/> if no match is found.</returns>
    /// <exception cref="ArgumentException">Thrown when multiple attributes match the specified name.</exception>
    private AttributeData? ResolveAttribute(string name)
    {
        // Explicit match to the attribute name wins first
        if (_attributes.TryGetValue(name, out var matched))
            return matched;

        // Otherwise, help the user and try to find the name starting with the provided text.
        // This is designed to help with accessing AVEVA columns that have type metadata (e.g., MyColumn(MxInteger))
        var prefix = $"{name}(";
        var matches = _attributes.Keys.Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList();

        return matches.Count switch
        {
            > 1 => throw new ArgumentException(
                $"Ambiguous attribute name '{name}'. Multiple attributes match: {string.Join(", ", matches)}. Please specify the full attribute name including type suffix."),
            1 => _attributes[matches[0]],
            _ => null
        };
    }
}