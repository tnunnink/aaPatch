namespace aaPatch.Tests;

using CliFx.Infrastructure;

[TestFixture]
public class PatchCommandTests
{
    private const string SimpleGalaxyDump =
        """
        :TEMPLATE=$Pump
        :Tagname,Description,HiHi
        P_101,Centrifugal Pump,100.0

        :TEMPLATE=$Valve
        :Tagname,Description,OpenLimit
        V_201,Gate Valve,True
        """;

    [Test]
    public async Task ExecuteAsync_SimplePatch_HasVerifiedOutput()
    {
        using var console = new FakeInMemoryConsole();
        console.WriteInput(SimpleGalaxyDump);

        var command = new PatchCommand
        {
            Patches = ["Description=Updated Pump"],
            TemplateFilter = "$Pump"
        };

        await command.ExecuteAsync(console);

        await Verify(console.ReadOutputString());
    }

    [Test]
    public async Task ExecuteAsync_FindReplacePatch_HasVerifiedOutput()
    {
        using var console = new FakeInMemoryConsole();
        console.WriteInput(SimpleGalaxyDump);

        var command = new PatchCommand
        {
            Patches = ["Description:Pump=Motor"]
        };

        await command.ExecuteAsync(console);

        await Verify(console.ReadOutputString());
    }

    [Test]
    public async Task ExecuteAsync_InputFileSpecified_HasVerifiedOutput()
    {
        using var console = new FakeInMemoryConsole();
        var inputFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files", "SimpleGalaxyDump.csv");

        var command = new PatchCommand
        {
            InputFile = inputFile,
            Patches = ["Description=File Updated"],
            TemplateFilter = "$Pump"
        };

        await command.ExecuteAsync(console);

        await Verify(console.ReadOutputString());
    }

    [Test]
    public async Task ExecuteAsync_OutputFileSpecified_HasVerifiedOutput()
    {
        using var console = new FakeInMemoryConsole();
        console.WriteInput(SimpleGalaxyDump);
        var outputFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.csv");

        try
        {
            var command = new PatchCommand
            {
                OutputFile = outputFile,
                Patches = ["Description=Output to File"]
            };

            await command.ExecuteAsync(console);

            var fileContent = await File.ReadAllTextAsync(outputFile);
            await Verify(fileContent);
        }
        finally
        {
            if (File.Exists(outputFile)) File.Delete(outputFile);
        }
    }

    [Test]
    public async Task ExecuteAsync_InputAndOutputFileSpecified_HasVerifiedOutput()
    {
        using var console = new FakeInMemoryConsole();
        var inputFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files", "SimpleGalaxyDump.csv");
        var outputFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.csv");

        try
        {
            var command = new PatchCommand
            {
                InputFile = inputFile,
                OutputFile = outputFile,
                Patches = ["Description=Both Files Specified"]
            };

            await command.ExecuteAsync(console);

            var fileContent = await File.ReadAllTextAsync(outputFile);
            await Verify(fileContent);
        }
        finally
        {
            if (File.Exists(outputFile)) File.Delete(outputFile);
        }
    }
}