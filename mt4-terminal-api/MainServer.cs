using System.Runtime.InteropServices;

namespace TradingAPI.MT4Server;

public struct MainServer
{
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string name;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string comment;

    public int is_demo;
    public int ping;
    public int dummy_C8;
    public int dummy_CC;
    public int ptr_dataserver;
    public int count;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string host_addr;

    public int time;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public int[] dummy_11C;

    public int ptr_next;

    public string Host => HostAndPort.parseStr(host_addr).Key;

    public int Port => HostAndPort.parseStr(host_addr).Value;
}