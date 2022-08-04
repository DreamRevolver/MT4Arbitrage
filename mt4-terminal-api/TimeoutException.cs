namespace TradingAPI.MT4Server;

public class TimeoutException : Exception
{
    public TimeoutException(string message)
        : base(message)
    {
    }
}
