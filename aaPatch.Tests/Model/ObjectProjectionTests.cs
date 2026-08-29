using aaPatch.Model;
using FluentAssertions;

namespace aaPatch.Tests.Model;

[TestFixture]
public class ObjectProjectionTests
{
    [Test]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null!)]
    public void Construct_InvalidInput_ShouldThrow(string input)
    {
        FluentActions.Invoking(() => _ = new ObjectProjection(input)).Should().Throw<ArgumentException>();
    }

    [Test]
    public void Construct_SimpleAttribute_ShouldParseCorrectly()
    {
        var projection = new ObjectProjection("Description");
        // We can't access private fields directly, so we test via Project
        
        var data = new ObjectData([new AttributeData("Description", "Test")]);
        var result = projection.Project(data).ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Description");
        result[0].Value.Should().Be("Test");
    }

    [Test]
    public void Construct_WithAlias_ShouldParseCorrectly()
    {
        var projection = new ObjectProjection("Desc := {Description}");
        
        var data = new ObjectData([new AttributeData("Description", "Test")]);
        var result = projection.Project(data).ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Desc");
        result[0].Value.Should().Be("Test");
    }

    [Test]
    public void Project_Wildcard_ShouldReturnAllAttributes()
    {
        var projection = new ObjectProjection("*");
        var attributes = new List<AttributeData>
        {
            new("A", 1),
            new("B", 2)
        };
        var data = new ObjectData(attributes);

        var result = projection.Project(data).ToList();

        result.Should().BeEquivalentTo(attributes);
    }

    [Test]
    public void Project_Expression_ShouldEvaluateCorrectly()
    {
        var projection = new ObjectProjection("Total := {A} + {B}");
        var data = new ObjectData([new AttributeData("A", 10), new AttributeData("B", 20)]);

        var result = projection.Project(data).ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Total");
        result[0].Value.Should().Be(30);
    }

    [Test]
    public void ImplicitConversion_ShouldWork()
    {
        ObjectProjection projection = "Alias := {Field}";
        
        var data = new ObjectData([new AttributeData("Field", "Value")]);
        var result = projection.Project(data).ToList();

        result[0].Name.Should().Be("Alias");
        result[0].Value.Should().Be("Value");
    }
}
