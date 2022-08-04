using Shared.Interfaces;

namespace Shared.Models;

public sealed class Arbitration
{
    
    private readonly IBrokerConnector _fast;
    private readonly IBrokerConnector _slow;
    private bool _isActive;
    public string FastName => _fast.Name;
    public string SlowName => _slow.Name;

    public MarketBook? FastLast { get; private set; }
    public MarketBook? SlowLast { get; private set; }
    public Pair Pair { get; }
    public bool Flag { get; private set; }

    public double Delta
    {
        get
        {
            if (SlowLast != null && FastLast != null)
            {
                return SlowLast.Value.Bid - FastLast.Value.Ask > Pair.Spread ? SlowLast.Value.Bid - FastLast.Value.Ask : FastLast.Value.Bid - SlowLast.Value.Ask;
            }
            return 0;
        }
    }

    public Arbitration(IBrokerConnector fast, IBrokerConnector slow, Pair pair)
    {
        _fast = fast;
        _slow = slow;
        Pair = pair;
    }

    public bool IsActive
    {
        get => _isActive;
        set
        {
            _isActive = value;
            if (value)
            {
                Flag = FastLast.Value.Bid - SlowLast.Value.Ask > Pair.Spread;
            }
        }
    }

    public void Update(BrokerType brokerType, MarketBook marketBook)
    {
        if (brokerType == BrokerType.Slow)
        {
            FastLast = marketBook;
        }
        else
        {
            SlowLast = marketBook;
        }
    }
    
}
