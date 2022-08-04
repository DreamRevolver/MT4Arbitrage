using System.Runtime.InteropServices;

namespace TradingAPI.MT4Server;

public struct ConGroupSec
{
    public int show;
    public int trade;
    public int execution;
    public double comm_base;
    public int comm_type;
    public int comm_lots;
    public double comm_agent;
    public int comm_agent_type;
    public int spread_diff;
    public int lot_min;
    public int lot_max;
    public int lot_step;
    public int ie_deviation;
    public int confirmation;
    public int trade_rights;
    public int ie_quick_mode;
    public int autocloseout_mode;
    public double comm_tax;
    public int comm_agent_lots;
    public int freemargin_mode;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
    public int[] reserved;
}