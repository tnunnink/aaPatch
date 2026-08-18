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
public class FormatRouter(Format format) : IObjectFormater
{
    private static readonly AvevaFormatter Aveva = new();
    private static readonly JsonFormatter Json = new();
    private static readonly CsvFormatter Csv = new();

    /// <inheritdoc />
    public IEnumerable<ObjectData> Read(string text)
    {
        return format switch
        {
            Format.Aveva => Aveva.Read(text),
            Format.Json => Json.Read(text),
            Format.Csv => Csv.Read(text),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }

    /// <inheritdoc />
    public string Write(IEnumerable<ObjectData> data)
    {
        return format switch
        {
            Format.Aveva => Aveva.Write(data),
            Format.Json => Json.Write(data),
            Format.Csv => Csv.Write(data),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, null)
        };
    }
}