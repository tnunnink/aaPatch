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

    [Test]
    public void Read_DifferentPrimitiveTypes_TypesShouldBePreserved()
    {
        var formatter = new JsonFormatter();
        const string json =
            """
            [
              {
                "Int": 10,
                "Long": 999999999999,
                "Double": 10.5,
                "Bool": true,
                "Null": null,
                "String": "hello"
              }
            ]
            """;

        var result = formatter.Read(json).ToList();
        var obj = result[0];

        using (Assert.EnterMultipleScope())
        {
            Assert.That(obj["Int"], Is.EqualTo(10));
            Assert.That(obj["Int"], Is.TypeOf<int>());
            Assert.That(obj["Long"], Is.EqualTo(999999999999L));
            Assert.That(obj["Long"], Is.TypeOf<long>());
            Assert.That(obj["Double"], Is.EqualTo(10.5));
            Assert.That(obj["Double"], Is.TypeOf<double>());
            Assert.That(obj["Bool"], Is.True);
            Assert.That(obj["Null"], Is.Null);
            Assert.That(obj["String"], Is.EqualTo("hello"));
        }
    }

    [Test]
    public void Read_NestedObjects_CapturedAsRawJsonStrings()
    {
        var formatter = new JsonFormatter();
        const string json =
            """
            [
              {
                "TagName": "T1",
                "Nested": { "Key": "Value" },
                "Array": [1, 2, 3]
              }
            ]
            """;

        var result = formatter.Read(json).ToList()[0];

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result["Nested"]?.ToString(), Does.Contain("\"Key\": \"Value\""));
            Assert.That(result["Array"]?.ToString(), Does.Match(@"\[\s*1,\s*2,\s*3\s*\]"));
        }
    }

    [Test]
    public void Read_HeterogeneousObjects_ShouldContainExpectedValues()
    {
        var formatter = new JsonFormatter();
        const string json =
            """
            [
              { "TagName": "T1", "A": 1 },
              { "TagName": "T2", "B": 2 }
            ]
            """;

        var result = formatter.Read(json).ToList();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(result[0]["A"], Is.EqualTo(1));
            Assert.That(result[0]["B"], Is.Null);

            Assert.That(result[1]["A"], Is.Null);
            Assert.That(result[1]["B"], Is.EqualTo(2));
        }
    }
}