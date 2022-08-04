using System.Text;

namespace TradingAPI.MT4Server;

internal class Demo
{
    internal static uint _Seed;

    public static DemoAccount get(
        string serverFilePath,
        int leverage,
        double balance,
        string name,
        string accountType,
        string country,
        string city,
        string state,
        string zip,
        string address,
        string phone,
        string email,
        string terminalCompany)
    {
        var keyValuePair = HostAndPort.parse(serverFilePath);
        return get(keyValuePair.Key, keyValuePair.Value, leverage, balance, name, accountType, country, city, state, zip, address, phone, email, terminalCompany);
    }

    public static DemoAccount get(
        string host,
        int port,
        int leverage,
        double balance,
        string name,
        string accountType,
        string country,
        string city,
        string state,
        string zip,
        string address,
        string phone,
        string email,
        string terminalCompany)
    {
        _Seed = 1303876527U;
        var connection = new Connection(new QuoteClient(0, null, host, port));
        var numArray1 = new byte[584];
        Array.Copy(StringFollowRand(name, 128), 0, numArray1, 4, 128);
        Array.Copy(StringFollowRand(accountType, 16), 0, numArray1, 132, 16);
        Array.Copy(StringFollowRand(country, 32), 0, numArray1, 148, 32);
        Array.Copy(StringFollowRand(city, 32), 0, numArray1, 180, 32);
        Array.Copy(StringFollowRand(state, 32), 0, numArray1, 212, 32);
        Array.Copy(StringFollowRand(zip, 16), 0, numArray1, 244, 16);
        Array.Copy(StringFollowRand(address, 96), 0, numArray1, 260, 96);
        Array.Copy(StringFollowRand("", 32), 0, numArray1, 356, 32);
        Array.Copy(StringFollowRand(phone, 32), 0, numArray1, 388, 32);
        Array.Copy(StringFollowRand(email, 48), 0, numArray1, 420, 48);
        Array.Copy(StringFollowRand(terminalCompany, 64), 0, numArray1, 484, 64);
        Array.Copy(StringFollowRand("", 15), 0, numArray1, 565, 15);
        Array.Copy(MT4Crypt.GetHardId(), 0, numArray1, 549, 16);
        BitConverter.GetBytes((int) DateTime.Now.Subtract(new DateTime(1970, 1, 1)).TotalSeconds).CopyTo(numArray1, 0);
        BitConverter.GetBytes(leverage).CopyTo(numArray1, 468);
        BitConverter.GetBytes(balance).CopyTo(numArray1, 472);
        BitConverter.GetBytes((ushort) 400).CopyTo(numArray1, 480);
        BitConverter.GetBytes(connection.CurrentBuild).CopyTo(numArray1, 482);
        var packetKey = (long) new vRSA(5931095820042777067UL).ComputePacketKey(numArray1);
        var numArray2 = new byte[593];
        numArray2[0] = 1;
        Array.Copy(numArray1, 0, numArray2, 1, numArray1.Length);
        BitConverter.GetBytes((ulong) packetKey).CopyTo(numArray2, 585);
        var reason = "";
        connection.Connect(ref reason);
        connection.SendEnrypt(numArray2);
        var numArray3 = connection.Receive(1);
        if (numArray3[0] != 0)
            throw new ServerException(numArray3[0]);
        var decrypt = connection.ReceiveDecrypt(32);
        return new DemoAccount
        {
            User = BitConverter.ToUInt32(decrypt, 8),
            Password = MT4Order.getString(decrypt, 16),
            Investor = MT4Order.getString(decrypt, 24)
        };
    }

    internal static byte[] StringFollowRand(string str, int size)
    {
        var destinationArray = new byte[size];
        var sourceArray = new byte[str.Length];
        Encoding.ASCII.GetBytes(str).CopyTo(sourceArray, 0);
        var length = str.Length < size ? str.Length : size;
        Array.Copy(sourceArray, destinationArray, length);
        size -= length;
        if (size == 0)
        {
            destinationArray[length - 1] = 0;
            return destinationArray;
        }

        var num = length + 1;
        destinationArray[length] = 0;
        for (--size; size > 0; --size)
        {
            _Seed = (uint) ((int) _Seed * 214013 + 2531011);
            destinationArray[num++] = (byte) ((_Seed >> 16) & byte.MaxValue);
        }

        return destinationArray;
    }
}
