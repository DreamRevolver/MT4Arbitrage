namespace TradingAPI.MT4Server;

public class ConvertBytes
{
    public static string toHex(byte[] bytes)
    {
        var hex = "";
        foreach (var num in bytes)
            hex = $"{hex}{num.ToString("X").PadLeft(2, '0')} ";
        return hex;
    }

    public static string toAscii(byte[] bytes)
    {
        var ascii = "";
        for (var index = 0; index < bytes.Length; ++index)
            ascii = bytes[index] != 0 ? ascii + (char) bytes[index] : $"{ascii} ";
        return ascii;
    }
}