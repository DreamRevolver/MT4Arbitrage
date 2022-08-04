namespace TradingAPI.MT4Server;

public class TradeTimeoutException : Exception
{
    public TradeTimeoutException(string message)
        : base(message)
    {
    }
}
