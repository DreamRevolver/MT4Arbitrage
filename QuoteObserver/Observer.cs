using System.Collections.Immutable;
using Shared.Extensions;
using Shared.Interfaces;
using Shared.Models;
using Shared.Utility;
using TradingAPI.MT4Server;
using Utf8Json;
using ILogger = log4net.Core.ILogger;

namespace QuoteObserver;

public class Observer : IArbitrationObserver
{
    
    private readonly ILogger _logger;

    private ImmutableDictionary<string, Pair> _pairs = ImmutableDictionary<string, Pair>.Empty;
    private ImmutableDictionary<string, IBrokerConnector> _fastBrokers = ImmutableDictionary<string, IBrokerConnector>.Empty;
    private ImmutableDictionary<string, IBrokerConnector> _slowBrokers = ImmutableDictionary<string, IBrokerConnector>.Empty;
    private ImmutableDictionary<string, Arbitration> _arbitrations = ImmutableDictionary<string, Arbitration>.Empty;

    public event EventHandler<Arbitration> OnArbitrationUpdate = delegate { };

    private static string GetArbitrationId(string symbol, IBrokerConnector fast, IBrokerConnector slow)
        => $"{symbol}|{fast.Name}|{slow.Name}";

    private BrokerType GetBrokerType(IBrokerConnector brokerConnector)
    {
        var name = brokerConnector.Name;
        Ensure.That(_fastBrokers.ContainsKey(name) || _slowBrokers.ContainsKey(name), "error message");
        return _fastBrokers.ContainsKey(name) ? BrokerType.Fast : BrokerType.Slow;
    }

    private void ArbitrationUpdateArbitrationUpdateHandler(object? sender, Arbitration arbitration)
    {
        _logger.Info(JsonSerializer.ToJsonString(arbitration), string.Empty);
    }
    
    private void UpdateArbitration(Arbitration arbitration)
    {
        if (arbitration.FastLast is not { } fastLast || arbitration.SlowLast is not { } slowLast)
        {
            return;
        }
        var pair = arbitration.Pair;
        var fast = fastLast.Bid - slowLast.Ask > pair.Spread;
        var slow = slowLast.Bid - fastLast.Ask > pair.Spread;
        var isActive = fast || slow;
        if (isActive == arbitration.IsActive)
        {
            return;
        }
        arbitration.IsActive = isActive;
        OnArbitrationUpdate(this, arbitration);
        // if (isActive)
        // {
        //     _logger.Info($"ARBITRAGE {pair.Symbol} {(fast ? arbitration.FastName : arbitration.SlowName)} Bid ({(fast ? fastLast.Bid : slowLast.Bid)}) - {(fast ?  arbitration.SlowName : arbitration.FastName)} Ask ({(fast ? slowLast.Ask : fastLast.Ask)}) > {pair.Spread}", "Observer");
        // }
    }

    private void TickHandler(object sender, MarketBook marketBook)
    {
        var brokerConnector = (IBrokerConnector) sender;
        var brokerType = GetBrokerType(brokerConnector);
        if (brokerType == BrokerType.Slow)
        {
            foreach (var fastBroker in _fastBrokers.Values)
            {
                var id = GetArbitrationId(marketBook.Pair, fastBroker, brokerConnector);
                var arbitration = _arbitrations[id];
                lock (arbitration)
                {
                    arbitration.Update(BrokerType.Slow, marketBook);
                    UpdateArbitration(arbitration);
                }
            }
        }
        else
        {
            foreach (var slowBroker in _slowBrokers.Values)
            {
                var id = GetArbitrationId(marketBook.Pair, brokerConnector, slowBroker);
                var arbitration = _arbitrations[id];
                lock (arbitration)
                {
                    arbitration.Update(BrokerType.Fast, marketBook);
                    UpdateArbitration(arbitration);
                }
            }
        }
    }

    public Observer(ILogger logger)
    {
        _logger = logger;
        //OnArbitrationUpdate += ArbitrationUpdateArbitrationUpdateHandler;
    }

    public void AddPairs(IEnumerable<Pair> pairs)
    {
        pairs.ToList().ForEach(pair =>
        {
            ImmutableInterlocked.Update(ref _pairs, _pairs => _pairs.Add(pair.Symbol, pair));
            _logger.Info($"ADD [Pair: {pair.Symbol}, Spread: {pair.Spread}]", "Observer");
        });
    }

    private void AddFastBroker(IBrokerConnector broker)
    {
        ImmutableInterlocked.AddOrUpdate(ref _fastBrokers, broker.Name, broker, (_, client) => client);
        _logger.Info($"ADD FAST BROKER {broker.Name}", "Observer");
        foreach (var brokerConnector in _slowBrokers.Select(i => i.Value))
        {
            foreach (var pair in Pairs)
            {
                _arbitrations = _arbitrations.Add(GetArbitrationId(pair.Symbol, broker, brokerConnector), new(broker, brokerConnector, pair));
            }
        }
    }

    private void AddSlowBroker(IBrokerConnector broker)
    {
        ImmutableInterlocked.AddOrUpdate(ref _slowBrokers, broker.Name, broker, (_, client) => client);
        _logger.Info($"ADD SLOW BROKER {broker.Name}", "Observer");
        foreach (var brokerConnector in _fastBrokers.Select(i => i.Value))
        {
            foreach (var pair in Pairs)
            {
                _arbitrations = _arbitrations.Add(GetArbitrationId(pair.Symbol, brokerConnector, broker), new(brokerConnector, broker, pair));
            }
        }
    }
    public IEnumerable<Pair> Pairs => _pairs.Values;

    public void AddConnector(IBrokerConnector brokerConnector, BrokerType brokerType)
    {
        //brokerConnector.UploadPairs(Pairs);
        switch (brokerType)
        {
            case BrokerType.Fast:
            {
                AddFastBroker(brokerConnector);
                break;
            }
            case BrokerType.Slow:
            {
                AddSlowBroker(brokerConnector);
                break;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(brokerType), brokerType, null);
        }
        brokerConnector.OnTick += TickHandler;
        // foreach (var pair in Pairs)
        // {
        //     brokerConnector.Subscribe(new()
        //     {
        //         Symbol = pair.Symbol
        //     });
        // }
    }
}
