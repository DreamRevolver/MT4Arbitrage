using System.Runtime.InteropServices;

namespace TradingAPI.MT4Server;

public struct ConSession
{
    public short open_hour;
    public short open_min;
    public short close_hour;
    public short close_min;
    public int open;
    public int close;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
    public short[] align;
}