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
    [TestCase("{Name} == \"John\"", true)]
    [TestCase("{Name} != \"John\"", false)]
    [TestCase("{Age} > 20", true)]
    [TestCase("{Age} < 30", true)]
    [TestCase("{Age} >= 25", true)]
    [TestCase("{Age} <= 25", true)]
    [TestCase("{IsActive} == true", true)]
    [TestCase("{IsActive} == false", false)]
    [TestCase("{Nullable} == null", true)]
    [TestCase("{Nullable} != null", false)]
    [TestCase("{Age} == 25 && {Name} == \"John\"", true)]
    [TestCase("{Age} == 25 && {Name} == \"Doe\"", false)]
    [TestCase("{Age} == 25 || {Name} == \"Doe\"", true)]
    [TestCase("!{IsActive}", false)]
    [TestCase("({Age} > 20 && {Age} < 30) || {Name} == \"Doe\"", true)]
    public void Compile_ComplexExpressions_ShouldEvaluateCorrectly(string expr, bool expected)
    {
        var data = new ObjectData([
            new AttributeData("Age", 25),
            new AttributeData("Name", "John"),
            new AttributeData("IsActive", true),
            new AttributeData("Nullable", null)
        ]);
        var expression = new ObjectExpression(expr);

        var predicate = expression.Compile<ObjectData, bool>();

        predicate(data).Should().Be(expected);
    }
}