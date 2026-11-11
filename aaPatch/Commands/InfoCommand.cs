using System.Diagnostics;
using System.Runtime.InteropServices;
using CliFx;
using CliFx.Binding;
using CliFx.Infrastructure;

namespace aaPatch.Commands;

[Command("info", Description = "Opens the aaPatch documentation in the default browser.")]
public partial class InfoCommand : ICommand
{
    private const string DocumentationUrl = "https://github.com/tnunnink/aaPatch#readme";

    /// <inheritdoc />
    public ValueTask ExecuteAsync(IConsole console)
    {
        console.Output.WriteLine($"Opening documentation at {DocumentationUrl}...");

        try
        {
            OpenUrl(DocumentationUrl);
        }
        catch
        {
            // Fall back gracefully if no browser or display is available
            console.Output.WriteLine($"Unable to open browser. Please visit: {DocumentationUrl}");
        }

        return default;
    }

    /// <summary>
    /// Opens the specified URL in the system default browser or URL handler.
    /// </summary>
    /// <param name="url">The URL to open.</param>
    private static void OpenUrl(string url)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("xdg-open", url);
            return;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start("open", url);
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }
}