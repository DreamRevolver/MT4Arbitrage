namespace TradingAPI.MT4Server;

public class RequoteException : ServerException
{
    public readonly double Ask;
    public readonly double Bid;

    public RequoteException(int code, double bid, double ask)
        : base(code)
    {
        Bid = bid;
        Ask = ask;
    }
}