using aaPatch.Model;
using FluentAssertions;

namespace aaPatch.Tests.Model;

[TestFixture]
public class AttributeValueTests
{
    [Test]
    public void Constructor_ValidString_ShouldBeExpectedValue()
    {
        var value = new AttributeValue("Test");

        value.ToString().Should().Be("Test");
    }

    [Test]
    [TestCase("", "")]
    [TestCase("   ", "   ")]
    [TestCase(true, "true")]
    [TestCase(false, "false")]
    [TestCase(0, "0")]
    [TestCase(1234, "1234")]
    [TestCase(999999999999, "999999999999")]
    [TestCase(123.456, "123.456")]
    [TestCase("This is a test value", "This is a test value")]
    public void ToString_WhenCalled_ShouldBeExpectedValue(object value, string expected)
    {
        var attribute = new AttributeValue(value);

        var result = attribute.ToString();

        result.Should().Be(expected);
    }

    [Test]
    public void ToString_NullValue_ShouldBeEmptyString()
    {
        var attribute = new AttributeValue(null!);

        var result = attribute.ToString();

        result.Should().BeEmpty();
    }
    
    [Test]
    public void IsNull_NullValue_ShouldBeTrue()
    {
        var attribute = new AttributeValue(null!);

        var result = attribute.IsNull;

        result.Should().BeTrue();
    }
    
    [Test]
    public void IsNull_NonNullValue_ShouldBeFalse()
    {
        var attribute = new AttributeValue(123);

        var result = attribute.IsNull;

        result.Should().BeFalse();
    }
    
    [Test]
    [TestCase("", typeof(string))]
    [TestCase("   ", typeof(string))]
    [TestCase(true, typeof(bool))]
    [TestCase(false, typeof(bool))]
    [TestCase(0, typeof(int))]
    [TestCase(1234, typeof(int))]
    [TestCase(-1234, typeof(int))]
    [TestCase(999999999999, typeof(long))]
    [TestCase(123.456, typeof(double))]
    [TestCase("This is a test value", typeof(string))]
    public void ValueType_WhenCalled_ShouldBeExpectedValue(object value, Type expected)
    {
        var attribute = new AttributeValue(value);

        var type = attribute.ValueType;

        type.Should().Be(expected);
    }
    
    [Test]
    public void EqualityOperator_EqualStringValues_ShouldBeTrue()
    {
        var value1 = new AttributeValue("test");
        var value2 = new AttributeValue("test");

        var result = value1 == value2;

        result.Should().BeTrue();
    }

    [Test]
    public void EqualityOperator_UnequalStringValues_ShouldBeFalse()
    {
        var value1 = new AttributeValue("test1");
        var value2 = new AttributeValue("test2");

        var result = value1 == value2;

        result.Should().BeFalse();
    }

    [Test]
    public void EqualityOperator_EqualBooleanValues_ShouldBeTrue()
    {
        var value1 = new AttributeValue(true);
        var value2 = new AttributeValue(true);

        var result = value1 == value2;

        result.Should().BeTrue();
    }

    [Test]
    public void EqualityOperator_UnequalBooleanValues_ShouldBeFalse()
    {
        var value1 = new AttributeValue(true);
        var value2 = new AttributeValue(false);

        var result = value1 == value2;

        result.Should().BeFalse();
    }

    [Test]
    public void EqualityOperator_EqualIntegerValues_ShouldBeTrue()
    {
        var value1 = new AttributeValue(42);
        var value2 = new AttributeValue(42);

        var result = value1 == value2;

        result.Should().BeTrue();
    }

    [Test]
    public void EqualityOperator_UnequalIntegerValues_ShouldBeFalse()
    {
        var value1 = new AttributeValue(42);
        var value2 = new AttributeValue(100);

        var result = value1 == value2;

        result.Should().BeFalse();
    }

    [Test]
    public void EqualityOperator_EqualLongValues_ShouldBeTrue()
    {
        var value1 = new AttributeValue(999999999999L);
        var value2 = new AttributeValue(999999999999L);

        var result = value1 == value2;

        result.Should().BeTrue();
    }

    [Test]
    public void EqualityOperator_UnequalLongValues_ShouldBeFalse()
    {
        var value1 = new AttributeValue(999999999999L);
        var value2 = new AttributeValue(888888888888L);

        var result = value1 == value2;

        result.Should().BeFalse();
    }

