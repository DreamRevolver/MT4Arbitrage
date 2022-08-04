namespace TradingAPI.MT4Server;

internal class MT4Order
{
    public enum Cmd
    {
        INSTANT = 64, // 0x00000040
        MARKET = 66, // 0x00000042
        PENDING = 67, // 0x00000043
        CLOSE_INSTANT = 68, // 0x00000044
        CLOSE_MARKET = 70, // 0x00000046
        MODIFY = 71, // 0x00000047
        DELETE_PENDING = 72, // 0x00000048
        CLOSE_BY = 73, // 0x00000049
        MULTIPLE_CLOSE_BY = 74 // 0x0000004A
    }

    public static byte[] get(
        int user,
        int ticket,
        Cmd cmd,
        Op operation,
        string symbol,
        int lots,
        double price,
        double sl,
        double tp,
        int slip,
        string comment,
        int magic,
        DateTime expiration,
        bool placedManually)
    {
        var numArray = new byte[97];
        numArray[0] = 190;
        numArray[1] = (byte) cmd;
        if (!placedManually)
            numArray[2] = 1;
        numArray[3] = (byte) operation;
        BitConverter.GetBytes(ticket).CopyTo(numArray, 5);
        BitConverter.GetBytes(magic).CopyTo(numArray, 9);
        if (symbol != null)
            vUTF.toByte(symbol).CopyTo(numArray, 13);
        BitConverter.GetBytes(lots).CopyTo(numArray, 25);
        BitConverter.GetBytes(price).CopyTo(numArray, 29);
        BitConverter.GetBytes(sl).CopyTo(numArray, 37);
        BitConverter.GetBytes(tp).CopyTo(numArray, 45);
        BitConverter.GetBytes(slip).CopyTo(numArray, 53);
        if (comment != null)
        {
            if (comment.Length > 31)
                comment = comment.Substring(0, 31);
            vUTF.toByte(comment).CopyTo(numArray, 57);
        }

        BitConverter.GetBytes(toMtTime(expiration)).CopyTo(numArray, 89);
        var num = user;
        for (var index = 1; index < 93; ++index)
            num += numArray[index];
        BitConverter.GetBytes(num).CopyTo(numArray, 93);
        return numArray;
    }

    public static Order read(byte[] buf, int i)
    {
        var tradeRecord = UDT.ReadStruct<TradeRecord>(buf, i);
        return new Order
        {
            Ex = tradeRecord,
            Ticket = tradeRecord.order,
            Symbol = vUTF.toString(tradeRecord.symbol, 0),
            Type = (Op) tradeRecord.cmd,
            Lots = tradeRecord.volume / 100.0,
            OpenTime = toNetTime(tradeRecord.open_time),
            OpenPrice = tradeRecord.open_price,
            StopLoss = tradeRecord.sl,
            TakeProfit = tradeRecord.tp,
            Expiration = toNetTime(tradeRecord.expiration),
            Commission = tradeRecord.commission,
            Swap = tradeRecord.storage,
            MagicNumber = tradeRecord.magic,
            Comment = tradeRecord.comment,
            CloseTime = toNetTime(tradeRecord.close_time),
            ClosePrice = tradeRecord.close_price,
            Profit = tradeRecord.profit,
            RateOpen = tradeRecord.conv_rates[0],
            RateClose = tradeRecord.conv_rates[1],
            RateMargin = tradeRecord.margin_rate
        };
    }

    public static string getString(byte[] buf, int start) => vUTF.toString(buf, start);

    public static int toMtTime(DateTime time)
    {
        var dateTime = new DateTime(1970, 1, 1);
        return time < dateTime ? 0 : (int) ((time.Ticks - dateTime.Ticks) / 10000000L);
    }

    public static DateTime toNetTime(int time) => new DateTime(new DateTime(1970, 1, 1).Ticks + time * 10000000L);
}
