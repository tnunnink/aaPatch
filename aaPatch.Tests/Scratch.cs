using aaPatch.Formats;
using System.Linq.Dynamic.Core;

namespace aaPatch.Tests;

[TestFixture]
public class Scratch
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
    public void TestingLinq()
    {
        var formatter = new AvevaFormatter();

        var data = formatter.Read(SimpleGalaxyDump).AsQueryable();
        
        var results = data.Where("it[\"TagName\"] == (\"P_102\")").ToList();

        Assert.That(results, Has.Count.EqualTo(1));
    }
}