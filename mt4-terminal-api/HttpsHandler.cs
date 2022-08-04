using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TradingAPI.MT4Server;

internal sealed class HttpsHandler : SocksHandler
{
    private string m_Password;
    private int m_receivedNewlineChars;

    public HttpsHandler(Socket server)
        : this(server, "")
    {
    }

    public HttpsHandler(Socket server, string user)
        : this(server, user, "")
    {
    }

    public HttpsHandler(Socket server, string user, string pass)
        : base(server, user) => Password = pass;

    private string Password
    {
        get => m_Password;
        set => m_Password = value != null ? value : throw new ArgumentNullException();
    }

    private byte[] GetConnectBytes(string host, int port)
    {
        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine(string.Format("CONNECT {0}:{1} HTTP/1.1", host, port));
        stringBuilder.AppendLine(string.Format("Host: {0}:{1}", host, port));
        if (!string.IsNullOrEmpty(Username))
        {
            var base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", Username, Password)));
            stringBuilder.AppendLine(string.Format("Proxy-Authorization: Basic {0}", base64String));
        }

        stringBuilder.AppendLine();
        return Encoding.ASCII.GetBytes(stringBuilder.ToString());
    }

    private void VerifyConnectHeader(byte[] buffer)
    {
        var str1 = Encoding.ASCII.GetString(buffer);
        var str2 = (str1.StartsWith("HTTP/1.1 ", StringComparison.OrdinalIgnoreCase) || str1.StartsWith("HTTP/1.0 ", StringComparison.OrdinalIgnoreCase)) && str1.EndsWith(" ") ? str1.Substring(9, 3) : throw new ProtocolViolationException();
        if (str2 != "200")
            throw new ProxyException($"Invalid HTTP status. Code: {str2}");
    }

    public override void Negotiate(IPEndPoint remoteEP)
    {
        if (remoteEP == null)
            throw new ArgumentNullException();
        Negotiate(remoteEP.Address.ToString(), remoteEP.Port);
    }

    public override void Negotiate(string host, int port)
    {
        if (host == null)
            throw new ArgumentNullException();
        if (port is <= 0 or > ushort.MaxValue || host.Length > byte.MaxValue)
            throw new ArgumentException();
        var connectBytes = GetConnectBytes(host, port);
        if (Server.Send(connectBytes, 0, connectBytes.Length, SocketFlags.None) < connectBytes.Length)
            throw new SocketException(10054);
        VerifyConnectHeader(ReadBytes(13));
        var num1 = 0;
        var buffer = new byte[1];
        while (num1 < 4)
        {
            var num2 = Server.Receive(buffer, 0, 1, SocketFlags.None) != 0 ? buffer[0] : throw new SocketException(10054);
            if (num2 == (num1 % 2 == 0 ? 13 : 10))
                ++num1;
            else
                num1 = num2 == 13 ? 1 : 0;
        }
    }

    public override IAsyncProxyResult BeginNegotiate(
        IPEndPoint remoteEP,
        HandShakeComplete callback,
        IPEndPoint proxyEndPoint) => BeginNegotiate(remoteEP.Address.ToString(), remoteEP.Port, callback, proxyEndPoint);

    public override IAsyncProxyResult BeginNegotiate(
        string host,
        int port,
        HandShakeComplete callback,
        IPEndPoint proxyEndPoint)
    {
        ProtocolComplete = callback;
        Buffer = GetConnectBytes(host, port);
        Server.BeginConnect(proxyEndPoint, OnConnect, Server);
        AsyncResult = new IAsyncProxyResult();
        return AsyncResult;
    }

    private void OnConnect(IAsyncResult ar)
    {
        try
        {
            Server.EndConnect(ar);
        }
        catch (Exception ex)
        {
            ProtocolComplete(ex);
            return;
        }

        try
        {
            Server.BeginSend(Buffer, 0, Buffer.Length, SocketFlags.None, OnConnectSent, null);
        }
        catch (Exception ex)
        {
            ProtocolComplete(ex);
        }
    }

    private void OnConnectSent(IAsyncResult ar)
    {
        try
        {
            HandleEndSend(ar, Buffer.Length);
            Buffer = new byte[13];
            Received = 0;
            Server.BeginReceive(Buffer, 0, 13, SocketFlags.None, OnConnectReceive, Server);
        }
        catch (Exception ex)
        {
            ProtocolComplete(ex);
        }
    }

    private void OnConnectReceive(IAsyncResult ar)
    {
        try
        {
            HandleEndReceive(ar);
        }
        catch (Exception ex)
        {
            ProtocolComplete(ex);
            return;
        }

        try
        {
            if (Received < 13)
            {
                Server.BeginReceive(Buffer, Received, 13 - Received, SocketFlags.None, OnConnectReceive, Server);
            }
            else
            {
                VerifyConnectHeader(Buffer);
                ReadUntilHeadersEnd(true);
            }
        }
        catch (Exception ex)
        {
            ProtocolComplete(ex);
        }
    }

    private void ReadUntilHeadersEnd(bool readFirstByte)
    {
        while (Server.Available > 0 && m_receivedNewlineChars < 4)
        {
            if (!readFirstByte)
                readFirstByte = false;
            else if (Server.Receive(Buffer, 0, 1, SocketFlags.None) == 0)
                throw new SocketException(10054);
            if (Buffer[0] == (m_receivedNewlineChars % 2 == 0 ? 13 : 10))
                ++m_receivedNewlineChars;
            else
                m_receivedNewlineChars = Buffer[0] == 13 ? 1 : 0;
        }

        if (m_receivedNewlineChars == 4)
            ProtocolComplete(null);
        else
            Server.BeginReceive(Buffer, 0, 1, SocketFlags.None, OnEndHeadersReceive, Server);
    }

    private void OnEndHeadersReceive(IAsyncResult ar)
    {
        try
        {
            HandleEndReceive(ar);
            ReadUntilHeadersEnd(false);
        }
        catch (Exception ex)
        {
            ProtocolComplete(ex);
        }
    }
}
