using Shared.Models;

namespace Shared.Interfaces;

public interface IArbitrationObserver
{
    event EventHandler<Arbitration> OnArbitrationUpdate;
    IEnumerable<Pair> Pairs { get;}
    void AddPairs(IEnumerable<Pair> pairs);
    void AddConnector(IBrokerConnector brokerConnector, BrokerType brokerType);
}
