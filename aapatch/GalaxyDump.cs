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
    /// Reads and parses object data from a file at the specified path. The file should contain data organized in templates,
    /// where each template consists of a template identifier, an attribute header line, and one or more data instance rows.
    /// </summary>
    /// <param name="filePath">The path of the file to be read. Cannot be null, empty, or consist only of whitespace.</param>
    /// <returns>An enumerable collection of <see cref="ObjectData"/> objects, each representing an instance of data from the file.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="filePath"/> is null, empty, or contains whitespace, or when the file content has an invalid template format.
    /// The expected format includes a template identifier line, an attribute header line, and at least one data instance row.
    /// </exception>
    /// <exception cref="IOException">Thrown when there is an error reading from the specified file.</exception>
    public static IEnumerable<ObjectData> Read(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or whitespace.", nameof(filePath));

        var text = File.ReadAllText(filePath);

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

            using var reader = new StringReader(instances);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
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
    /// Writes the provided collection of object data to a file at the specified path. The data is grouped by template,
    /// and each group is written as a segment consisting of a template line, a header line (representing attribute names),
    /// and multiple instance lines (representing attribute values).
    /// </summary>
    /// <param name="filePath">The path of the file where the object data will be written. Cannot be null or empty.</param>
    /// <param name="data">The collection of object data to write to the file. Cannot be null.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="filePath"/> or <paramref name="data"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is an empty string.</exception>
    /// <exception cref="IOException">Thrown when there is an error writing to the file.</exception>
    public static void Write(string filePath, IEnumerable<ObjectData> data)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("File path cannot be null or whitespace.", nameof(filePath));

        var groups = data.GroupBy(x => x.Template);
        var segmens = new List<string>();

        foreach (var group in groups)
        {
            var template = $":TEMPLATE={group.Key}";
            var header = string.Join(",", group.First().Attributes);
            var instances = string.Join(Environment.NewLine, group.Select(x => string.Join(",", x.Values)).ToArray());
            segmens.Add($"{template}{Environment.NewLine}{header}{Environment.NewLine}{instances}");
        }

        var text = string.Join(Environment.NewLine, segmens).Trim();
        File.WriteAllText(filePath, text);
    }
}