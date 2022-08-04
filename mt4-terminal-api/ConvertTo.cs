using System.Text;

namespace TradingAPI.MT4Server;

internal class ConvertTo
{
    public static DateTime StartTime = new(1970, 1, 1, 0, 0, 0, 0);

    private static readonly double[] DegreeP = new double[16]
    {
        1.0,
        10.0,
        100.0,
        1000.0,
        10000.0,
        100000.0,
        1000000.0,
        10000000.0,
        100000000.0,
        1000000000.0,
        10000000000.0,
        100000000000.0,
        1000000000000.0,
        10000000000000.0,
        100000000000000.0,
        1E+15
    };

    public static DateTime DateTime(long time) => StartTime.AddSeconds(time);

    public static DateTime DateTimeMs(long time) => StartTime.AddMilliseconds(time);

    public static long Long(DateTime time) => (long) time.Subtract(StartTime).TotalSeconds;

    internal static double LongLongToDouble(int digits, long value)
    {
        digits = Math.Min(digits, 11);
        return Math.Round(value / DegreeP[digits], digits);
    }

    internal static string String(byte[] buf)
    {
        var num = 0;
        for (var index = 0; index < buf.Length && (buf[index] != 0 || buf[index + 1] != 0); index += 2)
            ++num;
        var numArray = new byte[num * 2];
        for (var index = 0; index < num * 2; ++index)
            numArray[index] = buf[index];
        return Encoding.Unicode.GetString(numArray);
    }
}
