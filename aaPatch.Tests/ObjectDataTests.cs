namespace aaPatch.Tests;

[TestFixture]
public class ObjectDataTests
{
    private const string TemplateName = "$Pump";
    private const string TagName = "P_101";

    private static Dictionary<string, string?> CreateDefaultAttributes() => new()
    {
        { ":Tagname", TagName },
        { "Description", "Centrifugal Pump" },
        { "HiHi", "100.0" }
    };

    [Test]
    public void Constructor_ValidInput_InitializesProperties()
    {
        var attributes = CreateDefaultAttributes();

        var obj = new ObjectData(TemplateName, attributes);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(obj.Template, Is.EqualTo(TemplateName));
            Assert.That(obj.TagName, Is.EqualTo(TagName));
            Assert.That(obj.Attributes, Has.Member("Description"));
            Assert.That(obj.Values, Has.Member("Centrifugal Pump"));
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
        var attributes = new Dictionary<string, string?> { { "Description", "No Tagname Here" } };
        var obj = new ObjectData(TemplateName, attributes);
        Assert.Throws<InvalidOperationException>(() => _ = obj.TagName);
    }

    [Test]
    public void GetValue_ExistingAttribute_ReturnsValue()
    {
        var obj = new ObjectData(TemplateName, CreateDefaultAttributes());

        var value = obj.GetValue("Description");

        Assert.That(value, Is.EqualTo("Centrifugal Pump"));
    }

    [Test]
    public void GetValue_NonExistingAttribute_ThrowsKeyNotFoundException()
    {
        var obj = new ObjectData(TemplateName, CreateDefaultAttributes());

        Assert.Throws<KeyNotFoundException>(() => obj.GetValue("NonExistent"));
    }

    [Test]
    public void TryGetValue_ExistingAttribute_ReturnsTrueAndValue()
    {
        var obj = new ObjectData(TemplateName, CreateDefaultAttributes());

        var success = obj.TryGetValue("HiHi", out var value);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(success, Is.True);
            Assert.That(value, Is.EqualTo("100.0"));
        }
    }

    [Test]
    public void TryGetValue_NonExistingAttribute_ReturnsFalse()
    {
        var obj = new ObjectData(TemplateName, CreateDefaultAttributes());

        var success = obj.TryGetValue("NonExistent", out _);

        Assert.That(success, Is.False);
    }

    [Test]
    public void Patch_Assignment_UpdatesValue()
    {
        var obj = new ObjectData(TemplateName, CreateDefaultAttributes());

        obj.Patch("HiHi", "120.0");

        Assert.That(obj.GetValue("HiHi"), Is.EqualTo("120.0"));
    }

    [Test]
    public void Patch_NewAttribute_AddsValue()
    {
        var obj = new ObjectData(TemplateName, CreateDefaultAttributes());

        obj.Patch("NewAttr", "Value123");

        Assert.That(obj.GetValue("NewAttr"), Is.EqualTo("Value123"));
    }

    [Test]
    public void Patch_TagName_ThrowsArgumentException()
    {
        var obj = new ObjectData(TemplateName, CreateDefaultAttributes());

        Assert.Throws<ArgumentException>(() => obj.Patch(":Tagname", "NewTag"));
    }

    [Test]
    public void Patch_FindReplace_UpdatesValue()
    {
        var obj = new ObjectData(TemplateName, CreateDefaultAttributes());

        obj.Patch("Description", "Centrifugal", "Positive Displacement");

        Assert.That(obj.GetValue("Description"), Is.EqualTo("Positive Displacement Pump"));
    }

    [Test]
    public void Patch_Func_UpdatesAllValues()
    {
        var obj = new ObjectData(TemplateName, CreateDefaultAttributes());

        obj.Patch((_, val) => val?.ToUpper());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(obj.GetValue("Description"), Is.EqualTo("CENTRIFUGAL PUMP"));
            Assert.That(obj.TagName, Is.EqualTo("P_101")); // Tagname is also updated if not careful in func
        }
    }

    [Test]
    public void Patch_FuncWithPredicate_UpdatesSelectedValues()
    {
        var obj = new ObjectData(TemplateName, CreateDefaultAttributes());

        obj.Patch(
            (_, _) => "99.9",
            (key, _) => key == "HiHi"
        );

        using (Assert.EnterMultipleScope())
        {
            Assert.That(obj.GetValue("HiHi"), Is.EqualTo("99.9"));
            Assert.That(obj.GetValue("Description"), Is.EqualTo("Centrifugal Pump"));
        }
    }
}