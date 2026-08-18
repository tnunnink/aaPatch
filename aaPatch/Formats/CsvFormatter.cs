using aaPatch.Model;

namespace aaPatch.Formats;

public class CsvFormatter : IObjectFormater
{
    public IEnumerable<ObjectData> Read(string text)
    {
        throw new NotImplementedException();
    }

    public string Write(IEnumerable<ObjectData> data)
    {
        throw new NotImplementedException();
    }
}