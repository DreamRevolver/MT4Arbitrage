using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TradingAPI.MT4Server;

internal sealed class Socks4Handler : SocksHandler
{
    public Socks4Handler(Socket server, string user)
        : base(server, user)
    {
    }

    private byte[] GetHostPortBytes(string host, int port)
    {
        if (host == null)
            throw new ArgumentNullException();
        if (port is <= 0 or > ushort.MaxValue)
            throw new ArgumentException();
        var destinationArray = new byte[10 + Username.Length + host.Length];
        destinationArray[0] = 4;
        destinationArray[1] = 1;
        Array.Copy(PortToBytes(port), 0, destinationArray, 2, 2);
        destinationArray[4] = destinationArray[5] = destinationArray[6] = 0;
        destinationArray[7] = 1;
        Array.Copy(Encoding.ASCII.GetBytes(Username), 0, destinationArray, 8, Username.Length);
        destinationArray[8 + Username.Length] = 0;
        Array.Copy(Encoding.ASCII.GetBytes(host), 0, destinationArray, 9 + Username.Length, host.Length);
        destinationArray[9 + Username.Length + host.Length] = 0;
        return destinationArray;
    }

    private byte[] GetEndPointBytes(IPEndPoint remoteEP)
    {
        if (remoteEP == null)
            throw new ArgumentNullException();
        var destinationArray = new byte[9 + Username.Length];
        destinationArray[0] = 4;
        destinationArray[1] = 1;
        Array.Copy(PortToBytes(remoteEP.Port), 0, destinationArray, 2, 2);
        Array.Copy(remoteEP.Address.GetAddressBytes(), 0, destinationArray, 4, 4);
        Array.Copy(Encoding.ASCII.GetBytes(Username), 0, destinationArray, 8, Username.Length);
        destinationArray[8 + Username.Length] = 0;
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
        if (connect == null)
            throw new ArgumentNullException();
        if (connect.Length < 2)
            throw new ArgumentException();
        if (Server.Send(connect) < connect.Length)
            throw new SocketException(10054);
        if (ReadBytes(8)[1] != 90)
        {
            Server.Close();
            throw new ProxyException("Negotiation failed.");
        }
    }

    public override IAsyncProxyResult BeginNegotiate(
        string host,
        int port,
        HandShakeComplete callback,
        IPEndPoint proxyEndPoint)
    {
        ProtocolComplete = callback;
        Buffer = GetHostPortBytes(host, port);
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
        Buffer = GetEndPointBytes(remoteEP);
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
            Server.BeginSend(Buffer, 0, Buffer.Length, SocketFlags.None, OnSent, Server);
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
            HandleEndSend(ar, Buffer.Length);
        }
        catch (Exception ex)
        {
            ProtocolComplete(ex);
            return;
        }

        try
        {
            Buffer = new byte[8];
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
            if (Received == 8)
            {
                if (Buffer[1] == 90)
                {
                    ProtocolComplete(null);
                }
                else
                {
                    Server.Close();
                    ProtocolComplete(new ProxyException("Negotiation failed."));
                }
            }
            else
            {
                Server.BeginReceive(Buffer, Received, Buffer.Length - Received, SocketFlags.None, OnReceive, Server);
            }
        }
        catch (Exception ex)
        {
            ProtocolComplete(ex);
        }
    }
}
