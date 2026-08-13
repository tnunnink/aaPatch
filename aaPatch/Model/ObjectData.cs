using System.Collections;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace aaPatch.Model;

/// <summary>
/// Represents an exported object instance from a galaxy dump file. This record contains the parent template name and
/// tag name reference, along with the dynamic collection of attribute key/value pairs.
/// </summary>
public class ObjectData : IReadOnlyCollection<AttributeData>
{
    /// <summary>
    /// Provides a predefined string comparer that performs case-insensitive string comparisons.
    /// This comparer is commonly used when string operations need to ignore the case sensitivity
    /// of characters, such as in dictionary keys or grouping operations.
    /// </summary>
    private static readonly StringComparer Comparer = StringComparer.OrdinalIgnoreCase;

    /// <summary>
    /// Stores the collection of attribute data instances associated with this object.
    /// </summary>
    private readonly List<AttributeData> _attributes;

    /// <summary>
    /// Maintains a dictionary mapping attribute headers to their corresponding <see cref="AttributeData"/> instances,
    /// enabling efficient lookup and management of attributes by their header values.
    /// </summary>
    private readonly Dictionary<string, AttributeData> _byHeader;

    /// <summary>
    /// Maintains a mapping of attribute names to their corresponding collections of attributes.
    /// This dictionary enables quick access to attributes grouped by their names, which is useful
    /// for operations like retrieval, updates, and projections based on attribute names.
    /// </summary>
    private readonly Dictionary<string, List<AttributeData>> _byName;

    /// <summary>
    /// Represents an exported object instance from a galaxy dump file. This record contains the parent template name and
    /// tag name reference, along with the dynamic collection of attribute key/value pairs.
    /// </summary>
    public ObjectData(IEnumerable<AttributeData> attributes)
    {
        _attributes = [.. attributes];
        _byHeader = _attributes.ToDictionary(a => a.Header, Comparer);
        _byName = _attributes.GroupBy(a => a.Name).ToDictionary(x => x.Key, x => x.ToList(), Comparer);
    }

    /// <summary>
    /// Gets the total number of attributes associated with the object.
    /// </summary>
    public int Count => _attributes.Count;

    /// <summary>
    /// Gets the template string associated with this instance of the data.
    /// </summary>
    public string Template => GetRequiredValue(nameof(Template));

    /// <summary>
    /// Gets the tag name identifier for this object data instance.
    /// </summary>
    public string TagName => GetRequiredValue(nameof(TagName));

    /// <summary>
    /// Provides an indexer for accessing object data attributes by name. The indexer allows retrieval of the
    /// value associated with a specific attribute, including special cases for "Template" and "TagName".
    /// </summary>
    /// <param name="name">The name of the attribute to retrieve. Use "Template" or "TagName" to access their corresponding values,
    /// or the name of a specific object attribute.</param>
    /// <returns>The value of the requested attribute if it exists, or null if the attribute is not defined.</returns>
    public object? this[string name] => GetAttribute(name).Value;

    /// <summary>
    /// Determines whether the object matches the specified filter condition.
    /// The filter is represented as a string in the format "attributeName=pattern", where the
    /// attributeName is optional and defaults to the tag name if omitted. Pattern supports wildcards
    /// with '*' to match multiple characters.
    /// </summary>
    /// <param name="filter">
    /// A string representing the filter condition. If the filter is null or empty, this method
    /// returns true. Otherwise, the filter applies to the object's attributes or tag name based
    /// on the attributeName and pattern.
    /// </param>
    /// <returns>
    /// Returns true if the object's attributes or tag name match the specified filter
    /// condition; otherwise, false.
    /// </returns>
    public bool Matches(string? filter)
    {
        if (string.IsNullOrEmpty(filter))
            return true;

        var index = filter.IndexOf('=');
        var attributeName = index > 0 ? filter[..index] : nameof(TagName);
        var pattern = index > 0 ? filter[(index + 1)..] : filter;
        var value = this[attributeName]?.ToString() ?? string.Empty;
        return MatchesFilter(value, pattern);

        bool MatchesFilter(string v, string? p)
        {
            if (string.IsNullOrEmpty(p)) return true;
            var regex = $"^{Regex.Escape(p).Replace("\\*", ".*")}$";
            return Regex.IsMatch(v, regex, RegexOptions.IgnoreCase);
        }
    }

