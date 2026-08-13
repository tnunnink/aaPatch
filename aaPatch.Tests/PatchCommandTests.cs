using aaPatch.Commands;

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
            Filter = "template=$Pump"
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
            Filter = "Template=$Pump"
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
        // Using an invalid path character or non-existent drive/deeply invalid path
        const string outputFile = @"Z:\NonExistentDir\output.csv";

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
        Assert.That(output, Does.Not.Contain("V_201,Gate Valve,True")); // Filtered out
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
    public async Task ExecuteAsync_FindReplaceCaseInsensitiveByDefault_PatchesCorrectly()
    {
        using var console = new FakeInMemoryConsole();
        console.WriteInput(SimpleGalaxyDump);

        var command = new PatchCommand
        {
            Patches = ["Description:PUMP=Motor"]
        };

        await command.ExecuteAsync(console);

        var output = console.ReadOutputString();
        Assert.That(output, Does.Contain("P_101,Centrifugal Motor,100"));
    }

    [Test]
    public async Task ExecuteAsync_FindReplaceWithMatchCase_DoesNotPatchWhenCasingDiffers()
    {
        using var console = new FakeInMemoryConsole();
        console.WriteInput(SimpleGalaxyDump);

        var command = new PatchCommand
        {
            Patches = ["Description:PUMP=Motor"],
            MatchCase = true
        };

        await command.ExecuteAsync(console);

        var output = console.ReadOutputString();
        Assert.That(output, Does.Contain("P_101,Centrifugal Pump,100"));
    }

    [Test]
    public async Task ExecuteAsync_FindReplaceWithMatchCase_PatchesWhenCasingMatches()
    {
        using var console = new FakeInMemoryConsole();
        console.WriteInput(SimpleGalaxyDump);

        var command = new PatchCommand
        {
            Patches = ["Description:Pump=Motor"],
            MatchCase = true
        };

        await command.ExecuteAsync(console);

        var output = console.ReadOutputString();
        Assert.That(output, Does.Contain("P_101,Centrifugal Motor,100"));
    }

    [Test]
    public async Task ExecuteAsync_TemplateFilter_EmitsMatchingObjects()
    {
        using var console = new FakeInMemoryConsole();
        console.WriteInput(SimpleGalaxyDump);

        var command = new PatchCommand
        {
            Filter = "Template=$Pump"
        };

        await command.ExecuteAsync(console);

        var output = console.ReadOutputString();
        Assert.That(output, Does.Contain(":TEMPLATE=$Pump"));
        Assert.That(output, Does.Contain("P_101"));
        Assert.That(output, Does.Not.Contain(":TEMPLATE=$Valve"));
        Assert.That(output, Does.Not.Contain("V_201"));
    }

    [Test]
    public async Task ExecuteAsync_FilterOnly_EmitsMatchingObjects()
    {
        using var console = new FakeInMemoryConsole();
        console.WriteInput(SimpleGalaxyDump);

        var command = new PatchCommand
        {
            Filter = "P_101"
        };

        await command.ExecuteAsync(console);

        var output = console.ReadOutputString();
        Assert.That(output, Does.Contain("P_101"));
        Assert.That(output, Does.Not.Contain("P_102"));
        Assert.That(output, Does.Not.Contain("V_201"));
    }

    [Test]
    public async Task ExecuteAsync_FilterNoMatches_ProducesEmptyOutput()
    {
        using var console = new FakeInMemoryConsole();
        console.WriteInput(SimpleGalaxyDump);

        var command = new PatchCommand
        {
            Filter = "NonExistent"
        };

        await command.ExecuteAsync(console);

        var output = console.ReadOutputString();
        Assert.That(output, Is.Empty);
    }

    [Test]
    public async Task ExecuteAsync_WithSelections_IncludesOnlySelectedAttributes()
    {
        using var console = new FakeInMemoryConsole();
        console.WriteInput(SimpleGalaxyDump);

        var command = new PatchCommand
        {
            Selections = ["Template", "TagName", "Description"]
        };

        await command.ExecuteAsync(console);

        var output = console.ReadOutputString();
        Assert.That(output, Does.Contain(":TEMPLATE=$Pump"));
        Assert.That(output, Does.Contain(":Tagname,Description"));
        Assert.That(output, Does.Not.Contain("HiHi"));
    }

    [Test]
    public async Task ExecuteAsync_WithAliasedSelections_RenamesAttributes()
    {
        using var console = new FakeInMemoryConsole();
        console.WriteInput(SimpleGalaxyDump);

        var command = new PatchCommand
        {
            Selections = ["Template", "TagName", "Description=ShortDesc"]
        };

        await command.ExecuteAsync(console);

        var output = console.ReadOutputString();
        Assert.That(output, Does.Contain(":Tagname,ShortDesc"));
        Assert.That(output, Does.Not.Contain(",Description"));
    }

    [Test]
    public async Task ExecuteAsync_WithEmptySelections_IncludesAllAttributes()
    {
        using var console = new FakeInMemoryConsole();
        console.WriteInput(SimpleGalaxyDump);

        var command = new PatchCommand
        {
            Selections = []
        };

        await command.ExecuteAsync(console);

        var output = console.ReadOutputString();
        Assert.That(output, Does.Contain(":Tagname,Description,HiHi"));
    }

    [Test]
    public void ExecuteAsync_InvalidSelection_ThrowsCommandException()
    {
        using var console = new FakeInMemoryConsole();
        console.WriteInput(SimpleGalaxyDump);

        var command = new PatchCommand
        {
            Selections = ["NonExistent"]
        };

        var ex = Assert.ThrowsAsync<CommandException>(async () => await command.ExecuteAsync(console));
        Assert.That(ex.Message, Does.Contain("Attribute 'NonExistent' does not exist"));
    }

    [Test]
    public void ExecuteAsync_WithoutIdentitySelections_ThrowsCommandException()
    {
        using var console = new FakeInMemoryConsole();
        console.WriteInput(SimpleGalaxyDump);

        var command = new PatchCommand
        {
            Selections = ["Description"]
        };

        var ex = Assert.ThrowsAsync<CommandException>(async () => await command.ExecuteAsync(console));
        Assert.That(ex.Message, Does.Contain("Required attribute Template does not exist")
            .Or.Contain("Required attribute TagName does not exist"));
    }

    [Test]
    public async Task ExecuteAsync_FilterAndSelections_WorksTogether()
    {
        using var console = new FakeInMemoryConsole();
        console.WriteInput(SimpleGalaxyDump);

        var command = new PatchCommand
        {
            Filter = "Template=$Pump",
            Selections = ["Template", "TagName", "HiHi"]
        };

        await command.ExecuteAsync(console);

        var output = console.ReadOutputString();
        Assert.That(output, Does.Contain(":TEMPLATE=$Pump"));
        Assert.That(output, Does.Not.Contain(":TEMPLATE=$Valve"));
        Assert.That(output, Does.Contain(":Tagname,HiHi"));
        Assert.That(output, Does.Not.Contain("Description"));
    }

    [Test]
    public async Task ExecuteAsync_DefaultFormat_IsAveva()
    {
        using var console = new FakeInMemoryConsole();
        console.WriteInput(SimpleGalaxyDump);

        var command = new PatchCommand();

        await command.ExecuteAsync(console);

        var output = console.ReadOutputString();
        Assert.That(output, Does.Contain(":TEMPLATE=$Pump"));
    }

    [Test]
    public async Task ExecuteAsync_AvevaFormat_HasVerifiedOutput()
    {
        using var console = new FakeInMemoryConsole();
        console.WriteInput(SimpleGalaxyDump);

        var command = new PatchCommand
        {
            Format = "aveva"
        };

        await command.ExecuteAsync(console);

        await Verify(console.ReadOutputString());
    }

    [Test]
    public async Task ExecuteAsync_JsonFormat_HasVerifiedOutput()
    {
        using var console = new FakeInMemoryConsole();
        console.WriteInput(SimpleGalaxyDump);

        var command = new PatchCommand
        {
            Format = "json"
        };

        await command.ExecuteAsync(console);

        await Verify(console.ReadOutputString());
    }

    [Test]
    public void ExecuteAsync_InvalidFormat_ThrowsCommandException()
    {
        using var console = new FakeInMemoryConsole();
        console.WriteInput(SimpleGalaxyDump);

        var command = new PatchCommand
        {
            Format = "invalid"
        };

        var ex = Assert.ThrowsAsync<CommandException>(async () => await command.ExecuteAsync(console));
        Assert.That(ex.Message, Does.Contain("Unsupported output format 'invalid'"));
    }
}