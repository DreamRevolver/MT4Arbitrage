using System.Runtime.InteropServices;

namespace TradingAPI.MT4Server;

public struct ConSymbolGroup
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
    public string name;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string description;
}