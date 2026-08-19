using aaPatch.Model;

namespace aaPatch.Tests.Model;

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
        var attribute = new AttributeData("Description", "Pump");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(attribute.Name, Is.EqualTo("Description"));
            Assert.That(attribute.Value, Is.EqualTo("Pump"));
        }
    }

    [Test]
    public void Constructor_MxDouble_ParsesCorrectly()
    {
        var attribute = new AttributeData("Level", 55.5);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(attribute.Name, Is.EqualTo("Level"));
            Assert.That(attribute.Value, Is.EqualTo(55.5));
        }
    }

    [Test]
    public void Constructor_MxBoolean_ParsesCorrectly()
    {
        var attribute = new AttributeData("Status", true);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(attribute.Name, Is.EqualTo("Status"));
            Assert.That(attribute.Value, Is.True);
        }
    }

    [Test]
    public void Constructor_MxInteger_ParsesCorrectly()
    {
        var attribute = new AttributeData("Count", 10);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(attribute.Name, Is.EqualTo("Count"));
            Assert.That(attribute.Value, Is.EqualTo(10));
        }
    }

    [Test]
    public void Constructor_MxFloat_ParsesCorrectly()
    {
        var attribute = new AttributeData("Value", 1.5f);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(attribute.Name, Is.EqualTo("Value"));
            Assert.That(attribute.Value, Is.EqualTo(1.5f));
        }
    }

    [Test]
    public void Rename_ValidName_PreservesTypeAndValue()
    {
        var attribute = new AttributeData("Level", 55.5);

        var renamed = attribute.Rename("NewLevel");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(renamed.Name, Is.EqualTo("NewLevel"));
            Assert.That(renamed.Value, Is.EqualTo(55.5));
        }
    }

    [Test]
    public void Update_WhenCalled_SameInstanceNewValue()
    {
        // ReSharper disable once UseObjectOrCollectionInitializer
        var attribute = new AttributeData("Level", 55.5);

        attribute.Update(60.0);

        Assert.That(attribute.Value, Is.EqualTo(60.0));
    }

    [Test]
    public void ToString_Boolean_ReturnsLowercase()
    {
        var attrTrue = new AttributeData("Status", "true");
        var attrFalse = new AttributeData("Status", "false");

        using (Assert.EnterMultipleScope())
        {
            Assert.That(attrTrue.ToString(), Is.EqualTo("true"));
            Assert.That(attrFalse.ToString(), Is.EqualTo("false"));
        }
    }

    [Test]
    public void ToString_NullValue_ReturnsEmptyString()
    {
        var attribute = new AttributeData("Description", null);
        Assert.That(attribute.ToString(), Is.EqualTo(string.Empty));
    }
}