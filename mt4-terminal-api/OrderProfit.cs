namespace TradingAPI.MT4Server;

internal class OrderProfit
{
    private readonly QuoteClient QC;

    internal OrderProfit(QuoteClient qc) => QC = qc;

    internal void Update(Order order, double bid, double ask)
    {
        var ex = QC.GetSymbolInfo(order.Symbol).Ex;
        lock (ex)
        {
            if (order.StopLoss > 0.0)
                switch (order.Type)
                {
                    case Op.Buy when bid < order.StopLoss:
                        bid = order.StopLoss;
                        ask = order.StopLoss;
                        break;
                    case Op.Sell when ask > order.StopLoss:
                        bid = order.StopLoss;
                        ask = order.StopLoss;
                        break;
                }

            UpdateSymbolTick(ex, bid, ask);
            if (ex.profit_mode == 0)
            {
                switch (order.Type)
                {
                    case Op.Buy:
                    {
                        var num = order.Ex.volume * ex.bid_tickvalue / 100.0;
                        order.Profit = Math.Round(bid * num, ex.digits) - Math.Round(order.OpenPrice * num, ex.digits);
                        break;
                    }
                    case Op.Sell:
                    {
                        var num = order.Ex.volume * ex.ask_tickvalue / 100.0;
                        order.Profit = Math.Round(order.OpenPrice * num, ex.digits) - Math.Round(ask * num, ex.digits);
                        break;
                    }
                }
            }
            else
            {
                var num = 0.0;
                switch (order.Type)
                {
                    case Op.Buy:
                        num = (bid - order.OpenPrice) * ex.bid_tickvalue;
                        break;
                    case Op.Sell:
                        num = (order.OpenPrice - ask) * ex.ask_tickvalue;
                        break;
                }

                order.Profit = num * order.Ex.volume * 0.01;
                if (ex.profit_mode == 2)
                    order.Profit /= ex.tick_size;
            }

            order.Profit = Math.Round(order.Profit, ex.digits);
        }
    }

    internal bool GetTickRate(string symbol, QuoteEventArgs rate)
    {
        if (!QC.MT4Symbol.exist(symbol))
            return false;
        while (QC.GetQuote(symbol) == null)
            Thread.Sleep(1);
        var quote = QC.GetQuote(symbol);
        rate.Bid = quote.Bid;
        rate.Ask = quote.Ask;
        return true;
    }

    private bool memcmp(string str1, int ind, string str2, int count) => memcmp(str1.Substring(3), str2, count);

    private bool memcmp(string str1, string str2, int count)
    {
        for (var index = 0; index < count; ++index)
        {
            if (index == str1.Length)
                return str1.Length == str2.Length;
            if (index == str2.Length)
                return str1.Length == str2.Length;
            if (str1[index] != str2[index])
                return false;
        }

        return true;
    }

    private void memcpy(ref string dst, int dstind, string src, int srcind, int count)
    {
        var str1 = dst.Substring(0, dstind);
        char ch;
        for (var index = 0; index < count && index + srcind < src.Length; ++index)
        {
            var str2 = str1;
            ch = src[index + srcind];
            var str3 = ch.ToString();
            str1 = str2 + str3;
        }

        for (var index = dstind + count; index < dst.Length; ++index)
        {
            var str4 = str1;
            ch = dst[index];
            var str5 = ch.ToString();
            str1 = str4 + str5;
        }

        dst = str1;
    }

    private void memcpy(ref string dst, string src, int count)
    {
        var str1 = "";
        char ch;
        for (var index = 0; index < count; ++index)
        {
            var str2 = str1;
            ch = src[index];
            var str3 = ch.ToString();
            str1 = str2 + str3;
        }

        for (var index = count; index < dst.Length; ++index)
        {
            var str4 = str1;
            ch = dst[index];
            var str5 = ch.ToString();
            str1 = str4 + str5;
        }

        dst = str1;
    }

