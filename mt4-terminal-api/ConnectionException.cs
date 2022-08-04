namespace TradingAPI.MT4Server;

public class ConnectionException : ApplicationException
{
    public ConnectionException(string message)
        : base(message)
    {
    }
}
