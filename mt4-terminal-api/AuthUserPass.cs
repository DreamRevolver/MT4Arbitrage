using System.Net.Sockets;
using System.Text;

namespace TradingAPI.MT4Server;

internal sealed class AuthUserPass : AuthMethod
{
    private string m_Password;
    private string m_Username;

    public AuthUserPass(Socket server, string user, string pass)
        : base(server)
    {
        Username = user;
        Password = pass;
    }

    private string Username
    {
        get => m_Username;
        set => m_Username = value != null ? value : throw new ArgumentNullException();
    }

    private string Password
    {
        get => m_Password;
        set => m_Password = value != null ? value : throw new ArgumentNullException();
    }

    private byte[] GetAuthenticationBytes()
    {
        var destinationArray = new byte[3 + Username.Length + Password.Length];
        destinationArray[0] = 1;
        destinationArray[1] = (byte) Username.Length;
        Array.Copy(Encoding.ASCII.GetBytes(Username), 0, destinationArray, 2, Username.Length);
        destinationArray[Username.Length + 2] = (byte) Password.Length;
        Array.Copy(Encoding.ASCII.GetBytes(Password), 0, destinationArray, Username.Length + 3, Password.Length);
        return destinationArray;
    }

    private int GetAuthenticationLength() => 3 + Username.Length + Password.Length;

    public override void Authenticate()
    {
        if (Server.Send(GetAuthenticationBytes()) < GetAuthenticationLength())
            throw new SocketException(10054);
        var buffer = new byte[2];
        int num;
        for (var offset = 0; offset != 2; offset += num)
        {
            num = Server.Receive(buffer, offset, 2 - offset, SocketFlags.None);
            if (num == 0)
                throw new SocketException(10054);
        }

        if (buffer[1] != 0)
        {
            Server.Close();
            throw new ProxyException("Username/password combination rejected.");
        }
    }

    public override void BeginAuthenticate(HandShakeComplete callback)
    {
        CallBack = callback;
        Server.BeginSend(GetAuthenticationBytes(), 0, GetAuthenticationLength(), SocketFlags.None, OnSent, Server);
    }

    private void OnSent(IAsyncResult ar)
    {
        try
        {
            if (Server.EndSend(ar) < GetAuthenticationLength())
                throw new SocketException(10054);
            Buffer = new byte[2];
            Server.BeginReceive(Buffer, 0, 2, SocketFlags.None, OnReceive, Server);
        }
        catch (Exception ex)
        {
            CallBack(ex);
        }
    }

    private void OnReceive(IAsyncResult ar)
    {
        try
        {
            var num = Server.EndReceive(ar);
            if (num <= 0)
                throw new SocketException(10054);
            Received += num;
            if (Received == Buffer.Length)
            {
                if (Buffer[1] != 0)
                    throw new ProxyException("Username/password combination not accepted.");
                CallBack(null);
            }
            else
            {
                Server.BeginReceive(Buffer, Received, Buffer.Length - Received, SocketFlags.None, OnReceive, Server);
            }
        }
        catch (Exception ex)
        {
            CallBack(ex);
        }
    }
}
