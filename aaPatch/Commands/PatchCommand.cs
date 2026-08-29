using aaPatch.Extensions;
using aaPatch.Formats;
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
    [CommandOption("where", 'w', Description = $"Filter objects having specified values. {AdditionInfoMessage}")]
    public ObjectExpression? Filter { get; set; }

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
    [CommandOption("select", 's', Description = $"Attributes to include in output objects. {AdditionInfoMessage}")]
    public IReadOnlyList<ObjectProjection> Selections { get; set; } = [];

    /// <summary>
    /// Gets or sets a collection of attribute-based filters applied to objects in the Galaxy CSV.
    /// Filters specify criteria for selecting objects that contain specific attributes.
    /// </summary>
    [CommandOption("has", 'c', Description = $"Filter objects containing specific attributes. {AdditionInfoMessage}")]
    public IReadOnlyList<string> Attributes { get; set; } = [];

    /// <summary>
    /// Gets or sets the output format for the patched Galaxy CSV export.
    /// Supported options are "aveva" for AVEVA format and "json" for JSON format.
    /// Defaults to "aveva" if not specified.
    /// </summary>
    [CommandOption("format", 'f', Description = "Output format")]
    public Format Format { get; set; } = Format.Aveva;

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
            var input = InputFile is null
                ? await console.Input.ReadToEndAsync()
                : await File.ReadAllTextAsync(InputFile, cancellation);

            var formatter = new FormatRouter();
            var objects = formatter.Read(input).ToList();

            var data = objects
                .Filter(Filter)
                .Having(Attributes)
                .Patch(Patches)
                .Project(Selections)
                .ToList();

            var content = formatter.Write(data, Format);

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