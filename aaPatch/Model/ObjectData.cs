namespace aaPatch.Model;

/// <summary>
/// Represents an exported object instance from a galaxy dump file. This record contains the parent template name and
/// tag name reference, along with the dynamic collection of attribute key/value pairs.
/// </summary>
public class ObjectData
{
    /// <summary>
    /// Defines a constant key used to identify the attribute associated with the tag name in the object data.
    /// This key is used internally to access or verify the tag name attribute within the attribute collection.
    /// </summary>
    private const string TagNameKey = ":Tagname";

    /// <summary>
    /// Stores the key-value pairs of attributes associated with this object data instance.
    /// </summary>
    private readonly Dictionary<string, AttributeData> _attributes;

    /// <summary>
    /// Stores a collection of key/value pairs that represent modified attributes or patches
    /// applied to the object data. This dictionary is used to track changes or overrides
    /// to the original attributes of the object.
    /// </summary>
    private readonly List<AttributeData> _patches = [];

    /// <summary>
    /// Represents an exported object instance from a galaxy dump file. This record contains the parent template name and
    /// tag name reference, along with the dynamic collection of attribute key/value pairs.
    /// </summary>
    public ObjectData(string template, IEnumerable<AttributeData> attributes)
    {
        if (string.IsNullOrWhiteSpace(template))
            throw new ArgumentException("Template name is required for object data.");

        Template = template;
        _attributes = attributes.ToDictionary(a => a.Name);
    }

    /// <summary>
    /// Gets the template string associated with this instance of the data.
    /// </summary>
    public string Template { get; }

    /// <summary>
    /// Gets the tag name identifier for this object data instance.
    /// </summary>
    public string TagName => GetTagName();

    /// <summary>
    /// Provides access to the collection of attribute key/value pairs associated with the object instance.
    /// This collection represents dynamic data extracted or modified within the context of the object.
    /// </summary>
    public AttributeData[] Attributes => _attributes.Values.ToArray();

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
    /// Adds or updates an attribute with the specified value for this object data instance.
    /// </summary>
    /// <param name="name">The name of the attribute to patch. Cannot be null, whitespace, or the TagName key.</param>
    /// <param name="value">The value to assign to the attribute.</param>
    /// <returns>The current ObjectData instance for method chaining.</returns>
    public void Update(string name, string value)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Attribute name cannot be null or whitespace.", nameof(name));

        if (name == TagNameKey)
            throw new ArgumentException("Cannot modify the TagName attribute.", nameof(name));

        if (_attributes.TryGetValue(name, out var attribute))
        {
            var patch = attribute.With(value);
            if (!Equals(attribute.Value, patch.Value))
            {
                _patches.Add(patch);
            }
        }
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
                if (value is not null && value.Contains(find, comparison))
                {
                    _patches.Add(attribute.With(value.Replace(find, replace, comparison)));
                }
            }

            return;
        }

        // Apply to specified attribute name
        if (_attributes.TryGetValue(name, out var target) && target.Value is not null)
        {
            var value = target.Value.ToString();
            if (value is not null && value.Contains(find, comparison))
            {
                _patches.Add(target.With(value.Replace(find, replace, comparison)));
            }
        }
    }

    /// <summary>
    /// Generates a collection of formatted strings describing the changes (or "diffs") between the
    /// current attribute values and their patched values for the associated object. This method
    /// compares the original attributes with the patched values and creates a textual representation
    /// highlighting the differences.
    /// </summary>
    /// <returns>
    /// An enumerable collection of strings, where each string represents a diff in the format:
    /// "TagName: 'AttributeName' "OriginalValue" -> "PatchedValue"".
    /// </returns>
    public IEnumerable<string> Diffs()
    {
        return _patches.Select(p =>
        {
            var original = _attributes[p.Name];
            return $"{TagName}: '{original.Name}' \"{original.Value}\" -> \"{p.Value}\"";
        });
    }

    /// <summary>
    /// Applies all pending modifications stored in the internal patch collection to the object's attributes.
    /// Once invoked, the object's attributes are updated to reflect the changes contained in the patches.
    /// The patch collection is used to temporarily store modifications and is applied to the object's state during this method's execution.
    /// </summary>
    public void ApplyPatches()
    {
        foreach (var patch in _patches)
        {
            _attributes[patch.Name] = patch;
        }

        _patches.Clear();
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
    /// Retrieves the tag name of the object data instance from the attribute collection.
    /// The tag name is identified using a predefined key.
    /// </summary>
    /// <returns>
    /// The tag name associated with the object data instance.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the tag name attribute is missing or has an invalid value in the attribute collection.
    /// </exception>
    private string GetTagName()
    {
        if (!_attributes.TryGetValue(TagNameKey, out var attribute))
            throw new InvalidOperationException($"Required attribute {TagNameKey} does not exist.");

        var tagName = attribute.Value?.ToString();

        if (string.IsNullOrEmpty(tagName))
            throw new InvalidOperationException($"Required attribute {TagNameKey} has invalid null or empty value.");

        return tagName;
    }
}