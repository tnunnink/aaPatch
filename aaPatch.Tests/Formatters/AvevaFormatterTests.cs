using aaPatch.Formats;
using aaPatch.Model;
using FluentAssertions;

namespace aaPatch.Tests.Formatters;

[TestFixture]
public class AvevaFormatterTests
{
    private const string SimpleGalaxyDump =
        """
        :TEMPLATE=$Pump
        :Tagname,Description,HiHi
        P_101,Centrifugal Pump,100.0
        P_102,Vacuum Pump,80.0

        :TEMPLATE=$Valve
        :Tagname,Description,OpenLimit
        V_201,Gate Valve,True
        """;

    [Test]
    public void Read_ValidText_HasExpectedObjectCount()
    {
        var formatter = new AvevaFormatter();

        var result = formatter.Read(SimpleGalaxyDump).ToList();

        Assert.That(result, Has.Count.EqualTo(3));
    }

    [Test]
    public void Read_ValidText_HasExpectedObjectData()
    {
        var formatter = new AvevaFormatter();

        var result = formatter.Read(SimpleGalaxyDump).ToList();

        var p101 = result.First(x => x["TagName"] == "P_101");
        p101["Template"].Should().Be("$Pump");
        p101["Description"].Should().Be("Centrifugal Pump");
        p101["HiHi"].Should().Be("100.0");

        var v201 = result.First(x => x["TagName"] == "V_201");
        v201["Template"].Should().Be("$Valve");
        v201["OpenLimit"].Should().Be("True");
    }

    [Test]
    public async Task Write_ValidObjects_MatchesVerified()
    {
        var formatter = new AvevaFormatter();

        var objects = new List<ObjectData>
        {
            new(new List<AttributeData>
            {
                new("Template", "$Pump"),
                new("TagName", "P_101"),
                new("Description", "Pump 1")
            }),
            new(new List<AttributeData>
            {
                new("Template", "$Valve"),
                new("TagName", "V_101"),
                new("Description", "Valve 1")
            })
        };

        var result = formatter.Write(objects);

        await Verify(result);
    }

    [Test]
    public void Read_EmptyText_ThrowsArgumentException()
    {
        var formatter = new AvevaFormatter();

        Assert.Throws<ArgumentException>(() => formatter.Read(""));
        Assert.Throws<ArgumentException>(() => formatter.Read("   "));
    }

    [Test]
    public void Read_WithTypedHeaders_ParsesValuesCorrectly()
    {
        var formatter = new AvevaFormatter();
        const string dump =
            """
            :TEMPLATE=$Pump
            :Tagname,HiHi(MxFloat),OpenLimit(MxBoolean),Count(MxInteger),Level(MxDouble)
            P_101,100.5,true,10,55.55
            """;

        var result = formatter.Read(dump).ToList()[0];
        result["HiHi(MxFloat)"].Should().Be(100.5f);
        result["OpenLimit(MxBoolean)"].Should().Be(true);
        result["Count(MxInteger)"].Should().Be(10);
        result["Level(MxDouble)"].Should().Be(55.55);
    }

    [Test]
    public async Task Write_WithTypedValues_MatchesVerified()
    {
        var formatter = new AvevaFormatter();
        var objects = new List<ObjectData>
        {
            new(new List<AttributeData>
            {
                new("Template", "$Pump"),
                new("TagName", "P_101"),
                new("HiHi(MxFloat)", 100.5f),
                new("Count(MxInteger)", 10),
                new("Enabled(MxBoolean)", true),
                new("Level(MxDouble)", 55.55)
            })
        };

        var result = formatter.Write(objects);

        await Verify(result);
    }
}