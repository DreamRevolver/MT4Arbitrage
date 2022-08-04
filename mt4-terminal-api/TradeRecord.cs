using System.Runtime.InteropServices;

namespace TradingAPI.MT4Server;

public struct TradeRecord
{
    public int order;
    public int login;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
    public byte[] symbol;

    public int digits;
    public int cmd;
    public int volume;
    public int open_time;
    public int state;
    public double open_price;
    public double sl;
    public double tp;
    public int close_time;
    public int value_date;
    public int expiration;
    public int place_type;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public double[] conv_rates;

    public double commission;
    public double commission_agent;
    public double storage;
    public double close_price;
    public double profit;
    public double taxes;
    public int magic;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string comment;

    public int internal_id;
    public int activation;
    public int spread;
    public double margin_rate;
    public int timestamp;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
    public int[] reserved;

    public int next;
}