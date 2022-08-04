namespace TradingAPI.MT4Server;

public class OrderClient
{
    private static int RequestId = 1;
    private static readonly object RequestIdLock = new();
    internal Logger Log;
    internal MT4Symbol MT4Symbol;
    public bool PlacedManually = true;
    internal QuoteClient QuoteClient;
    internal bool QuoteClientInitSymbols;
    public int TradeTimeout = 30000;

    public OrderClient()
    {
    }

    public OrderClient(QuoteClient qc)
    {
        Log = new Logger(this);
        Log.trace("Initializing order client");
        Trial.check();
        QuoteClient = qc;
        MT4Symbol = qc.MT4Symbol;
        QuoteClientInitSymbols = true;
        qc.OrderClient = this;
    }

    public event OrderProgressEventHandler OnOrderProgress;

    private static int getRequestId()
    {
        lock (RequestIdLock)
        {
            return RequestId++;
        }
    }

    public void Init(QuoteClient qc)
    {
        Log = new Logger(this);
        Log.trace("Initializing order client");
        Trial.check();
        QuoteClient = qc;
        MT4Symbol = qc.MT4Symbol;
        QuoteClientInitSymbols = true;
        qc.OrderClient = this;
    }

    public Order OrderModify(
        int ticket,
        string symbol,
        Op type,
        double volume,
        double price,
        double stoploss,
        double takeprofit,
        DateTime expiration)
    {
        symbol = QuoteClient.MT4Symbol.correctCase(symbol);
        return executeRequest(MT4Order.get(QuoteClient.User, ticket, MT4Order.Cmd.MODIFY, type, symbol, (int) Math.Round(volume / 0.01), price, stoploss, takeprofit, 0, null, 0, expiration, PlacedManually), getRequestId());
    }

    public int OrderModifyAsync(
        int ticket,
        string symbol,
        Op type,
        double volume,
        double price,
        double stoploss,
        double takeprofit,
        DateTime expiration)
    {
        symbol = QuoteClient.MT4Symbol.correctCase(symbol);
        var buf = MT4Order.get(QuoteClient.User, ticket, MT4Order.Cmd.MODIFY, type, symbol, 0, price, stoploss, takeprofit, 0, null, 0, expiration, PlacedManually);
        var requestId = getRequestId();
        executeRequestAsync(buf, requestId);
        return requestId;
    }

    public Order OrderClose(
        string symbol,
        int ticket,
        double volume,
        double price,
        int slippage)
    {
        symbol = QuoteClient.MT4Symbol.correctCase(symbol);
        var cmd = MT4Order.Cmd.CLOSE_INSTANT;
        if (MT4Symbol.getInfo(symbol).Execution == Execution.Market)
        {
            cmd = MT4Order.Cmd.CLOSE_MARKET;
            price = 0.0;
            slippage = 0;
        }

        var lots = (int) Math.Round(volume / 0.01);
        var order = executeRequest(MT4Order.get(QuoteClient.User, ticket, cmd, Op.Buy, symbol, lots, price, 0.0, 0.0, slippage, null, 0, new DateTime(), PlacedManually), getRequestId());
        lock (QuoteClient.Orders)
        {
            if (QuoteClient.Orders.ContainsKey(ticket))
                QuoteClient.Orders.Remove(ticket);
        }

        return order;
    }

    public int OrderCloseAsync(
        string symbol,
        int ticket,
        double volume,
        double price,
        int slippage)
    {
        symbol = QuoteClient.MT4Symbol.correctCase(symbol);
        var cmd = MT4Order.Cmd.CLOSE_INSTANT;
        if (MT4Symbol.getInfo(symbol).Execution == Execution.Market)
        {
            cmd = MT4Order.Cmd.CLOSE_MARKET;
            price = 0.0;
            slippage = 0;
        }

        var lots = (int) Math.Round(volume / 0.01);
        var buf = MT4Order.get(QuoteClient.User, ticket, cmd, Op.Buy, symbol, lots, price, 0.0, 0.0, slippage, null, 0, new DateTime(), PlacedManually);
        var requestId = getRequestId();
        executeRequestAsync(buf, requestId);
        return requestId;
    }

