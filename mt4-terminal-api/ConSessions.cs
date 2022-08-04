using System.Runtime.InteropServices;

namespace TradingAPI.MT4Server;

public struct ConSessions
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3, ArraySubType = UnmanagedType.Struct)]
    public ConSession[] quote;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3, ArraySubType = UnmanagedType.Struct)]
    public ConSession[] trade;

    public int quote_overnight;
    public int trade_overnight;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public int[] reserved;
}