    /// <summary>
    /// Projects a subset of attributes from the current object based on the provided selection criteria.
    /// Creates a new instance of <see cref="ObjectData"/> that includes only the attributes specified
    /// in the selections, with optional renaming of attributes if aliasing is provided in the selection string.
    /// </summary>
    /// <param name="selections">
    /// A collection of strings representing attribute names to include in the projection. Each string can optionally
    /// take the form "Name=Alias", where "Name" is the original attribute name and "Alias" is the desired name in the projection.
    /// </param>
    /// <returns>
    /// A new <see cref="ObjectData"/> instance containing the specified subset of attributes, with any aliases applied as specified.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when a specified attribute name is null, empty, or whitespace, or if the attribute does not exist in the current object.
    /// </exception>
    public ObjectData Project(IEnumerable<string> selections)
    {
        ArgumentNullException.ThrowIfNull(selections);
        var attributes = new Dictionary<string, AttributeData>(StringComparer.OrdinalIgnoreCase);

        foreach (var selection in selections)
        {
            if (string.IsNullOrWhiteSpace(selection))
                throw new ArgumentException("Selection string cannot be null, empty, or whitespace.",
                    nameof(selections));

            var index = selection.IndexOf('=');
            var name = index > 0 ? selection[..index] : selection;
            var alias = index > 0 ? selection[(index + 1)..] : string.Empty;

            var attribute = GetAttribute(name);
            attribute = string.IsNullOrWhiteSpace(alias) ? attribute.Duplicate() : attribute.Rename(alias);

            if (!attributes.TryAdd(attribute.Header, attribute))
                throw new ArgumentException(
                    $"Duplicate attribute name '{attribute.Header}' in projection. Attribute names must be unique after aliasing.",
                    nameof(selections));
        }

        return new ObjectData(attributes.Values);
    }

    /// <summary>
    /// Adds new attributes to the current object instance. If any of the provided attribute names
    /// already exist in the object, an exception is thrown to prevent duplicates.
    /// </summary>
    /// <param name="additions">
    /// A collection of attribute strings in the format "name=value", where "name" specifies the
    /// attribute name and "value" specifies its corresponding value. Attribute names must not
    /// be null, empty, or whitespace.
    /// </param>
    /// <returns>
    /// The current <see cref="ObjectData"/> instance with the new attributes added.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when an attribute with the same name already exists or if the attribute name
    /// is null, empty, or whitespace.
    /// </exception>
    public ObjectData Add(params string[] additions)
    {
        foreach (var addition in additions)
        {
            var index = addition.IndexOf('=');
            if (index <= 0)
                throw new ArgumentException($"Invalid addition format '{addition}'. Expected 'Attribute=Value'.");

            var name = addition[..index].Trim();

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Attribute name cannot be null or whitespace.");

            if (_byHeader.ContainsKey(name))
                throw new ArgumentException(
                    $"Cannot add attribute '{name}' because it already exists. Use --patch to modify an existing attribute.");

            var value = addition[(index + 1)..];
            var attribute = new AttributeData(name, value);

            _attributes.Add(attribute);
            _byHeader[attribute.Header] = attribute;

            if (!_byName.TryAdd(attribute.Name, [attribute]))
                _byName[attribute.Name].Add(attribute);
        }

        return this;
    }

    /// <summary>
    /// Applies a set of patches to the current object attributes based on the specified criteria.
    /// </summary>
    /// <param name="patches">A collection of patch strings to apply to the object's attributes.</param>
    /// <param name="matchCase">A boolean indicating whether patch matching should be case-sensitive.</param>
    /// <returns>The current <see cref="ObjectData"/> instance with the applied patches.</returns>
    public ObjectData Apply(IEnumerable<string> patches, bool matchCase = false)
    {
        foreach (var patch in patches)
            Apply(patch, matchCase);

        return this;
    }

