using System.Text.RegularExpressions;
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
    [CommandOption("filter", 'f', Description = $"Filter expression to filter objects. {AdditionInfoMessage}")]
    public string? Filter { get; set; }

    /// <summary>
    /// Gets the collection of patches to apply to matching objects.
    /// Supports two formats: 'Attribute=Value' for direct assignment, or 'Attribute:Find=Replace' for find-replace operations.
    /// </summary>
    [CommandOption("patch", 'p', Description = $"Patch expression to apply. {AdditionInfoMessage}")]
    public IReadOnlyList<string> Patches { get; set; } = [];
    
    /// <summary>
    /// Gets the collection of attribute names to include in the output.
    /// When specified, only the selected attributes will be included in the output objects.
    /// If not specified, all attributes are included.
    /// </summary>
    [CommandOption("select", 's', Description = $"Selection expression to specify attributes to include in output. {AdditionInfoMessage}")]
    public IReadOnlyList<string> Select { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether to perform case-sensitive matching for find-replace operations.
    /// Default is false (case-insensitive).
    /// </summary>
    [CommandOption("match-case", 'm', Description = "Perform case-sensitive matching for find-replace operations.")]
    public bool MatchCase { get; set; }

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

            var patches = objects
                .Where(x => x.Matches(Filter))
                .Select(GeneratePatch)
                .ToList();

            var write = OutputFile is null
                ? console.Output.WriteAsync(GalaxyDump.Write(patches))
                : File.WriteAllTextAsync(OutputFile, GalaxyDump.Write(patches), cancellation);

            await write;
        }
        catch (Exception e)
        {
            throw new CommandException($"Patch failed with error '{e.Message}'", innerException: e);
        }
    }

    /// <summary>
    /// Applies the specified patches to the target object by modifying its attributes based on direct assignments or find-replace operations.
    /// </summary>
    /// <param name="target">The target object to which the patches will be applied.</param>
    /// <returns>The modified target object with the patches applied.</returns>
    /// <exception cref="CommandException">Thrown when an invalid patch format is encountered.</exception>
    private ObjectData GeneratePatch(ObjectData target)
    {
        foreach (var patch in Patches)
        {
            if (patch.StartsWith(':') && patch.Contains('='))
            {
                // Global Find and Replace Mode -> ":Find=Replace"
                var parts = patch[1..].Split('=', 2);

                if (parts.Length != 2)
                    throw new CommandException("Invalid global find-replace format. Expected ':Find=Replace'.");

                target.Replace(parts[0], parts[1], matchCase: MatchCase);
            }
            else if (patch.Contains(':') && patch.IndexOf(':') < patch.IndexOf('='))
            {
                // Attribute-specific Find and Replace Mode -> "Attribute:Find=Replace"
                var parts = patch.Split([':', '='], 3);

                if (parts.Length != 3)
                    throw new CommandException("Invalid find-replace patch format. Expected 'Attribute:Find=Replace'.");

                target.Replace(parts[1], parts[2], parts[0], matchCase: MatchCase);
            }
            else
            {
                // Direct Assignment Mode -> "Attribute=Value"
                var parts = patch.Split('=', 2);

                if (parts.Length != 2)
                    throw new CommandException("Invalid patch format. Expected 'Attribute=Value'.");

                target.Update(parts[0], parts[1]);
            }
        }

        return target;
    }
}