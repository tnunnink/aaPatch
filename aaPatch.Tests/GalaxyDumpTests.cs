namespace aaPatch.Tests;

public class GalaxyDumpTests
{
    [Test]
    public void ReadTestFileShouldHaveExpectedData()
    {
        var data = GalaxyDump.Read(@"C:\Users\tnunn\Documents\Test\SUN_EGPT.csv");
        
        Assert.That(data, Is.Not.Empty);
    }
}