namespace TradingAPI.MT4Server;

internal class HostAndPort
{
    public static KeyValuePair<string, int> parse(string serverFilePath)
    {
        if (!File.Exists(serverFilePath))
            throw new Exception($"{serverFilePath} not found");
        try
        {
            var numArray = File.ReadAllBytes(serverFilePath);
            var index1 = 216;
            var str = "";
            while (numArray[index1] != 0 && numArray[index1] != 58)
                str += ((char) numArray[index1++]).ToString();
            int num;
            if (numArray[index1] == 0)
            {
                num = 443;
            }
            else
            {
                var index2 = index1 + 1;
                var s = "";
                while (numArray[index2] != 0)
                    s += ((char) numArray[index2++]).ToString();
                num = int.Parse(s);
            }

            return new KeyValuePair<string, int>(str.Trim(), num);
        }
        catch (Exception)
        {
            throw new Exception("Unable to parse host and port.");
        }
    }

    public static KeyValuePair<string, int> parseStr(string ip)
    {
        var index = 0;
        var str = "";
        while (index < ip.Length && ip[index] != ':')
            str += ip[index++].ToString();
        int num1;
        if (index == ip.Length)
        {
            num1 = 443;
        }
        else
        {
            var num2 = index + 1;
            var s = "";
            while (num2 < ip.Length)
                s += ip[num2++].ToString();
            num1 = int.Parse(s);
        }

        return new KeyValuePair<string, int>(str.Trim(), num1);
    }
}
