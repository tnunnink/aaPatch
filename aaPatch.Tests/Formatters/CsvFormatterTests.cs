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

    [Test]
    public void Read_EmptyFields_RemainEmptyString()
    {
        var formatter = new CsvFormatter();
        const string csv = "TagName,Description\nP_101,";

        var result = formatter.Read(csv).ToList()[0];
        result["Description"].Should().Be("");
    }

    [Test]
    public void Write_ReorderedAttributes_AlignsUnderCorrectHeaders()
    {
        var obj1 = new ObjectData([
            new AttributeData("A", "1"),
            new AttributeData("B", "2"),
            new AttributeData("C", "3")
        ]);

        var obj2 = new ObjectData([
            new AttributeData("C", "6"),
            new AttributeData("A", "4"),
            new AttributeData("B", "5")
        ]);

        var formatter = new CsvFormatter();
        var csv = formatter.Write([obj1, obj2]);

        var lines = csv.Split(Environment.NewLine);
        lines[0].Should().Be("A,B,C");
        lines[1].Should().Be("1,2,3");
        lines[2].Should().Be("4,5,6");
    }

    [Test]
    public void Write_HeterogeneousSchemas_EmitsUnionHeadersAndEmptyFields()
    {
        var obj1 = new ObjectData([
            new AttributeData("TagName", "Tag1"),
            new AttributeData("Description", "Desc1")
        ]);

        var obj2 = new ObjectData([
            new AttributeData("TagName", "Tag2"),
            new AttributeData("HiHi", 100.0)
        ]);

        var formatter = new CsvFormatter();
        var csv = formatter.Write([obj1, obj2]);

        var lines = csv.Split(Environment.NewLine);
        lines[0].Should().Be("TagName,Description,HiHi");
        lines[1].Should().Be("Tag1,Desc1,");
        lines[2].Should().Be("Tag2,,100");
    }
}