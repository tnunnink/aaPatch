using aaPatch.Model;

namespace aaPatch.Formats;

/// <summary>
/// Defines the contract for formatting object data. This interface provides methods
/// to read and write formatted object data, converting between data strings and
/// collections of object representations.
/// </summary>
public interface IObjectFormater
{
    /// <summary>
    /// Reads the provided data input and returns a collection of formatted object data.
    /// </summary>
    /// <param name="text">The input data to be formatted and converted into object data.</param>
    /// <returns>A collection of <see cref="ObjectData"/> instances derived from the input data.</returns>
    public IEnumerable<ObjectData> Read(string text);

    /// <summary>
    /// Converts a collection of object data into a formatted string representation.
    /// </summary>
    /// <param name="data">A collection of <see cref="ObjectData"/> instances to be formatted and converted into a string representation.</param>
    /// <returns>A formatted string representation of the provided collection of object data.</returns>
    public string Write(IEnumerable<ObjectData> data);
}