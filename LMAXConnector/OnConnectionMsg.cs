using QuickFix.Fields;
using QuickFix.FIX44;
using Shared.Models;

namespace LMAXConnector;

public sealed class OnConnectionMsg : Logon
{
    public BrokerEvent Event { get; init; }
    internal OnConnectionMsg() { }
    internal OnConnectionMsg(EncryptMethod aEncryptMethod, HeartBtInt aHeartBtInt) : base(aEncryptMethod, aHeartBtInt) { }
    public static implicit operator BrokerEvent(OnConnectionMsg onConnectionMsg) => onConnectionMsg.Event;
}
