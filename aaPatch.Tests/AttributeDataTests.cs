namespace aaPatch.Tests;

[TestFixture]
public class AttributeDataTests
{
    [Test]
    public void Constructor_SimpleString_ParsesCorrectly()
    {
        var attr = new AttributeData("Description", "Pump");
        Assert.That(attr.Name, Is.EqualTo("Description"));
        Assert.That(attr.Value, Is.EqualTo("Pump"));
        Assert.That(attr.Header, Is.EqualTo("Description"));
    }

    [Test]
    public void Constructor_MxDouble_ParsesCorrectly()
    {
        var attr = new AttributeData("Level(MxDouble)", "55.5");
        Assert.That(attr.Name, Is.EqualTo("Level"));
        Assert.That(attr.Value, Is.EqualTo(55.5));
    }

    [Test]
    public void Constructor_MxBoolean_ParsesCorrectly()
    {
        var attr = new AttributeData("Status(MxBoolean)", "true");
        Assert.That(attr.Name, Is.EqualTo("Status"));
        Assert.That(attr.Value, Is.EqualTo(true));
    }

    [Test]
    public void Constructor_MxInteger_ParsesCorrectly()
    {
        var attr = new AttributeData("Count(MxInteger)", "10");
        Assert.That(attr.Name, Is.EqualTo("Count"));
        Assert.That(attr.Value, Is.EqualTo(10));
    }

    [Test]
    public void With_CreatesNewInstanceWithSameHeader()
    {
        var original = new AttributeData("Level(MxDouble)", "55.5");
        var updated = original.With("60.0");

        Assert.That(updated.Header, Is.EqualTo(original.Header));
        Assert.That(updated.Value, Is.EqualTo(60.0));
        Assert.AreNotSame(original, updated);
    }

    [Test]
    public void ToString_Boolean_ReturnsLowercase()
    {
        var attrTrue = new AttributeData("Status(MxBoolean)", "true");
        var attrFalse = new AttributeData("Status(MxBoolean)", "false");

        Assert.That(attrTrue.ToString(), Is.EqualTo("true"));
        Assert.That(attrFalse.ToString(), Is.EqualTo("false"));
    }

    [Test]
    public void ToString_NullValue_ReturnsEmptyString()
    {
        var attr = new AttributeData("Description", null);
        Assert.That(attr.ToString(), Is.EqualTo(string.Empty));
    }

    [Test]
    public void Constructor_InvalidHeader_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _ = new AttributeData("", "value"));
    }
}
