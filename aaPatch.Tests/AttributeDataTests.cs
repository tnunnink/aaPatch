using aaPatch.Model;

namespace aaPatch.Tests;

[TestFixture]
public class AttributeDataTests
{
    [Test]
    public void Constructor_InvalidHeader_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _ = new AttributeData("", "value"));
    }

    [Test]
    public void Constructor_SimpleString_ParsesCorrectly()
    {
        var attr = new AttributeData("Description", "Pump");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(attr.Name, Is.EqualTo("Description"));
            Assert.That(attr.Type, Is.EqualTo(typeof(string)));
            Assert.That(attr.Value, Is.EqualTo("Pump"));
            Assert.That(attr.Header, Is.EqualTo("Description"));
            Assert.That(attr.IsIdentity, Is.False);
        }
    }

    [Test]
    public void Constructor_MxDouble_ParsesCorrectly()
    {
        var attr = new AttributeData("Level(MxDouble)", "55.5");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(attr.Name, Is.EqualTo("Level"));
            Assert.That(attr.Type, Is.EqualTo(typeof(double)));
            Assert.That(attr.Value, Is.EqualTo(55.5));
            Assert.That(attr.Header, Is.EqualTo("Level(MxDouble)"));
            Assert.That(attr.IsIdentity, Is.False);
        }
    }

    [Test]
    public void Constructor_MxBoolean_ParsesCorrectly()
    {
        var attr = new AttributeData("Status(MxBoolean)", "true");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(attr.Name, Is.EqualTo("Status"));
            Assert.That(attr.Type, Is.EqualTo(typeof(bool)));
            Assert.That(attr.Value, Is.EqualTo(true));
            Assert.That(attr.Header, Is.EqualTo("Status(MxBoolean)"));
            Assert.That(attr.IsIdentity, Is.False);
        }
    }

    [Test]
    public void Constructor_MxInteger_ParsesCorrectly()
    {
        var attr = new AttributeData("Count(MxInteger)", "10");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(attr.Name, Is.EqualTo("Count"));
            Assert.That(attr.Type, Is.EqualTo(typeof(int)));
            Assert.That(attr.Value, Is.EqualTo(10));
            Assert.That(attr.Header, Is.EqualTo("Count(MxInteger)"));
            Assert.That(attr.IsIdentity, Is.False);
        }
    }

    [Test]
    public void Constructor_MxFloat_ParsesCorrectly()
    {
        var attr = new AttributeData("Value(MxFloat)", "1.5");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(attr.Name, Is.EqualTo("Value"));
            Assert.That(attr.Type, Is.EqualTo(typeof(float)));
            Assert.That(attr.Value, Is.EqualTo(1.5f));
            Assert.That(attr.Header, Is.EqualTo("Value(MxFloat)"));
            Assert.That(attr.IsIdentity, Is.False);
        }
    }

    [Test]
    public void Constructor_UnknownType_DefaultsToString()
    {
        var attr = new AttributeData("Something(UnknownType)", "value");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(attr.Name, Is.EqualTo("Something"));
            Assert.That(attr.Type, Is.EqualTo(typeof(string)));
            Assert.That(attr.Value, Is.EqualTo("value"));
            Assert.That(attr.Header, Is.EqualTo("Something(UnknownType)"));
            Assert.That(attr.IsIdentity, Is.False);
        }
    }

    [Test]
    public void Constructor_TemplateIdentity_SetsIsIdentityTrue()
    {
        var attr = new AttributeData("Template", "$Pump");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(attr.Name, Is.EqualTo("Template"));
            Assert.That(attr.IsIdentity, Is.True);
        }
    }

    [Test]
    public void Constructor_TagNameIdentity_SetsIsIdentityTrue()
    {
        var attr = new AttributeData("TagName", "P_101");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(attr.Name, Is.EqualTo("TagName"));
            Assert.That(attr.IsIdentity, Is.True);
        }
    }

    [Test]
    public void IsIdentity_CaseInsensitive_ReturnsTrue()
    {
        var attr = new AttributeData("template", "$Pump");
        Assert.That(attr.IsIdentity, Is.True);
    }

    [Test]
    public void Rename_PreservesTypeAndValue()
    {
        var attr = new AttributeData("Level(MxDouble)", "55.5");
        var renamed = attr.Rename("NewLevel");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(renamed.Name, Is.EqualTo("NewLevel"));
            Assert.That(renamed.Type, Is.EqualTo(typeof(double)));
            Assert.That(renamed.Value, Is.EqualTo(55.5));
            Assert.That(renamed.Header, Is.EqualTo("NewLevel(MxDouble)"));
        }
    }

    [Test]
    public void Update_WhenCalled_SameInstanceNewValue()
    {
        var original = new AttributeData("Level(MxDouble)", "55.5");

        var updated = original.Update("60.0");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(updated.Header, Is.EqualTo(original.Header));
            Assert.That(updated.Value, Is.EqualTo(60.0));
            Assert.AreSame(original, updated);
        }
    }

    [Test]
    public void ToString_Boolean_ReturnsLowercase()
    {
        var attrTrue = new AttributeData("Status(MxBoolean)", "true");
        var attrFalse = new AttributeData("Status(MxBoolean)", "false");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(attrTrue.ToString(), Is.EqualTo("true"));
            Assert.That(attrFalse.ToString(), Is.EqualTo("false"));
        }
    }

    [Test]
    public void ToString_NullValue_ReturnsEmptyString()
    {
        var attr = new AttributeData("Description", null);
        Assert.That(attr.ToString(), Is.EqualTo(string.Empty));
    }
}