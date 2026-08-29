using aaPatch.Formats;
using aaPatch.Model;
using FluentAssertions;

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

        result.Should().HaveCount(2);
        result[0]["TagName"].Should().Be("P_101");
        result[0]["Description"].Should().Be("Centrifugal Pump");
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

        formatter.Invoking(f => f.Read("not json")).Should().Throw<ArgumentException>();
        formatter.Invoking(f => f.Read("{ \"not\": \"an array\" }")).Should().Throw<ArgumentException>();
    }

    [Test]
    public void Read_EmptyString_ThrowsArgumentException()
    {
        var formatter = new JsonFormatter();
        formatter.Invoking(f => f.Read("")).Should().Throw<ArgumentException>();
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

        var result = formatter.Read(json).ToList()[0];

        result["Int"].Should().Be(10);
        result["Int"]?.Type.Should().Be(typeof(int));
        result["Long"].Should().Be(999999999999L);
        result["Long"]?.Type.Should().Be(typeof(long));
        result["Double"].Should().Be(10.5);
        result["Double"]?.Type.Should().Be(typeof(double));
        result["Bool"].Should().Be(true);
        result["String"].Should().Be("hello");
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

        result["Nested"]?.ToString().Should().Contain("\"Key\": \"Value\"");
        result["Array"]?.ToString().Should().MatchRegex(@"\[\s*1,\s*2,\s*3\s*\]");
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

        result[0]["A"].Should().Be(1);
        result[0]["B"].Should().BeNull();

        result[1]["A"].Should().BeNull();
        result[1]["B"].Should().Be(2);
    }
}