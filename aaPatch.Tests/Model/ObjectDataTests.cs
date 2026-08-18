using aaPatch.Model;

namespace aaPatch.Tests.Model;

[TestFixture]
public class ObjectDataTests
{
    private static List<AttributeData> CreateDefaultAttributes() =>
    [
        new("Template", "$Pump"),
        new("TagName", "P_101"),
        new("Description", "Centrifugal Pump"),
        new("HiHi(MxDouble)", "100.0")
    ];

    [Test]
    public void Constructor_ValidInput_InitializesProperties()
    {
        var attributes = CreateDefaultAttributes();

        var data = new ObjectData(attributes);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(data.Template, Is.EqualTo("$Pump"));
            Assert.That(data.TagName, Is.EqualTo("P_101"));
            Assert.That(data.Select(a => a.Name), Has.Member("Description"));
            Assert.That(data.Select(a => a.Value), Has.Member("Centrifugal Pump"));
        }
    }

    [Test]
    public void Template_MissingAttribute_ThrowsInvalidOperationException()
    {
        var attributes = new List<AttributeData> { new("Description", "No Tagname Here") };

        var data = new ObjectData(attributes);

        Assert.Throws<InvalidOperationException>(() => _ = data.Template);
    }

    [Test]
    public void TagName_MissingAttribute_ThrowsInvalidOperationException()
    {
        var attributes = new List<AttributeData> { new("Description", "No Tagname Here") };

        var data = new ObjectData(attributes);

        Assert.Throws<InvalidOperationException>(() => _ = data.TagName);
    }

    [Test]
    public void Indexer_NonExistingAttribute_ReturnsNull()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        var value = data["NonExistent"];

        Assert.That(value, Is.Null);
    }

    [Test]
    public void Indexer_ExistingAttribute_ReturnsValue()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(data["Template"], Is.EqualTo("$Pump"));
            Assert.That(data["TagName"], Is.EqualTo("P_101"));
            Assert.That(data["Description"], Is.EqualTo("Centrifugal Pump"));
            Assert.That(data["HiHi"], Is.EqualTo(100.0));
        }
    }

    [Test]
    public void Apply_Assignment_UpdatesValue()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        data.Apply("HiHi=120.0");

        Assert.That(data["HiHi"], Is.EqualTo(120.0));
    }

    [Test]
    public void Apply_NonExistingAttribute_DoesNothing()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        data.Apply("NewAttr=Value123");

        Assert.That(data["NewAttr"], Is.Null);
    }

    [Test]
    public void Apply_SpecifiedAttribute_UpdatesValue()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        data.Apply("Description:Centrifugal=Positive Displacement");

        Assert.That(data["Description"], Is.EqualTo("Positive Displacement Pump"));
    }

    [Test]
    public void Apply_CaseInsensitiveByDefault_UpdatesValue()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        data.Apply("Description:CENTRIFUGAL=Positive Displacement");

        Assert.That(data["Description"], Is.EqualTo("Positive Displacement Pump"));
    }

    [Test]
    public void Apply_MatchCase_DoesNotUpdateValueWhenCasingDiffers()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        data.Apply("Description:CENTRIFUGAL=Positive Displacement", matchCase: true);

        Assert.That(data["Description"], Is.EqualTo("Centrifugal Pump"));
    }

    [Test]
    public void Apply_MatchCase_UpdatesValueWhenCasingMatches()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        data.Apply("Description:Centrifugal=Positive Displacement", matchCase: true);

        Assert.That(data["Description"], Is.EqualTo("Positive Displacement Pump"));
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

        Assert.That(data["Attr1"], Is.EqualTo("Value3"));
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

        Assert.DoesNotThrow(() => data.Apply(":Match=Replaced"));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(data["Attr1"], Is.EqualTo("Replaced1"));
            Assert.That(data["Attr2"], Is.EqualTo("Replaced2"));
        }
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

        using (Assert.EnterMultipleScope())
        {
            Assert.That(data.Template, Is.EqualTo("$Motor"));
            Assert.That(data.TagName, Is.EqualTo("P_999"));
            Assert.That(data["Attr1"], Is.EqualTo("Motor_P_999"));
        }
    }

    [Test]
    public void Indexer_IdentityLookup_IsCaseInsensitive()
    {
        var data = new ObjectData(new List<AttributeData>
        {
            new("Template", "$Pump"),
            new("TagName", "P_101"),
        });

        using (Assert.EnterMultipleScope())
        {
            Assert.That(data["Template"], Is.EqualTo("$Pump"));
            Assert.That(data["template"], Is.EqualTo("$Pump"));
            Assert.That(data["TEMPLATE"], Is.EqualTo("$Pump"));

            Assert.That(data["TagName"], Is.EqualTo("P_101"));
            Assert.That(data["tagname"], Is.EqualTo("P_101"));
            Assert.That(data["TAGNAME"], Is.EqualTo("P_101"));
        }
    }

    [Test]
    public void Project_SingleAttribute_ReturnsObjectWithSelectedAttribute()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        var projected = data.Project(["TagName"]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(projected, Has.Count.EqualTo(1));
            Assert.That(projected.TagName, Is.EqualTo("P_101"));
        }
    }

    [Test]
    public void Project_MultipleAttributes_ReturnsObjectWithSelectedAttributes()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        var projected = data.Project(["Template", "TagName"]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(projected, Has.Count.EqualTo(2));
            Assert.That(projected.Template, Is.EqualTo("$Pump"));
            Assert.That(projected.TagName, Is.EqualTo("P_101"));
        }
    }

    [Test]
    public void Project_AliasedAttribute_ReturnsObjectWithRenamedAttribute()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        var projected = data.Project("Description=Desc");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(projected, Has.Count.EqualTo(1));
            Assert.That(projected["Desc"], Is.EqualTo("Centrifugal Pump"));
            Assert.That(projected["Description"], Is.Null);
        }
    }

    [Test]
    public void Project_MixedSelection_ReturnsObjectWithBothTypes()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        var projected = data.Project(["TagName", "Description=Desc"]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(projected, Has.Count.EqualTo(2));
            Assert.That(projected.TagName, Is.EqualTo("P_101"));
            Assert.That(projected["Desc"], Is.EqualTo("Centrifugal Pump"));
        }
    }

    [Test]
    public void Project_CaseInsensitiveSelection_MatchesExistingAttribute()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        var projected = data.Project(["tagname", "DESCRIPTION=Desc"]);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(projected, Has.Count.EqualTo(2));
            Assert.That(projected.TagName, Is.EqualTo("P_101"));
            Assert.That(projected["Desc"], Is.EqualTo("Centrifugal Pump"));
        }
    }

    [Test]
    public void Project_PreservationOfValues_MaintainsAttributeValues()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        var projected = data.Project(["HiHi"]);

        Assert.That(projected["HiHi"], Is.EqualTo(100.0));
    }

    [Test]
    public void Project_NonExistentAttribute_ReturnsEmptyResult()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        var result = data.Project("NonExistent");
        
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void Project_DuplicateAlias_ThrowsArgumentException()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        Assert.Throws<ArgumentException>(() => data.Project(["TagName=Same", "Template=Same"]));
    }

    [Test]
    public void Project_EmptyOriginalName_ThrowsArgumentException()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        var result = data.Project("=Alias");

        Assert.That(result, Is.Empty);
    }
}