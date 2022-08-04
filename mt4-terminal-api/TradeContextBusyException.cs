namespace TradingAPI.MT4Server;

public class TradeContextBusyException : Exception
{
    public TradeContextBusyException()
        : base("All order connections busy or desconnected.")
    {
    }
}
