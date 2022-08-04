using System.Security.Cryptography;

namespace TradingAPI.MT4Server;

internal class MT4Symbol
{
    private static readonly Dictionary<string, object> Locks = new();
    internal Dictionary<string, SymbolInfo> Info;
    public string[] Names;
    internal Dictionary<ushort, string> Symbols;

    private SymbolInfo CreateSymbolInfo(SymbolInfoEx rec)
    {
        var symbolInfo = new SymbolInfo
        {
            Ex = rec,
            Code = (ushort) rec.count,
            Digits = rec.digits
        };
        symbolInfo.Point = Math.Pow(10.0, -symbolInfo.Digits);
        symbolInfo.StopsLevel = rec.stops_level;
        symbolInfo.SwapLong = rec.swap_long;
        symbolInfo.SwapShort = rec.swap_short;
        symbolInfo.Spread = rec.spread;
        symbolInfo.MarginCurrency = rec.margin_currency;
        symbolInfo.FreezeLevel = rec.freeze_level;
        symbolInfo.ContractSize = rec.contract_size;
        symbolInfo.Currency = rec.currency;
        symbolInfo.MarginDivider = rec.margin_divider;
        symbolInfo.Execution = (Execution) rec.exemode;
        symbolInfo.ProfitMode = (ProfitMode) rec.profit_mode;
        symbolInfo.MarginMode = (MarginMode) rec.margin_mode;
        return symbolInfo;
    }

    private void init(byte[] buf)
    {
        if (buf == null)
            throw new Exception("Symbols buffer is null");
        try
        {
            Symbols = new Dictionary<ushort, string>();
            Info = new Dictionary<string, SymbolInfo>();
            var num = buf.Length / 1936;
            for (var index = 0; index < num; ++index)
            {
                var rec = UDT.ReadStruct<SymbolInfoEx>(buf, index * 1936);
                var symbolInfo = CreateSymbolInfo(rec);
                var key = vUTF.toString(rec.symbol, 0);
                Symbols.Add(symbolInfo.Code, key);
                Info.Add(key, symbolInfo);
            }

            Names = new string[Symbols.Count];
            Symbols.Values.CopyTo(Names, 0);
        }
        catch (Exception)
        {
            throw new Exception("Unable to parse symbols");
        }
    }

    internal void update(byte[] buf)
    {
        var num = buf.Length / 1952;
        for (var index = 0; index < num; ++index)
        {
            var rec = UDT.ReadStruct<SymbolInfoEx>(buf, index * 1952 + 16);
            if (Info.ContainsKey(vUTF.toString(rec.symbol, 0)))
                Info[vUTF.toString(rec.symbol, 0)] = CreateSymbolInfo(rec);
        }
    }

    internal void UpdateMargin(ConGroupMargin cgm)
    {
        if (!Info.ContainsKey(vUTF.toString(cgm.symbol, 0)))
            return;
        var symbolInfo = Info[vUTF.toString(cgm.symbol, 0)];
        var flag = false;
        if (cgm.margin_divider != double.MaxValue)
        {
            symbolInfo.MarginDivider = cgm.margin_divider;
            symbolInfo.Ex.margin_divider = cgm.margin_divider;
            flag = true;
        }

        if (cgm.swap_long != double.MaxValue)
        {
            symbolInfo.SwapLong = cgm.swap_long;
            symbolInfo.Ex.swap_long = cgm.swap_long;
            flag = true;
        }

        if (cgm.swap_short != double.MaxValue)
        {
            symbolInfo.SwapShort = cgm.swap_short;
            symbolInfo.Ex.swap_short = cgm.swap_short;
            flag = true;
        }

        if (!flag)
            return;
        Info[vUTF.toString(cgm.symbol, 0)] = symbolInfo;
    }

    public string getSymbol(ushort code)
    {
        if (Symbols == null)
            throw new Exception("Symbols not initialized");
        return Symbols.ContainsKey(code) ? Symbols[code] : throw new Exception($"Symbol not found for code {code}");
    }

    public ushort getCode(string symbol)
    {
        if (Info == null)
            throw new Exception("Symbols not initialized");
        if (!Info.ContainsKey(symbol))
            throw new Exception($"Info not found for symbol {symbol}");
        return Info[symbol].Code;
    }

    public bool exist(string symbol) => Symbols.ContainsValue(symbol);

    public SymbolInfo getInfo(string symbol)
    {
        if (Info == null)
            throw new Exception("Symbols not initialized");
        return Info.ContainsKey(symbol.Trim()) ? Info[symbol.Trim()] : throw new Exception($"{symbol} not exist");
    }

    public string correctCase(string symbol)
    {
        if (symbol == null)
            return null;
        if (Info == null)
            throw new Exception("Symbols not initialized");
        foreach (var key in Info.Keys)
            if (key.ToLower() == symbol.ToLower())
                return key;
        throw new Exception($"{symbol} not exist");
    }

    public void LoadSymbols(Connection con, string symPath)
    {
        if (symPath == null)
        {
            init(con.ReceiveSymbols());
        }
        else
        {
            symPath = symPath.Trim();
            symPath += "\\";
            symPath.Replace("\\\\", "\\");
            symPath = symPath.ToLower();
            var str = $"{symPath}{con.QC.Host}-{con.QC.Port}.sym";
            lock (Locks)
            {
                if (!Locks.ContainsKey(str))
                    Locks.Add(str, new object());
            }

            lock (Locks[str])
            {
                var numArray1 = new byte[17];
                numArray1[0] = 8;
                byte[] numArray2 = null;
                if (File.Exists(str))
                {
                    numArray2 = File.ReadAllBytes(str);
                    new MD5CryptoServiceProvider().ComputeHash(numArray2).CopyTo(numArray1, 1);
                }

                con.SendEncode(numArray1);
                var decode = con.ReceiveDecode(1);
                if (decode[0] == 0)
                {
                    var copmressed = con.ReceiveCopmressed();
                    File.WriteAllBytes(str, copmressed);
                    init(copmressed);
                }
                else if (numArray2 != null)
                {
                    init(numArray2);
                }
                else
                {
                    if (decode[0] == 1)
                        throw new Exception("Trades is not allowed");
                    throw new ServerException(decode[0]);
                }
            }
        }
    }
}
