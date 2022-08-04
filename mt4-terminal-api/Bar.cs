namespace TradingAPI.MT4Server;

public struct Bar
{
    public DateTime Time;
    public double Open;
    public double High;
    public double Low;
    public double Close;
    public double Volume;

    public override string ToString() => $"{Time} {Open} {High} {Low} {Close}";
}
