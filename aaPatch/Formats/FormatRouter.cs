using System.Text.RegularExpressions;
using aaPatch.Model;

namespace aaPatch.Formats;

/// <summary>
/// The FormatRouter class provides functionality for reading and writing data
/// in different formats by routing the operations to the appropriate format handler.
/// It implements the IObjectFormater interface.
/// </summary>
/// <remarks>
/// Supported formats include AVEVA, CSV, and JSON.
/// </remarks>
public class FormatRouter : IObjectFormater
{
    private static readonly AvevaFormatter Aveva = new();
    private static readonly JsonFormatter Json = new();
    private static readonly CsvFormatter Csv = new();

    /// <inheritdoc />
    public IEnumerable<ObjectData> Read(string text)
    {
        var detected = DetectFormat(text);

        return detected switch
        {
            Format.Aveva => Aveva.Read(text),
            Format.Json => Json.Read(text),
            Format.Csv => Csv.Read(text),
            _ => throw new ArgumentOutOfRangeException(nameof(text), detected, null)
        };
    }

    /// <summary>
    /// Writes the provided object data to a specific format.
    /// </summary>
    /// <param name="data">The collection of object data to be written.</param>
    /// <param name="format">The target format in which to write the data (e.g., AVEVA, JSON, CSV).</param>
    /// <returns>A string representation of the data in the specified format.</returns>
    public string Write(IEnumerable<ObjectData> data, Format format)
    {
        return format switch
        {
            Format.Aveva => Aveva.Write(data),
            Format.Json => Json.Write(data),
            Format.Csv => Csv.Write(data),
            _ => ((IObjectFormater)this).Write(data)
        };
    }

    /// <inheritdoc />
    string IObjectFormater.Write(IEnumerable<ObjectData> data)
    {
        return Aveva.Write(data);
    }

    /// <summary>
    /// Defines a regular expression used to separate object data segments by templates within a text structure.
    /// This separator identifies segments starting with the predefined template key and processes them in a case-insensitive, multiline context.
    /// </summary>
    private static readonly Regex TemplateMatch = new($"^{Regex.Escape(":TEMPLATE=")}",
        RegexOptions.Multiline | RegexOptions.IgnoreCase);

    /// <summary>
    /// Detects the format of the provided text content.
    /// </summary>
    /// <param name="text">The input text whose format needs to be determined.</param>
    /// <returns>The detected <see cref="Format"/> of the input text.</returns>
    private static Format DetectFormat(string text)
    {
        var trimmed = text.TrimStart();

        if (string.IsNullOrEmpty(trimmed))
            throw new ArgumentException("Input text cannot be null or empty.", nameof(text));

        if (trimmed.StartsWith('[')) return Format.Json;
        if (TemplateMatch.IsMatch(trimmed)) return Format.Aveva;
        return Format.Csv;
    }
}