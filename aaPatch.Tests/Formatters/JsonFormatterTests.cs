using aaPatch.Formats;
using aaPatch.Model;

namespace aaPatch.Tests.Formatters;

[TestFixture]
public class JsonFormatterTests
{
    private const string SimpleJson =
        """
        [
          {
            "TagName": "P_101",
            "Description": "Centrifugal Pump",
            "Value": 100.0,
            "Enabled": true,
            "Comment": null
          },
          {
            "TagName": "V_201",
            "Description": "Gate Valve",
            "Value": 20,
            "Enabled": false
          }
        ]
        """;

    [Test]
    public void Read_ValidJson_ReturnsExpectedObjects()
    {
        var formatter = new JsonFormatter();

        var result = formatter.Read(SimpleJson).ToList();


        using (Assert.EnterMultipleScope())
        {
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0]["TagName"], Is.EqualTo("P_101"));
            Assert.That(result[0]["Description"], Is.EqualTo("Centrifugal Pump"));
        }
    }

    [Test]
    public async Task Write_ValidObjects_MatchesVerified()
    {
        var formatter = new JsonFormatter();
        var objects = new List<ObjectData>
        {
            new(new List<AttributeData>
            {
                new("TagName", "P_101"),
                new("Description", "Pump 1"),
                new("Value", 10.5),
                new("Enabled", true),
                new("NullValue", null)
            })
        };

        var result = formatter.Write(objects);

        await Verify(result);
    }

    [Test]
    public void Read_InvalidJson_ThrowsArgumentException()
    {
        var formatter = new JsonFormatter();

        Assert.Throws<ArgumentException>(() => formatter.Read("not json"));
        Assert.Throws<ArgumentException>(() => formatter.Read("{ \"not\": \"an array\" }"));
    }

    [Test]
    public void Read_EmptyString_ThrowsArgumentException()
    {
        var formatter = new JsonFormatter();
        Assert.Throws<ArgumentException>(() => formatter.Read(""));
    }
}