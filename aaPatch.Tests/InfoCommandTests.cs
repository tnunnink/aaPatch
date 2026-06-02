using aaPatch.Commands;
using CliFx.Infrastructure;

namespace aaPatch.Tests;

[TestFixture]
public class InfoCommandTests
{
    [Test]
    public async Task ExecuteAsync_PrintsInfoText()
    {
        using var console = new FakeInMemoryConsole();
        var command = new InfoCommand();

        await command.ExecuteAsync(console);

        await Verify(console.ReadOutputString());
    }
}