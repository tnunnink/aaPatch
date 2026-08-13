using System.Globalization;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;

namespace aaPatch.Model;

/// <summary>
/// Provides methods to read and write object data stored in files using a template-based structure.
/// </summary>
public static class GalaxyDump
{
    /// <summary>
    /// Represents a constant key used to identify the template attribute in the object data.
    /// This key is used internally for accessing or verifying the template string associated with the object.
    /// </summary>
    private const string TemplateKey = ":TEMPLATE=";

    /// <summary>
    /// Represents a constant key used to identify the tag name attribute in the object data.
    /// This key is used internally for accessing or verifying the tag name string associated with an object.
    /// </summary>
    private const string TagNameKey = ":Tagname";

    /// <summary>
    /// Defines a regular expression used to separate object data segments by templates within a text structure.
    /// This separator identifies segments starting with the predefined template key and processes them in a case-insensitive, multiline context.
    /// </summary>
    private static readonly Regex TemplateSeparator = new(
        $"^{Regex.Escape(TemplateKey)}",
        RegexOptions.Multiline | RegexOptions.IgnoreCase);

    /// <summary>
    /// Reads a text representation of object data organized by templates and converts it into a collection of <see cref="ObjectData"/> instances.
    /// Each segment of the text must correspond to a template with associated object data structured in a tabular format.
    /// </summary>
    /// <param name="text">The text content to parse, representing template-based object data. Cannot be null, empty, or whitespace.</param>
    /// <returns>A collection of <see cref="ObjectData"/> extracted from the provided text.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="text"/> is null, empty, or does not follow the expected format.</exception>
    public static IEnumerable<ObjectData> Read(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("The text parameter cannot be null or empty.", nameof(text));

        var templates = TemplateSeparator.Split(text)
            .Select(segment => segment.Trim())
            .Where(segment => segment.StartsWith('$'));

        return templates.SelectMany(ReadTemplate);

        IEnumerable<ObjectData> ReadTemplate(string segment)
        {
            // We know that each segment needs at least 3 lines (template identifier, attribute header, and instance(s) row)
            var lines = segment.Split(["\r\n", "\n"], StringSplitOptions.None);

            switch (lines.Length)
            {
                case < 2:
                    throw new ArgumentException(
                        """
                        Invalid template format: Missing column header row.
                        Expected format is ':TEMPLATE=<name>' followed by a header row with column names.
                        """
                    );
            }

            // Read the template name for this set of object instances and recombine
            // all the records to single string that CsvHelper can easily parse for us.
            var template = lines[0];
            var instances = string.Join(Environment.NewLine, lines[1..]);

            using var reader = new StringReader(instances);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Mode = CsvMode.RFC4180,
                TrimOptions = TrimOptions.Trim
            });

            var records = csv.GetRecords<dynamic>().Cast<IDictionary<string, object?>>().ToArray();

            foreach (var record in records)
            {
                // Manually inject normalized template and tag name attributes into the object data.
                // This way we don't have to use special colon syntax to reference these values.
                // The write method will restructure the output according to AVEVA format.
                var attributes = new Dictionary<string, AttributeData>
                {
                    ["Template"] = new("Template", template),
                    ["TagName"] = new("TagName", record[TagNameKey]?.ToString())
                };

                foreach (var item in record)
                {
                    if (StringComparer.OrdinalIgnoreCase.Equals(item.Key, TagNameKey))
                        continue;

                    attributes[item.Key] = new AttributeData(item.Key, item.Value?.ToString());
                }

                yield return new ObjectData(attributes.Values);
            }
        }
    }

    /// <summary>
    /// Writes a collection of <see cref="ObjectData"/> instances to a text format based on a template-based structured format.
    /// Each group of object data is organized by its template, including templates, headers, and instance values.
    /// </summary>
    /// <param name="data">The collection of <see cref="ObjectData"/> to write. Cannot be null.</param>
    /// <returns>A string representing the serialized object data in a template-based format.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="data"/> is null.</exception>
    public static string Write(IEnumerable<ObjectData> data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var config = new CsvConfiguration(CultureInfo.InvariantCulture) { Mode = CsvMode.RFC4180 };
        using var writer = new StringWriter();
        using var csv = new CsvWriter(writer, config);

        // need to group output by template for system to import correctly
        var groups = data.GroupBy(x => x.Template);

        foreach (var group in groups)
        {
            // First line for each group is the template key.
            writer.WriteLine($"{TemplateKey}{group.Key}");

            // Write the leading tag name key for each instance.
            csv.WriteField(TagNameKey);

            // Write remaining headers based on the first object.
            group.First().Where(a => !a.IsIdentity).Select(a => a.Header).ToList().ForEach(csv.WriteField);
            csv.NextRecord();

            // Write row for each instance in the template group.
            foreach (var instance in group)
            {
                //Explicitly write the tag name as the first attribute
                csv.WriteField(instance.TagName);

                foreach (var attribute in instance.Where(a => !a.IsIdentity))
                    csv.WriteField(attribute.ToString());

                csv.NextRecord();
            }

            writer.WriteLine();
        }

        return writer.ToString().TrimEnd();
    }
}