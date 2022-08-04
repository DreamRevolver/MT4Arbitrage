namespace TradingAPI.MT4Server;

internal class BarHistory
{
    private const int TIMEOUT = 10000;
    private byte[] Bytes;
    private readonly QuoteClient QuoteClient;

    public BarHistory(QuoteClient quoteClient) => QuoteClient = quoteClient;

    public void send(Connection con)
    {
        if (Bytes == null)
            return;
        con.SendEncode(Bytes);
        Bytes = null;
    }

    public void request(string symbol, Timeframe tf, DateTime from, short count)
    {
        for (var index = 0; index < 10000 && Bytes != null; ++index)
            Thread.Sleep(1);
        if (Bytes != null)
            throw new Exception($"Previous request have not sent in {10000} ms");
        var numArray = new byte[25];
        numArray[0] = 156;
        numArray[1] = 1;
        vUTF.toByte(symbol).CopyTo(numArray, 3);
        BitConverter.GetBytes((int) tf).CopyTo(numArray, 15);
        BitConverter.GetBytes(MT4Order.toMtTime(from)).CopyTo(numArray, 19);
        BitConverter.GetBytes(count).CopyTo(numArray, 23);
        Bytes = numArray;
    }

    public void parse(string symbol, Timeframe tf, byte[] buf)
    {
        var barList = new List<Bar>();
        for (var index = 0; index < buf.Length; index += 28)
        {
            var num = Math.Pow(10.0, -QuoteClient.MT4Symbol.getInfo(symbol).Digits);
            var bar = new Bar
            {
                Time = MT4Order.toNetTime(BitConverter.ToInt32(buf, index)),
                Open = num * BitConverter.ToInt32(buf, index + 4)
            };
            bar.High = bar.Open + num * BitConverter.ToInt32(buf, index + 8);
            bar.Low = bar.Open + num * BitConverter.ToInt32(buf, index + 12);
            bar.Close = bar.Open + num * BitConverter.ToInt32(buf, index + 16);
            bar.Volume = BitConverter.ToDouble(buf, index + 20);
            barList.Add(bar);
        }

        QuoteClient.onQuoteHistory(symbol, tf, barList.ToArray());
    }
}
