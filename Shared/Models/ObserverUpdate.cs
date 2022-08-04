namespace Shared.Models;

public struct ObserverUpdate
{
    public string pair;
    public string fastBroker;
    public string slowBroker;
    public MarketBook fastMarketBook;
    public MarketBook slowMarketBook;
    public double spread;

    public ObserverUpdate(string pair, string fastBroker, string slowBroker, MarketBook fastMarketBook, MarketBook slowMarketBook, double spread)
    {
        this.pair = pair;
        this.fastBroker = fastBroker;
        this.slowBroker = slowBroker;
        this.fastMarketBook = fastMarketBook;
        this.slowMarketBook = slowMarketBook;
        this.spread = spread;
    }

    public double Delta => fastMarketBook.Bid - slowMarketBook.Ask;
}