    public void OrderCloseBy(string symbol, int ticket1, int ticket2)
    {
        symbol = QuoteClient.MT4Symbol.correctCase(symbol);
        executeRequest(MT4Order.get(QuoteClient.User, ticket1, MT4Order.Cmd.CLOSE_BY, Op.Buy, symbol, 0, 0.0, 0.0, 0.0, 0, null, ticket2, new DateTime(), PlacedManually), getRequestId());
    }

    public int OrderCloseByAsync(string symbol, int ticket1, int ticket2)
    {
        symbol = QuoteClient.MT4Symbol.correctCase(symbol);
        var buf = MT4Order.get(QuoteClient.User, ticket1, MT4Order.Cmd.CLOSE_BY, Op.Buy, symbol, 0, 0.0, 0.0, 0.0, 0, null, ticket2, new DateTime(), PlacedManually);
        var requestId = getRequestId();
        executeRequestAsync(buf, requestId);
        return requestId;
    }

    public void OrderMultipleCloseBy(string symbol)
    {
        symbol = QuoteClient.MT4Symbol.correctCase(symbol);
        executeRequest(MT4Order.get(QuoteClient.User, 0, MT4Order.Cmd.MULTIPLE_CLOSE_BY, Op.Buy, symbol, 0, 0.0, 0.0, 0.0, 0, null, 0, new DateTime(), PlacedManually), getRequestId());
    }

    public int OrderMultipleCloseByAsync(string symbol)
    {
        symbol = QuoteClient.MT4Symbol.correctCase(symbol);
        var buf = MT4Order.get(QuoteClient.User, 0, MT4Order.Cmd.MULTIPLE_CLOSE_BY, Op.Buy, symbol, 0, 0.0, 0.0, 0.0, 0, null, 0, new DateTime(), PlacedManually);
        var requestId = getRequestId();
        executeRequestAsync(buf, requestId);
        return requestId;
    }

    public void OrderDelete(int ticket, Op type, string symbol, double volume, double price)
    {
        symbol = QuoteClient.MT4Symbol.correctCase(symbol);
        var lots = (int) Math.Round(volume / 0.01);
        executeRequest(MT4Order.get(QuoteClient.User, ticket, MT4Order.Cmd.DELETE_PENDING, type, symbol, lots, price, 0.0, 0.0, 0, null, 0, new DateTime(), PlacedManually), getRequestId());
        lock (QuoteClient.Orders)
        {
            if (!QuoteClient.Orders.ContainsKey(ticket))
                return;
            QuoteClient.Orders.Remove(ticket);
        }
    }

    public int OrderDeleteAsync(int ticket, Op type, string symbol, double volume, double price)
    {
        symbol = QuoteClient.MT4Symbol.correctCase(symbol);
        var lots = (int) Math.Round(volume / 0.01);
        var buf = MT4Order.get(QuoteClient.User, ticket, MT4Order.Cmd.DELETE_PENDING, type, symbol, lots, price, 0.0, 0.0, 0, null, 0, new DateTime(), PlacedManually);
        var requestId = getRequestId();
        executeRequestAsync(buf, requestId);
        return requestId;
    }

    public Order OrderSend(string symbol, Op operation, double volume, double price) => OrderSend(symbol, operation, volume, price, 0);

    public Order OrderSend(
        string symbol,
        Op operation,
        double volume,
        double price,
        int slippage = 0,
        double stoploss = 0.0,
        double takeprofit = 0.0,
        string comment = null,
        int magic = 0,
        DateTime expiration = default)
    {
        symbol = QuoteClient.MT4Symbol.correctCase(symbol);
        var cmd = MT4Order.Cmd.INSTANT;
        if (operation is Op.BuyLimit or Op.BuyStop or Op.SellLimit or Op.SellStop)
        {
            cmd = MT4Order.Cmd.PENDING;
        }
        else if (MT4Symbol.getInfo(symbol).Execution == Execution.Market)
        {
            cmd = MT4Order.Cmd.MARKET;
            price = 0.0;
            slippage = 0;
        }

        var lots = (int) Math.Round(volume / 0.01);
        var order = executeRequest(MT4Order.get(QuoteClient.User, 0, cmd, operation, symbol, lots, price, stoploss, takeprofit, slippage, comment, magic, expiration, PlacedManually), getRequestId());
        lock (QuoteClient.Orders)
        {
            if (!QuoteClient.Orders.ContainsKey(order.Ticket))
                QuoteClient.Orders.Add(order.Ticket, order);
        }

        return order;
    }

