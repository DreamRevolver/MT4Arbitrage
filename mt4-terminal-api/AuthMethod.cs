using System.Net.Sockets;

namespace TradingAPI.MT4Server;

internal abstract class AuthMethod
{
    protected HandShakeComplete CallBack;
    private Socket m_Server;

    public AuthMethod(Socket server) => Server = server;

    protected Socket Server
    {
        get => m_Server;
        set => m_Server = value != null ? value : throw new ArgumentNullException();
    }

    protected byte[] Buffer { get; set; }

    protected int Received { get; set; }

    public abstract void Authenticate();

    public abstract void BeginAuthenticate(HandShakeComplete callback);
}
