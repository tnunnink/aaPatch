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
    public void Write_ValidObjects_ReturnsExpectedFormat()
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

        Assert.That(result, Does.StartWith(":TEMPLATE=$Pump"));
        Assert.That(result, Does.Contain(":Tagname,Description"));
        Assert.That(result, Does.Contain("P_101,Pump 1"));
        Assert.That(result, Does.Contain(":TEMPLATE=$Valve"));
        Assert.That(result, Does.Contain("V_101,Valve 1"));
    }

    [Test]
    public void Read_EmptyText_ThrowsArgumentException()
    {
        var formatter = new AvevaFormatter();

        Assert.Throws<ArgumentException>(() => formatter.Read(""));
        Assert.Throws<ArgumentException>(() => formatter.Read("   "));
    }
}