    [Test]
    public void EqualityOperator_EqualDoubleValues_ShouldBeTrue()
    {
        var value1 = new AttributeValue(123.456);
        var value2 = new AttributeValue(123.456);

        var result = value1 == value2;

        result.Should().BeTrue();
    }

    [Test]
    public void EqualityOperator_UnequalDoubleValues_ShouldBeFalse()
    {
        var value1 = new AttributeValue(123.456);
        var value2 = new AttributeValue(789.012);

        var result = value1 == value2;

        result.Should().BeFalse();
    }

    [Test]
    public void InequalityOperator_EqualValues_ShouldBeFalse()
    {
        var value1 = new AttributeValue(42);
        var value2 = new AttributeValue(42);

        var result = value1 != value2;

        result.Should().BeFalse();
    }

    [Test]
    public void InequalityOperator_UnequalValues_ShouldBeTrue()
    {
        var value1 = new AttributeValue(42);
        var value2 = new AttributeValue(100);

        var result = value1 != value2;

        result.Should().BeTrue();
    }

    [Test]
    public void Equals_EqualValues_ShouldBeTrue()
    {
        var value1 = new AttributeValue("test");
        var value2 = new AttributeValue("test");

        var result = value1.Equals(value2);

        result.Should().BeTrue();
    }

    [Test]
    public void Equals_UnequalValues_ShouldBeFalse()
    {
        var value1 = new AttributeValue("test1");
        var value2 = new AttributeValue("test2");

        var result = value1.Equals(value2);

        result.Should().BeFalse();
    }

    [Test]
    public void Equals_NullValue_ShouldBeFalse()
    {
        var value1 = new AttributeValue("test");

        var result = value1.Equals(null);

        result.Should().BeFalse();
    }

    [Test]
    public void Equals_EqualValueStringVsNumeric_ShouldBeTrue()
    {
        var left = new AttributeValue("10.5");
        var right = new AttributeValue(10.5);

        var result = left.Equals(right);

        result.Should().BeTrue();
    }
    
    [Test]
    public void Equals_NotEqualValueStringVsNumeric_ShouldBeFalse()
    {
        var left = new AttributeValue("10.5");
        var right = new AttributeValue(10.56);

        var result = left.Equals(right);

        result.Should().BeFalse();
    }
    
    [Test]
    public void Equals_EqualValueStringVsBoolean_ShouldBeTrue()
    {
        var left = new AttributeValue("true");
        var right = new AttributeValue(true);

        var result = left.Equals(right);

        result.Should().BeTrue();
    }
    
    [Test]
    public void Equals_NotEqualValueStringVsBoolean_ShouldBeFalse()
    {
        var left = new AttributeValue("true");
        var right = new AttributeValue(false);

        var result = left.Equals(right);

        result.Should().BeFalse();
    }

    [Test]
    public void Equals_EqualDifferentNumericTypes_ShouldBeTrue()
    {
        var left = new AttributeValue(20.0);
        var right = new AttributeValue(20);

        var result = left.Equals(right);

        result.Should().BeTrue();
    }
    
    [Test]
    public void Equals_NotEqualDifferentNumericTypes_ShouldBeFalse()
    {
        var left = new AttributeValue(20.123);
        var right = new AttributeValue(20);

        var result = left.Equals(right);

        result.Should().BeFalse();
    }

    [Test]
    public void GetHashCode_EqualValues_ShouldReturnSameHashCode()
    {
        var value1 = new AttributeValue(42);
        var value2 = new AttributeValue(42);

        var hash1 = value1.GetHashCode();
        var hash2 = value2.GetHashCode();

        hash1.Should().Be(hash2);
    }

    [Test]
    public void LessThanOperator_SmallerValue_ShouldBeTrue()
    {
        var value1 = new AttributeValue(10);
        var value2 = new AttributeValue(20);

        var result = value1 < value2;

        result.Should().BeTrue();
    }

    [Test]
    public void LessThanOperator_EqualValues_ShouldBeFalse()
    {
        var value1 = new AttributeValue(20);
        var value2 = new AttributeValue(20);

        var result = value1 < value2;

        result.Should().BeFalse();
    }

