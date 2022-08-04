using System.Collections.Immutable;
using Autofac;
using QuickFix;
using QuickFix.Fields;
using Shared.Extensions;
using Shared.Interfaces;
using Shared.Models;
using ILogger = log4net.Core.ILogger;

namespace LMAXConnector;

public partial class FixLMAXBrokerConnector : MessageCracker, IBrokerConnector
{
    private readonly IInitiator _initiator;
    private readonly ILifetimeScope _lifetimeScope;
    private SessionSettings _settings;
    private readonly List<Instrument> PairsToSubscribe = new List<Instrument>();
    public FixLMAXBrokerConnector(ILifetimeScope lifetimeScope, IInitiator initiator, SessionSettings settings, ILogger logger)
    {
        _lifetimeScope = lifetimeScope;
        _initiator = initiator;
        _settings = settings;
        _logger = logger;
        Instruments = _lifetimeScope.ResolveNamed<List<Instrument>>("PAIRS");
    }
    public event EventHandler<MarketBook> OnTick = delegate { };
    public event Action<BrokerEvent> OnChannelEvent = delegate { };

    public string Name => "LMAX";

    public bool IsConnected { get; }
    public IEnumerable<Instrument> Instruments { get; set; }
    private ILogger _logger;


    public SessionID _sess
    {
        get { return _initiator.GetSessionIDs().First(); }
    }
    private void SubscribeOnInstruments(BrokerEvent brokerEvent)
    {
        switch (brokerEvent)
        {
            case BrokerEvent.SessionLogon:
            {
                foreach (var inst in PairsToSubscribe)
                {
                    Subscribe(new Instrument{Symbol = inst.Symbol, ConId = inst.ConId});
                }
                break;
            }
        }
    }

    
    public void UploadPairs(IEnumerable<Pair> pairs)
    {
        pairs.ToList().ForEach(pair =>
        {
            if (Instruments.Any(i => i.Symbol == pair.Symbol))
            {
                PairsToSubscribe.Add(Instruments.FirstOrDefault(i => i.Symbol == pair.Symbol));
            }
        });
    }
    public void Subscribe(Instrument instrument)
    {
        var mdRequest = new MarketDataRequest();
        mdRequest.Set(new MDReqID(instrument.ConId.ToString()));
        var type = SubscriptionRequestType.SNAPSHOT_PLUS_UPDATES;
        mdRequest.Set(new SubscriptionRequestType(type));
        // mdRequest.Set(new QuickFix.Fields.MarketDepth(model== SubscriptionModel.TopBook?1:0));// top of book
        mdRequest.Set(new MarketDepth(5)); // top of book
        mdRequest.Set(new MDUpdateType(0)); // LMAX supported values 0 - Full Refresh
        var entryTypesbid
            = new MarketDataRequest.NoMDEntryTypesGroup();
        entryTypesbid.Set(new MDEntryType(MDEntryType.BID));
        mdRequest.AddGroup(entryTypesbid);
        var entryTypesask
            = new MarketDataRequest.NoMDEntryTypesGroup();
        entryTypesask.Set(new MDEntryType(MDEntryType.OFFER));
        mdRequest.AddGroup(entryTypesask);
        mdRequest.Set(new NoRelatedSym(1));
        var InstrGroup =
            new MarketDataRequest.NoRelatedSymGroup();
        //InstrGroup.Set(new SymbolSfx(instrument.Symbol));
        InstrGroup.Set(new SecurityID(instrument.ConId.ToString()));
        InstrGroup.Set(new SecurityIDSource("8"));
        mdRequest.AddGroup(InstrGroup);
        try
        {
            _logger.Info($"Subscribing to {instrument.Symbol}", "FixLMAXBrokerConnector.Subscribe");
            Session.SendToTarget(mdRequest, _sess);
        }
        catch (Exception ex)
        {
            _logger.Error($"Subscribing error - {ex.Message}", "FixLMAXBrokerConnector.Subscribe");
        }
    }

    public void Start()
    {
        try
        {
            _logger.Info("Starting LMAX FIX connector", "FixLMAXBrokerConnector");
            OnChannelEvent += @event => { SubscribeOnInstruments(@event);};
            _initiator.Start();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "FixLMAXBrokerConnector.Start");
        }
    }

    public void Stop()
    {
        try
        {
            _logger.Info("Stoping LMAX FIX connector", "FixLMAXBrokerConnector");
            _initiator.Stop();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "FixLMAXBrokerConnector.Stop");
        }
    }
}
