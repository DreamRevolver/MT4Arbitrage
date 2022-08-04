namespace TradingAPI.MT4Server;

public struct OrderProgressEventArgs
{
    public int TempID;
    public ProgressType Type;
    public Order Order;
    public Exception Exception;

    public override string ToString()
    {
        var str = $"{TempID} {Type} {Exception}";
        if (Exception != null)
            str = $"{str} {Exception.Message}";
        return str;
    }
}
