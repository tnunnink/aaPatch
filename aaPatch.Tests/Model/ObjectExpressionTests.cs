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
}