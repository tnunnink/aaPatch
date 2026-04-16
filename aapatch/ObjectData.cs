namespace aaPatch;

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
    private readonly IDictionary<string, string?> _attributes;

    /// <summary>
    /// Represents an exported object instance from a galaxy dump file. This record contains the parent template name and
    /// tag name reference, along with the dynamic collection of attribute key/value pairs.
    /// </summary>
    public ObjectData(string template, IDictionary<string, string?> attributes)
    {
        if (string.IsNullOrWhiteSpace(template))
            throw new ArgumentException("Template name is required for object data.");

        Template = template;
        _attributes = attributes;
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
    /// Provides a collection of attribute keys associated with the object data instance.
    /// This property returns an array of strings, representing the keys of all attributes
    /// stored within the object data.
    /// </summary>
    public string[] Attributes => _attributes.Keys.ToArray();

    /// <summary>
    /// Provides an array containing all values of the key-value pairs stored
    /// as attributes in the object data instance.
    /// </summary>
    public string?[] Values => _attributes.Values.ToArray();

    /// <summary>
    /// Retrieves the value associated with the specified attribute in this object data instance.
    /// </summary>
    /// <param name="attribute">The name of the attribute to retrieve. Cannot be null or whitespace.</param>
    /// <returns>The value of the specified attribute.</returns>
    /// <exception cref="ArgumentException">Thrown when the attribute name is null, empty, or whitespace.</exception>
    public string? GetValue(string attribute)
    {
        if (string.IsNullOrWhiteSpace(attribute))
            throw new ArgumentException("Attribute name cannot be null or whitespace.", nameof(attribute));

        return _attributes[attribute];
    }

    /// <summary>
    /// Attempts to retrieve the value associated with the specified attribute.
    /// </summary>
    /// <param name="attribute">The name of the attribute to retrieve. Cannot be null or whitespace.</param>
    /// <param name="value">
    /// When this method returns, contains the value associated with the specified attribute if the attribute exists; otherwise, the default value for the <see cref="string"/> type.
    /// </param>
    /// <returns><c>true</c> if the attribute exists and a value is retrieved; otherwise, <c>false</c>.</returns>
    public bool TryGetValue(string attribute, out string value)
    {
        if (string.IsNullOrWhiteSpace(attribute))
            throw new ArgumentException("Attribute name cannot be null or whitespace.", nameof(attribute));

        return _attributes.TryGetValue(attribute, out value!);
    }

    /// <summary>
    /// Adds or updates an attribute with the specified value for this object data instance.
    /// </summary>
    /// <param name="attribute">The name of the attribute to patch. Cannot be null, whitespace, or the TagName key.</param>
    /// <param name="value">The value to assign to the attribute.</param>
    /// <returns>The current ObjectData instance for method chaining.</returns>
    public void Patch(string attribute, string value)
    {
        if (string.IsNullOrWhiteSpace(attribute))
            throw new ArgumentException("Attribute name cannot be null or whitespace.", nameof(attribute));

        if (attribute == TagNameKey)
            throw new ArgumentException("Cannot modify the TagName attribute directly.", nameof(attribute));

        if (!_attributes.TryAdd(attribute, value))
            _attributes[attribute] = value;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="attribute"></param>
    /// <param name="find"></param>
    /// <param name="replace"></param>
    /// <returns></returns>
    public void Patch(string attribute, string find, string replace)
    {
        if (string.IsNullOrWhiteSpace(attribute))
            throw new ArgumentException("Attribute name cannot be null or whitespace.", nameof(attribute));

        if (_attributes.TryGetValue(attribute, out var current) && current is not null)
        {
            var updated = current.Replace(find, replace);
            _attributes[attribute] = updated;
        }
    }

    /// <summary>
    /// Applies a transformation function to each key-value pair in the attribute dictionary of this object data
    /// instance and updates their values.
    /// </summary>
    /// <param name="update">A delegate that defines the transformation to apply.
    /// Takes the attribute key and current value as parameters and returns the updated value.</param>
    /// <returns>The current instance of <see cref="ObjectData"/> with updated attributes.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="update"/> parameter is null.</exception>
    public ObjectData Patch(Func<string, string?, string?> update)
    {
        ArgumentNullException.ThrowIfNull(update);

        foreach (var attribute in _attributes)
        {
            var value = update.Invoke(attribute.Key, attribute.Value);
            _attributes[attribute.Key] = value;
        }

        return this;
    }

    /// <summary>
    /// Updates the attributes of the current object data instance based on the specified update function and predicate.
    /// Attributes that match the predicate will be updated with the new value generated by the update function.
    /// </summary>
    /// <param name="update">A function that takes an attribute name and its current value and returns the updated value.</param>
    /// <param name="predicate">A function that takes an attribute name and its current value and determines whether the attribute should be updated.</param>
    /// <returns>The updated instance of the current object data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the update function or predicate is null.</exception>
    public ObjectData Patch(Func<string, string?, string> update, Func<string, string?, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(update);
        ArgumentNullException.ThrowIfNull(predicate);

        foreach (var attribute in _attributes)
        {
            if (!predicate.Invoke(attribute.Key, attribute.Value)) continue;
            var value = update.Invoke(attribute.Key, attribute.Value);
            _attributes[attribute.Key] = value;
        }

        return this;
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
        if (!_attributes.TryGetValue(TagNameKey, out var tagName) || string.IsNullOrWhiteSpace(tagName))
        {
            throw new InvalidOperationException(
                $"Required attribute {TagNameKey} does not exist or has an invalid value.");
        }

        return tagName;
    }
}