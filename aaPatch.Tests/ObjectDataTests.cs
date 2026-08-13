using aaPatch.Model;

namespace aaPatch.Tests;

[TestFixture]
public class ObjectDataTests
{
    private const string Template = "$Pump";
    private const string TagName = "P_101";

    private static List<AttributeData> CreateDefaultAttributes() =>
    [
        new(":TEMPLATE", Template),
        new(":Tagname", TagName),
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
            Assert.That(data.Template, Is.EqualTo(Template));
            Assert.That(data.TagName, Is.EqualTo(TagName));
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
            Assert.That(data["Template"], Is.EqualTo(Template));
            Assert.That(data["TagName"], Is.EqualTo(TagName));
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
    public void Update_TagName_ThrowsArgumentException()
    {
        var data = new ObjectData(CreateDefaultAttributes());

        Assert.Throws<ArgumentException>(() => data.Update(":Tagname", "NewTag"));
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
}