using Shared.Models;

namespace LMAXConnector;

public static class LmaxInstrumentParser
{
    public static List<Instrument> ParseFromFile(string fileName)
    {
        return File.ReadAllLines(fileName)
            .Skip(1)
            .Select(v => FromCsv(v))
            .ToList();
    }

    public static Instrument FromCsv(string csvLine)
    {
        string[] values = csvLine.Split(',');
        Instrument instrument = new Instrument
        {
            Symbol = string.Join("",values[2].Split('/')),
            ConId = Convert.ToInt32(values[1])
        };
        return instrument;
    }
}
