using System.Runtime.InteropServices;
using System.Text;

namespace TradingAPI.MT4Server;

internal class UDT
{
    public static T ReadStructOld<T>(Stream fs)
    {
        var data = new byte[fs.Length];
        fs.Read(data, 0, data.Length);
        return ReadStructOld<T>(data, 0);
    }

    public static void WriteStruct(Stream fs, object obj)
    {
        var length = Marshal.SizeOf(obj);
        var destination = new byte[length];
        var num = Marshal.AllocHGlobal(length);
        Marshal.StructureToPtr(obj, num, false);
        Marshal.Copy(num, destination, 0, length);
        fs.Write(destination, 0, length);
        Marshal.FreeHGlobal(num);
    }

    public static T ReadStructOld<T>(byte[] data, int offset)
    {
        var num1 = Marshal.SizeOf(typeof(T));
        var num2 = Marshal.AllocHGlobal(num1);
        Marshal.Copy(data, offset, num2, num1);
        var structure = (T) Marshal.PtrToStructure(num2, typeof(T));
        Marshal.FreeHGlobal(num2);
        return structure;
    }

    public static byte[] WriteStruct(object obj)
    {
        var numArray = new byte[Marshal.SizeOf(obj)];
        var gcHandle = GCHandle.Alloc(numArray, (GCHandleType) 3);
        Marshal.StructureToPtr(obj, gcHandle.AddrOfPinnedObject(), false);
        gcHandle.Free();
        return numArray;
    }

