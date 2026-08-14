namespace aaPatch.Model;

public sealed record SchemaFilter
{
    public SchemaFilter(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Schema filter expression cannot be null, empty, or whitespace.",
                nameof(input));

        Attribute = input;
    }

    public string Attribute { get; }
}