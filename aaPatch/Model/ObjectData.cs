using System.Text.RegularExpressions;

namespace aaPatch.Model;

/// <summary>
/// Represents an exported object instance from a galaxy dump file. This record contains the parent template name and
/// tag name reference, along with the dynamic collection of attribute key/value pairs.
/// </summary>
public class ObjectData
{
    /// <summary>
    /// Defines a constant key used to identify the parent template name associated with the object data.
    /// This key is used internally to access or validate the template name within the attribute collection.
    /// </summary>
    private const string TemplateKey = ":template";

    /// <summary>
    /// Defines a constant key used to identify the attribute associated with the tag name in the object data.
    /// This key is used internally to access or verify the tag name attribute within the attribute collection.
    /// </summary>
    private const string TagNameKey = ":tagname";

    /// <summary>
    /// Stores the key-value pairs of attributes associated with this object data instance.
    /// </summary>
    private readonly Dictionary<string, AttributeData> _attributes;

    /// <summary>
    /// Represents an exported object instance from a galaxy dump file. This record contains the parent template name and
    /// tag name reference, along with the dynamic collection of attribute key/value pairs.
    /// </summary>
    public ObjectData(IEnumerable<AttributeData> attributes)
    {
        _attributes = attributes.ToDictionary(a => a.Name, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the template string associated with this instance of the data.
    /// </summary>
    public string Template => GetRequiredValue(TemplateKey);

    /// <summary>
    /// Gets the tag name identifier for this object data instance.
    /// </summary>
    public string TagName => GetRequiredValue(TagNameKey);

    /// <summary>
    /// Provides access to the collection of attribute key/value pairs associated with the object instance.
    /// This collection represents dynamic data extracted or modified within the context of the object.
    /// </summary>
    public AttributeData[] Attributes => [.. _attributes.Values];

    /// <summary>
    /// Provides an indexer for accessing object data attributes by name. The indexer allows retrieval of the
    /// value associated with a specific attribute, including special cases for "Template" and "TagName".
    /// </summary>
    /// <param name="name">The name of the attribute to retrieve. Use "Template" or "TagName" to access their corresponding values,
    /// or the name of a specific object attribute.</param>
    /// <returns>The value of the requested attribute if it exists, or null if the attribute is not defined.</returns>
    public object? this[string name]
    {
        get
        {
            return name switch
            {
                "Template" => Template,
                "TagName" => TagName,
                _ => _attributes.GetValueOrDefault(name)?.Value
            };
        }
    }

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
    public bool IsMatch(string? filter)
    {
        if (string.IsNullOrEmpty(filter))
            return true;

        var index = filter.IndexOf('=');
        var attributeName = index > 0 ? filter[..index] : TagNameKey;
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
    /// Adds or updates an attribute with the specified value for this object data instance.
    /// </summary>
    /// <param name="name">The name of the attribute to patch. Cannot be null, whitespace, or the TagName key.</param>
    /// <param name="value">The value to assign to the attribute.</param>
    /// <returns>The current ObjectData instance for method chaining.</returns>
    public void Update(string name, string value)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Attribute name cannot be null or whitespace.", nameof(name));

        if (StringComparer.OrdinalIgnoreCase.Equals(TagNameKey, name))
            throw new ArgumentException("Cannot modify the TagName attribute.", nameof(name));

        if (!_attributes.TryGetValue(name, out var attribute))
            throw new ArgumentException($"Attribute '{name}' does not exist in the object.", nameof(name));

        _attributes[name] = attribute.With(value);
    }

    /// <summary>
    /// Replaces occurrences of a specified substring with a replacement string in the values of the object's attributes.
    /// The method can target all attributes or a specific attribute based on the provided name.
    /// </summary>
    /// <param name="find">The substring to search for in the attribute values.</param>
    /// <param name="replace">The string to replace the found substring with.</param>
    /// <param name="name">The name of the specific attribute to apply the operation to. If null, the operation is applied to all attributes.</param>
    /// <param name="matchCase">True to perform a case-sensitive search; false to perform a case-insensitive search. Default is false.</param>
    public void Replace(string find, string replace, string? name = null, bool matchCase = false)
    {
        var comparison = matchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

        // Apply to all attributes if no name is specified.
        if (name is null)
        {
            foreach (var attribute in _attributes.Values)
            {
                if (attribute.Name == TagNameKey) continue;
                var value = attribute.Value?.ToString();
                if (value is null || !value.Contains(find, comparison)) continue;
                _attributes[attribute.Name] = attribute.With(value.Replace(find, replace, comparison));
            }

            return;
        }
        
        // Apply to specified attribute name
        if (!_attributes.TryGetValue(name, out var target))
            throw new ArgumentException($"Attribute '{name}' does not exist in the object.", nameof(name));
        
        var current = target.Value?.ToString();
        if (current is null || !current.Contains(find, comparison)) return;
        _attributes[target.Name] = target.With(current.Replace(find, replace, comparison));
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
        return string.Join(",", _attributes.Values.Select(a => a.ToString()));
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
        if (!_attributes.TryGetValue(key, out var attribute))
            throw new InvalidOperationException($"Required attribute {key} does not exist.");

        var result = attribute.Value?.ToString();

        if (string.IsNullOrEmpty(result))
            throw new InvalidOperationException($"Required attribute {key} has an invalid null or empty value.");

        return result;
    }
}