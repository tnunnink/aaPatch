using aaPatch.Model;

namespace aaPatch.Tests;

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
            Assert.That(data.Attributes.Select(a => a.Name), Has.Member("Description"));
            Assert.That(data.Attributes.Select(a => a.Value), Has.Member("Centrifugal Pump"));
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

        Assert.That(data["NonExistent"], Is.Null);
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
            Assert.That(data["NonExistent"], Is.Null);
        }
    }

    [Test]
    public void Update_Assignment_UpdatesValue()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        data.Update("HiHi", "120.0");

        Assert.That(data["HiHi"], Is.EqualTo(120.0));
    }

    [Test]
    public void Update_NonExistingAttribute_ThrowsArgumentException()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        Assert.Throws<ArgumentException>(() => data.Update("NewAttr", "Value123"));
    }

    [Test]
    public void Replace_SpecifiedAttribute_UpdatesValue()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        data.Replace("Centrifugal", "Positive Displacement", "Description");

        Assert.That(data["Description"], Is.EqualTo("Positive Displacement Pump"));
    }

    [Test]
    public void Replace_CaseInsensitiveByDefault_UpdatesValue()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        data.Replace("CENTRIFUGAL", "Positive Displacement", "Description");

        Assert.That(data["Description"], Is.EqualTo("Positive Displacement Pump"));
    }

    [Test]
    public void Replace_MatchCase_DoesNotUpdateValueWhenCasingDiffers()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        data.Replace("CENTRIFUGAL", "Positive Displacement", "Description", matchCase: true);

        Assert.That(data["Description"], Is.EqualTo("Centrifugal Pump"));
    }

    [Test]
    public void Replace_MatchCase_UpdatesValueWhenCasingMatches()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        data.Replace("Centrifugal", "Positive Displacement", "Description", matchCase: true);

        Assert.That(data["Description"], Is.EqualTo("Positive Displacement Pump"));
    }

    [Test]
    public void Update_Sequential_SeesPreviousResults()
    {
        var attributes = new List<AttributeData>
        {
            new(":template", "$Pump"),
            new(":tagname", "P_101"),
            new("Attr1", "Value1")
        };
        var obj = new ObjectData(attributes);

        // Patch 1: Value1 -> Value2
        obj.Update("Attr1", "Value2");
        // Patch 2: Value2 -> Value3
        obj.Update("Attr1", "Value3");

        Assert.That(obj["Attr1"], Is.EqualTo("Value3"));
    }

    [Test]
    public void Replace_Global_ModifiesMultipleFieldsWithoutThrowing()
    {
        var attributes = new List<AttributeData>
        {
            new("Template", "$Pump"),
            new("TagName", "P_101"),
            new("Attr1", "Match1"),
            new("Attr2", "Match2")
        };
        var obj = new ObjectData(attributes);

        Assert.DoesNotThrow(() => obj.Replace("Match", "Replaced"));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(obj["Attr1"], Is.EqualTo("Replaced1"));
            Assert.That(obj["Attr2"], Is.EqualTo("Replaced2"));
        }
    }

    [Test]
    public void Replace_Global_UpdatedAllFieldsIncludingIdentity()
    {
        var data = new ObjectData(new List<AttributeData>
        {
            new("Template", "$Pump"),
            new("TagName", "P_101"),
            new("Attr1", "Pump_P_101")
        });

        data.Replace("Pump", "Motor");
        data.Replace("P_101", "P_999");

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
}