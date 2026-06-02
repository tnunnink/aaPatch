namespace aaPatch.Tests;

[TestFixture]
public class ObjectDataTests
{
    private const string TemplateName = "$Pump";
    private const string TagName = "P_101";

    private static List<AttributeData> CreateDefaultAttributes() =>
    [
        new(":Tagname", TagName),
        new("Description", "Centrifugal Pump"),
        new("HiHi(MxDouble)", "100.0")
    ];

    [Test]
    public void Constructor_ValidInput_InitializesProperties()
    {
        var attributes = CreateDefaultAttributes();

        var data = new ObjectData(TemplateName, attributes);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(data.Template, Is.EqualTo(TemplateName));
            Assert.That(data.TagName, Is.EqualTo(TagName));
            Assert.That(data.Attributes.Select(a => a.Name), Has.Member("Description"));
            Assert.That(data.Attributes.Select(a => a.Value), Has.Member("Centrifugal Pump"));
        }
    }

    [Test]
    public void Constructor_EmptyTemplate_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _ = new ObjectData("", CreateDefaultAttributes()));
        Assert.Throws<ArgumentException>(() => _ = new ObjectData(" ", CreateDefaultAttributes()));
    }

    [Test]
    public void TagName_MissingAttribute_ThrowsInvalidOperationException()
    {
        var attributes = new List<AttributeData> { new("Description", "No Tagname Here") };
        var data = new ObjectData(TemplateName, attributes);
        Assert.Throws<InvalidOperationException>(() => _ = data.TagName);
    }

    [Test]
    public void Indexer_NonExistingAttribute_ReturnsNull()
    {
        var data = new ObjectData(TemplateName, CreateDefaultAttributes());
        
        Assert.That(data["NonExistent"], Is.Null);
    }

    [Test]
    public void Indexer_ExistingAttribute_ReturnsValue()
    {
        var data = new ObjectData(TemplateName, CreateDefaultAttributes());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(data["Template"], Is.EqualTo(TemplateName));
            Assert.That(data["TagName"], Is.EqualTo(TagName));
            Assert.That(data["Description"], Is.EqualTo("Centrifugal Pump"));
            Assert.That(data["HiHi"], Is.EqualTo(100.0));
            Assert.That(data["NonExistent"], Is.Null);
        }
    }

    [Test]
    public void Update_Assignment_UpdatesValue()
    {
        var data = new ObjectData(TemplateName, CreateDefaultAttributes());

        data.Update("HiHi", "120.0");
        data.ApplyPatches();

        Assert.That(data["HiHi"], Is.EqualTo(120.0));
    }

    [Test]
    public void Update_ExistingAttribute_RequiresSaveChanges()
    {
        var data = new ObjectData(TemplateName, CreateDefaultAttributes());

        data.Update("HiHi", "120.0");
        data.ApplyPatches();

        Assert.That(data["HiHi"], Is.EqualTo(120.0));
    }

    [Test]
    public void Update_NonExistingAttribute_DoesNothing()
    {
        var data = new ObjectData(TemplateName, CreateDefaultAttributes());

        data.Update("NewAttr", "Value123");
        data.ApplyPatches();

        Assert.That(data["NewAttr"], Is.Null);
    }

    [Test]
    public void Update_TagName_ThrowsArgumentException()
    {
        var data = new ObjectData(TemplateName, CreateDefaultAttributes());

        Assert.Throws<ArgumentException>(() => data.Update(":Tagname", "NewTag"));
    }

    [Test]
    public void Replace_SpecifiedAttribute_UpdatesValue()
    {
        var data = new ObjectData(TemplateName, CreateDefaultAttributes());

        data.Replace("Centrifugal", "Positive Displacement", "Description");
        data.ApplyPatches();

        Assert.That(data["Description"], Is.EqualTo("Positive Displacement Pump"));
    }

    [Test]
    public void Diffs_ReturnsFormattedStrings()
    {
        var data = new ObjectData(TemplateName, CreateDefaultAttributes());

        data.Update("Description", "New Pump");
        data.Update("HiHi", "150.0");

        var diffs = data.Diffs().ToList();

        Assert.That(diffs, Has.Count.EqualTo(2));
        Assert.That(diffs[0], Is.EqualTo($"{TagName}: 'Description' \"Centrifugal Pump\" -> \"New Pump\""));
        Assert.That(diffs[1], Is.EqualTo($"{TagName}: 'HiHi' \"100\" -> \"150\""));
    }

    [Test]
    public void ApplyPatches_ClearsPatches()
    {
        var data = new ObjectData(TemplateName, CreateDefaultAttributes());

        data.Update("Description", "New Pump");
        Assert.That(data.Diffs(), Is.Not.Empty);

        data.ApplyPatches();
        Assert.That(data.Diffs(), Is.Empty);
    }
}