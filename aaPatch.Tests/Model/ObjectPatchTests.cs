using aaPatch.Extensions;
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
        var patch = new ObjectPatch("it.Replace(\"Old\", \"New\")");
        var instance = new ObjectData(
        [
            new AttributeData("Desc", "Old text"),
            new AttributeData("InputSource", "Old source")
        ]);

        var result = patch.Apply(instance);

        result["Desc"].Should().Be("New text");
        result["InputSource"].Should().Be("New source");
    }

    [Test]
    public void Apply_TargetedPatch_MatchingName_ShouldUpdate()
    {
        var patch = new ObjectPatch("{Desc} := {Desc} + \"_updated\"");
        var instance = new ObjectData([new AttributeData("Desc", "Old")]);

        var result = patch.Apply(instance);

        result["Desc"].Should().Be("Old_updated");
    }

    [Test]
    public void Apply_TargetedPatch_MissingName_ShouldAddAttribute()
    {
        var patch = new ObjectPatch("{Other} := {Desc} + \"_updated\"");
        var instance = new ObjectData([new AttributeData("Desc", "Old")]);

        var result = patch.Apply(instance);

        result.Attributes.Should().ContainSingle(a => a.Name == "Other").Which.Value.Should().Be("Old_updated");
        result.Attributes.Should().ContainSingle(a => a.Name == "Desc").Which.Value.Should().Be("Old");
    }

    [Test]
    public void Apply_TargetedPatch_CaseInsensitiveMatch_ShouldUpdate()
    {
        var patch = new ObjectPatch("{DESC} := {Desc} + \"_updated\"");
        var instance = new ObjectData([new AttributeData("Desc", "Old")]);

        var result = patch.Apply(instance);

        result.Attributes.Single().Value.Should().Be("Old_updated");
    }

    [Test]
    public void Apply_TargetedPatch_CrossFieldReference_ShouldUpdate()
    {
        var patch = new ObjectPatch("{InputSource} := \"PLC_\" + {TagName} + \".PV\"");
        var instance = new ObjectData(
        [
            new AttributeData("TagName", "P_101"),
            new AttributeData("InputSource", "Old")
        ]);

        var result = patch.Apply(instance);

        result.Attributes.Single(a => a.Name == "InputSource").Value.Should().Be("PLC_P_101.PV");
    }

    [Test]
    public void Apply_TargetedPatch_SelfReferentialReplace_ShouldUpdate()
    {
        var patch = new ObjectPatch("{Description} := {Description}.Replace(\"Pump\", \"Motor\")");
        var instance = new ObjectData([new AttributeData("Description", "Centrifugal Pump")]);

        var result = patch.Apply(instance);

        result.Attributes.Single().Value.Should().Be("Centrifugal Motor");
    }

    [Test]
    public void Apply_SequentialPatches_ShouldAccumulateChanges()
    {
        var instance = new ObjectData(
        [
            new AttributeData("TagName", "P_101"),
            new AttributeData("Description", "Pump")
        ]);
        ObjectPatch[] patches =
        [
            new("{Description} := {Description} + \" updated\""),
            new("{TagName} := \"P_102\""),
            new("{InputSource} := \"PLC_\" + {TagName} + \".\" + {Description}")
        ];

        var result = new[] { instance }.Patch(patches).Single();

        result.Attributes.Single(a => a.Name == "Description").Value.Should().Be("Pump updated");
        result.Attributes.Single(a => a.Name == "TagName").Value.Should().Be("P_102");
        result.Attributes.Single(a => a.Name == "InputSource").Value.Should().Be("PLC_P_102.Pump updated");
    }

    [Test]
    public void ImplicitConversion_ShouldWork()
    {
        ObjectPatch patch = "{Desc} := \"New\"";
        var instance = new ObjectData([new AttributeData("Desc", "Old")]);

        var result = patch.Apply(instance);

        result.Attributes.Single().Value.Should().Be("New");
    }
}
