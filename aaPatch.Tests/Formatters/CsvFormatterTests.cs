using aaPatch.Formats;
using aaPatch.Model;

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

        Assert.That(result, Has.Count.EqualTo(2));

        var p101 = result[0];
        Assert.That(p101["TagName"], Is.EqualTo("P_101"));
        Assert.That(p101["Description"], Is.EqualTo("Centrifugal Pump"));
        Assert.That(p101["Value"], Is.EqualTo("100.0"));

        var v201 = result[1];
        Assert.That(v201["TagName"], Is.EqualTo("V_201"));
        Assert.That(v201["Description"], Is.EqualTo("Gate Valve"));
        Assert.That(v201["Value"], Is.EqualTo("true"));
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

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Write_EmptyCollection_ThrowsInvalidOperationException()
    {
        var formatter = new CsvFormatter();

        Assert.Throws<InvalidOperationException>(() => formatter.Write([]));
    }
}