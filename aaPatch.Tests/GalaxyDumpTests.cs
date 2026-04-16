namespace aaPatch.Tests;

public class GalaxyDumpTests
{
    [Test]
    public void ReadTestFileShouldHaveExpectedData()
    {
        var text = File.ReadAllText(@"C:\Users\tnunn\Documents\Test\SUN_EGPT.csv");

        var data = GalaxyDump.Read(text);

        Assert.That(data, Is.Not.Empty);
    }
}