namespace TradingAPI.MT4Server;

public class Order
{
    public double ClosePrice;
    public DateTime CloseTime;
    public string Comment;
    public double Commission;
    public TradeRecord Ex;
    public DateTime Expiration;
    public double Lots;
    public int MagicNumber;
    public double OpenPrice;
    public DateTime OpenTime;
    public double Profit;
    public double RateClose;
    public double RateMargin;
    public double RateOpen;
    public double StopLoss;
    public double Swap;
    public string Symbol;
    public double TakeProfit;
    public int Ticket;
    public Op Type;

    public bool PlacedManually => Ex.place_type == 0;

    public override string ToString() => $"{Ticket} {Symbol} {Type}";
}
