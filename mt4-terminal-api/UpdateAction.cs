namespace TradingAPI.MT4Server;

public enum UpdateAction
{
    PositionOpen,
    PositionClose,
    PositionModify,
    PendingOpen,
    PendingClose,
    PendingModify,
    PendingFill,
    Balance,
    Credit
}