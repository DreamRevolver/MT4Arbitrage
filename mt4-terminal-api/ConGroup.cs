using System.Runtime.InteropServices;

namespace TradingAPI.MT4Server;

public struct ConGroup
{
    private const int MAX_SEC_GROUPS = 32;
    private const int MAX_SEC_GROPS_MARGIN = 128;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
    public string group;

    public int enable;
    public int timeout;
    public int adv_security;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string company;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string signature;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string smtp_server;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string smtp_login;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string smtp_password;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string support_email;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string templates;

    public int copies;
    public int reports;
    public int default_leverage;
    public double default_deposit;
    public int maxsecurities;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32, ArraySubType = UnmanagedType.Struct)]
    public ConGroupSec[] secgroups;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128, ArraySubType = UnmanagedType.Struct)]
    public ConGroupMargin[] secmargins;

    public int secmargins_total;

    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
    public string currency;

    public double credit;
    public int margin_call;
    public int margin_mode;
    public int margin_stopout;
    public double interestrate;
    public int use_swap;
    public int news;
    public int rights;
    public int check_ie_prices;
    public int maxpositions;
    public int close_reopen;
    public int hedge_prohibited;
    public int close_fifo;
    internal int s34E8;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
    public int[] unused_rights;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
    public byte[] securities_hash;

    public int margin_type;
    public int archive_period;
    public int archive_max_balance;
    public int stopout_skip_hedged;
    public int archive_pending_period;

    [MarshalAs(UnmanagedType.ByValArray, SizeConst = 26)]
    public int[] reserved;

    public bool IsHiRisk => (uint) (rights & 64) > 0U;

    public void ResetHiRisk()
    {
        rights &= -65;
    }
}