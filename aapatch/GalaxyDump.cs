using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace aaPatch;

/// <summary>
/// Provides methods to read and write object data stored in files using a template-based structure.
/// </summary>
public static class GalaxyDump
{
    /// <summary>
    /// Represents a constant key used to identify the template attribute in the object data.
    /// This key is used internally for accessing or verifying the template string associated with the object.
    /// </summary>
    private const string TemplateKey = ":Template=";

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
            throw new ArgumentException("");

        var templates = text
            .Split(string.Concat(Environment.NewLine, Environment.NewLine), StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .ToArray();

        return templates.SelectMany(ReadTemplate);

        IEnumerable<ObjectData> ReadTemplate(string segment)
        {
            // Skip any segment that does not have our template key at the start.
            if (!segment.StartsWith(TemplateKey, StringComparison.OrdinalIgnoreCase))
                return [];

            // We know that each segment needs at least 3 lines (template identifier, attribute header, and instance(s) row)
            var lines = segment.Split(Environment.NewLine);

            switch (lines.Length)
            {
                case < 2:
                    throw new ArgumentException(
                        """
                        Invalid template format: Missing column header row.
                        Expected format is ':TEMPLATE=<name>' followed by a header row with column names.
                        """
                    );
                case < 3:
                    throw new ArgumentException(
                        """
                        Invalid template format: No object instance data found.
                        Expected at least one data row following the ':TEMPLATE=<name>' and column header rows.
                        """
                    );
            }

            // Read the template name for this set of object instances and recombine
            // all the records to single string that CsvHelper can easily parse for us.
            var template = lines[0].Replace(TemplateKey, string.Empty);
            var instances = string.Join(Environment.NewLine, lines[1..]);

            using var sr = new StringReader(instances);
            using var csv = new CsvReader(sr, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Mode = CsvMode.RFC4180,
                TrimOptions = TrimOptions.Trim
            });

            // Read all attributes but cast results to dictionary of string values for readability
            var attributes = csv.GetRecords<dynamic>().Select(r =>
                ((IDictionary<string, object?>)r).ToDictionary(x => x.Key, x => x.Value?.ToString())
            );

            return attributes.Select(a => new ObjectData(template, a)).ToArray();
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
        var groups = data.GroupBy(x => x.Template);
        var segmens = new List<string>();

        foreach (var group in groups)
        {
            var template = $":TEMPLATE={group.Key}";
            var header = string.Join(",", group.First().Attributes);
            var instances = string.Join(Environment.NewLine, group.Select(x => string.Join(",", x.Values)).ToArray());
            segmens.Add($"{template}{Environment.NewLine}{header}{Environment.NewLine}{instances}");
        }

        return string.Join(Environment.NewLine, segmens).Trim();
    }
}