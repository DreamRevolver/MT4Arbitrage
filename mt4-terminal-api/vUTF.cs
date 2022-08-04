namespace TradingAPI.MT4Server;

public class vUTF
{
    public static byte[] toByte(string str)
    {
        var numArray = new byte[str.Length];
        for (var index = 0; index < str.Length; ++index)
            numArray[index] = (byte) (str[index] & (uint) byte.MaxValue);
        return numArray;
    }

    public static string toString(byte[] bytes, int offset)
    {
        var str = "";
        for (var index = offset; index < bytes.Length && bytes[index] != 0; ++index)
            str += ((char) bytes[index]).ToString();
        return str;
    }
}