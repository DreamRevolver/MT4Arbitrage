using System.Collections.Immutable;
using Shared.Models;
using TradingAPI.MT4Server;

namespace QuoteObserver;

public class QuoteClientObserver : QuoteClient
{
    public BrokerType brokerType { get; }
    public ImmutableDictionary<string, (double, double)> lastBidAsks = ImmutableDictionary<string, (double, double)>.Empty;

    public QuoteClientObserver(BrokerType brokerType)
    {
        this.brokerType = brokerType;
    }

    public void Update(string symbol, double bid, double ask)
    {
        ImmutableInterlocked.AddOrUpdate(ref lastBidAsks, symbol, (bid, ask), (_, tuple) => tuple);
    }
}
