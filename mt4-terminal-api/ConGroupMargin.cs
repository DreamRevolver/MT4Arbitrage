using System.Runtime.InteropServices;

namespace TradingAPI.MT4Server;

public struct ConGroupMargin
{
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
    public byte[] symbol;

    public double swap_long;
    public double swap_short;
    public double margin_divider;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
    public int[] reserved;
}