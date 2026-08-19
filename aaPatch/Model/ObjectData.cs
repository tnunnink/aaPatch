using System.Collections;
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
    public object? this[string name] => TryResolveAttribute(name, out var attribute) ? attribute.Value : null;

    /// <summary>
    /// Determines whether the object matches the specified filter condition.
    /// The filter is represented as a string in the format "attributeName=pattern", where the
    /// attributeName is optional and defaults to the tag name if omitted. Pattern supports wildcards
    /// with '*' to match multiple characters.
    /// </summary>
    /// <param name="filters">
    /// A string representing the filter condition. If the filter is null or empty, this method
    /// returns true. Otherwise, the filter applies to the object's attributes or tag name based
    /// on the attributeName and pattern.
    /// </param>
    /// <returns>
    /// Returns true if the object's attributes or tag name match the specified filter
    /// condition; otherwise, false.
    /// </returns>
    public bool Match(params ObjectFilter[] filters)
    {
        ArgumentNullException.ThrowIfNull(filters);

        foreach (var filter in filters)
        {
            if (!TryResolveAttribute(filter.Attribute, out var attribute))
                return false;

            var value = attribute.Value?.ToString() ?? string.Empty;
            var regex = $"^{Regex.Escape(filter.Pattern).Replace("\\*", ".*")}$";
            var match = Regex.IsMatch(value, regex, RegexOptions.IgnoreCase);
            if (!match) return false;
        }

        return true;
    }

    /// <summary>
    /// Applies a set of patches to the current object attributes based on the specified criteria.
    /// </summary>
    /// <param name="patches">A collection of patch strings to apply to the object's attributes.</param>
    /// <param name="matchCase">A boolean indicating whether patch matching should be case-sensitive.</param>
    /// <returns>The current <see cref="ObjectData"/> instance with the applied patches.</returns>
    public ObjectData Apply(IEnumerable<ObjectPatch> patches, bool matchCase = false)
    {
        foreach (var patch in patches)
            Apply(patch, matchCase);

        return this;
    }

    /// <summary>
    /// Applies the specified patch to the object data, modifying or updating its attributes
    /// based on the patch's defined conditions. Supports case-sensitive and case-insensitive
    /// operations based on the provided parameter.
    /// </summary>
    /// <param name="patch">An instance of <see cref="ObjectPatch"/> specifying the target attribute,
    /// the search term, and the replacement value. Can define an attribute-specific operation
    /// or a global search-and-replace operation across all attributes.</param>
    /// <param name="matchCase">A boolean value indicating whether the string matching
    /// should be case-sensitive. Defaults to false for case-insensitive operations.</param>
    public void Apply(ObjectPatch patch, bool matchCase = false)
    {
        switch (patch.Type)
        {
            case PatchType.ReplaceAll when patch.Find is not null:
                ReplaceAll(patch.Find, patch.Replacement, matchCase);
                break;
            case PatchType.Replace when patch.Find is not null:
                ReplaceFor(patch.Attribute, patch.Find, patch.Replacement, matchCase);
                break;
            default:
                Update(patch.Attribute, patch.Replacement);
                break;
        }
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
    public ObjectData Add(params AttributeData[] additions)
    {
        foreach (var addition in additions)
        {
            if (!_attributes.TryAdd(addition.Name, addition))
                throw new ArgumentException(
                    $"Cannot add attribute '{addition.Name}' because it already exists.\n" +
                    $"Use --patch to modify an existing attribute.");
        }

        return this;
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
    public ObjectData Project(params FieldSelection[] selections)
    {
        ArgumentNullException.ThrowIfNull(selections);
        var attributes = new Dictionary<string, AttributeData>(StringComparer.OrdinalIgnoreCase);

        foreach (var selection in selections)
        {
            if (!TryResolveAttribute(selection.Attribute, out var attribute))
                continue;

            attribute = string.IsNullOrWhiteSpace(selection.Alias)
                ? attribute.Duplicate()
                : attribute.Rename(selection.Alias);

            if (!attributes.TryAdd(attribute.Name, attribute))
                throw new ArgumentException(
                    $"Duplicate attribute name '{attribute.Name}' in projection. Attribute names must be unique after aliasing.",
                    nameof(selections));
        }

        return new ObjectData(attributes.Values);
    }

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
    /// Adds or updates an attribute with the specified value for this object data instance.
    /// </summary>
    /// <param name="name">The name of the attribute to patch. Cannot be null, whitespace, or the TagName key.</param>
    /// <param name="value">The value to assign to the attribute.</param>
    /// <returns>The current ObjectData instance for method chaining.</returns>
    private void Update(string name, string value)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Attribute name cannot be null or whitespace.", nameof(name));

        if (!TryResolveAttribute(name, out var attribute))
            return;

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

        if (!TryResolveAttribute(name, out var attribute))
            return;

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

        foreach (var attribute in _attributes.Values)
        {
            var value = attribute.Value?.ToString();
            if (value is null || !value.Contains(find, comparison)) continue;
            attribute.Update(value.Replace(find, replace, comparison));
        }
    }

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