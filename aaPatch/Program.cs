using CliFx;
using CliFx.Infrastructure;

namespace aaPatch;

public static class Program
{
    public static async Task<int> Main()
    {
        return await new CommandLineApplicationBuilder()
            .SetTitle("aaPatch")
            .SetDescription("")
            .SetExecutableName("aapatch")
            .UseConsole(new SystemConsole())
            .AddCommandsFromThisAssembly()
            .Build()
            .RunAsync();
    }
}