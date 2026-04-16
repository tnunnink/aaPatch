using System.Text.RegularExpressions;
using CliFx;
using CliFx.Binding;
using CliFx.Infrastructure;

namespace aaPatch;

/// <summary>
/// Command-line tool for patching Galaxy dump CSV files by modifying object attributes based on filters and patch operations.
/// Supports filtering objects by template and tag name, and applying attribute modifications through direct assignment or find-replace operations.
/// </summary>
[Command(Description = "Patches Galaxy dump CSV files by modifying object attributes based on specified filters and operations.")]
public partial class PatchCommand : ICommand
{
    /// <summary>
    /// Gets or sets the path to the input CSV file containing Galaxy dump data.
    /// If not specified, input is read from standard input (stdin).
    /// </summary>
    [CommandOption("input", 'i', Description = "Path to the input CSV file. If not specified, reads from stdin.")]
    public string? InputFile { get; set; }

    /// <summary>
    /// Gets or sets the path to the output CSV file where patched Galaxy dump data will be written.
    /// If not specified, the output is written to standard output (stdout).
    /// </summary>
    [CommandOption("output", 'o', Description = "Path to the output CSV file. If not specified, writes to stdout.")]
    public string? OutputFile { get; set; }

    /// <summary>
    /// Gets the collection of patches to apply to matching objects.
    /// Supports two formats: 'Attribute=Value' for direct assignment, or 'Attribute:Find=Replace' for find-replace operations.
    /// </summary>
    [CommandOption("attribute", 'a', Description = "Patch to apply in 'Attribute=Value' format or 'Attribute:Find=Replace' format for find-replace operations.")]
    public IReadOnlyList<string> Patches { get; init; } = [];

    /// <summary>
    /// Gets the template name filter pattern used to select which objects to patch.
    /// Supports wildcard patterns (e.g., $Pump*). If not specified, all templates are matched.
    /// </summary>
    [CommandOption("template", Description = "Template filter (supports wildcards, e.g. $Pump*)")]
    public string? TemplateFilter { get; init; }

    /// <summary>
    /// Gets the tag name filter pattern used to select which objects to patch.
    /// Supports wildcard patterns. If not specified, all tag names are matched.
    /// </summary>
    [CommandOption("tag", Description = "Tag name filter (supports wildcards)")]
    public string? TagFilter { get; init; }

    /// <summary>
    /// Executes the patch command by reading Galaxy dump data, applying filters and patches, and writing the modified output.
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

            var objects = GalaxyDump.Read(csv)
                .Where(x => MatchesFilter(x.Template, TemplateFilter) && MatchesFilter(x.TagName, TagFilter))
                .Select(ApplyPatches)
                .ToList();

            var write = OutputFile is null
                ? console.Output.WriteAsync(GalaxyDump.Write(objects))
                : File.WriteAllTextAsync(OutputFile, GalaxyDump.Write(objects), cancellation);

            await write;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    /// <summary>
    /// Applies a series of patching operations to the provided ObjectData instance based on the specified attribute-value or attribute-find-replace definitions.
    /// </summary>
    /// <param name="target">The object to which the patches will be applied.</param>
    /// <returns>The updated ObjectData instance with all patches applied.</returns>
    /// <exception cref="CommandException">Thrown if a patch string is invalid or improperly formatted.</exception>
    private ObjectData ApplyPatches(ObjectData target)
    {
        foreach (var patch in Patches)
        {
            if (patch.Contains(':') && patch.IndexOf(':') < patch.IndexOf('='))
            {
                // Find and Replace Mode -> "Attribute:Find=Replace"
                var parts = patch.Split([':', '='], 3);

                if (parts.Length != 3)
                    throw new CommandException("Invalid find-replace patch format. Expected 'Attribute:Find=Replace'.");

                target.Patch(parts[0], parts[1], parts[2]);
            }
            else
            {
                // Direct Assignment Mode -> "Attribute=Value"
                var parts = patch.Split('=', 2);

                if (parts.Length != 2)
                    throw new CommandException("Invalid patch format. Expected 'Attribute=Value'.");

                target.Patch(parts[0], parts[1]);
            }
        }

        return target;
    }

    /// <summary>
    /// Determines if a given value matches a specified pattern.
    /// Supports patterns with wildcards (e.g., '*') by converting them into regular expressions.
    /// </summary>
    /// <param name="value">The value to evaluate against the pattern.</param>
    /// <param name="pattern">The pattern to match the value against. Can include wildcards ('*'). Null or empty patterns count as a match for all values.</param>
    /// <returns>True if the value matches the pattern or if the pattern is null/empty; otherwise, false.</returns>
    private static bool MatchesFilter(string value, string? pattern)
    {
        if (string.IsNullOrEmpty(pattern))
            return true;

        var regex = $"^{Regex.Escape(pattern).Replace("\\*", ".*")}$";

        return Regex.IsMatch(value, regex, RegexOptions.IgnoreCase);
    }
}