using aaPatch.Formats;

namespace aaPatch.Tests.Integration;

[TestFixture]
public class ConversionTests
{
    [Test]
    public async Task JsonToAveva_RoundTrip()
    {
        const string json =
            """
            [
              {
                "Template": "$Pump",
                "TagName": "P_101",
                "HiHi(MxFloat)": 100.5,
                "Count(MxInteger)": 10,
                "Enabled(MxBoolean)": true
              }
            ]
            """;

        var jsonFormatter = new JsonFormatter();
        var avevaFormatter = new AvevaFormatter();

        var objects = jsonFormatter.Read(json).ToList();
        var aveva = avevaFormatter.Write(objects);

        await Verify(aveva).UseTextForParameters("JsonToAveva");
    }

    [Test]
    public async Task AvevaToCsv_RoundTrip()
    {
        const string aveva =
            """
            :TEMPLATE=$Pump
            :Tagname,HiHi(MxFloat),Enabled(MxBoolean)
            P_101,100.5,true
            """;

        var avevaFormatter = new AvevaFormatter();
        var csvFormatter = new CsvFormatter();

        var objects = avevaFormatter.Read(aveva).ToList();
        var csv = csvFormatter.Write(objects);

        await Verify(csv).UseTextForParameters("AvevaToCsv");
    }

    [Test]
    public async Task CsvToJson_RoundTrip()
    {
        const string csv = "TagName,Value\nP_101,100.5";
        var csvFormatter = new CsvFormatter();
        var jsonFormatter = new JsonFormatter();

        var objects = csvFormatter.Read(csv).ToList();
        var json = jsonFormatter.Write(objects);

        await Verify(json).UseTextForParameters("CsvToJson");
    }

    [Test]
    public async Task AvevaToJson_RoundTrip()
    {
        const string aveva =
            """
            :TEMPLATE=$Pump
            :Tagname,HiHi(MxFloat),Enabled(MxBoolean)
            P_101,100.5,true
            """;

        var avevaFormatter = new AvevaFormatter();
        var jsonFormatter = new JsonFormatter();

        var objects = avevaFormatter.Read(aveva).ToList();
        var json = jsonFormatter.Write(objects);

        await Verify(json).UseTextForParameters("AvevaToJson");
    }

    [Test]
    public async Task CsvToAveva_RoundTrip()
    {
        const string csv =
            """
            Template,TagName,HiHi(MxFloat),Enabled(MxBoolean)
            $Pump,P_101,100.5,true
            """;

        var csvFormatter = new CsvFormatter();
        var avevaFormatter = new AvevaFormatter();

        var objects = csvFormatter.Read(csv).ToList();
        var aveva = avevaFormatter.Write(objects);

        await Verify(aveva).UseTextForParameters("CsvToAveva");
    }
}