    internal bool UpdateSymbolTick(SymbolInfoEx sym, double bid, double ask)
    {
        lock (sym)
        {
            if (sym.profit_mode == 2 && (sym.tick_value <= 0.0 || sym.tick_size <= 0.0))
                return false;
            var rate = new QuoteEventArgs();
            var src = UDT.readString(sym.symbol, 0, sym.symbol.Length);
            var dst = "";
            switch (sym.profit_mode)
            {
                case 0:
                {
                    if (memcmp(src, QC.Account.currency, 3))
                    {
                        sym.bid_tickvalue = sym.contract_size / bid;
                        sym.ask_tickvalue = sym.contract_size / ask;
                    }
                    else if (memcmp(src, 3, QC.Account.currency, 3))
                    {
                        sym.bid_tickvalue = sym.contract_size;
                        sym.ask_tickvalue = sym.contract_size;
                    }
                    else
                    {
                        memcpy(ref dst, QC.Account.currency, 3);
                        memcpy(ref dst, 3, src, 3, 3);
                        memcpy(ref dst, 6, src, 6, 6);
                        if (GetTickRate(dst, rate))
                        {
                            sym.bid_tickvalue = sym.contract_size / rate.Bid;
                            sym.ask_tickvalue = sym.contract_size / rate.Ask;
                        }
                        else
                        {
                            memcpy(ref dst, 0, src, 3, 3);
                            memcpy(ref dst, 3, QC.Account.currency, 0, 3);
                            if (GetTickRate(dst, rate))
                            {
                                sym.bid_tickvalue = sym.contract_size * rate.Bid;
                                sym.ask_tickvalue = sym.contract_size * rate.Ask;
                            }
                            else
                            {
                                memcpy(ref dst, "USD", 3);
                                memcpy(ref dst, 3, src, 3, 3);
                                if (GetTickRate(dst, rate))
                                {
                                    sym.bid_tickvalue = sym.contract_size / rate.Bid;
                                    sym.ask_tickvalue = sym.contract_size / rate.Ask;
                                }
                                else
                                {
                                    memcpy(ref dst, 0, src, 3, 3);
                                    memcpy(ref dst, 3, "USD", 0, 3);
                                    if (GetTickRate(dst, rate))
                                    {
                                        sym.bid_tickvalue = sym.contract_size * rate.Bid;
                                        sym.ask_tickvalue = sym.contract_size * rate.Ask;
                                    }
                                }

                                memcpy(ref dst, "USD", 3);
                                memcpy(ref dst, 3, QC.Account.currency, 0, 3);
                                if (GetTickRate(dst, rate))
                                {
                                    sym.bid_tickvalue *= rate.Bid;
                                    sym.ask_tickvalue *= rate.Ask;
                                }
                                else
                                {
                                    memcpy(ref dst, QC.Account.currency, 3);
                                    memcpy(ref dst, 3, "USD", 0, 3);
                                    if (GetTickRate(dst, rate))
                                    {
                                        sym.bid_tickvalue /= rate.Bid;
                                        sym.ask_tickvalue /= rate.Ask;
                                    }
                                }
                            }
                        }
                    }

                    return sym.bid_tickvalue is > 0.0 and < double.MaxValue && sym.ask_tickvalue is > 0.0 and < double.MaxValue;
                }
                case < 1:
                case > 2:
                    return false;
            }

            var num = sym.profit_mode == 2 ? sym.tick_value : sym.contract_size;
            sym.bid_tickvalue = num;
            sym.ask_tickvalue = num;
            if (sym.currency != QC.Account.currency)
            {
                memcpy(ref dst, QC.Account.currency, 3);
                memcpy(ref dst, 3, sym.currency, 0, 3);
                if (dst.Length > 6)
                    dst = dst.Substring(0, 6);
                if (GetTickRate(dst, rate))
                {
                    sym.bid_tickvalue /= rate.Bid;
                    sym.ask_tickvalue = sym.bid_tickvalue;
                    return true;
                }

                memcpy(ref dst, sym.currency, 3);
                memcpy(ref dst, 3, QC.Account.currency, 0, 3);
                if (GetTickRate(dst, rate))
                {
                    sym.bid_tickvalue *= rate.Bid;
                    sym.ask_tickvalue = sym.bid_tickvalue;
                    return true;
                }

                memcpy(ref dst, "USD", 3);
                memcpy(ref dst, 3, sym.currency, 0, 3);
                if (GetTickRate(dst, rate))
                {
                    sym.bid_tickvalue /= rate.Bid;
                    sym.ask_tickvalue = sym.bid_tickvalue;
                }
                else
                {
                    memcpy(ref dst, sym.currency, 3);
                    memcpy(ref dst, 3, "USD", 0, 3);
                    if (!GetTickRate(dst, rate))
                        return false;
                    sym.bid_tickvalue *= rate.Bid;
                    sym.ask_tickvalue *= rate.Ask;
                }

                memcpy(ref dst, "USD", 3);
                memcpy(ref dst, 3, QC.Account.currency, 0, 3);
                if (GetTickRate(dst, rate))
                {
                    sym.bid_tickvalue *= rate.Bid;
                    sym.ask_tickvalue *= rate.Ask;
                }
                else
                {
                    memcpy(ref dst, QC.Account.currency, 3);
                    memcpy(ref dst, 3, "USD", 0, 3);
                    if (!GetTickRate(dst, rate))
                        return false;
                    sym.bid_tickvalue /= rate.Bid;
                    sym.ask_tickvalue /= rate.Ask;
                }
            }

            return sym.bid_tickvalue is > 0.0 and < double.MaxValue && sym.ask_tickvalue is > 0.0 and < double.MaxValue;
        }
    }
}
