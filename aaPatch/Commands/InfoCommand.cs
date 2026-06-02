using CliFx;
using CliFx.Binding;
using CliFx.Infrastructure;

namespace aaPatch.Commands;

[Command("info", Description = "Displays information on how to use the patch syntax.")]
public partial class InfoCommand : ICommand
{
    public ValueTask ExecuteAsync(IConsole console)
    {
        const string infoText =
            """
            aaPatch Syntax Information
            ==========================

            The patch command supports three primary modes of operation:

            1. Direct Assignment
               Format: Attribute=Value
               Example: -p "Description=New Description"
               Sets the specified attribute to the given value.

            2. Attribute-Specific Find and Replace
               Format: Attribute:Find=Replace
               Example: -p "TagName:Old=New"
               Searches for 'Old' within the 'TagName' attribute and replaces it with 'New'.

            3. Global Find and Replace
               Format: :Find=Replace
               Example: -p ":Area1=Area2"
               Searches for 'Area1' across ALL attributes and replaces it with 'Area2'.

            Filters and Templates:
            ----------------------
            Use -f or --filter to select specific objects.
            Format: [Attribute=]Pattern
            Example: -f "Pump*" (Matches TagName starting with Pump)
            Example: -f "Area=Area1" (Matches objects where Area is Area1)

            Use -t or --templates to limit patches to specific templates (e.g., -t "$UserDefined").
            """;

        console.Output.WriteLine(infoText);
        return default;
    }
}
