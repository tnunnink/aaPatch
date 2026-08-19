using System.Globalization;
using System.Text.RegularExpressions;
using aaPatch.Model;
using CsvHelper;
using CsvHelper.Configuration;

namespace aaPatch.Formats;

/// <summary>
/// Provides methods to read and write data formatted according to Aveva object templates.
/// This class implements functionality for parsing structured textual input into
/// collections of objects and serializing object data into a text representation suitable
/// for Aveva systems.
/// </summary>
public class AvevaFormatter : IObjectFormater
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
    /// Represents the identifier used to denote the tag name attribute within the Aveva object template.
    /// </summary>
    private const string TemplateId = "Template";

    /// <summary>
    /// Represents the identifier used to denote the tag name attribute within the Aveva object template.
    /// </summary>
    private const string TagNameId = "TagName";

    /// <summary>
    /// Defines a regular expression used to separate object data segments by templates within a text structure.
    /// This separator identifies segments starting with the predefined template key and processes them in a case-insensitive, multiline context.
    /// </summary>
    private static readonly Regex TemplateSeparator = new(
        $"^{Regex.Escape(TemplateKey)}",
        RegexOptions.Multiline | RegexOptions.IgnoreCase);

    /// <summary>
    /// Reads a structured text input containing object data organized by templates and parses it into
    /// a collection of <see cref="ObjectData"/> instances.
    /// </summary>
    /// <param name="text">
    /// The text input to process. The input should contain data segregated into templates
    /// with a defined header row and object instance rows.
    /// </param>
    /// <returns>
    /// A collection of <see cref="ObjectData"/> representing the parsed object data. Each item
    /// in the collection corresponds to an individual object instance defined within the input text.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when the provided text is null, empty, or in an invalid format, such as missing
    /// required headers or properly formatted template sections.
    /// </exception>
    public IEnumerable<ObjectData> Read(string text)
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
                    [TemplateId] = new(TemplateId, template),
                    [TagNameId] = new(TagNameId, record[TagNameKey]?.ToString())
                };

                foreach (var item in record)
                {
                    if (StringComparer.OrdinalIgnoreCase.Equals(item.Key, TagNameKey))
                        continue;

                    var type = ParseHeaderType(item.Key);
                    var value = ParseValue(item.Value?.ToString(), type);
                    attributes[item.Key] = new AttributeData(item.Key, value);
                }

                yield return new ObjectData(attributes.Values);
            }
        }
    }

    /// <summary>
    /// Writes a collection of <see cref="ObjectData"/> instances to a CSV-formatted string.
    /// The output will group entries by their "Template" attribute, and the resulting CSV
    /// will be formatted to ensure proper system import.
    /// </summary>
    /// <param name="data">
    /// A collection of <see cref="ObjectData"/> instances that represent the objects to write
    /// to the CSV output.
    /// </param>
    /// <returns>
    /// A CSV formatted string representing the provided object data. The format ensures that the
    /// objects are grouped by template with appropriate headers and values.
    /// </returns>
    public string Write(IEnumerable<ObjectData> data)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture) { Mode = CsvMode.RFC4180 };
        using var writer = new StringWriter();
        using var csv = new CsvWriter(writer, config);

        // Need to group output by template for system to import correctly
        var groups = data.GroupBy(x => x[TemplateId]);

        foreach (var group in groups)
        {
            // First line for each group is the template key.
            writer.WriteLine($"{TemplateKey}{group.Key}");

            // Write the leading tag name key for each instance.
            csv.WriteField(TagNameKey);

            // Write remaining headers based on the first object.
            //todo we probably need to ensure header order matches the order we write the value...
            var header = group.First().Where(a => !IsIdentity(a)).Select(a => a.Name);
            group.First().Where(a => !IsIdentity(a)).Select(a => a.Name).ToList().ForEach(csv.WriteField);
            csv.NextRecord();

            // Write row for each instance in the template group.
            foreach (var instance in group)
            {
                //Explicitly write the tag name as the first attribute
                csv.WriteField(instance[TagNameId]);

                foreach (var attribute in instance.Where(a => !IsIdentity(a)))
                    csv.WriteField(attribute.ToString());

                csv.NextRecord();
            }

            writer.WriteLine();
        }

        return writer.ToString().TrimEnd();
    }

    /// <summary>
    /// Determines whether an attribute is considered an identity attribute,
    /// based on its name being "TagName" or "Template".
    /// </summary>
    /// <param name="attribute">
    /// The attribute to evaluate. The attribute is an instance of <see cref="AttributeData"/>
    /// containing a name and optional value.
    /// </param>
    /// <returns>
    /// A boolean value indicating whether the specified attribute is an identity attribute.
    /// Returns true if the attribute name is "TagName" or "Template"; otherwise, false.
    /// </returns>
    private static bool IsIdentity(AttributeData attribute) => attribute.Name is TagNameId or TemplateId;

    /// <summary>
    /// Parses the given text representation of a value into an object of the specified type.
    /// This method attempts to convert the text into a corresponding value of the expected
    /// type, such as a boolean, integer, floating-point number, or string.
    /// </summary>
    private static object? ParseValue(string? text, Type type)
    {
        return text switch
        {
            null => null,
            _ when type == typeof(bool) => bool.Parse(text),
            _ when type == typeof(int) => int.Parse(text),
            _ when type == typeof(long) => long.Parse(text),
            _ when type == typeof(float) => float.Parse(text),
            _ when type == typeof(double) => double.Parse(text),
            _ => text
        };
    }

    /// <summary>
    /// Parses the type information from a header string and maps it to a corresponding .NET <see cref="Type"/>.
    /// </summary>
    /// <param name="header">
    /// The header string containing type information. The type detail is typically enclosed in parentheses,
    /// such as "(MxBoolean)", "(MxInteger)", or other recognized formats.
    /// </param>
    /// <returns>
    /// A <see cref="Type"/> corresponding to the extracted type information. If the type is not explicitly
    /// defined or recognized, the method defaults to <see cref="string"/>.
    /// </returns>
    private static Type ParseHeaderType(string header)
    {
        var typeStart = header.IndexOf('(') + 1;
        var typeName = typeStart > 0 ? header[typeStart..].TrimEnd(')') : string.Empty;

        return typeName switch
        {
            "MxBoolean" => typeof(bool),
            "MxInteger" => typeof(int),
            "MxFloat" => typeof(float),
            "MxDouble" => typeof(double),
            _ => typeof(string)
        };
    }
}