    public static T ReadStruct<T>(byte[] buf, int of)
    {
        var name = typeof(T).Name;
        switch (name)
        {
            case "ConSession":
            {
                var conSession = new ConSession
                {
                    open_hour = BitConverter.ToInt16(buf, of),
                    open_min = BitConverter.ToInt16(buf, of + 2),
                    close_hour = BitConverter.ToInt16(buf, of + 4),
                    close_min = BitConverter.ToInt16(buf, of + 6),
                    open = BitConverter.ToInt32(buf, of + 8),
                    close = BitConverter.ToInt32(buf, of + 12),
                    align = new short[8]
                };
                for (var index = 0; index < 8; ++index)
                    conSession.align[index] = BitConverter.ToInt16(buf, of + 16 + index * 2);
                return (T) Convert.ChangeType(conSession, typeof(T));
            }
            case "ConSessions":
            {
                var conSessions = new ConSessions
                {
                    quote = new ConSession[3]
                };
                for (var index1 = 0; index1 < 3; ++index1)
                {
                    var conSession = new ConSession
                    {
                        open_hour = BitConverter.ToInt16(buf, of + index1 * 32),
                        open_min = BitConverter.ToInt16(buf, of + index1 * 32 + 2),
                        close_hour = BitConverter.ToInt16(buf, of + index1 * 32 + 4),
                        close_min = BitConverter.ToInt16(buf, of + index1 * 32 + 6),
                        open = BitConverter.ToInt32(buf, of + index1 * 32 + 8),
                        close = BitConverter.ToInt32(buf, of + index1 * 32 + 12),
                        align = new short[8]
                    };
                    for (var index2 = 0; index2 < 8; ++index2)
                        conSession.align[index2] = BitConverter.ToInt16(buf, of + index1 * 32 + 16 + index2 * 2);
                    conSessions.quote[index1] = conSession;
                }

                conSessions.trade = new ConSession[3];
                for (var index3 = 0; index3 < 3; ++index3)
                {
                    var conSession = new ConSession
                    {
                        open_hour = BitConverter.ToInt16(buf, of + index3 * 32 + 96),
                        open_min = BitConverter.ToInt16(buf, of + index3 * 32 + 98),
                        close_hour = BitConverter.ToInt16(buf, of + index3 * 32 + 100),
                        close_min = BitConverter.ToInt16(buf, of + index3 * 32 + 102),
                        open = BitConverter.ToInt32(buf, of + index3 * 32 + 104),
                        close = BitConverter.ToInt32(buf, of + index3 * 32 + 108),
                        align = new short[8]
                    };
                    for (var index4 = 0; index4 < 8; ++index4)
                        conSession.align[index4] = BitConverter.ToInt16(buf, of + index3 * 32 + 112 + index4 * 2);
                    conSessions.trade[index3] = conSession;
                }

                conSessions.quote_overnight = BitConverter.ToInt32(buf, of + 192);
                conSessions.trade_overnight = BitConverter.ToInt32(buf, of + 196);
                conSessions.reserved = new int[2];
                for (var index = 0; index < 2; ++index)
                    conSessions.reserved[index] = BitConverter.ToInt32(buf, of + 200 + index * 4);
                return (T) Convert.ChangeType(conSessions, typeof(T));
            }
            case "TradeRecord":
            {
                var tradeRecord = new TradeRecord
                {
                    order = BitConverter.ToInt32(buf, of),
                    login = BitConverter.ToInt32(buf, of + 4),
                    symbol = new byte[12]
                };
                for (var index = 0; index < 12; ++index)
                    tradeRecord.symbol[index] = buf[of + 8 + index];
                tradeRecord.digits = BitConverter.ToInt32(buf, of + 20);
                tradeRecord.cmd = BitConverter.ToInt32(buf, of + 24);
                tradeRecord.volume = BitConverter.ToInt32(buf, of + 28);
                tradeRecord.open_time = BitConverter.ToInt32(buf, of + 32);
                tradeRecord.state = BitConverter.ToInt32(buf, of + 36);
                tradeRecord.open_price = BitConverter.ToDouble(buf, of + 40);
                tradeRecord.sl = BitConverter.ToDouble(buf, of + 48);
                tradeRecord.tp = BitConverter.ToDouble(buf, of + 56);
                tradeRecord.close_time = BitConverter.ToInt32(buf, of + 64);
                tradeRecord.value_date = BitConverter.ToInt32(buf, of + 68);
                tradeRecord.expiration = BitConverter.ToInt32(buf, of + 72);
                tradeRecord.place_type = buf[of + 76];
                tradeRecord.conv_rates = new double[2];
                for (var index = 0; index < 2; ++index)
                    tradeRecord.conv_rates[index] = BitConverter.ToDouble(buf, of + 80 + index * 8);
                tradeRecord.commission = BitConverter.ToDouble(buf, of + 96);
                tradeRecord.commission_agent = BitConverter.ToDouble(buf, of + 104);
                tradeRecord.storage = BitConverter.ToDouble(buf, of + 112);
                tradeRecord.close_price = BitConverter.ToDouble(buf, of + 120);
                tradeRecord.profit = BitConverter.ToDouble(buf, of + 128);
                tradeRecord.taxes = BitConverter.ToDouble(buf, of + 136);
                tradeRecord.magic = BitConverter.ToInt32(buf, of + 144);
                tradeRecord.comment = readString(buf, of + 148, 32);
                tradeRecord.internal_id = BitConverter.ToInt32(buf, of + 180);
                tradeRecord.activation = BitConverter.ToInt32(buf, of + 184);
                tradeRecord.spread = BitConverter.ToInt32(buf, of + 188);
                tradeRecord.margin_rate = BitConverter.ToDouble(buf, of + 192);
                tradeRecord.timestamp = BitConverter.ToInt32(buf, of + 200);
                tradeRecord.reserved = new int[4];
                for (var index = 0; index < 4; ++index)
                    tradeRecord.reserved[index] = BitConverter.ToInt32(buf, of + 204 + index * 4);
                tradeRecord.next = BitConverter.ToInt32(buf, of + 220);
                return (T) Convert.ChangeType(tradeRecord, typeof(T));
            }
            case "SymbolInfoEx":
            {
                var symbolInfoEx = new SymbolInfoEx
                {
                    symbol = new byte[12]
                };
                for (var index = 0; index < 12; ++index)
                    symbolInfoEx.symbol[index] = buf[of + index];
                symbolInfoEx.description = readString(buf, of + 12, 64);
                symbolInfoEx.source = readString(buf, of + 76, 12);
                symbolInfoEx.currency = readString(buf, of + 88, 12);
                symbolInfoEx.type = BitConverter.ToInt32(buf, of + 100);
                symbolInfoEx.digits = BitConverter.ToInt32(buf, of + 104);
                symbolInfoEx.trade = BitConverter.ToInt32(buf, of + 108);
                symbolInfoEx.background_color = BitConverter.ToInt32(buf, of + 112);
                symbolInfoEx.count = BitConverter.ToInt32(buf, of + 116);
                symbolInfoEx.count_original = BitConverter.ToInt32(buf, of + 120);
                symbolInfoEx.external_unused = new int[7];
                for (var index = 0; index < 7; ++index)
                    symbolInfoEx.external_unused[index] = BitConverter.ToInt32(buf, of + 124 + index * 4);
                symbolInfoEx.realtime = BitConverter.ToInt32(buf, of + 152);
                symbolInfoEx.starting = BitConverter.ToInt32(buf, of + 156);
                symbolInfoEx.expiration = BitConverter.ToInt32(buf, of + 160);
                symbolInfoEx.sessions = new ConSessions[7];
                for (var index5 = 0; index5 < 7; ++index5)
                {
                    var conSessions = new ConSessions
                    {
                        quote = new ConSession[3]
                    };
                    for (var index6 = 0; index6 < 3; ++index6)
                    {
                        var conSession = new ConSession
                        {
                            open_hour = BitConverter.ToInt16(buf, of + index5 * 208 + index6 * 32 + 164),
                            open_min = BitConverter.ToInt16(buf, of + index5 * 208 + index6 * 32 + 166),
                            close_hour = BitConverter.ToInt16(buf, of + index5 * 208 + index6 * 32 + 168),
                            close_min = BitConverter.ToInt16(buf, of + index5 * 208 + index6 * 32 + 170),
                            open = BitConverter.ToInt32(buf, of + index5 * 208 + index6 * 32 + 172),
                            close = BitConverter.ToInt32(buf, of + index5 * 208 + index6 * 32 + 176),
                            align = new short[8]
                        };
                        for (var index7 = 0; index7 < 8; ++index7)
                            conSession.align[index7] = BitConverter.ToInt16(buf, of + index5 * 208 + index6 * 32 + 180 + index7 * 2);
                        conSessions.quote[index6] = conSession;
                    }

                    conSessions.trade = new ConSession[3];
                    for (var index8 = 0; index8 < 3; ++index8)
                    {
                        var conSession = new ConSession
                        {
                            open_hour = BitConverter.ToInt16(buf, of + index5 * 208 + index8 * 32 + 260),
                            open_min = BitConverter.ToInt16(buf, of + index5 * 208 + index8 * 32 + 262),
                            close_hour = BitConverter.ToInt16(buf, of + index5 * 208 + index8 * 32 + 264),
                            close_min = BitConverter.ToInt16(buf, of + index5 * 208 + index8 * 32 + 266),
                            open = BitConverter.ToInt32(buf, of + index5 * 208 + index8 * 32 + 268),
                            close = BitConverter.ToInt32(buf, of + index5 * 208 + index8 * 32 + 272),
                            align = new short[8]
                        };
                        for (var index9 = 0; index9 < 8; ++index9)
                            conSession.align[index9] = BitConverter.ToInt16(buf, of + index5 * 208 + index8 * 32 + 276 + index9 * 2);
                        conSessions.trade[index8] = conSession;
                    }

                    conSessions.quote_overnight = BitConverter.ToInt32(buf, of + index5 * 208 + 356);
                    conSessions.trade_overnight = BitConverter.ToInt32(buf, of + index5 * 208 + 360);
                    conSessions.reserved = new int[2];
                    for (var index10 = 0; index10 < 2; ++index10)
                        conSessions.reserved[index10] = BitConverter.ToInt32(buf, of + index5 * 208 + 364 + index10 * 4);
                    symbolInfoEx.sessions[index5] = conSessions;
                }

                symbolInfoEx.profit_mode = BitConverter.ToInt32(buf, of + 1620);
                symbolInfoEx.profit_reserved = BitConverter.ToInt32(buf, of + 1624);
                symbolInfoEx.filter = BitConverter.ToInt32(buf, of + 1628);
                symbolInfoEx.filter_counter = BitConverter.ToInt32(buf, of + 1632);
                symbolInfoEx.filter_limit = BitConverter.ToDouble(buf, of + 1636 + 4);
                symbolInfoEx.filter_smoothing = BitConverter.ToInt32(buf, of + 1644 + 4);
                symbolInfoEx.filter_reserved = BitConverter.ToInt32(buf, of + 1648 + 4);
                symbolInfoEx.logging = BitConverter.ToInt32(buf, of + 1652 + 4);
                symbolInfoEx.spread = BitConverter.ToInt32(buf, of + 1656 + 4);
                symbolInfoEx.spread_balance = BitConverter.ToInt32(buf, of + 1660 + 4);
                symbolInfoEx.exemode = BitConverter.ToInt32(buf, of + 1664 + 4);
                symbolInfoEx.swap_enable = BitConverter.ToInt32(buf, of + 1668 + 4);
                symbolInfoEx.swap_type = BitConverter.ToInt32(buf, of + 1672 + 4);
                symbolInfoEx.swap_long = BitConverter.ToDouble(buf, of + 1676 + 4);
                symbolInfoEx.swap_short = BitConverter.ToDouble(buf, of + 1684 + 4);
                symbolInfoEx.swap_rollover3days = BitConverter.ToInt32(buf, of + 1692 + 4);
                symbolInfoEx.contract_size = BitConverter.ToDouble(buf, of + 1696 + 4 + 4);
                symbolInfoEx.tick_value = BitConverter.ToDouble(buf, of + 1704 + 4 + 4);
                symbolInfoEx.tick_size = BitConverter.ToDouble(buf, of + 1712 + 4 + 4);
                symbolInfoEx.stops_level = BitConverter.ToInt32(buf, of + 1720 + 4 + 4);
                symbolInfoEx.gtc_pendings = BitConverter.ToInt32(buf, of + 1724 + 4 + 4);
                symbolInfoEx.margin_mode = BitConverter.ToInt32(buf, of + 1728 + 4 + 4);
                symbolInfoEx.margin_initial = BitConverter.ToDouble(buf, of + 1732 + 4 + 4 + 4);
                symbolInfoEx.margin_initial = Math.Round(symbolInfoEx.margin_initial, 8);
                symbolInfoEx.margin_maintenance = BitConverter.ToDouble(buf, of + 1740 + 4 + 4 + 4);
                symbolInfoEx.margin_maintenance = Math.Round(symbolInfoEx.margin_maintenance, 8);
                symbolInfoEx.margin_hedged = BitConverter.ToDouble(buf, of + 1748 + 4 + 4 + 4);
                symbolInfoEx.margin_divider = BitConverter.ToDouble(buf, of + 1756 + 4 + 4 + 4);
                symbolInfoEx.point = BitConverter.ToDouble(buf, of + 1764 + 4 + 4 + 4);
                symbolInfoEx.multiply = BitConverter.ToDouble(buf, of + 1772 + 4 + 4 + 4);
                symbolInfoEx.bid_tickvalue = BitConverter.ToDouble(buf, of + 1780 + 4 + 4 + 4);
                symbolInfoEx.ask_tickvalue = BitConverter.ToDouble(buf, of + 1788 + 4 + 4 + 4);
                symbolInfoEx.long_only = BitConverter.ToInt32(buf, of + 1796 + 4 + 4);
                symbolInfoEx.instant_max_volume = BitConverter.ToInt32(buf, of + 1800 + 4 + 4 + 4);
                symbolInfoEx.margin_currency = readString(buf, of + 1804 + 4 + 4 + 4, 12);
                symbolInfoEx.freeze_level = BitConverter.ToInt32(buf, of + 1816 + 4 + 4 + 4);
                symbolInfoEx.margin_hedged_strong = BitConverter.ToInt32(buf, of + 1820 + 4 + 4 + 4);
                symbolInfoEx.value_date = BitConverter.ToInt32(buf, of + 1824 + 4 + 4 + 4);
                symbolInfoEx.quotes_delay = BitConverter.ToInt32(buf, of + 1828 + 4 + 4 + 4);
                symbolInfoEx.swap_openprice = BitConverter.ToInt32(buf, of + 1832 + 4 + 4 + 4);
                symbolInfoEx.unused = new int[22];
                for (var index = 0; index < 22; ++index)
                    symbolInfoEx.unused[index] = BitConverter.ToInt32(buf, of + 1836 + index * 4 + 4 + 4 + 4);
                return (T) Convert.ChangeType(symbolInfoEx, typeof(T));
            }
            case "Server":
                return (T) Convert.ChangeType(new Server
                {
                    server = readStringASCII(buf, of, 64),
                    ip = BitConverter.ToInt32(buf, of + 64),
                    desc = readString(buf, of + 68, 64),
                    is_proxy = BitConverter.ToInt32(buf, of + 132),
                    priority = BitConverter.ToInt32(buf, of + 136),
                    loading = BitConverter.ToInt32(buf, of + 140),
                    ip_internal = BitConverter.ToInt32(buf, of + 144),
                    ping = BitConverter.ToInt32(buf, of + 148),
                    reserved = BitConverter.ToInt32(buf, of + 152),
                    ptr_next = BitConverter.ToInt32(buf, of + 156)
                }, typeof(T));
            case "MainServer":
            {
                var mainServer = new MainServer
                {
                    name = readStringASCII(buf, of, 64),
                    comment = readStringASCII(buf, of + 64, 128),
                    is_demo = BitConverter.ToInt32(buf, of + 192),
                    ping = BitConverter.ToInt32(buf, of + 196),
                    dummy_C8 = BitConverter.ToInt32(buf, of + 200),
                    dummy_CC = BitConverter.ToInt32(buf, of + 204),
                    ptr_dataserver = BitConverter.ToInt32(buf, of + 208),
                    count = BitConverter.ToInt32(buf, of + 212),
                    host_addr = readStringASCII(buf, of + 216, 64),
                    time = BitConverter.ToInt32(buf, of + 280),
                    dummy_11C = new int[16]
                };
                for (var index = 0; index < 16; ++index)
                    mainServer.dummy_11C[index] = BitConverter.ToInt32(buf, of + 284 + index * 4);
                mainServer.ptr_next = BitConverter.ToInt32(buf, of + 348);
                return (T) Convert.ChangeType(mainServer, typeof(T));
            }
            case "ServerInfo":
                return (T) Convert.ChangeType(new ServerInfo
                {
                    name = readString(buf, of, 64),
                    comment = readString(buf, of + 64, 128)
                }, typeof(T));
            case "ConGroupSec":
            {
                var conGroupSec = new ConGroupSec
                {
                    show = BitConverter.ToInt32(buf, of),
                    trade = BitConverter.ToInt32(buf, of + 4),
                    execution = BitConverter.ToInt32(buf, of + 8),
                    comm_base = BitConverter.ToDouble(buf, of + 12),
                    comm_type = BitConverter.ToInt32(buf, of + 20),
                    comm_lots = BitConverter.ToInt32(buf, of + 24),
                    comm_agent = BitConverter.ToDouble(buf, of + 28),
                    comm_agent_type = BitConverter.ToInt32(buf, of + 36),
                    spread_diff = BitConverter.ToInt32(buf, of + 40),
                    lot_min = BitConverter.ToInt32(buf, of + 44),
                    lot_max = BitConverter.ToInt32(buf, of + 48),
                    lot_step = BitConverter.ToInt32(buf, of + 52),
                    ie_deviation = BitConverter.ToInt32(buf, of + 56),
                    confirmation = BitConverter.ToInt32(buf, of + 60),
                    trade_rights = BitConverter.ToInt32(buf, of + 64),
                    ie_quick_mode = BitConverter.ToInt32(buf, of + 68),
                    autocloseout_mode = BitConverter.ToInt32(buf, of + 72),
                    comm_tax = BitConverter.ToDouble(buf, of + 76),
                    comm_agent_lots = BitConverter.ToInt32(buf, of + 84),
                    freemargin_mode = BitConverter.ToInt32(buf, of + 88),
                    reserved = new int[3]
                };
                for (var index = 0; index < 3; ++index)
                    conGroupSec.reserved[index] = BitConverter.ToInt32(buf, of + 92 + index * 4);
                return (T) Convert.ChangeType(conGroupSec, typeof(T));
            }
            case "ConGroupMargin":
            {
                var conGroupMargin = new ConGroupMargin
                {
                    symbol = new byte[12]
                };
                for (var index = 0; index < 12; ++index)
                    conGroupMargin.symbol[index] = buf[of + index];
                conGroupMargin.swap_long = Math.Round(BitConverter.ToDouble(buf, of + 12), 8);
                if (conGroupMargin.swap_long == double.NaN)
                    conGroupMargin.swap_long = 0.0;
                if (conGroupMargin.swap_long > 2.0)
                    conGroupMargin.swap_long = 0.0;
                conGroupMargin.swap_short = Math.Round(BitConverter.ToDouble(buf, of + 20), 8);
                if (conGroupMargin.swap_short > 2.0)
                    conGroupMargin.swap_short = 0.0;
                conGroupMargin.margin_divider = Math.Round(BitConverter.ToDouble(buf, of + 28), 8);
                if (conGroupMargin.margin_divider > 2.0)
                    conGroupMargin.margin_divider = 0.0;
                conGroupMargin.reserved = new int[7];
                for (var index = 0; index < 7; ++index)
                    conGroupMargin.reserved[index] = BitConverter.ToInt32(buf, of + 36 + index * 4);
                return (T) Convert.ChangeType(conGroupMargin, typeof(T));
            }
            case "ConGroup":
            {
                var conGroup = new ConGroup
                {
                    @group = readString(buf, of, 16),
                    enable = BitConverter.ToInt32(buf, of + 16),
                    timeout = BitConverter.ToInt32(buf, of + 20),
                    adv_security = BitConverter.ToInt32(buf, of + 24),
                    company = readString(buf, of + 28, 128),
                    signature = readString(buf, of + 156, 256),
                    smtp_server = readString(buf, of + 412, 64),
                    smtp_login = readString(buf, of + 476, 32),
                    smtp_password = readString(buf, of + 508, 32),
                    support_email = readString(buf, of + 540, 64),
                    templates = readString(buf, of + 604, 32),
                    copies = BitConverter.ToInt32(buf, of + 636),
                    reports = BitConverter.ToInt32(buf, of + 640),
                    default_leverage = BitConverter.ToInt32(buf, of + 644),
                    default_deposit = BitConverter.ToDouble(buf, of + 648),
                    maxsecurities = BitConverter.ToInt32(buf, of + 656),
                    secgroups = new ConGroupSec[32]
                };
                for (var index11 = 0; index11 < 32; ++index11)
                {
                    var conGroupSec = new ConGroupSec
                    {
                        show = BitConverter.ToInt32(buf, of + index11 * 112 + 660 + 4),
                        trade = BitConverter.ToInt32(buf, of + index11 * 112 + 664 + 4),
                        execution = BitConverter.ToInt32(buf, of + index11 * 112 + 668 + 4),
                        comm_base = BitConverter.ToDouble(buf, of + index11 * 112 + 672 + 8),
                        comm_type = BitConverter.ToInt32(buf, of + index11 * 112 + 680 + 8),
                        comm_lots = BitConverter.ToInt32(buf, of + index11 * 112 + 684 + 8),
                        comm_agent = BitConverter.ToDouble(buf, of + index11 * 112 + 688 + 8),
                        comm_agent_type = BitConverter.ToInt32(buf, of + index11 * 112 + 696 + 8),
                        spread_diff = BitConverter.ToInt32(buf, of + index11 * 112 + 700 + 8),
                        lot_min = BitConverter.ToInt32(buf, of + index11 * 112 + 704 + 8),
                        lot_max = BitConverter.ToInt32(buf, of + index11 * 112 + 708 + 8),
                        lot_step = BitConverter.ToInt32(buf, of + index11 * 112 + 712 + 8),
                        ie_deviation = BitConverter.ToInt32(buf, of + index11 * 112 + 716 + 8),
                        confirmation = BitConverter.ToInt32(buf, of + index11 * 112 + 720 + 8),
                        trade_rights = BitConverter.ToInt32(buf, of + index11 * 112 + 724 + 8),
                        ie_quick_mode = BitConverter.ToInt32(buf, of + index11 * 112 + 728 + 8),
                        autocloseout_mode = BitConverter.ToInt32(buf, of + index11 * 112 + 732 + 8),
                        comm_tax = BitConverter.ToDouble(buf, of + index11 * 112 + 736 + 8),
                        comm_agent_lots = BitConverter.ToInt32(buf, of + index11 * 112 + 744 + 8),
                        freemargin_mode = BitConverter.ToInt32(buf, of + index11 * 112 + 748 + 8),
                        reserved = new int[4]
                    };
                    for (var index12 = 0; index12 < 4; ++index12)
                        conGroupSec.reserved[index12] = BitConverter.ToInt32(buf, of + index11 * 112 + 752 + index12 * 4 + 8);
                    conGroup.secgroups[index11] = conGroupSec;
                }

                conGroup.secmargins = new ConGroupMargin[128];
                for (var index13 = 0; index13 < 128; ++index13)
                {
                    var conGroupMargin = new ConGroupMargin
                    {
                        symbol = new byte[12]
                    };
                    for (var index14 = 0; index14 < 12; ++index14)
                        conGroupMargin.symbol[index14] = buf[of + index13 * 72 + 4248 + index14];
                    conGroupMargin.swap_long = BitConverter.ToDouble(buf, of + index13 * 72 + 4260 + 4);
                    conGroupMargin.swap_short = BitConverter.ToDouble(buf, of + index13 * 72 + 4268 + 4);
                    conGroupMargin.margin_divider = BitConverter.ToDouble(buf, of + index13 * 72 + 4276 + 4);
                    conGroupMargin.reserved = new int[8];
                    for (var index15 = 0; index15 < 8; ++index15)
                        conGroupMargin.reserved[index15] = BitConverter.ToInt32(buf, of + index13 * 72 + 4284 + index15 * 4 + 4);
                    conGroup.secmargins[index13] = conGroupMargin;
                }

                conGroup.secmargins_total = BitConverter.ToInt32(buf, of + 12180 + 264 + 1020);
                conGroup.currency = readString(buf, of + 12184 + 264 + 1020, 12);
                conGroup.credit = BitConverter.ToDouble(buf, of + 12196 + 264 + 1020);
                conGroup.margin_call = BitConverter.ToInt32(buf, of + 12204 + 264 + 1020);
                conGroup.margin_mode = BitConverter.ToInt32(buf, of + 12208 + 264 + 1020);
                conGroup.margin_stopout = BitConverter.ToInt32(buf, of + 12212 + 264 + 1020);
                conGroup.interestrate = BitConverter.ToDouble(buf, of + 12216 + 264 + 1020);
                conGroup.use_swap = BitConverter.ToInt32(buf, of + 12224 + 264 + 1020 + 4);
                conGroup.news = BitConverter.ToInt32(buf, of + 12228 + 264 + 1020 + 4);
                conGroup.rights = BitConverter.ToInt32(buf, of + 12232 + 264 + 1020 + 4);
                conGroup.check_ie_prices = BitConverter.ToInt32(buf, of + 12236 + 264 + 1020 + 4);
                conGroup.maxpositions = BitConverter.ToInt32(buf, of + 12240 + 264 + 1020 + 4);
                conGroup.close_reopen = BitConverter.ToInt32(buf, of + 1224 + 264 + 10204 + 4);
                conGroup.hedge_prohibited = BitConverter.ToInt32(buf, of + 12248 + 264 + 1020 + 4);
                conGroup.close_fifo = BitConverter.ToInt32(buf, of + 12252 + 264 + 1020 + 4);
                conGroup.s34E8 = BitConverter.ToInt32(buf, of + 12256 + 264 + 1020 + 4);
                conGroup.unused_rights = new int[2];
                for (var index = 0; index < 2; ++index)
                    conGroup.unused_rights[index] = BitConverter.ToInt32(buf, of + 12260 + index * 4 + 264 + 1020 + 4);
                conGroup.securities_hash = new byte[16];
                for (var index = 0; index < 16; ++index)
                    conGroup.securities_hash[index] = buf[of + 12268 + index + 264 + 1020 + 4];
                conGroup.margin_type = BitConverter.ToInt32(buf, of + 12284 + 264 + 1020 + 4);
                conGroup.archive_period = BitConverter.ToInt32(buf, of + 12288 + 264 + 1020 + 4);
                conGroup.archive_max_balance = BitConverter.ToInt32(buf, of + 12292 + 264 + 1020 + 4);
                conGroup.stopout_skip_hedged = BitConverter.ToInt32(buf, of + 12296 + 264 + 1020 + 4);
                conGroup.archive_pending_period = BitConverter.ToInt32(buf, of + 12300 + 264 + 1020 + 4);
                conGroup.reserved = new int[26];
                for (var index = 0; index < 26; ++index)
                    conGroup.reserved[index] = BitConverter.ToInt32(buf, of + 12304 + index * 4) + 264 + 1020 + 4;
                return (T) Convert.ChangeType(conGroup, typeof(T));
            }
        }

        if (!name.Equals("ConSymbolGroup"))
            throw new Exception($"Unknown type {name}");
        return (T) Convert.ChangeType(new ConSymbolGroup
        {
            name = readString(buf, of, 16),
            description = readString(buf, of + 16, 64)
        }, typeof(T));
    }

    internal static string readString(byte[] buf, int of, int len)
    {
        var destinationArray = new byte[len];
        Array.Copy(buf, of, destinationArray, 0, len);
        var str1 = Encoding.Default.GetString(destinationArray);
        var str2 = "";
        for (var index = 0; index < str1.Length && str1[index] != char.MinValue; ++index)
            str2 += str1[index].ToString();
        return str2;
    }

    internal static string readStringASCII(byte[] buf, int of, int len)
    {
        var byteList = new List<byte>();
        for (var index = 0; index < len && (buf[of + index] != 0 || index <= 0); ++index)
            byteList.Add(buf[of + index]);
        if (byteList.Count > 0 && byteList[0] == 0)
            byteList.RemoveAt(0);
        return Encoding.ASCII.GetString(byteList.ToArray());
    }
}
