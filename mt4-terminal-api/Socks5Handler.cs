using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TradingAPI.MT4Server;

internal sealed class Socks5Handler : SocksHandler
{
    private string m_Password;

    public Socks5Handler(Socket server)
        : this(server, "")
    {
    }

    public Socks5Handler(Socket server, string user)
        : this(server, user, "")
    {
    }

    public Socks5Handler(Socket server, string user, string pass)
        : base(server, user) => Password = pass;

    private string Password
    {
        get => m_Password;
        set => m_Password = value != null ? value : throw new ArgumentNullException();
    }

    private byte[] HandShake { get; set; }

    private void Authenticate()
    {
        if (Server.Send(new byte[4]
            {
                5,
                2,
                0,
                2
            }) < 4)
            throw new SocketException(10054);
        var numArray = ReadBytes(2);
        if (numArray[1] == byte.MaxValue)
            throw new ProxyException("No authentication method accepted.");
        AuthMethod authMethod = numArray[1] switch
        {
            0 => new AuthNone(Server),
            2 => new AuthUserPass(Server, Username, Password),
            _ => throw new ProtocolViolationException()
        };

        authMethod.Authenticate();
    }

    private byte[] GetHostPortBytes(string host, int port)
    {
        if (host == null)
            throw new ArgumentNullException();
        if (port is <= 0 or > ushort.MaxValue || host.Length > byte.MaxValue)
            throw new ArgumentException();
        var destinationArray = new byte[7 + host.Length];
        destinationArray[0] = 5;
        destinationArray[1] = 1;
        destinationArray[2] = 0;
        destinationArray[3] = 3;
        destinationArray[4] = (byte) host.Length;
        Array.Copy(Encoding.ASCII.GetBytes(host), 0, destinationArray, 5, host.Length);
        Array.Copy(PortToBytes(port), 0, destinationArray, host.Length + 5, 2);
        return destinationArray;
    }

    private byte[] GetEndPointBytes(IPEndPoint remoteEP)
    {
        if (remoteEP == null)
            throw new ArgumentNullException();
        var destinationArray = new byte[10];
        destinationArray[0] = 5;
        destinationArray[1] = 1;
        destinationArray[2] = 0;
        destinationArray[3] = 1;
        Array.Copy(remoteEP.Address.GetAddressBytes(), 0, destinationArray, 4, 4);
        Array.Copy(PortToBytes(remoteEP.Port), 0, destinationArray, 8, 2);
        return destinationArray;
    }

    public override void Negotiate(string host, int port)
    {
        Negotiate(GetHostPortBytes(host, port));
    }

    public override void Negotiate(IPEndPoint remoteEP)
    {
        Negotiate(GetEndPointBytes(remoteEP));
    }

    private void Negotiate(byte[] connect)
    {
        Authenticate();
        if (Server.Send(connect) < connect.Length)
            throw new SocketException(10054);
        var numArray1 = ReadBytes(4);
        if (numArray1[1] != 0)
        {
            Server.Close();
            throw new ProxyException(numArray1[1]);
        }

        switch (numArray1[3])
        {
            case 1:
                ReadBytes(6);
                break;
            case 3:
                ReadBytes(ReadBytes(1)[0] + 2);
                break;
            case 4:
                ReadBytes(18);
                break;
            default:
                Server.Close();
                throw new ProtocolViolationException();
        }
    }

    public override IAsyncProxyResult BeginNegotiate(
        string host,
        int port,
        HandShakeComplete callback,
        IPEndPoint proxyEndPoint)
    {
        ProtocolComplete = callback;
        HandShake = GetHostPortBytes(host, port);
        Server.BeginConnect(proxyEndPoint, OnConnect, Server);
        AsyncResult = new IAsyncProxyResult();
        return AsyncResult;
    }

    public override IAsyncProxyResult BeginNegotiate(
        IPEndPoint remoteEP,
        HandShakeComplete callback,
        IPEndPoint proxyEndPoint)
    {
        ProtocolComplete = callback;
        HandShake = GetEndPointBytes(remoteEP);
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
            Server.BeginSend(new byte[4]
            {
                5,
                2,
                0,
                2
            }, 0, 4, SocketFlags.None, OnAuthSent, Server);
        }
        catch (Exception ex)
        {
            ProtocolComplete(ex);
        }
    }

    private void OnAuthSent(IAsyncResult ar)
    {
        try
        {
            HandleEndSend(ar, 4);
        }
        catch (Exception ex)
        {
            ProtocolComplete(ex);
            return;
        }

        try
        {
            Buffer = new byte[1024];
            Received = 0;
            Server.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, OnAuthReceive, Server);
        }
        catch (Exception ex)
        {
            ProtocolComplete(ex);
        }
    }

    private void OnAuthReceive(IAsyncResult ar)
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
            if (Received < 2)
            {
                Server.BeginReceive(Buffer, Received, Buffer.Length - Received, SocketFlags.None, OnAuthReceive, Server);
            }
            else
            {
                AuthMethod authMethod;
                switch (Buffer[1])
                {
                    case 0:
                        authMethod = new AuthNone(Server);
                        break;
                    case 2:
                        authMethod = new AuthUserPass(Server, Username, Password);
                        break;
                    default:
                        ProtocolComplete(new SocketException());
                        return;
                }

                authMethod.BeginAuthenticate(OnAuthenticated);
            }
        }
        catch (Exception ex)
        {
            ProtocolComplete(ex);
        }
    }

    private void OnAuthenticated(Exception e)
    {
        if (e != null)
            ProtocolComplete(e);
        else
            try
            {
                Server.BeginSend(HandShake, 0, HandShake.Length, SocketFlags.None, OnSent, Server);
            }
            catch (Exception ex)
            {
                ProtocolComplete(ex);
            }
    }

    private void OnSent(IAsyncResult ar)
    {
        try
        {
            HandleEndSend(ar, HandShake.Length);
        }
        catch (Exception ex)
        {
            ProtocolComplete(ex);
            return;
        }

        try
        {
            Buffer = new byte[5];
            Received = 0;
            Server.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, OnReceive, Server);
        }
        catch (Exception ex)
        {
            ProtocolComplete(ex);
        }
    }

    private void OnReceive(IAsyncResult ar)
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
            if (Received == Buffer.Length)
                ProcessReply(Buffer);
            else
                Server.BeginReceive(Buffer, Received, Buffer.Length - Received, SocketFlags.None, OnReceive, Server);
        }
        catch (Exception ex)
        {
            ProtocolComplete(ex);
        }
    }

    private void ProcessReply(byte[] buffer)
    {
        switch (buffer[3])
        {
            case 1:
                Buffer = new byte[5];
                break;
            case 3:
                Buffer = new byte[buffer[4] + 2];
                break;
            case 4:
                buffer = new byte[17];
                break;
            default:
                throw new ProtocolViolationException();
        }

        Received = 0;
        Server.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, OnReadLast, Server);
    }

    private void OnReadLast(IAsyncResult ar)
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
            if (Received == Buffer.Length)
                ProtocolComplete(null);
            else
                Server.BeginReceive(Buffer, Received, Buffer.Length - Received, SocketFlags.None, OnReadLast, Server);
        }
        catch (Exception ex)
        {
            ProtocolComplete(ex);
        }
    }
}
