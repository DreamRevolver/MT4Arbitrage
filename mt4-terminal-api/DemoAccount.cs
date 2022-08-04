namespace TradingAPI.MT4Server;

public struct DemoAccount
{
    public uint User;
    public string Password;
    public string Investor;

    public override string ToString() => $"{User} {Password} {Investor}";
}