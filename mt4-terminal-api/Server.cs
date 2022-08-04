using System.Runtime.InteropServices;

namespace TradingAPI.MT4Server;

public struct Server
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string server;

    public int ip;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string desc;

    public int is_proxy;
    public int priority;
    public int loading;
    public int ip_internal;
    public int ping;
    public int reserved;
    public int ptr_next;

    public string Host => HostAndPort.parseStr(server).Key;

    public int Port => HostAndPort.parseStr(server).Value;
}