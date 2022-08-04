namespace TradingAPI.MT4Server;

public enum ProgressType
{
    Rejected,
    Accepted,
    InProcess,
    Opened,
    Closed,
    Modified,
    PendingDeleted,
    ClosedBy,
    MultipleClosedBy,
    Timeout,
    Price,
    Exception
}