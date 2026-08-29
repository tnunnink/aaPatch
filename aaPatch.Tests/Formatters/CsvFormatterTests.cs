using aaPatch.Formats;
using aaPatch.Model;
using FluentAssertions;

namespace aaPatch.Tests.Formatters;

[TestFixture]
public class CsvFormatterTests
{
    private const string StandardCsv =
        """
        TagName,Description,Value
        P_101,Centrifugal Pump,100.0
        V_201,Gate Valve,true
        """;

    [Test]
    public void Read_ValidCsv_ReturnsExpectedObjects()
    {
        var formatter = new CsvFormatter();

        var result = formatter.Read(StandardCsv).ToList();

        result.Should().HaveCount(2);

        result[0]["TagName"].Should().Be("P_101");
        result[0]["Description"].Should().Be("Centrifugal Pump");
        result[0]["Value"].Should().Be("100.0");

        result[1]["TagName"].Should().Be("V_201");
        result[1]["Description"].Should().Be("Gate Valve");
        result[1]["Value"].Should().Be("true");
    }

    [Test]
    public async Task Write_ValidObjects_MatchesVerified()
    {
        var formatter = new CsvFormatter();
        var objects = new List<ObjectData>
        {
            new(new List<AttributeData>
            {
                new("TagName", "P_101"),
                new("Description", "Pump 1"),
                new("Value", 10.5)
            }),
            new(new List<AttributeData>
            {
                new("TagName", "V_101"),
                new("Description", "Valve 1"),
                new("Value", true)
            })
        };

        var result = formatter.Write(objects);

        await Verify(result);
    }

    [Test]
    public void Read_EmptyCsv_ReturnsEmptyCollection()
    {
        var formatter = new CsvFormatter();

        // CsvHelper might return empty or throw depending on how it's used.
        // Let's see what happens with just headers.
        var result = formatter.Read("TagName,Description").ToList();

        result.Should().BeEmpty();
    }

    [Test]
    public void Write_EmptyCollection_ThrowsInvalidOperationException()
    {
        var formatter = new CsvFormatter();

        formatter.Invoking(f => f.Write([])).Should().Throw<InvalidOperationException>();
    }
}