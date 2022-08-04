using log4net.Core;
using Shared.Extensions;
using Shared.Interfaces;
using Shared.Models;
using TradingAPI.MT4Server;

namespace QuoteObserver;

public class QuoteClientConnector : IBrokerConnector
{
    public QuoteClientConnector(QuoteClient quoteClient, ILogger logger)
    {
        _quoteClient = quoteClient;
        _logger = logger;
        _quoteClient.OnConnect += OnConnect;
        _quoteClient.OnQuote += OnQuote;
        _logger.Info("CREATE CONNECTOR", $"{Name}");
    }

    public string Name => _quoteClient.User.ToString();
    public bool IsConnected => _quoteClient.Connected;
    public IEnumerable<Instrument> Instruments { get; set; }
    public event Action<BrokerEvent>? OnChannelEvent;
    public event EventHandler<MarketBook>? OnTick;
    private readonly QuoteClient _quoteClient;
    private ILogger _logger;

    private void OnQuote(object sender, QuoteEventArgs args)
    {
        MarketBook marketBook = new MarketBook()
        {
            Ask = args.Ask,
            Bid = args.Bid,
            Pair = args.Symbol,
            Time = args.Time
        };
        OnTick(this,marketBook);
    }

    private void OnConnect(object sender, ConnectEventArgs args)
    {
        _logger.Info("CONNECTED", $"{Name}");
        foreach (var instr in Instruments)
        {
            Subscribe(instr);
        }
    }

    public void UploadPairs(IEnumerable<Pair> pairs)
    {
        List<Instrument> instruments = new List<Instrument>();
        foreach (var pair in pairs)
        {
            instruments.Add(new Instrument(){Symbol = pair.Symbol});
        }

        Instruments = instruments;
    }

    public void Subscribe(Instrument instrument)
    {
        try
        {
            _quoteClient.Subscribe(instrument.Symbol);
            _logger.Info($"SUBSCRIBE [Pair: {instrument.Symbol}]", $"{Name}");
        }
        catch (Exception exc)
        {
            _logger.Error($"SUBSCRIBE FAILED [Error: {exc.Message}, StackTrace: {exc.StackTrace}]", $"{Name}");
        }
    }

    public void Start()
    {
        try
        {
            _logger.Info("START CLIENT", $"{Name}");
            _quoteClient.Connect();
        }
        catch (Exception exc)
        {
            _logger.Error($"FAILED TO START [Error: {exc.Message}, StackTrace: {exc.StackTrace}]", $"{Name}");
        }
    }

    public void Stop()
    {
        try
        {
            _logger.Info("STOP CLIENT", $"{Name}");
            _quoteClient.Disconnect();
        }
        catch (Exception exc)
        {
            _logger.Error($"FAILED TO STOP [Error: {exc.Message}, StackTrace: {exc.StackTrace}]", $"{Name}");
        }
    }
}
