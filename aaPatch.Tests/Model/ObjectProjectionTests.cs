using aaPatch.Extensions;
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
    [TestCase("*InputSource")]
    [TestCase("%InputSource")]
    public void Project_WildcardSuffix_ShouldMatchCaseInsensitively(string pattern)
    {
        var projection = new ObjectProjection(pattern);
        var data = new ObjectData(
        [
            new AttributeData("InputSource", "Direct"),
            new AttributeData("PlantInputSource", "Plant"),
            new AttributeData("Description", "Other")
        ]);

        var result = projection.Project(data).ToList();

        result.Select(a => a.Name).Should().BeEquivalentTo("InputSource", "PlantInputSource");
    }

    [Test]
    [TestCase("%10%")]
    [TestCase("*10*")]
    public void Project_InfixWildcard_ShouldMatch(string pattern)
    {
        var projection = new ObjectProjection(pattern);
        var data = new ObjectData(
        [
            new AttributeData("Plant10InputSource", "Match"),
            new AttributeData("Plant11InputSource", "No match")
        ]);

        var result = projection.Project(data).ToList();

        result.Should().ContainSingle().Which.Name.Should().Be("Plant10InputSource");
    }

    [Test]
    public void Project_OverlappingSelections_ShouldDeduplicateAttributes()
    {
        var data = new ObjectData(
        [
            new AttributeData("TagName", "P_101"),
            new AttributeData("Description", "Pump")
        ]);
        ObjectProjection[] projections = ["TagName", "*"];

        var result = new[] { data }.Project(projections).Single();

        result.Attributes.Should().HaveCount(2);
        result.Attributes.Should().ContainSingle(a => a.Name.Equals("TagName", StringComparison.OrdinalIgnoreCase))
            .Which.Value.Should().Be("P_101");
        result.Attributes.Select(a => a.Name).Should().BeEquivalentTo("TagName", "Description");
    }

    [Test]
    public void Project_SingleCharacterWildcard_ShouldNotMatchMultipleCharacters()
    {
        var projection = new ObjectProjection("Alarm?");
        var data = new ObjectData(
        [
            new AttributeData("Alarm1", "Match"),
            new AttributeData("Alarm12", "No match")
        ]);

        var result = projection.Project(data).ToList();

        result.Should().ContainSingle().Which.Name.Should().Be("Alarm1");
    }

    [Test]
    public void Project_WildcardWithRegexMetacharacter_ShouldEscapeAttributeName()
    {
        var projection = new ObjectProjection("Valve.PV*");
        var data = new ObjectData(
        [
            new AttributeData("Valve.PV_101", "Match"),
            new AttributeData("ValveXPV_101", "No match")
        ]);

        var result = projection.Project(data).ToList();

        result.Should().ContainSingle().Which.Name.Should().Be("Valve.PV_101");
    }

    [Test]
    public void Project_NonMatchingWildcard_ShouldReturnEmpty()
    {
        var projection = new ObjectProjection("Missing*");
        var data = new ObjectData([new AttributeData("Description", "Value")]);

        var result = projection.Project(data);

        result.Should().BeEmpty();
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
