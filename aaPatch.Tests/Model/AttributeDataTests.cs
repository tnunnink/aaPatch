using aaPatch.Model;
using FluentAssertions;

namespace aaPatch.Tests.Model;

[TestFixture]
public class AttributeDataTests
{
    [Test]
    public void Constructor_InvalidName_ThrowsArgumentException()
    {
        var constructor = () => _ = new AttributeData("", "value");
        
        constructor.Should().Throw<ArgumentException>();
    }

    [Test]
    public void Constructor_ValidName_HasExpectedValues()
    {
        var attribute = new AttributeData("Description", "Pump");

        attribute.Name.Should().Be("Description");
        attribute.Value.Should().Be("Pump");
    }


    [Test]
    [TestCase(0)]
    [TestCase(123)]
    [TestCase(-123)]
    [TestCase(9876532346)]
    [TestCase(55.5)]
    [TestCase(0.000111)]
    [TestCase(true)]
    [TestCase("this is a test")]
    public void Constructor_DifferentTypedValues_HaxExpectedValue(object value)
    {
        var attribute = new AttributeData("MyValue", value);

        attribute.Value.Should().Be(value);
    }

    [Test]
    public void Constructor_MxBoolean_ParsesCorrectly()
    {
        var attribute = new AttributeData("Status", true);

        attribute.Name.Should().Be("Status");
        attribute.Value.Should().Be(true);
    }

    [Test]
    public void Constructor_MxInteger_ParsesCorrectly()
    {
        var attribute = new AttributeData("Count", 10);

        attribute.Name.Should().Be("Count");
        attribute.Value.Should().Be(10);
    }

    [Test]
    public void Constructor_MxFloat_ParsesCorrectly()
    {
        var attribute = new AttributeData("Value", 1.5f);

        attribute.Name.Should().Be("Value");
        attribute.Value.Should().Be(1.5f);
    }

    [Test]
    public void Rename_ValidName_PreservesTypeAndValue()
    {
        var attribute = new AttributeData("Level", 55.5);

        var renamed = attribute.Rename("NewLevel");

        renamed.Name.Should().Be("NewLevel");
        renamed.Value.Should().Be(55.5);
    }

    [Test]
    public void Update_WhenCalled_SameInstanceNewValue()
    {
        // ReSharper disable once UseObjectOrCollectionInitializer
        var attribute = new AttributeData("Level", 55.5);

        var updated = attribute.Update(60.0);

        updated.Value.Should().Be(60.0);
    }

    [Test]
    public void ToString_Boolean_ReturnsLowercase()
    {
        var attrTrue = new AttributeData("Status", "true");
        var attrFalse = new AttributeData("Status", "false");

        attrTrue.ToString().Should().Be("true");
        attrFalse.ToString().Should().Be("false");
    }

    [Test]
    public void ToString_NullValue_ReturnsEmptyString()
    {
        var attribute = new AttributeData("Description", null);
        attribute.ToString().Should().Be(string.Empty);
    }

    [Test]
    public void Update_StringToInt_ConvertsCorrectly()
    {
        var attribute = new AttributeData("Count", 10);

        var updated = attribute.Update("20");

        updated.Value.Should().Be(20);
    }

    [Test]
    public void Update_DoubleToInt_ConvertsCorrectly()
    {
        var attribute = new AttributeData("Count", 10);

        var updated = attribute.Update(20.5);

        updated.Value.Should().Be(20);
    }

    [Test]
    public void Update_BoolToString_ConvertsCorrectly()
    {
        var attribute = new AttributeData("Flag", "maybe");

        var updated = attribute.Update(true);

        updated.Value.Should().Be("True");
    }

    [Test]
    public void Update_InvalidFormat_ThrowsFormatException()
    {
        var attribute = new AttributeData("Count", 10);

        var act = () => attribute.Update("not a number");

        act.Should().Throw<FormatException>();
    }
}