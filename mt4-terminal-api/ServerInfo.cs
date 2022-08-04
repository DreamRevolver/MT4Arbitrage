using System.Runtime.InteropServices;

namespace TradingAPI.MT4Server;

public struct ServerInfo
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string name;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string comment;
}