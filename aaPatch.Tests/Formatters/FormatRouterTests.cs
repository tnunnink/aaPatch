using aaPatch.Formats;

namespace aaPatch.Tests.Formatters;

[TestFixture]
public class FormatRouterTests
{
    [Test]
    public void Read_JsonInput_DetectedAndParsed()
    {
        var router = new FormatRouter(); // Format passed to constructor is for Write
        const string json = "[{\"TagName\":\"T1\"}]";

        var result = router.Read(json).ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0]["TagName"], Is.EqualTo("T1"));
    }

    [Test]
    public void Read_AvevaInput_DetectedAndParsed()
    {
        var router = new FormatRouter();
        const string aveva = ":TEMPLATE=$T\n:Tagname\nT1";

        var result = router.Read(aveva).ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0]["TagName"], Is.EqualTo("T1"));
    }

    [Test]
    public void Read_CsvInput_DetectedAndParsed()
    {
        var router = new FormatRouter();
        const string csv = "TagName\nT1";

        var result = router.Read(csv).ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0]["TagName"], Is.EqualTo("T1"));
    }

    [Test]
    [TestCase("  [{\"TagName\":\"T1\"}]", "T1")]
    [TestCase("\n:TEMPLATE=$T\n:Tagname\nT1", "T1")]
    public void Read_InputWithLeadingWhitespace_DetectedAndParsed(string input, string expectedTagName)
    {
        var router = new FormatRouter();

        var result = router.Read(input).ToList();

        Assert.That(result[0]["TagName"], Is.EqualTo(expectedTagName));
    }
}