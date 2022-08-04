namespace TradingAPI.MT4Server;

public class ConnectException : Exception
{
    public ConnectException(string message)
        : base(message)
    {
    }
}
