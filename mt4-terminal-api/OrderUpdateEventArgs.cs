namespace TradingAPI.MT4Server;

public struct OrderUpdateEventArgs
{
    public Order Order;
    public UpdateAction Action;
}