    public int OrderSendAsync(
        string symbol,
        Op operation,
        double volume,
        double price,
        int slippage,
        double stoploss,
        double takeprofit,
        string comment,
        int magic,
        DateTime expiration)
    {
        symbol = QuoteClient.MT4Symbol.correctCase(symbol);
        var cmd = MT4Order.Cmd.INSTANT;
        if (operation is Op.BuyLimit or Op.BuyStop or Op.SellLimit or Op.SellStop)
        {
            cmd = MT4Order.Cmd.PENDING;
        }
        else if (MT4Symbol.getInfo(symbol).Execution == Execution.Market)
        {
            cmd = MT4Order.Cmd.MARKET;
            price = 0.0;
            slippage = 0;
        }

        var lots = (int) Math.Round(volume / 0.01);
        var buf = MT4Order.get(QuoteClient.User, 0, cmd, operation, symbol, lots, price, stoploss, takeprofit, slippage, comment, magic, expiration, PlacedManually);
        var requestId = getRequestId();
        executeRequestAsync(buf, requestId);
        return requestId;
    }

    internal void onProgress(byte[] buf, ProgressType type, int RequestID, Exception exception)
    {
        var args = new OrderProgressEventArgs
        {
            TempID = RequestID,
            Type = type,
            Exception = exception
        };
        try
        {
            if (type is not ProgressType.Opened and not ProgressType.Closed and not ProgressType.Modified) goto label_5;
            args.Order = MT4Order.read(buf, 0);
        }
        catch (Exception ex)
        {
            Log.exception(ex);
        }

        label_5:
        ExecuteEvent(onProgr, args);
    }

    private void ExecuteEvent(WaitCallback callback, object args)
    {
        switch (QuoteClient.ProcessEvents)
        {
            case ProcessEvents.SingleThread:
                callback(args);
                break;
            case ProcessEvents.NewThread:
                new Thread((ThreadStart) (() => callback(args))).Start();
                break;
            case ProcessEvents.ThreadPool:
                ThreadPool.QueueUserWorkItem(callback, args);
                break;
        }
    }

    private void onProgr(object args)
    {
        try
        {
            if (OnOrderProgress == null)
                return;
            OnOrderProgress(this, (OrderProgressEventArgs) args);
        }
        catch (Exception ex)
        {
            Log.exception(ex);
        }
    }

    private Order executeRequest(byte[] buf, int tempID)
    {
        var transaction = getTransaction(tempID, buf);
        QuoteClient.CmdHandler.SendOrderTransaction(QuoteClient.Connection, transaction);
        return waitTransactionResult(transaction);
    }

    private void executeRequestAsync(byte[] buf, int tempID)
    {
        var item = getTransaction(tempID, buf);
        new Thread((ThreadStart) (() => execTransaction(item))).Start();
    }

    private void execTransaction(object obj)
    {
        try
        {
            var orderTransaction = (OrderTransaction) obj;
            QuoteClient.CmdHandler.SendOrderTransaction(QuoteClient.Connection, orderTransaction);
            waitTransactionResult(orderTransaction);
        }
        catch (Exception ex)
        {
            onProgress(null, ProgressType.Exception, ((OrderTransaction) obj).RequestID, ex);
        }
    }

    private Order waitTransactionResult(OrderTransaction item)
    {
        for (var index = 0; index < TradeTimeout / 1000; ++index)
        {
            if (!QuoteClient.Connected)
                throw new ConnectException("Connection lost during trading operation");
            item.GotResult.WaitOne(1000);
            if (item.OrderException != null)
                throw item.OrderException;
            if (item.OrderResult != null)
                return item.OrderResult.Length == 0 ? null : MT4Order.read(item.OrderResult, 0);
        }

        throw new TradeTimeoutException($"No reply from server in {TradeTimeout} ms");
    }

    private OrderTransaction getTransaction(int tempID, byte[] req)
    {
        if (QuoteClient.IsInvestor)
            throw new Exception("Investor password was used. Trading not allowed.");
        var transaction = new OrderTransaction(this, req)
        {
            RequestID = tempID
        };
        lock (QuoteClient.CmdHandler.Transactions)
        {
            QuoteClient.CmdHandler.Transactions.Add(tempID, transaction);
        }

        return transaction;
    }
}
