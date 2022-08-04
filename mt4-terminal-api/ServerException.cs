namespace TradingAPI.MT4Server;

public class ServerException : Exception
{
    public readonly int Code;

    public ServerException(int code)
        : base(ErrorDescription.get(code)) => Code = code;
}
