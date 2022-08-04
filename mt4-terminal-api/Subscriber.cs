namespace TradingAPI.MT4Server;

internal class Subscriber
{
    private readonly Logger Log;
    private readonly QuoteClient QuoteClient;
    internal readonly Dictionary<string, QuoteEventArgs> Quotes = new();
    private readonly List<byte[]> Queue = new();

    public Subscriber(QuoteClient quoteClient, Logger log)
    {
        Log = log;
        QuoteClient = quoteClient;
    }

    public QuoteEventArgs getQuote(string symbol)
    {
        if (!QuoteClient.MT4Symbol.exist(symbol))
            return null;
        lock (Quotes)
        {
            if (!Quotes.ContainsKey(symbol))
            {
                Quotes.Add(symbol, null);
                request();
            }
        }

        return Quotes[symbol];
    }

    public bool subscribed(string symbol) => Quotes.ContainsKey(symbol);

    public void subscribe(string symbol)
    {
        lock (Quotes)
        {
            if (Quotes.ContainsKey(symbol))
                return;
            Quotes.Add(symbol, null);
            request();
        }
    }

    public void subscribe(string[] symbols)
    {
        lock (Quotes)
        {
            foreach (var symbol in symbols)
                if (!Quotes.ContainsKey(symbol))
                    Quotes.Add(symbol, null);
            request();
        }
    }

    public void unsubscribe(string symbol)
    {
        lock (Quotes)
        {
            if (!Quotes.ContainsKey(symbol))
                return;
            Quotes.Remove(symbol);
            request();
        }
    }

    private void request()
    {
        var numArray = new byte[3 + Quotes.Keys.Count * 2];
        numArray[0] = 150;
        BitConverter.GetBytes((ushort) Quotes.Keys.Count).CopyTo(numArray, 1);
        var index = 3;
        foreach (var key in Quotes.Keys)
        {
            BitConverter.GetBytes(QuoteClient.MT4Symbol.getCode(key)).CopyTo(numArray, index);
            index += 2;
        }

        lock (Queue)
        {
            Queue.Add(numArray);
        }
    }

    public void send(Connection con)
    {
        lock (Queue)
        {
            while (Queue.Count > 0)
            {
                con.SendEncode(Queue[0]);
                Queue.RemoveAt(0);
            }
        }
    }

    public void parse(byte[] buf)
    {
        try
        {
            var dictionary = new Dictionary<string, QuoteEventArgs>();
            for (var index1 = 0; index1 < buf.Length / 14; ++index1)
            {
                var numArray = new byte[14];
                for (var index2 = 0; index2 < 14; ++index2)
                    numArray[index2] = buf[index1 * 14 + index2];
                var uint16 = BitConverter.ToUInt16(numArray, 0);
                var quoteEventArgs = new QuoteEventArgs
                {
                    Time = MT4Order.toNetTime(BitConverter.ToInt32(numArray, 2))
                };
                if (QuoteClient.MT4Symbol.Symbols.ContainsKey(uint16))
                {
                    quoteEventArgs.Symbol = QuoteClient.MT4Symbol.getSymbol(uint16);
                    quoteEventArgs.Ask = BitConverter.ToSingle(numArray, numArray.Length - 4);
                    if (quoteEventArgs.Ask >= 0.0001)
                    {
                        var digits = QuoteClient.MT4Symbol.getInfo(quoteEventArgs.Symbol).Digits;
                        quoteEventArgs.Ask = Math.Round(quoteEventArgs.Ask, digits);
                        quoteEventArgs.Bid = BitConverter.ToSingle(numArray, numArray.Length - 8);
                        if (quoteEventArgs.Bid >= 0.0001)
                        {
                            quoteEventArgs.Bid = Math.Round(quoteEventArgs.Bid, digits);
                            if (dictionary.ContainsKey(quoteEventArgs.Symbol))
                                dictionary[quoteEventArgs.Symbol] = quoteEventArgs;
                            else
                                dictionary.Add(quoteEventArgs.Symbol, quoteEventArgs);
                        }
                    }
                }
            }

            lock (Quotes)
            {
                Log.trace("Quote Call event");
                foreach (var key in dictionary.Keys)
                    if (Quotes.ContainsKey(key))
                    {
                        Quotes[key] = dictionary[key];
                        QuoteClient.onQuote(dictionary[key]);
                    }
            }
        }
        catch (Exception ex)
        {
            Log.exception(ex);
        }
    }
}
