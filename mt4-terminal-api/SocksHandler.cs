using System.Net;
using System.Net.Sockets;

namespace TradingAPI.MT4Server;

internal abstract class SocksHandler
{
    private Socket m_Server;
    private string m_Username;
    protected HandShakeComplete ProtocolComplete;

    public SocksHandler(Socket server, string user)
    {
        Server = server;
        Username = user;
    }

    protected Socket Server
    {
        get => m_Server;
        set => m_Server = value != null ? value : throw new ArgumentNullException();
    }

    protected string Username
    {
        get => m_Username;
        set => m_Username = value != null ? value : throw new ArgumentNullException();
    }

    protected IAsyncProxyResult AsyncResult { get; set; }

    protected byte[] Buffer { get; set; }

    protected int Received { get; set; }

    protected byte[] PortToBytes(int port)
    {
        return new byte[2]
        {
            (byte) (port / 256),
            (byte) (port % 256)
        };
    }

    protected byte[] AddressToBytes(long address)
    {
        return new byte[]
        {
            (byte) ((ulong) address % 256UL),
            (byte) ((ulong) (address / 256L) % 256UL),
            (byte) ((ulong) (address / 65536L) % 256UL),
            (byte) ((ulong) address / 16777216UL)
        };
    }

    protected byte[] ReadBytes(int count)
    {
        var buffer = count > 0 ? new byte[count] : throw new ArgumentException();
        int num;
        for (var offset = 0; offset != count; offset += num)
        {
            num = Server.Receive(buffer, offset, count - offset, SocketFlags.None);
            if (num == 0)
                throw new SocketException(10054);
        }

        return buffer;
    }

    protected void HandleEndReceive(IAsyncResult ar)
    {
        var num = Server.EndReceive(ar);
        if (num <= 0)
            throw new SocketException(10054);
        Received += num;
    }

    protected void HandleEndSend(IAsyncResult ar, int expectedLength)
    {
        if (Server.EndSend(ar) < expectedLength)
            throw new SocketException(10054);
    }

    public abstract void Negotiate(string host, int port);

    public abstract void Negotiate(IPEndPoint remoteEP);

    public abstract IAsyncProxyResult BeginNegotiate(
        IPEndPoint remoteEP,
        HandShakeComplete callback,
        IPEndPoint proxyEndPoint);

    public abstract IAsyncProxyResult BeginNegotiate(
        string host,
        int port,
        HandShakeComplete callback,
        IPEndPoint proxyEndPoint);
}
