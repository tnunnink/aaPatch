namespace aaPatch.Commands;

/// <summary>
/// Centralized command and option descriptions used across CLI commands.
/// </summary>
public static class CommandDescriptions
{
    // @formatter:off
    private const string AdditionalInfoMessage = "Use command 'aapatch info' for format rules.";
    public const string PatchCommand = "Patches Galaxy CSV exports by modifying object attributes based on provided criteria.";
    public const string InputFile = "Path to the input file. If not specified, reads from stdin.";
    public const string OutputFile = "Path to the output file. If not specified, writes to stdout.";
    public const string Where = $"Filter objects having specified values. {AdditionalInfoMessage}";
    public const string Patch = $"Patch expression to apply. {AdditionalInfoMessage}";
    public const string Select = $"Attributes to include in output objects. {AdditionalInfoMessage}";
    public const string Has = $"Filter objects containing specific attributes. {AdditionalInfoMessage}";
    public const string Format = "Output format: 'aveva', 'csv', or 'json'. (default: 'aveva')";
    public const string Prune = "Only include changed attributes and modified objects in the output.";
    public const string Distinct = "Collapse duplicate objects across all attributes, or by specified attributes when provided.";
    // @formatter:on
}