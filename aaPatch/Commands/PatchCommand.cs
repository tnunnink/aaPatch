using aaPatch.Model;
using CliFx;
using CliFx.Binding;
using CliFx.Infrastructure;

namespace aaPatch.Commands;

/// <summary>
/// Command-line tool for patching Galaxy dump CSV files by modifying object attributes based on filters and patch operations.
/// Supports filtering objects by template and tag name and applying attribute modifications through direct assignment or find-replace operations.
/// </summary>
[Command(Description = "Patches Galaxy CSV exports by modifying object attributes based provided criteria.")]
public partial class PatchCommand : ICommand
{
    private const string AdditionInfoMessage = "Use command 'aaPatch info' for format rules.";

    /// <summary>
    /// Gets or sets the path to the input file containing Galaxy dump CSV.
    /// If not specified, input is read from standard input (stdin).
    /// </summary>
    [CommandOption("input", 'i', Description = "Path to the input file. If not specified, reads from stdin.")]
    public string? InputFile { get; set; }

    /// <summary>
    /// Gets or sets the path to the output file where patched Galaxy dump data will be written.
    /// If not specified, the output is written to standard output (stdout).
    /// </summary>
    [CommandOption("output", 'o', Description = "Path to the output file. If not specified, writes to stdout.")]
    public string? OutputFile { get; set; }

    /// <summary>
    /// Gets the filter pattern used to select which objects are included in the output.
    /// Supports wildcard patterns. If not specified, all objects are matched.
    /// </summary>
    [CommandOption("filter", 'f', Description = $"Filter expressions to filter objects. {AdditionInfoMessage}")]
    public IReadOnlyCollection<ObjectFilter> Filters { get; set; } = [];

    /// <summary>
    /// Gets the collection of patches to apply to matching objects.
    /// Supports two formats: 'Attribute=Value' for direct assignment, or 'Attribute:Find=Replace' for find-replace operations.
    /// </summary>
    [CommandOption("patch", 'p', Description = $"Patch expression to apply. {AdditionInfoMessage}")]
    public IReadOnlyList<ObjectPatch> Patches { get; set; } = [];

    /// <summary>
    /// Gets the collection of attribute names to include in the output.
    /// When specified, only the selected attributes will be included in the output objects.
    /// If not specified, all attributes are included.
    /// </summary>
    [CommandOption("select", 's', Description = $"Specify attributes to include in output. {AdditionInfoMessage}")]
    public IReadOnlyList<FieldSelection> Selections { get; set; } = [];

    /// <summary>
    /// Gets the collection of static attributes to add to each output object.
    /// Format: 'Attribute=Value'.
    /// </summary>
    [CommandOption("add", 'a', Description = $"Attributes to add to each output object. {AdditionInfoMessage}")]
    public IReadOnlyList<AttributeData> Additions { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether to perform case-sensitive matching for find-replace operations.
    /// Default is false (case-insensitive).
    /// </summary>
    [CommandOption("match-case", 'm', Description = "Perform case-sensitive matching for find-replace operations.")]
    public bool MatchCase { get; set; }

    /// <summary>
    /// Gets or sets the output format for the patched Galaxy CSV export.
    /// Supported options are "aveva" for AVEVA format and "json" for JSON format.
    /// Defaults to "aveva" if not specified.
    /// </summary>
    [CommandOption("format", Description = "Output format: aveva or json.")]
    public string Format { get; set; } = "aveva";

    /// <summary>
    /// Executes the patch command by reading Galaxy dump data, filtering the output objects, applying patches, and writing the result.
    /// </summary>
    /// <param name="console">The console interface for input/output operations and cancellation handling.</param>
    /// <returns>A ValueTask representing the asynchronous operation.</returns>
    public async ValueTask ExecuteAsync(IConsole console)
    {
        var cancellation = console.RegisterCancellationHandler();

        try
        {
            var csv = InputFile is null
                ? await console.Input.ReadToEndAsync()
                : await File.ReadAllTextAsync(InputFile, cancellation);

            var objects = GalaxyDump.Read(csv).ToList();

            var output = objects
                .Where(x => x.Match([.. Filters])) // Filter objects
                .Select(x => x.Apply(Patches, MatchCase)) // Patch objects
                .Select(x => x.Add([.. Additions])) // Add fields
                .Select(x => Selections.Count > 0 ? x.Project([.. Selections]) : x) // Select fields
                .ToList();

            var content = output.Serialize(Format);

            var write = OutputFile is null
                ? console.Output.WriteAsync(content)
                : File.WriteAllTextAsync(OutputFile, content, cancellation);

            await write;
        }
        catch (Exception e) when (e is not CommandException)
        {
            throw new CommandException($"Patch failed with error '{e.Message}'", innerException: e);
        }
    }
}