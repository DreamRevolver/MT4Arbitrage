using System.Net;
using System.Net.Sockets;

namespace TradingAPI.MT4Server;

public class ProxySocket : Socket
{
    private AsyncCallback CallBack;
    private string m_ProxyPass;
    private string m_ProxyUser;

    public ProxySocket(
        AddressFamily addressFamily,
        SocketType socketType,
        ProtocolType protocolType)
        : this(addressFamily, socketType, protocolType, "")
    {
    }

    public ProxySocket(
        AddressFamily addressFamily,
        SocketType socketType,
        ProtocolType protocolType,
        string proxyUsername)
        : this(addressFamily, socketType, protocolType, proxyUsername, "")
    {
    }

    public ProxySocket(
        AddressFamily addressFamily,
        SocketType socketType,
        ProtocolType protocolType,
        string proxyUsername,
        string proxyPassword)
        : base(addressFamily, socketType, protocolType)
    {
        ProxyUser = proxyUsername;
        ProxyPass = proxyPassword;
        ToThrow = new InvalidOperationException();
    }

    public IPEndPoint ProxyEndPoint { get; set; }

    public ProxyTypes ProxyType { get; set; }

    private object State { get; set; }

    public string ProxyUser
    {
        get => m_ProxyUser;
        set => m_ProxyUser = value != null ? value : throw new ArgumentNullException();
    }

    public string ProxyPass
    {
        get => m_ProxyPass;
        set => m_ProxyPass = value != null ? value : throw new ArgumentNullException();
    }

    private IAsyncProxyResult AsyncResult { get; set; }

    private Exception ToThrow { get; set; }

    private int RemotePort { get; set; }

    public new void Connect(EndPoint remoteEP)
    {
        if (remoteEP == null)
            throw new ArgumentNullException("<remoteEP> cannot be null.");
        if (ProtocolType != ProtocolType.Tcp || ProxyType == ProxyTypes.None || ProxyEndPoint == null)
        {
            base.Connect(remoteEP);
        }
        else
        {
            base.Connect(ProxyEndPoint);
            switch (ProxyType)
            {
                case ProxyTypes.Https:
                    new HttpsHandler(this, ProxyUser, ProxyPass).Negotiate((IPEndPoint) remoteEP);
                    break;
                case ProxyTypes.Socks4:
                    new Socks4Handler(this, ProxyUser).Negotiate((IPEndPoint) remoteEP);
                    break;
                default:
                {
                    if (ProxyType != ProxyTypes.Socks5)
                        return;
                    new Socks5Handler(this, ProxyUser, ProxyPass).Negotiate((IPEndPoint) remoteEP);
                    break;
                }
            }
        }
    }

    public new void Connect(string host, int port)
    {
        if (host == null)
            throw new ArgumentNullException("<host> cannot be null.");
        if (port is <= 0 or > ushort.MaxValue)
            throw new ArgumentException("Invalid port.");
        if (ProtocolType != ProtocolType.Tcp || ProxyType == ProxyTypes.None || ProxyEndPoint == null)
        {
            base.Connect(new IPEndPoint(Dns.GetHostEntry(host).AddressList[0], port));
        }
        else
        {
            base.Connect(ProxyEndPoint);
            switch (ProxyType)
            {
                case ProxyTypes.Https:
                    new HttpsHandler(this, ProxyUser, ProxyPass).Negotiate(host, port);
                    break;
                case ProxyTypes.Socks4:
                    new Socks4Handler(this, ProxyUser).Negotiate(host, port);
                    break;
                default:
                {
                    if (ProxyType != ProxyTypes.Socks5)
                        return;
                    new Socks5Handler(this, ProxyUser, ProxyPass).Negotiate(host, port);
                    break;
                }
            }
        }
    }

