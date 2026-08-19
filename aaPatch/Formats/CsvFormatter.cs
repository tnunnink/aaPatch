using System.Globalization;
using aaPatch.Model;
using CsvHelper;
using CsvHelper.Configuration;

namespace aaPatch.Formats;

/// <summary>
/// Provides functionality for formatting data as CSV strings and parsing
/// CSV-formatted strings into objects. Implements the <see cref="IObjectFormater"/> interface.
/// </summary>
public class CsvFormatter : IObjectFormater
{
    /// <summary>
    /// Reads data from a CSV-formatted string and converts it into a collection of <see cref="ObjectData"/> instances.
    /// </summary>
    /// <param name="text">The CSV-formatted string containing data to be read and processed.</param>
    /// <returns>An enumerable collection of <see cref="ObjectData"/> instances representing the parsed data.</returns>
    /// <exception cref="ArgumentException">Thrown if duplicate keys are found in any CSV record.</exception>
    public IEnumerable<ObjectData> Read(string text)
    {
        using var reader = new StringReader(text);
        using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Mode = CsvMode.RFC4180,
            TrimOptions = TrimOptions.Trim
        });

        var records = csv.GetRecords<dynamic>().Cast<IDictionary<string, object?>>().ToArray();

        foreach (var record in records)
        {
            var attributes = new Dictionary<string, AttributeData>();

            foreach (var item in record)
                if (!attributes.TryAdd(item.Key, new AttributeData(item.Key, item.Value)))
                    throw new ArgumentException($"Duplicate key '{item.Key}' found in CSV record.");

            yield return new ObjectData(attributes.Values);
        }
    }

    /// <summary>
    /// Converts a collection of <see cref="ObjectData"/> instances into a CSV-formatted string.
    /// </summary>
    /// <param name="data">The collection of <see cref="ObjectData"/> instances to be written into a CSV format.</param>
    /// <returns>A CSV-formatted string representing the provided collection of <see cref="ObjectData"/> instances.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the provided collection of <see cref="ObjectData"/> instances is empty.</exception>
    public string Write(IEnumerable<ObjectData> data)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture) { Mode = CsvMode.RFC4180 };
        using var writer = new StringWriter();
        using var csv = new CsvWriter(writer, config);

        var collection = data.ToArray();

        if (collection.Length == 0)
            throw new InvalidOperationException("Cannot write CSV data: the collection is empty.");

        var headers = collection[0].Select(a => a.Name).ToList();
        headers.ForEach(csv.WriteField);
        csv.NextRecord();

        foreach (var record in collection)
        {
            foreach (var attribute in record)
                csv.WriteField(attribute.ToString());

            csv.NextRecord();
        }

        return writer.ToString().TrimEnd();
    }
}