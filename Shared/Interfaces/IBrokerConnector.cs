using Shared.Models;

namespace Shared.Interfaces;

public interface IBrokerConnector
{
    string Name { get; }

    bool IsConnected { get; }

    IEnumerable<Instrument> Instruments { get; set; }

    event Action<BrokerEvent> OnChannelEvent;

    event EventHandler<MarketBook> OnTick;

    void UploadPairs(IEnumerable<Pair> pairs);
    void Subscribe(Instrument instrument);

    void Start();

    void Stop();
}
