using aaPatch.Formats;
using FluentAssertions;

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

        result.Should().HaveCount(1);
        result[0]["TagName"].Should().Be("T1");
    }

    [Test]
    public void Read_AvevaInput_DetectedAndParsed()
    {
        var router = new FormatRouter();
        const string aveva = ":TEMPLATE=$T\n:Tagname\nT1";

        var result = router.Read(aveva).ToList();

        result.Should().HaveCount(1);
        result[0]["TagName"].Should().Be("T1");
    }

    [Test]
    public void Read_CsvInput_DetectedAndParsed()
    {
        var router = new FormatRouter();
        const string csv = "TagName\nT1";

        var result = router.Read(csv).ToList();

        result.Should().HaveCount(1);
        result[0]["TagName"].Should().Be("T1");
    }

    [Test]
    [TestCase("  [{\"TagName\":\"T1\"}]", "T1")]
    [TestCase("\n:TEMPLATE=$T\n:Tagname\nT1", "T1")]
    public void Read_InputWithLeadingWhitespace_DetectedAndParsed(string input, string expectedTagName)
    {
        var router = new FormatRouter();

        var result = router.Read(input).ToList();

        result[0]["TagName"].Should().Be(expectedTagName);
    }
}