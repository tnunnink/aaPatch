using aaPatch.Model;
using FluentAssertions;

namespace aaPatch.Tests.Model;

[TestFixture]
public class ObjectPatchTests
{
    [Test]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase(null!)]
    public void Construct_InvalidInput_ShouldThrow(string input)
    {
        FluentActions.Invoking(() => _ = new ObjectPatch(input)).Should().Throw<ArgumentException>();
    }

    [Test]
    public void Construct_TargetedPatchWithoutBraces_ShouldThrow()
    {
        FluentActions.Invoking(() => _ = new ObjectPatch("Target := 10")).Should().Throw<ArgumentException>()
            .WithMessage("A targeted patch must use '{Attribute} := Expression'.");
    }

    [Test]
    public void Construct_EmptyExpression_ShouldThrow()
    {
        FluentActions.Invoking(() => _ = new ObjectPatch("{Target} := ")).Should().Throw<ArgumentException>()
            .WithMessage("Patch expression cannot be empty.");
    }

    [Test]
    public void Apply_GlobalPatch_ShouldUpdateAnyAttribute()
    {
        var patch = new ObjectPatch("it + \"_updated\"");
        var attribute = new AttributeData("Desc", "Old");

        var result = patch.Apply(attribute);

        result.Value.Should().Be("Old_updated");
    }

    [Test]
    public void Apply_TargetedPatch_MatchingName_ShouldUpdate()
    {
        var patch = new ObjectPatch("{Desc} := it + \"_updated\"");
        var attribute = new AttributeData("Desc", "Old");

        var result = patch.Apply(attribute);

        result.Value.Should().Be("Old_updated");
    }

    [Test]
    public void Apply_TargetedPatch_NonMatchingName_ShouldNotUpdate()
    {
        var patch = new ObjectPatch("{Other} := it + \"_updated\"");
        var attribute = new AttributeData("Desc", "Old");

        var result = patch.Apply(attribute);

        result.Value.Should().Be("Old");
        result.Should().NotBeSameAs(attribute); // It calls Duplicate()
    }

    [Test]
    public void Apply_TargetedPatch_CaseInsensitiveMatch_ShouldUpdate()
    {
        var patch = new ObjectPatch("{DESC} := it + \"_updated\"");
        var attribute = new AttributeData("Desc", "Old");

        var result = patch.Apply(attribute);

        result.Value.Should().Be("Old_updated");
    }

    [Test]
    public void ImplicitConversion_ShouldWork()
    {
        ObjectPatch patch = "{Desc} := \"New\"";
        var attribute = new AttributeData("Desc", "Old");

        var result = patch.Apply(attribute);

        result.Value.Should().Be("New");
    }
}
