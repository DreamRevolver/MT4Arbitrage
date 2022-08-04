using System.Runtime.InteropServices;

namespace TradingAPI.MT4Server;

[StructLayout(LayoutKind.Sequential)]
public class SymbolInfoEx
{
    public double ask_tickvalue;
    public int background_color;
    public double bid_tickvalue;
    public double contract_size;
    public int count;
    public int count_original;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
    public string currency;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string description;

    public int digits;
    public int exemode;
    public int expiration;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
    public int[] external_unused;

    public int filter;
    public int filter_counter;
    public double filter_limit;
    public int filter_reserved;
    public int filter_smoothing;
    public int freeze_level;
    public int gtc_pendings;
    public int instant_max_volume;
    public int logging;
    public int long_only;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
    public string margin_currency;

    public double margin_divider;
    public double margin_hedged;
    public int margin_hedged_strong;
    public double margin_initial;
    public double margin_maintenance;
    public int margin_mode;
    public double multiply;
    public double point;
    public int profit_mode;
    public int profit_reserved;
    public int quotes_delay;
    public int realtime;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7, ArraySubType = UnmanagedType.Struct)]
    public ConSessions[] sessions;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
    public string source;

    public int spread;
    public int spread_balance;
    public int starting;
    public int stops_level;
    public int swap_enable;
    public double swap_long;
    public int swap_openprice;
    public int swap_rollover3days;
    public double swap_short;
    public int swap_type;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
    public byte[] symbol;

    public double tick_size;
    public double tick_value;
    public int trade;
    public int type;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
    public int[] unused;

    public int value_date;
}