    [Test]
    public void LessThanOperator_LargerValue_ShouldBeFalse()
    {
        var value1 = new AttributeValue(30);
        var value2 = new AttributeValue(20);

        var result = value1 < value2;

        result.Should().BeFalse();
    }

    [Test]
    public void LessThanOrEqualOperator_SmallerValue_ShouldBeTrue()
    {
        var value1 = new AttributeValue(10);
        var value2 = new AttributeValue(20);

        var result = value1 <= value2;

        result.Should().BeTrue();
    }

    [Test]
    public void LessThanOrEqualOperator_EqualValues_ShouldBeTrue()
    {
        var value1 = new AttributeValue(20);
        var value2 = new AttributeValue(20);

        var result = value1 <= value2;

        result.Should().BeTrue();
    }

    [Test]
    public void LessThanOrEqualOperator_LargerValue_ShouldBeFalse()
    {
        var value1 = new AttributeValue(30);
        var value2 = new AttributeValue(20);

        var result = value1 <= value2;

        result.Should().BeFalse();
    }

    [Test]
    public void GreaterThanOperator_LargerValue_ShouldBeTrue()
    {
        var value1 = new AttributeValue(30);
        var value2 = new AttributeValue(20);

        var result = value1 > value2;

        result.Should().BeTrue();
    }

    [Test]
    public void GreaterThanOperator_EqualValues_ShouldBeFalse()
    {
        var value1 = new AttributeValue(20);
        var value2 = new AttributeValue(20);

        var result = value1 > value2;

        result.Should().BeFalse();
    }

    [Test]
    public void GreaterThanOperator_SmallerValue_ShouldBeFalse()
    {
        var value1 = new AttributeValue(10);
        var value2 = new AttributeValue(20);

        var result = value1 > value2;

        result.Should().BeFalse();
    }

    [Test]
    public void GreaterThanOrEqualOperator_LargerValue_ShouldBeTrue()
    {
        var value1 = new AttributeValue(30);
        var value2 = new AttributeValue(20);

        var result = value1 >= value2;

        result.Should().BeTrue();
    }

    [Test]
    public void GreaterThanOrEqualOperator_EqualValues_ShouldBeTrue()
    {
        var value1 = new AttributeValue(20);
        var value2 = new AttributeValue(20);

        var result = value1 >= value2;

        result.Should().BeTrue();
    }

    [Test]
    public void GreaterThanOrEqualOperator_SmallerValue_ShouldBeFalse()
    {
        var value1 = new AttributeValue(10);
        var value2 = new AttributeValue(20);

        var result = value1 >= value2;

        result.Should().BeFalse();
    }

    [Test]
    public void ComparisonOperators_DifferentTypesThatCanBeConverted_ShouldWorkCorrectly()
    {
        var value1 = new AttributeValue(10.5);
        var value2 = new AttributeValue(20);

        var result = value1 < value2;

        result.Should().BeTrue();
    }
    
    [Test]
    public void ComparisonOperators_StringNumericType_ShouldWorkCorrectly()
    {
        var value1 = new AttributeValue("10.5");
        var value2 = new AttributeValue(12.0);

        var result = value1 < value2;

        result.Should().BeTrue();
    }
    
    [Test]
    public void ComparisonOperators_NonNumericStringComparedToNumber_ThrowsException()
    {
        var value1 = new AttributeValue(10.5);
        var value2 = new AttributeValue("test");

        FluentActions.Invoking(() => _ = value1 < value2).Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Values of types 'Double' and 'String' cannot be compared.");
    }

    [Test]
    public void CompareTo_EqualValues_ShouldReturnZero()
    {
        var value1 = new AttributeValue(42);
        var value2 = new AttributeValue(42);

        var result = value1.CompareTo(value2);

        result.Should().Be(0);
    }

    [Test]
    public void CompareTo_SmallerValue_ShouldReturnNegative()
    {
        var value1 = new AttributeValue(10);
        var value2 = new AttributeValue(20);

        var result = value1.CompareTo(value2);

        result.Should().BeNegative();
    }

    [Test]
    public void CompareTo_LargerValue_ShouldReturnPositive()
    {
        var value1 = new AttributeValue(30);
        var value2 = new AttributeValue(20);

        var result = value1.CompareTo(value2);

        result.Should().BePositive();
    }
    
}