using System.Net.Sockets;

namespace TradingAPI.MT4Server;

internal sealed class AuthNone : AuthMethod
{
    public AuthNone(Socket server)
        : base(server)
    {
    }

    public override void Authenticate()
    {
    }

    public override void BeginAuthenticate(HandShakeComplete callback)
    {
        callback(null);
    }
}