using System.Collections.Immutable;
using Shared.Interfaces;
using Shared.Models;

namespace QuoteObserver;

internal class LmaxFixConnectorObserver
{
    public ImmutableDictionary<string, (double, double)> lastBidAsks = ImmutableDictionary<string, (double, double)>.Empty;

    public void Update(ITickData data)
    {
        ImmutableInterlocked.AddOrUpdate(ref lastBidAsks, data.Pair, (data.Bid, data.Ask), (_, tuple) => tuple);
    }
}
