namespace TradingAPI.MT4Server;

public struct QuoteHistoryEventArgs
{
    public string Symbol;
    public Timeframe Timeframe;
    public Bar[] Bars;
}