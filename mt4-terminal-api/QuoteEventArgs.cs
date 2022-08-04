namespace TradingAPI.MT4Server;

public class QuoteEventArgs
{
    public double Ask;
    public double Bid;
    public string Symbol;
    public DateTime Time;

    public override string ToString() => $"{Symbol} {Bid} {Ask}";

    public double GetBid() => Bid;

    public double GetAsk() => Ask;
}
