namespace aaPatch.Tests;
using CliFx;
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
            Templates = ["$Pump"]
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
            Templates = ["$Pump"]
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

    [Test]
    public void ExecuteAsync_InputFileNotFound_ThrowsCommandException()
    {
        using var console = new FakeInMemoryConsole();
        var command = new PatchCommand
        {
            InputFile = "non_existent_file.csv",
            Patches = ["Description=Updated"]
        };

        var ex = Assert.ThrowsAsync<CommandException>(async () => await command.ExecuteAsync(console));
        Assert.That(ex.Message, Does.Contain("Patch failed with error"));
    }

    [Test]
    public async Task ExecuteAsync_EmptyInputFile_ThrowsCommandException()
    {
        using var console = new FakeInMemoryConsole();
        var inputFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.csv");
        await File.WriteAllTextAsync(inputFile, "");

        try
        {
            var command = new PatchCommand
            {
                InputFile = inputFile,
                Patches = ["Description=Updated"]
            };

            var ex = Assert.ThrowsAsync<CommandException>(async () => await command.ExecuteAsync(console));
            Assert.That(ex.Message, Does.Contain("The text parameter cannot be null or empty"));
        }
        finally
        {
            if (File.Exists(inputFile)) File.Delete(inputFile);
        }
    }

    [Test]
    public async Task ExecuteAsync_InvalidOutputFileDir_ThrowsCommandException()
    {
        using var console = new FakeInMemoryConsole();
        console.WriteInput(SimpleGalaxyDump);
        // Using an invalid path characters or non-existent drive/deeply invalid path
        var outputFile = @"Z:\NonExistentDir\output.csv"; 

        var command = new PatchCommand
        {
            OutputFile = outputFile,
            Patches = ["Description=Updated"]
        };

        var ex = Assert.ThrowsAsync<CommandException>(async () => await command.ExecuteAsync(console));
        Assert.That(ex.Message, Does.Contain("Patch failed with error"));
    }

    [Test]
    public async Task ExecuteAsync_FilterByAttribute_OnlyPatchesMatches()
    {
        using var console = new FakeInMemoryConsole();
        console.WriteInput(SimpleGalaxyDump);

        var command = new PatchCommand
        {
            Filter = "Description=Centrifugal*",
            Patches = ["HiHi=200.0"]
        };

        await command.ExecuteAsync(console);
        
        var output = console.ReadOutputString();
        Assert.That(output, Does.Contain("P_101,Centrifugal Pump,200")); // Patched
        Assert.That(output, Does.Contain("V_201,Gate Valve,True"));      // Not patched but still in output
    }

    [Test]
    public async Task ExecuteAsync_TestObjectDump_HasVerifiedOutput()
    {
        using var console = new FakeInMemoryConsole();
        var inputFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Files", "TestObjectDump.csv");

        var command = new PatchCommand
        {
            InputFile = inputFile,
            Patches = ["ShortDesc=This is a patched file"]
        };

        await command.ExecuteAsync(console);

        await Verify(console.ReadOutputString());
    }

    [Test]
    public async Task ExecuteAsync_PreviewMode_LogsToErrorStream()
    {
        using var console = new FakeInMemoryConsole();
        console.WriteInput(SimpleGalaxyDump);

        var command = new PatchCommand
        {
            Patches = ["Description=Updated"],
            Preview = true
        };

        await command.ExecuteAsync(console);

        var output = console.ReadOutputString();
        var error = console.ReadErrorString();

        Assert.That(output, Is.Empty);
        Assert.That(error, Does.Contain("P_101: 'Description' \"Centrifugal Pump\" -> \"Updated\""));
        Assert.That(error, Does.Contain("V_201: 'Description' \"Gate Valve\" -> \"Updated\""));
    }
}