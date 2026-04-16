namespace aaPatch.Tests;

[TestFixture]
public class GalaxyDumpTests
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
    public void Read_ValidText_ReturnsExpectedObjects()
    {
        var result = GalaxyDump.Read(SimpleGalaxyDump).ToList();

        Assert.That(result, Has.Count.EqualTo(3));

        var p101 = result.First(x => x.TagName == "P_101");
        Assert.That(p101.Template, Is.EqualTo("$Pump"));
        Assert.That(p101.GetValue("Description"), Is.EqualTo("Centrifugal Pump"));
        Assert.That(p101.GetValue("HiHi"), Is.EqualTo("100.0"));

        var v201 = result.First(x => x.TagName == "V_201");
        Assert.That(v201.Template, Is.EqualTo("$Valve"));
        Assert.That(v201.GetValue("OpenLimit"), Is.EqualTo("True"));
    }

    [Test]
    public void Write_ValidObjects_ReturnsExpectedFormat()
    {
        var objects = new List<ObjectData>
        {
            new("$Pump", new Dictionary<string, string?>
            {
                { ":Tagname", "P_101" },
                { "Description", "Pump 1" }
            }),
            new("$Valve", new Dictionary<string, string?>
            {
                { ":Tagname", "V_101" },
                { "Description", "Valve 1" }
            })
        };

        var result = GalaxyDump.Write(objects);

        Assert.That(result, Does.StartWith(":TEMPLATE=$Pump"));
        Assert.That(result, Does.Contain(":Tagname,Description"));
        Assert.That(result, Does.Contain("P_101,Pump 1"));
        Assert.That(result, Does.Contain(":TEMPLATE=$Valve"));
        Assert.That(result, Does.Contain("V_101,Valve 1"));
    }

    [Test]
    public void Read_EmptyText_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => GalaxyDump.Read(""));
        Assert.Throws<ArgumentException>(() => GalaxyDump.Read("   "));
    }
}