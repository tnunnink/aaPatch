using aaPatch.Extensions;
using aaPatch.Model;
using FluentAssertions;

namespace aaPatch.Tests.Model;

[TestFixture]
public class ObjectExpressionTests
{
    [Test]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null!)]
    public void Construct_InvalidExpression_ShouldThrow(string text)
    {
        FluentActions.Invoking(() => _ = new ObjectExpression(text)).Should().Throw<ArgumentException>();
    }

    [Test]
    public void Construct_ValidExpression_ShouldNotBeNull()
    {
        var expression = new ObjectExpression("{Test} > 10");

        expression.Should().NotBeNull();
    }

    [Test]
    public void Compile_FilterExpressionWithReference_ShouldProduceExpectedResult()
    {
        List<ObjectData> data = [new([new AttributeData("Test", 11)])];
        var expression = new ObjectExpression("{Test} > 10");

        var predicate = expression.Compile<ObjectData, bool>();

        data.Where(predicate).Should().HaveCount(1);
    }

    [Test]
    public void Compile_ValidExpression_ShouldHaveCorrectNormalizedString()
    {
        var value = new AttributeValue("This is an old description");
        var expression = new ObjectExpression("\"This is a static value test\"");

        var selector = expression.Compile<AttributeValue, object?>();

        selector(value).Should().Be("This is a static value test");
    }

    [Test]
    public void Compile_AttributeExpression_ShouldProduceExpectedValue()
    {
        var value = new AttributeValue("Motor 103");
        var expression = new ObjectExpression("it.Replace(\"Motor\", \"Pump\")");

        var selector = expression.Compile<AttributeValue, object?>();

        selector(value).Should().Be("Pump 103");
    }

    [Test]
    [TestCase("{Age} == 25", true)]
    [TestCase("{Age} != 25", false)]
    [TestCase("{Name} == 'John'", true)]
    [TestCase("{Name} != 'John'", false)]
    [TestCase("{Age} > 20", true)]
    [TestCase("{Age} < 30", true)]
    [TestCase("{Age} >= 25", true)]
    [TestCase("{Age} <= 25", true)]
    [TestCase("{IsActive} == true", true)]
    [TestCase("{IsActive} == false", false)]
    [TestCase("{Nullable}.IsNull", true)]
    [TestCase("!{Nullable}.IsNull", false)]
    [TestCase("{Age} == 25 && {Name} == 'John'", true)]
    [TestCase("{Age} == 25 && {Name} == 'Doe'", false)]
    [TestCase("{Age} == 25 || {Name} == 'Doe'", true)]
    [TestCase("!{IsActive}", false)]
    [TestCase("({Age} > 20 && {Age} < 30) || {Name} == 'Doe'", true)]
    public void Compile_ComplexExpressions_ShouldEvaluateCorrectly(string expr, bool expected)
    {
        var expression = new ObjectExpression(expr);
        var data = new ObjectData([
            new AttributeData("Age", 25),
            new AttributeData("Name", "John"),
            new AttributeData("IsActive", true),
            new AttributeData("Nullable", AttributeValue.Null)
        ]);

        var predicate = expression.Compile<ObjectData, bool>();

        predicate(data).Should().Be(expected);
    }
    [Test]
    public void Compile_SingleQuotedWithEscapedQuote_ShouldProduceExpectedResult()
    {
        var data = new ObjectData([new AttributeData("Desc", "O'Reilly")]);
        var expression = new ObjectExpression("{Desc} == 'O''Reilly'");
        var predicate = expression.Compile<ObjectData, bool>();

        predicate(data).Should().BeTrue();
    }

    [Test]
    public void Compile_NonExistingAttributeReference_ShouldFilterObject()
    {
        List<ObjectData> data =
        [
            new([new AttributeData("Value", 123)]),
            new([new AttributeData("Desc", "This will be filtered")]),
            new([new AttributeData("Value", 456)]),
            new([new AttributeData("Value", 33)])
        ];

        var predicate = new ObjectExpression("{Value} > 100").Compile<ObjectData, bool>();

        data.Where(predicate).Should().HaveCount(2);
    }

    [Test]
    public void Compile_MissingAttributeInExpression_ShouldBeFalseInsteadOfCrash()
    {
        var data = new ObjectData([new AttributeData("Present", 100)]);
        var expr = new ObjectExpression("{Missing} > 10");
        var predicate = expr.Compile<ObjectData, bool>();

        predicate(data).Should().BeFalse();
    }
    
    [Test]
    public void Unique_NullKeys_ReturnsOriginalCollection()
    {
        List<ObjectData> data =
        [
            new([new AttributeData("Tag", "P_101")]),
            new([new AttributeData("Tag", "P_101")])
        ];

        var result = data.Unique(null).ToList();

        result.Should().HaveCount(2);
    }

    [Test]
    public void Unique_EmptyKeys_CollapsesIdenticalObjects()
    {
        List<ObjectData> data =
        [
            new([new AttributeData("Tag", "P_101"), new AttributeData("Desc", "Pump")]),
            new([new AttributeData("Tag", "P_101"), new AttributeData("Desc", "Pump")]),
            new([new AttributeData("Tag", "P_102"), new AttributeData("Desc", "Valve")])
        ];

        var result = data.Unique([]).ToList();

        result.Should().HaveCount(2);
        result[0]["Tag"].Should().Be("P_101");
        result[1]["Tag"].Should().Be("P_102");
    }

    [Test]
    public void Unique_EmptyKeys_IgnoresAttributeOrderAndCase()
    {
        List<ObjectData> data =
        [
            new([new AttributeData("Tag", "P_101"), new AttributeData("Desc", "Pump")]),
            new([new AttributeData("desc", "Pump"), new AttributeData("TAG", "P_101")])
        ];

        var result = data.Unique([]).ToList();

        result.Should().HaveCount(1);
    }

    [Test]
    public void Unique_EmptyKeys_DifferentValues_PreservesBoth()
    {
        List<ObjectData> data =
        [
            new([new AttributeData("Tag", "P_101"), new AttributeData("Desc", "Pump A")]),
            new([new AttributeData("Tag", "P_101"), new AttributeData("Desc", "Pump B")])
        ];

        var result = data.Unique([]).ToList();

        result.Should().HaveCount(2);
    }

    [Test]
    public void Unique_SingleKey_CollapsesBySpecifiedAttribute()
    {
        List<ObjectData> data =
        [
            new([new AttributeData("Tag", "P_101"), new AttributeData("Desc", "First Seen")]),
            new([new AttributeData("Tag", "P_101"), new AttributeData("Desc", "Second Seen")]),
            new([new AttributeData("Tag", "P_102"), new AttributeData("Desc", "Different")])
        ];

        var result = data.Unique(["Tag"]).ToList();

        result.Should().HaveCount(2);
        result[0]["Desc"].Should().Be("First Seen"); // Preserves first encountered instance
        result[1]["Tag"].Should().Be("P_102");
    }

    [Test]
    public void Unique_SingleKey_CaseInsensitiveMatching()
    {
        List<ObjectData> data =
        [
            new([new AttributeData("Tag", "P_101")]),
            new([new AttributeData("Tag", "p_101")])
        ];

        var result = data.Unique(["tag"]).ToList();

        result.Should().HaveCount(1);
        result[0]["Tag"].Should().Be("P_101");
    }

    [Test]
    public void Unique_MultipleKeys_CollapsesOnlyWhenAllKeysMatch()
    {
        List<ObjectData> data =
        [
            new([new AttributeData("Unit", "01"), new AttributeData("Tag", "P_101"), new AttributeData("Val", 10)]),
            new([new AttributeData("Unit", "01"), new AttributeData("Tag", "P_101"), new AttributeData("Val", 20)]), // Duplicate on Unit+Tag
            new([new AttributeData("Unit", "02"), new AttributeData("Tag", "P_101"), new AttributeData("Val", 10)])  // Different Unit
        ];

        var result = data.Unique(["Unit", "Tag"]).ToList();

        result.Should().HaveCount(2);
        result[0]["Val"].Should().Be(10);
        result[1]["Unit"].Should().Be("02");
    }

    [Test]
    public void Unique_SpecifiedKeyDoesNotExist_ThrowsInvalidOperationException()
    {
        List<ObjectData> data =
        [
            new([new AttributeData("Tag", "P_101")]),
            new([new AttributeData("Other", "Value")])
        ];

        var act = () => data.Unique(["Tag"]).ToList();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot distinct by attribute 'Tag': attribute does not exist on object.");
    }
}