    /// <summary>
    /// Applies a modification to the object's attributes based on the provided patch string.
    /// Supports three modes:
    /// - Global Find and Replace (":Find=Replace"): Replaces all instances of a specified value
    /// across attributes.
    /// - Attribute-specific Find and Replace ("Attribute:Find=Replace"): Updates a specific attribute
    /// by finding and replacing the specified value.
    /// - Direct Assignment ("Attribute=Value"): Sets the specified attribute to a new value.
    /// </summary>
    /// <param name="patch">
    /// The modification instruction to apply. The format is required to be one of the supported modes:
    /// ":Find=Replace", "Attribute:Find=Replace", or "Attribute=Value".
    /// </param>
    /// <param name="matchCase">
    /// A flag indicating whether the operation should be case-sensitive. Defaults to false.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when the format of the patch string is invalid or when attempting to modify a
    /// non-existing attribute.
    /// </exception>
    public void Apply(string patch, bool matchCase = false)
    {
        if (patch.StartsWith(':') && patch.Contains('='))
        {
            // Global Find and Replace Mode -> ":Find=Replace"
            var parts = patch[1..].Split('=', 2);

            if (parts.Length != 2)
                throw new ArgumentException("Invalid global find-replace format. Expected ':Find=Replace'.");

            ReplaceAll(parts[0], parts[1], matchCase);
        }
        else if (patch.Contains(':') && patch.IndexOf(':') < patch.IndexOf('='))
        {
            // Attribute-specific Find and Replace Mode -> "Attribute:Find=Replace"
            var parts = patch.Split([':', '='], 3);

            if (parts.Length != 3)
                throw new ArgumentException(
                    "Invalid find-replace patch format. Expected 'Attribute:Find=Replace'.");

            ReplaceFor(parts[0], parts[1], parts[2], matchCase);
        }
        else
        {
            // Direct Assignment Mode -> "Attribute=Value"
            var parts = patch.Split('=', 2);

            if (parts.Length != 2)
                throw new ArgumentException("Invalid patch format. Expected 'Attribute=Value'.");

            Update(parts[0], parts[1]);
        }
    }

    /// <summary>
    /// Returns a string representation of the object data by concatenating the values
    /// of all attributes, separated by commas.
    /// </summary>
    /// <returns>
    /// A comma-separated string that represents the values of the object's attributes.
    /// </returns>
    public override string ToString()
    {
        return string.Join(",", _attributes.Select(a => a.ToString()));
    }

    /// <inheritdoc />
    public IEnumerator<AttributeData> GetEnumerator()
    {
        return _attributes.AsEnumerable().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Adds or updates an attribute with the specified value for this object data instance.
    /// </summary>
    /// <param name="name">The name of the attribute to patch. Cannot be null, whitespace, or the TagName key.</param>
    /// <param name="value">The value to assign to the attribute.</param>
    /// <returns>The current ObjectData instance for method chaining.</returns>
    private void Update(string name, string value)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Attribute name cannot be null or whitespace.", nameof(name));

        var attribute = GetAttribute(name);
        attribute.Update(value);
    }

    /// <summary>
    /// Replaces a specified substring within the value of an attribute with another substring,
    /// optionally considering case sensitivity during the replacement.
    /// </summary>
    /// <param name="name">The name of the attribute whose value is to be modified.</param>
    /// <param name="find">The substring to find within the attribute's value.</param>
    /// <param name="replace">The substring to replace the found substring with.</param>
    /// <param name="matchCase">A boolean indicating whether the replacement should respect case sensitivity. If true, the comparison is case-sensitive.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when the specified attribute name is null, whitespace, or does not exist, or if the name cannot uniquely identify the attribute.
    /// </exception>
    private void ReplaceFor(string name, string find, string replace, bool matchCase = false)
    {
        var comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        var attribute = GetAttribute(name);

        var value = attribute.Value?.ToString();
        if (value is null || !value.Contains(find, comparison)) return;
        attribute.Update(value.Replace(find, replace, comparison));
    }

    /// <summary>
    /// Replaces all occurrences of a specified substring within the attribute values of the object data.
    /// </summary>
    /// <param name="find">The substring to search for within attribute values.</param>
    /// <param name="replace">The substring to replace the found occurrences with.</param>
    /// <param name="matchCase">Specifies whether the search should be case-sensitive. Default is false.</param>
    private void ReplaceAll(string find, string replace, bool matchCase = false)
    {
        var comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        foreach (var attribute in _attributes)
        {
            var value = attribute.Value?.ToString();
            if (value is null || !value.Contains(find, comparison)) continue;
            attribute.Update(value.Replace(find, replace, comparison));
        }
    }

