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

        value.Should().BeNull();
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

    /*
    [Test]
    public void Apply_Assignment_UpdatesValue()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        data.Apply("HiHi=120.0");

        data["HiHi"].Should().Be(120.0);
    }

    [Test]
    public void Apply_NonExistingAttribute_DoesNothing()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        data.Apply("NewAttr=Value123");

        data["NewAttr"].Should().BeNull();
    }

    [Test]
    public void Apply_SpecifiedAttribute_UpdatesValue()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        data.Apply("Description:Centrifugal=Positive Displacement");

        data["Description"].Should().Be("Positive Displacement Pump");
    }

    [Test]
    public void Apply_CaseInsensitiveByDefault_UpdatesValue()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        data.Apply("Description:CENTRIFUGAL=Positive Displacement");

        data["Description"].Should().Be("Positive Displacement Pump");
    }

    [Test]
    public void Apply_MatchCase_DoesNotUpdateValueWhenCasingDiffers()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        data.Apply("Description:CENTRIFUGAL=Positive Displacement", matchCase: true);

        data["Description"].Should().Be("Centrifugal Pump");
    }

    [Test]
    public void Apply_MatchCase_UpdatesValueWhenCasingMatches()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        data.Apply("Description:Centrifugal=Positive Displacement", matchCase: true);

        data["Description"].Should().Be("Positive Displacement Pump");
    }

    [Test]
    public void Apply_Sequential_SeesPreviousResults()
    {
        var data = new ObjectData(new List<AttributeData>
        {
            new("Template", "$Pump"),
            new("TagName", "P_101"),
            new("Attr1", "Value1")
        });

        data.Apply("Attr1=Value2");
        data.Apply("Attr1=Value3");

        data["Attr1"].Should().Be("Value3");
    }

    [Test]
    public void Apply_Global_ModifiesMultipleFieldsWithoutThrowing()
    {
        var data = new ObjectData(new List<AttributeData>
        {
            new("Template", "$Pump"),
            new("TagName", "P_101"),
            new("Attr1", "Match1"),
            new("Attr2", "Match2")
        });

        var act = () => data.Apply(":Match=Replaced");
        act.Should().NotThrow();

        data["Attr1"].Should().Be("Replaced1");
        data["Attr2"].Should().Be("Replaced2");
    }

    [Test]
    public void Apply_Global_UpdatedAllFieldsIncludingIdentity()
    {
        var data = new ObjectData(new List<AttributeData>
        {
            new("Template", "$Pump"),
            new("TagName", "P_101"),
            new("Attr1", "Pump_P_101")
        });

        data.Apply(":Pump=Motor");
        data.Apply(":P_101=P_999");

        data["Template"].Should().Be("$Motor");
        data["TagName"].Should().Be("P_999");
        data["Attr1"].Should().Be("Motor_P_999");
    }

    [Test]
    public void Indexer_IdentityLookup_IsCaseInsensitive()
    {
        var data = new ObjectData(new List<AttributeData>
        {
            new("Template", "$Pump"),
            new("TagName", "P_101"),
        });

        data["Template"].Should().Be("$Pump");
        data["template"].Should().Be("$Pump");
        data["TEMPLATE"].Should().Be("$Pump");

        data["TagName"].Should().Be("P_101");
        data["tagname"].Should().Be("P_101");
        data["TAGNAME"].Should().Be("P_101");
    }

    [Test]
    public void Project_SingleAttribute_ReturnsObjectWithSelectedAttribute()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        var projected = data.Select(["TagName"]);

        projected.Should().HaveCount(1);
        projected["TagName"].Should().Be("P_101");
    }

    [Test]
    public void Project_MultipleAttributes_ReturnsObjectWithSelectedAttributes()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        var projected = data.Select(["Template", "TagName"]);

        projected.Should().HaveCount(2);
        projected["Template"].Should().Be("$Pump");
        projected["TagName"].Should().Be("P_101");
    }

    [Test]
    public void Project_AliasedAttribute_ReturnsObjectWithRenamedAttribute()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        var projected = data.Select("Description=Desc");

        projected.Should().HaveCount(1);
        projected["Desc"].Should().Be("Centrifugal Pump");
        projected["Description"].Should().BeNull();
    }

    [Test]
    public void Project_MixedSelection_ReturnsObjectWithBothTypes()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        var projected = data.Select(["TagName", "Description=Desc"]);

        projected.Should().HaveCount(2);
        projected["TagName"].Should().Be("P_101");
        projected["Desc"].Should().Be("Centrifugal Pump");
    }

    [Test]
    public void Project_CaseInsensitiveSelection_MatchesExistingAttribute()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        var projected = data.Select(["tagname", "DESCRIPTION=Desc"]);

        projected.Should().HaveCount(2);
        projected["TagName"].Should().Be("P_101");
        projected["Desc"].Should().Be("Centrifugal Pump");
    }

    [Test]
    public void Project_PreservationOfValues_MaintainsAttributeValues()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        var projected = data.Select("HiHi");

        projected["HiHi"].Should().Be(100.0);
    }

    [Test]
    public void Project_NonExistentAttribute_ReturnsEmptyResult()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        var result = data.Select("NonExistent");

        result.Should().BeEmpty();
    }

    [Test]
    public void Project_DuplicateAlias_ThrowsArgumentException()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        var act = () => data.Select("TagName=Same", "Template=Same");
        act.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Project_EmptyOriginalName_ThrowsArgumentException()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        var result = data.Select("=Alias");

        result.Should().BeEmpty();
    }

    [Test]
    public void Match_TypeAwareMatching()
    {
        var data = new ObjectData(new List<AttributeData>
        {
            new("TagName", "P_101"),
            new("Value(MxDouble)", 100.5)
        });

        // Filter value is string "100.5", should match against double 100.5
        data.Match("Value=100.5").Should().BeTrue();
        data.Match("Value=100*").Should().BeTrue();
    }

    [Test]
    public void Apply_TypeAwarePatching()
    {
        var data = new ObjectData(new List<AttributeData>
        {
            new("TagName", "P_101"),
            new("Enabled(MxBoolean)", true)
        });

        // Patch with string "false", should update the boolean value
        data.Apply("Enabled=false");
        data["Enabled"].Should().Be(false);
        data["Enabled"]?.Type.Should().Be(typeof(bool));
    }

    [Test]
    public void Apply_ReplaceOnTypedValue()
    {
        var data = new ObjectData(new List<AttributeData>
        {
            new("TagName", "P_101"),
            new("Count(MxInteger)", 10)
        });

        // Replace "1" with "2" -> "20"
        data.Apply("Count:1=2");
        data["Count"].Should().Be(20);
        data["Count"]?.Type.Should().Be(typeof(int));
    }*/
}