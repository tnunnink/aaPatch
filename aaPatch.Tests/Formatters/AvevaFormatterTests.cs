using aaPatch.Formats;
using aaPatch.Model;

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

        var p101 = result.First(x => x.Match("P_101"));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(p101["Template"], Is.EqualTo("$Pump"));
            Assert.That(p101["Description"], Is.EqualTo("Centrifugal Pump"));
            Assert.That(p101["HiHi"], Is.EqualTo("100.0"));
        }

        var v201 = result.First(x => x.Match("V_201"));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(v201["Template"], Is.EqualTo("$Valve"));
            Assert.That(v201["OpenLimit"], Is.EqualTo("True"));
        }
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
        var dump =
            """
            :TEMPLATE=$Pump
            :Tagname,HiHi(MxFloat),OpenLimit(MxBoolean),Count(MxInteger)
            P_101,100.5,true,10
            """;

        var result = formatter.Read(dump).ToList();
        var p101 = result[0];

        Assert.Multiple(() =>
        {
            Assert.That(p101["HiHi(MxFloat)"], Is.EqualTo(100.5f));
            Assert.That(p101["OpenLimit(MxBoolean)"], Is.EqualTo(true));
            Assert.That(p101["Count(MxInteger)"], Is.EqualTo(10));
        });
    }
}