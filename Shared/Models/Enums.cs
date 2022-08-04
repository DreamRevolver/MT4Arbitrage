namespace Shared.Models;

public enum BrokerEvent
{
    SessionLogon,
    SessionLogout,
    SessionError,
    SessionMessage,
    FailureRequest,
    Subscribed,
    Unsubscribed
}

public enum BrokerType
{
    Fast,
    Slow
}
