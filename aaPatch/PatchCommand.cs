using System.Text.RegularExpressions;
using CliFx;
using CliFx.Binding;
using CliFx.Infrastructure;

namespace aaPatch;

/// <summary>
/// Command-line tool for patching Galaxy dump CSV files by modifying object attributes based on filters and patch operations.
/// Supports filtering objects by template and tag name and applying attribute modifications through direct assignment or find-replace operations.
/// </summary>
[Command(Description =
    "Patches Galaxy dump CSV files by modifying object attributes based on specified filters and operations.")]
public partial class PatchCommand : ICommand
{
    private const string AdditionInfoMessage = "Use command 'aaPatch info' for format rules.";

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
    [CommandOption("patch", 'p', Description = $"Patch expression to apply. {AdditionInfoMessage}")]
    public IReadOnlyList<string> Patches { get; set; } = [];

    /// <summary>
    /// Gets the tag name filter pattern used to select which objects to patch.
    /// Supports wildcard patterns. If not specified, all tag names are matched.
    /// </summary>
    [CommandOption("filter", 'f', Description = $"Filter expression to filter objects. {AdditionInfoMessage}")]
    public string? Filter { get; set; }

    /// <summary>
    /// Gets the template name filter pattern used to select which objects to patch.
    /// Supports wildcard patterns (e.g., $Pump*). If not specified, all templates are matched.
    /// </summary>
    [CommandOption("templates", 't', Description = $"Templates to apply patch to. {AdditionInfoMessage}")]
    public IReadOnlyList<string> Templates { get; set; } = [];

    /// <summary>
    /// Gets or sets a value indicating whether to preview the changes without applying them.
    /// If set to true, the command simulates the modifications and displays the potential outcome.
    /// </summary>
    [CommandOption("preview", Description = "Preview changes without applying them.")]
    public bool Preview { get; set; }

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

            var patches = GalaxyDump.Read(csv).Select(GeneratePatch).ToList();

            if (Preview)
            {
                // Preview all changes by printing to console and return without applying.
                var diffs = patches.SelectMany(x => x.Diffs()).ToList();
                diffs.ForEach(d => console.Error.WriteLine(d));
                return;
            }

            // If we get here, then we need to apply the patches that were generated.
            patches.ForEach(p => p.ApplyPatches());

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
        if (Templates.Count > 0 && Templates.All(t => !MatchesFilter(target.Template, t)))
            return target;

        foreach (var patch in Patches)
        {
            if (patch.StartsWith(':') && patch.Contains('='))
            {
                // Global Find and Replace Mode -> ":Find=Replace"
                var parts = patch[1..].Split('=', 2);

                if (parts.Length != 2)
                    throw new CommandException("Invalid global find-replace format. Expected ':Find=Replace'.");

                target.Replace(parts[0], parts[1]);
            }
            else if (patch.Contains(':') && patch.IndexOf(':') < patch.IndexOf('='))
            {
                // Attribute-specific Find and Replace Mode -> "Attribute:Find=Replace"
                var parts = patch.Split([':', '='], 3);

                if (parts.Length != 3)
                    throw new CommandException("Invalid find-replace patch format. Expected 'Attribute:Find=Replace'.");

                target.Replace(parts[1], parts[2], parts[0]);
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