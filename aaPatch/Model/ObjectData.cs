using System.Collections;
using System.Linq.Dynamic.Core.CustomTypeProviders;
using System.Security.Cryptography;
using System.Text;

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
    /// A unique identifier that represents the specific instance of the exported object data.
    /// This property generates a new globally unique identifier (GUID) to ensure object-level
    /// uniqueness, which can be useful for distinguishing records in a system or tracking.
    /// </summary>
    public Guid RecordId { get; } = Guid.NewGuid();

    /// <summary>
    /// 
    /// </summary>
    public string SchemaId => field ??= GetSchemaHash();

    /// <summary>
    /// Computes a unique hash for the schema based on the current set of attribute headers.
    /// This hash is generated using the SHA-256 algorithm applied to a comma-separated,
    /// case-insensitive list of attribute names and returned as a Base64-encoded string.
    /// </summary>
    /// <returns>
    /// A Base64-encoded string representing the SHA-256 hash of the schema's attribute headers.
    /// </returns>
    private string GetSchemaHash()
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

        foreach (var name in _attributes.Keys)
        {
            var normalized = Encoding.UTF8.GetBytes(name.ToUpperInvariant());
            hash.AppendData(BitConverter.GetBytes(normalized.Length));
            hash.AppendData(normalized);
        }

        return Convert.ToHexString(hash.GetHashAndReset());
    }

    /// <summary>
    /// Gets the total number of attributes associated with the object.
    /// </summary>
    public int Count => _attributes.Count;

    /// <summary>
    /// Retrieves the value of an attribute by its name in the context of the current object.
    /// This indexer provides access to a specific attribute's value based on the provided attribute name.
    /// </summary>
    /// <param name="name">The name of the attribute to retrieve.</param>
    /// <returns>The value of the attribute as an <see cref="AttributeValue"/> object.</returns>
    /// <exception cref="ArgumentException">Thrown when multiple attributes match the specified name.</exception>
    public AttributeValue this[string name] => ResolveAttribute(name);

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
    /// Resolves the value of an attribute by its name. If an exact match is not found, attempts to find an attribute
    /// whose name starts with the specified text. Throws an exception if multiple matches are found.
    /// </summary>
    /// <param name="name">The name of the attribute to resolve. This can be either the full attribute name or the prefix.</param>
    /// <returns>The resolved <see cref="AttributeValue"/>. Returns <see cref="AttributeValue.Null"/> if no match is found.</returns>
    /// <exception cref="ArgumentException">Thrown when multiple attributes match the specified name.</exception>
    private AttributeValue ResolveAttribute(string name)
    {
        // Explicit match to the attribute name wins first
        if (_attributes.TryGetValue(name, out var matched))
            return matched.Value;

        // Otherwise, help the user and try to find the name starting with the provided text.
        // This is designed to help with accessing AVEVA columns that have type metadata (e.g., MyColumn(MxInteger))
        var prefix = $"{name}(";
        var matches = _attributes.Keys.Where(k => k.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)).ToList();

        return matches.Count switch
        {
            > 1 => throw new ArgumentException(
                $"Ambiguous attribute name '{name}'. Multiple attributes match: {string.Join(", ", matches)}. Please specify the full attribute name including type suffix."),
            1 => _attributes[matches[0]].Value,
            _ => AttributeValue.Null
        };
    }
}