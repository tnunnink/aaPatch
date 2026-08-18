namespace aaPatch.Formats;

/// <summary>
/// Specifies the available data serialization formats for reading or writing operations
/// in the context of object data processing.
/// </summary>
public enum Format
{
    /// <summary>
    /// Represents the Aveva format for data serialization.
    /// This format is specifically used for integration or compatibility
    /// with Aveva system data and workflows, typically rendering data in a manner
    /// suitable for use with Aveva applications.
    /// </summary>
    Aveva,

    /// <summary>
    /// Represents the CSV (Comma-Separated Values) format for data serialization.
    /// It is used to read or write object data in a text-based format where values are separated by commas
    /// and records are represented as rows. Commonly used for tabular data exchange.
    /// </summary>
    Csv,

    /// <summary>
    /// Represents the JSON (JavaScript Object Notation) format for data serialization.
    /// JSON is a lightweight data-interchange format that uses a text-based structure
    /// to represent objects as key-value pairs and arrays. It is commonly used for
    /// structured data exchange, particularly in web applications and APIs.
    /// </summary>
    Json
}