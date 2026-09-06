using aaPatch.Model;
using FluentAssertions;

namespace aaPatch.Tests.Model;

[TestFixture]
public class AttributeValueTests
{
    #region Constructor and Properties

    [Test]
    public void Constructor_ValidString_ShouldBeExpectedValue()
    {
        var value = new AttributeValue("Test");

        value.ToString().Should().Be("Test");
    }

    [Test]
    public void Constructor_WithAttributeValue_ShouldExtractUnderlyingValue()
    {
        var original = new AttributeValue(42);
        var wrapped = new AttributeValue(original);

        wrapped.Type.Should().Be<int>();
        wrapped.ToString().Should().Be("42");
    }

    [Test]
    public void Null_NullValue_ShouldBeTrue()
    {
        var attribute = AttributeValue.Null;

        attribute.GetValue().Should().Be(null);
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
    public void Type_WhenCalled_ShouldBeExpectedValue(object value, Type expected)
    {
        var attribute = new AttributeValue(value);

        var type = attribute.Type;

        type.Should().Be(expected);
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

    #endregion

    #region Conversions and Implicit Operators

    [Test]
    public void As_ConvertType_ShouldReturnNewAttributeValueWithConvertedType()
    {
        var value = new AttributeValue("123");
        var converted = value.As(typeof(int));

        converted.Type.Should().Be<int>();
        converted.ToString().Should().Be("123");
    }

    [Test]
    public void ImplicitOperator_FromDifferentTypes_ShouldCreateAttributeValue()
    {
        AttributeValue b = true;
        AttributeValue s = (short)1;
        AttributeValue i = 10;
        AttributeValue l = 100L;
        AttributeValue d = 1.5;
        AttributeValue str = "test";

        b.Type.Should().Be<bool>();
        s.Type.Should().Be<short>();
        i.Type.Should().Be<int>();
        l.Type.Should().Be<long>();
        d.Type.Should().Be<double>();
        str.Type.Should().Be<string>();
    }

    #endregion

    #region Equality and HashCode

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
    public void Equals_BothNullValues_ShouldBeTrue()
    {
        var value1 = new AttributeValue(null);
        var value2 = new AttributeValue(null);

        var result = value1.Equals(value2);

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

    #endregion

    #region Comparison Operators and CompareTo

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

    [Test]
    public void CompareTo_BothNullValues_ShouldReturnZero()
    {
        var value1 = new AttributeValue(null);
        var value2 = new AttributeValue(null);

        var result = value1.CompareTo(value2);

        result.Should().Be(0);
    }

    [Test]
    public void CompareTo_Null_ShouldReturnPositive()
    {
        var value = new AttributeValue(42);
        var result = value.CompareTo(null);
        result.Should().Be(1);
    }

    #endregion

    #region String Operations

    [Test]
    [TestCase("Hello World", "Hello", true, true)]
    [TestCase("Hello World", "hello", false, true)]
    [TestCase("Hello World", "hello", true, false)]
    [TestCase(null, "test", false, false)]
    public void Contains_WhenCalled_ShouldReturnExpectedResult(object? value, string text, bool match, bool expected)
    {
        var attribute = new AttributeValue(value);

        attribute.Contains(text, match).Should().Be(expected);
    }

    [Test]
    [TestCase("abcde", "a%e", true)]
    [TestCase("abcde", "a?c%e", true)]
    [TestCase("abcde", "a%z", false)]
    [TestCase("abcde", "bc", false)] // No wildcards, full match required
    [TestCase(null, "%", false)]
    public void Like_WhenCalled_ShouldReturnExpectedResult(object? value, string pattern, bool expected)
    {
        var attribute = new AttributeValue(value);

        attribute.Like(pattern).Should().Be(expected);
    }

    [Test]
    [TestCase("123-456", @"^\d{3}-\d{3}$", true)]
    [TestCase("abc", @"^\d+$", false)]
    [TestCase(null, ".*", false)]
    public void Matches_WhenCalled_ShouldReturnExpectedResult(object? value, string pattern, bool expected)
    {
        var attribute = new AttributeValue(value);

        var result = attribute.Matches(pattern);

        result.Should().Be(expected);
    }

    [Test]
    [TestCase("Hello World", "World", "Universe", true, "Hello Universe")]
    [TestCase("Hello World", "world", "Universe", false, "Hello Universe")]
    [TestCase("Hello World", "world", "Universe", true, "Hello World")]
    [TestCase("123", "2", "4", false, "143")]
    [TestCase(123, "2", "4", false, 143)]
    public void Replace_WhenCalled_ShouldReturnExpected(object? value, string find, string replace,
        bool match, object? expected)
    {
        var attribute = new AttributeValue(value);

        var result = attribute.Replace(find, replace, match);

        result.Should().Be(expected);
    }

    #endregion

    #region Null Semantics

    [Test]
    public void Null_Equality_ShouldBeTrue()
    {
        var attribute = AttributeValue.Null;

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        (attribute == null).Should().BeTrue();
        (null == attribute).Should().BeTrue();
        (attribute == AttributeValue.Null).Should().BeTrue();
        attribute?.Equals(null).Should().BeTrue();
        attribute?.Equals(AttributeValue.Null).Should().BeTrue();
    }

    [Test]
    public void Null_Inequality_ShouldBeFalse()
    {
        var attribute = AttributeValue.Null;
        
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        (attribute != null).Should().BeFalse();
        (null != attribute).Should().BeFalse();
        (attribute != AttributeValue.Null).Should().BeFalse();
    }

    [Test]
    [TestCase(5)]
    [TestCase(5.0)]
    [TestCase("test")]
    [TestCase(true)]
    public void Null_Comparisons_ShouldAllBeFalse(object value)
    {
        var attribute = AttributeValue.Null;
        var other = new AttributeValue(value);

        (attribute > other).Should().BeFalse("Null > value should be false");
        (attribute < other).Should().BeFalse("Null < value should be false");
        (attribute >= other).Should().BeFalse("Null >= value should be false");
        (attribute <= other).Should().BeFalse("Null <= value should be false");

        (other > attribute).Should().BeFalse("value > Null should be false");
        (other < attribute).Should().BeFalse("value < Null should be false");
        (other >= attribute).Should().BeFalse("value >= Null should be false");
        (other <= attribute).Should().BeFalse("value <= Null should be false");
    }

    [Test]
    public void Null_StringMethods_ShouldBeFalse()
    {
        var nullValue = AttributeValue.Null;

        nullValue.Contains("test").Should().BeFalse();
        nullValue.Like("%").Should().BeFalse();
        nullValue.Matches(".*").Should().BeFalse();
    }

    #endregion
}