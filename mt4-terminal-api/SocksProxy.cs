using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TradingAPI.MT4Server;

public class SocksProxy
{
    private static readonly string[] errorMsgs = new string[]
    {
        "Operation completed successfully.",
        "General SOCKS server failure.",
        "Connection not allowed by ruleset.",
        "Network unreachable.",
        "Host unreachable.",
        "Connection refused.",
        "TTL expired.",
        "Command not supported.",
        "Address type not supported.",
        "Unknown error."
    };

    private SocksProxy()
    {
    }

    public static Socket ConnectToSocks5Proxy(
        string proxyAdress,
        ushort proxyPort,
        string destAddress,
        ushort destPort,
        string userName,
        string password)
    {
        IPAddress ipAddress = null;
        var buffer1 = new byte[257];
        var buffer2 = new byte[257];
        IPAddress address;
        try
        {
            address = IPAddress.Parse(proxyAdress);
        }
        catch (FormatException)
        {
            address = Dns.GetHostByAddress(proxyAdress).AddressList[0];
        }

        try
        {
            ipAddress = IPAddress.Parse(destAddress);
        }
        catch (FormatException)
        {
        }

        var remoteEP = new IPEndPoint(address, proxyPort);
        var socks5Proxy = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socks5Proxy.Connect(remoteEP);
        ushort num1 = 0;
        int index1 = num1;
        var num2 = (ushort) (index1 + 1);
        buffer1[index1] = 5;
        int index2 = num2;
        var num3 = (ushort) (index2 + 1);
        buffer1[index2] = 2;
        int index3 = num3;
        var num4 = (ushort) (index3 + 1);
        buffer1[index3] = 0;
        int index4 = num4;
        var size1 = (ushort) (index4 + 1);
        buffer1[index4] = 2;
        socks5Proxy.Send(buffer1, size1, SocketFlags.None);
        if (socks5Proxy.Receive(buffer2, 2, SocketFlags.None) != 2)
            throw new ConnectionException("Bad response received from proxy server.");
        if (buffer2[1] == byte.MaxValue)
        {
            socks5Proxy.Close();
            throw new ConnectionException("None of the authentication method was accepted by proxy server.");
        }

        ushort num5 = 0;
        int index5 = num5;
        var num6 = (ushort) (index5 + 1);
        buffer1[index5] = 5;
        int index6 = num6;
        var index7 = (ushort) (index6 + 1);
        int length1 = (byte) userName.Length;
        buffer1[index6] = (byte) length1;
        var bytes1 = Encoding.Default.GetBytes(userName);
        bytes1.CopyTo(buffer1, index7);
        var num7 = (ushort) (index7 + (uint) (ushort) bytes1.Length);
        int index8 = num7;
        var index9 = (ushort) (index8 + 1);
        int length2 = (byte) password.Length;
        buffer1[index8] = (byte) length2;
        var bytes2 = Encoding.Default.GetBytes(password);
        bytes2.CopyTo(buffer1, index9);
        var size2 = (ushort) (index9 + (uint) (ushort) bytes2.Length);
        socks5Proxy.Send(buffer1, size2, SocketFlags.None);
        if (socks5Proxy.Receive(buffer2, 2, SocketFlags.None) != 2)
            throw new ConnectionException("Bad response received from proxy server.");
        if (buffer2[1] != 0)
            throw new ConnectionException("Bad Usernaem/Password.");
        ushort num8 = 0;
        int index10 = num8;
        var num9 = (ushort) (index10 + 1);
        buffer1[index10] = 5;
        int index11 = num9;
        var num10 = (ushort) (index11 + 1);
        buffer1[index11] = 1;
        int index12 = num10;
        var size3 = (ushort) (index12 + 1);
        buffer1[index12] = 0;
        if (ipAddress != null)
        {
            switch (ipAddress.AddressFamily)
            {
                case AddressFamily.InterNetwork:
                    int index13 = size3;
                    var index14 = (ushort) (index13 + 1);
                    buffer1[index13] = 1;
                    var addressBytes1 = ipAddress.GetAddressBytes();
                    addressBytes1.CopyTo(buffer1, index14);
                    size3 = (ushort) (index14 + (uint) (ushort) addressBytes1.Length);
                    break;
                case AddressFamily.InterNetworkV6:
                    int index15 = size3;
                    var index16 = (ushort) (index15 + 1);
                    buffer1[index15] = 4;
                    var addressBytes2 = ipAddress.GetAddressBytes();
                    addressBytes2.CopyTo(buffer1, index16);
                    size3 = (ushort) (index16 + (uint) (ushort) addressBytes2.Length);
                    break;
            }
        }
        else
        {
            int index17 = size3;
            var num11 = (ushort) (index17 + 1);
            buffer1[index17] = 3;
            int index18 = num11;
            var index19 = (ushort) (index18 + 1);
            int num12 = Convert.ToByte(destAddress.Length);
            buffer1[index18] = (byte) num12;
            var bytes3 = Encoding.Default.GetBytes(destAddress);
            bytes3.CopyTo(buffer1, index19);
            size3 = (ushort) (index19 + (uint) (ushort) bytes3.Length);
        }

        var bytes4 = BitConverter.GetBytes(destPort);
        for (var index20 = bytes4.Length - 1; index20 >= 0; --index20)
            buffer1[size3++] = bytes4[index20];
        socks5Proxy.Send(buffer1, size3, SocketFlags.None);
        socks5Proxy.Receive(buffer2);
        if (buffer2[1] != 0)
            throw new ConnectionException(errorMsgs[buffer2[1]]);
        return socks5Proxy;
    }
}