    /// <summary>
    /// Retrieves the value associated with the specified key from the attribute collection.
    /// If the key does not exist or the value is null or empty, an exception is thrown.
    /// </summary>
    /// <param name="key">The key of the attribute to retrieve.</param>
    /// <returns>The value of the specified attribute as a non-null, non-empty string.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the specified key is not found or if the corresponding value is null or empty.
    /// </exception>
    private string GetRequiredValue(string key)
    {
        if (!_byHeader.TryGetValue(key, out var attribute))
            throw new InvalidOperationException($"Required attribute {key} does not exist.");

        var result = attribute.Value?.ToString();

        if (string.IsNullOrEmpty(result))
            throw new InvalidOperationException($"Required attribute {key} has an invalid null or empty value.");

        return result;
    }

    /// <summary>
    /// Retrieves an attribute based on the provided text, which can correspond
    /// to either the header name or a non-unique attribute name in the object data.
    /// </summary>
    /// <param name="text">
    /// The name of the attribute to retrieve. This can be either the exact header
    /// name or a non-unique attribute name. Must not be null, empty, or whitespace.
    /// </param>
    /// <returns>
    /// An <see cref="AttributeData"/> instance representing the matching attribute.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if the provided text is null, empty, only whitespace, or does not
    /// match any attributes in the object.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the provided text matches multiple attributes with the same name,
    /// causing ambiguity.
    /// </exception>
    private AttributeData GetAttribute(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Attribute name cannot be null or whitespace.", nameof(text));

        // Explicit match to the header text first
        if (_byHeader.TryGetValue(text, out var matched))
            return matched;

        // Otherwise try to get by name which might not be unique.
        if (!_byName.TryGetValue(text, out var attributes))
            throw new ArgumentException($"Attribute '{text}' does not exist in the object.", nameof(text));

        if (attributes.Count > 1)
            throw new InvalidOperationException(
                $"Attribute name '{text}' matches {attributes.Count} attributes and cannot be uniquely identified. " +
                $"Use the full header name to specify the exact attribute.");

        return attributes[0];
    }
}

/// <summary>
/// Provides extension methods for the ObjectData class, enabling additional functionalities such as serialization
/// of ObjectData instances into various formats.
/// </summary>
public static class ObjectDataExtensions
{
    /// <summary>
    /// Defines the JSON serialization options used for customizing the behavior of JSON output,
    /// such as enabling indented formatting for better readability of the serialized data.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    /// <summary>
    /// Serializes a collection of ObjectData instances into a specified output format.
    /// Supported formats are "aveva" and "json".
    /// </summary>
    /// <param name="data">The collection of ObjectData instances to serialize.</param>
    /// <param name="format">The output format for serialization. Supported values are "aveva" and "json".</param>
    /// <returns>The serialized string representation of the ObjectData collection.</returns>
    /// <exception cref="ArgumentException">Thrown when an unsupported output format is specified.</exception>
    public static string Serialize(this IEnumerable<ObjectData> data, string format)
    {
        return format.Trim().ToLowerInvariant() switch
        {
            "aveva" => GalaxyDump.Write(data),
            "json" => WriteJson([.. data]),
            _ => throw new ArgumentException($"Unsupported output format '{format}'.")
        };

        string WriteJson(ICollection<ObjectData> d)
        {
            var duplicate = d.SelectMany(a => a.GroupBy(x => x.Name)).FirstOrDefault(g => g.Count() > 1);

            if (duplicate is not null)
                throw new InvalidOperationException(
                    $"Cannot serialize to JSON: Attribute '{duplicate.Key}' appears multiple times in an object. " +
                    "JSON format requires unique attribute names. Use --select with aliases to rename duplicate attributes.");

            var dictionary = d.Select(x => x.ToDictionary(
                a => a.Name,
                a => a.Value,
                StringComparer.OrdinalIgnoreCase)
            );

            return JsonSerializer.Serialize(dictionary, JsonOptions);
        }
    }
}