    public new IAsyncResult BeginConnect(
        EndPoint remoteEP,
        AsyncCallback callback,
        object state)
    {
        if (remoteEP == null)
            throw new ArgumentNullException();
        if (ProtocolType != ProtocolType.Tcp || ProxyType == ProxyTypes.None || ProxyEndPoint == null)
            return base.BeginConnect(remoteEP, callback, state);
        CallBack = callback;
        switch (ProxyType)
        {
            case ProxyTypes.Https:
                AsyncResult = new HttpsHandler(this, ProxyUser, ProxyPass).BeginNegotiate((IPEndPoint) remoteEP, OnHandShakeComplete, ProxyEndPoint);
                return AsyncResult;
            case ProxyTypes.Socks4:
                AsyncResult = new Socks4Handler(this, ProxyUser).BeginNegotiate((IPEndPoint) remoteEP, OnHandShakeComplete, ProxyEndPoint);
                return AsyncResult;
        }

        if (ProxyType != ProxyTypes.Socks5)
            return null;
        AsyncResult = new Socks5Handler(this, ProxyUser, ProxyPass).BeginNegotiate((IPEndPoint) remoteEP, OnHandShakeComplete, ProxyEndPoint);
        return AsyncResult;
    }

    public new IAsyncResult BeginConnect(
        string host,
        int port,
        AsyncCallback callback,
        object state)
    {
        if (host == null)
            throw new ArgumentNullException();
        if (port is <= 0 or > ushort.MaxValue)
            throw new ArgumentException();
        CallBack = callback;
        if (ProtocolType != ProtocolType.Tcp || ProxyType == ProxyTypes.None || ProxyEndPoint == null)
        {
            RemotePort = port;
            AsyncResult = BeginDns(host, OnHandShakeComplete);
            return AsyncResult;
        }

        switch (ProxyType)
        {
            case ProxyTypes.Https:
                AsyncResult = new HttpsHandler(this, ProxyUser, ProxyPass).BeginNegotiate(host, port, OnHandShakeComplete, ProxyEndPoint);
                return AsyncResult;
            case ProxyTypes.Socks4:
                AsyncResult = new Socks4Handler(this, ProxyUser).BeginNegotiate(host, port, OnHandShakeComplete, ProxyEndPoint);
                return AsyncResult;
        }

        if (ProxyType != ProxyTypes.Socks5)
            return null;
        AsyncResult = new Socks5Handler(this, ProxyUser, ProxyPass).BeginNegotiate(host, port, OnHandShakeComplete, ProxyEndPoint);
        return AsyncResult;
    }

    public new void EndConnect(IAsyncResult asyncResult)
    {
        if (asyncResult == null)
            throw new ArgumentNullException();
        if (!(asyncResult is IAsyncProxyResult))
        {
            base.EndConnect(asyncResult);
        }
        else
        {
            if (!asyncResult.IsCompleted)
                asyncResult.AsyncWaitHandle.WaitOne();
            if (ToThrow != null)
                throw ToThrow;
        }
    }

    internal IAsyncProxyResult BeginDns(string host, HandShakeComplete callback)
    {
        try
        {
            Dns.BeginGetHostEntry(host, OnResolved, this);
            return new IAsyncProxyResult();
        }
        catch
        {
            throw new SocketException();
        }
    }

    private void OnResolved(IAsyncResult asyncResult)
    {
        try
        {
            base.BeginConnect(new IPEndPoint(Dns.EndGetHostEntry(asyncResult).AddressList[0], RemotePort), OnConnect, State);
        }
        catch (Exception ex)
        {
            OnHandShakeComplete(ex);
        }
    }

    private void OnConnect(IAsyncResult asyncResult)
    {
        try
        {
            base.EndConnect(asyncResult);
            OnHandShakeComplete(null);
        }
        catch (Exception ex)
        {
            OnHandShakeComplete(ex);
        }
    }

    private void OnHandShakeComplete(Exception error)
    {
        if (error != null)
            Close();
        ToThrow = error;
        AsyncResult.Reset();
        if (CallBack == null)
            return;
        CallBack(AsyncResult);
    }
}
