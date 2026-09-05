using aaPatch.Model;
using FluentAssertions;

namespace aaPatch.Tests.Model;

[TestFixture]
public class ObjectDataTests
{
    private static List<AttributeData> CreateDefaultAttributes() =>
    [
        new("Template", "$Pump"),
        new("TagName", "P_101"),
        new("Description", "Centrifugal Pump"),
        new("HiHi(MxDouble)", 100.0)
    ];

    [Test]
    public void Constructor_ValidInput_InitializesProperties()
    {
        var attributes = CreateDefaultAttributes();

        var data = new ObjectData(attributes);

        data["Template"].Should().Be("$Pump");
        data["TagName"].Should().Be("P_101");
        data.Select(a => a.Name).Should().Contain("Description");
        data.Select(a => a.Value).Should().Contain("Centrifugal Pump");
    }

    [Test]
    public void Indexer_NonExistingAttribute_ReturnsNull()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        var value = data["NonExistent"];

        value.Should().Be(AttributeValue.Null);
    }

    [Test]
    public void Indexer_ExistingAttribute_ReturnsValue()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        data["Template"].Should().Be("$Pump");
        data["TagName"].Should().Be("P_101");
        data["Description"].Should().Be("Centrifugal Pump");
        data["HiHi"].Should().Be(100.0);
    }

    [Test]
    public void Has_VariousScenarios_ReturnsExpectedResult()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        data.Has([]).Should().BeTrue();
        data.Has(["Template", "TagName"]).Should().BeTrue();
        data.Has(["HiHi"]).Should().BeTrue(); // Prefix match
        data.Has(["NonExistent"]).Should().BeFalse();
        data.Has(["TagName", "NonExistent"]).Should().BeFalse();
    }

    [Test]
    public void ToString_ReturnsCommaSeparatedValues()
    {
        var data = new ObjectData([
            new AttributeData("A", "Val1"),
            new AttributeData("B", "Val2")
        ]);

        data.ToString().Should().Be("[A, Val1],[B, Val2]");
    }

    [Test]
    public void Indexer_AmbiguousPrefixMatch_ThrowsArgumentException()
    {
        var data = new ObjectData([
            new AttributeData("Test(Type1)", "V1"),
            new AttributeData("Test(Type2)", "V2")
        ]);

        var act = () => _ = data["Test"];

        act.Should().Throw<ArgumentException>().WithMessage("*Ambiguous attribute name 'Test'*");
    }

    [Test]
    public void Indexer_ExplicitMatch_WinsOverPrefixMatch()
    {
        var data = new ObjectData([
            new AttributeData("Test", "Explicit"),
            new AttributeData("Test(Type)", "Prefix")
        ]);

        data["Test"].Should().Be("Explicit");
    }

    [Test]
    public void Indexer_PrefixMatch_IsCaseInsensitive()
    {
        var data = new ObjectData([
            new AttributeData("SomeField(MxString)", "Value")
        ]);

        // ReSharper disable once StringLiteralTypo
        data["somefield"].Should().Be("Value");
    }

    [Test]
    public void GetEnumerator_IteratesOverAllAttributes()
    {
        var attributes = CreateDefaultAttributes();
        var data = new ObjectData(attributes);

        data.Should().BeEquivalentTo(attributes);
    }

    [Test]
    public void Count_ReturnsAttributeCount()
    {
        var data = new ObjectData(CreateDefaultAttributes());
        data.Count.Should().Be(4);
    }
}