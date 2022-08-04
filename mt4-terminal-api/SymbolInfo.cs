namespace TradingAPI.MT4Server;

public struct SymbolInfo
{
    public Execution Execution;
    public int StopsLevel;
    public int Digits;
    public double Point;
    public double SwapLong;
    public double SwapShort;
    public int Spread;
    public int FreezeLevel;
    public string MarginCurrency;
    public ProfitMode ProfitMode;
    public MarginMode MarginMode;
    public double ContractSize;
    public string Currency;
    public double MarginDivider;
    public SymbolInfoEx Ex;
    public ushort Code;

    public override string ToString() => $"{Digits} {StopsLevel} {Execution}";
}