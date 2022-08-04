namespace TradingAPI.MT4Server;

[Serializable]
public class ProxyException : Exception
{
    public ProxyException()
        : this("An error occured while talking to the proxy server.")
    {
    }

    public ProxyException(string message)
        : base(message)
    {
    }

    public ProxyException(int socks5Error)
        : this(Socks5ToString(socks5Error))
    {
    }

    public static string Socks5ToString(int socks5Error)
    {
        return socks5Error switch
        {
            0 => "Connection succeeded.",
            1 => "General SOCKS server failure.",
            2 => "Connection not allowed by ruleset.",
            3 => "Network unreachable.",
            4 => "Host unreachable.",
            5 => "Connection refused.",
            6 => "TTL expired.",
            7 => "Command not supported.",
            8 => "Address type not supported.",
            _ => "Unspecified SOCKS error."
        };
    }
}
