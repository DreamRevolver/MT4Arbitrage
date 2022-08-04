namespace TradingAPI.MT4Server;

public class OrderClientSafe
{
    public OrderClient OrderClient;
    public QuoteClient QuoteClient;
    public int TradeTimeoutSafe = 300000;

    public OrderClientSafe(OrderClient oc)
    {
        QuoteClient = oc.QuoteClient;
        OrderClient = oc;
        oc.TradeTimeout = 30000;
    }

    private int GetId() => int.Parse(Guid.NewGuid().ToString().GetHashCode().ToString().Replace("-", ""));

    public Order OrderSend(
        string symbol,
        Op operation,
        double volume,
        double price,
        int slippage,
        double stoploss,
        double takeprofit,
        string comment,
        DateTime expiration)
    {
        var id = GetId();
        var now = DateTime.Now;
        while (DateTime.Now.Subtract(now).TotalMilliseconds < TradeTimeoutSafe)
            try
            {
                return OrderClient.OrderSend(symbol, operation, volume, price, slippage, stoploss, takeprofit, comment, id, expiration);
            }
            catch (ConnectException)
            {
                while (!QuoteClient.Connected)
                    Thread.Sleep(1);
                foreach (var openedOrder in QuoteClient.GetOpenedOrders())
                    if (openedOrder.MagicNumber == id)
                        return openedOrder;
            }
            catch (TradeTimeoutException)
            {
                QuoteClient.ReconnectAsync();
                while (!QuoteClient.Connected)
                    Thread.Sleep(1);
                foreach (var openedOrder in QuoteClient.GetOpenedOrders())
                    if (openedOrder.MagicNumber == id)
                        return openedOrder;
            }

        throw new TradeTimeoutException($"Cannot send order in {TradeTimeoutSafe / 1000} seconds");
    }

    public Order OrderClose(
        string symbol,
        int ticket,
        double volume,
        double price,
        int slippage)
    {
        var now = DateTime.Now;
        while (DateTime.Now.Subtract(now).TotalMilliseconds < TradeTimeoutSafe)
            try
            {
                return OrderClient.OrderClose(symbol, ticket, volume, price, slippage);
            }
            catch (ConnectException)
            {
                while (!QuoteClient.Connected)
                    Thread.Sleep(1);
                foreach (var closedOrder in QuoteClient.ClosedOrders)
                    if (closedOrder.Ticket == ticket)
                        return closedOrder;
            }
            catch (TradeTimeoutException)
            {
                QuoteClient.ReconnectAsync();
                while (!QuoteClient.Connected)
                    Thread.Sleep(1);
                foreach (var closedOrder in QuoteClient.ClosedOrders)
                    if (closedOrder.Ticket == ticket)
                        return closedOrder;
            }

        throw new TradeTimeoutException($"Cannot close order in {TradeTimeoutSafe / 1000} seconds");
    }

    public void OrderDelete(int ticket, Op type, string symbol, double volume, double price)
    {
        var now = DateTime.Now;
        while (DateTime.Now.Subtract(now).TotalMilliseconds < TradeTimeoutSafe)
            try
            {
                OrderClient.OrderDelete(ticket, type, symbol, volume, price);
                return;
            }
            catch (ConnectException)
            {
                while (!QuoteClient.Connected)
                    Thread.Sleep(1);
                foreach (var closedOrder in QuoteClient.ClosedOrders)
                    if (closedOrder.Ticket == ticket)
                        return;
            }
            catch (TradeTimeoutException)
            {
                QuoteClient.ReconnectAsync();
                while (!QuoteClient.Connected)
                    Thread.Sleep(1);
                foreach (var closedOrder in QuoteClient.ClosedOrders)
                    if (closedOrder.Ticket == ticket)
                        return;
            }

        throw new TradeTimeoutException($"Cannot close order in {TradeTimeoutSafe / 1000} seconds");
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
        var now = DateTime.Now;
        while (DateTime.Now.Subtract(now).TotalMilliseconds < TradeTimeoutSafe)
            try
            {
                return OrderClient.OrderModify(ticket, symbol, type, volume, price, stoploss, takeprofit, expiration);
            }
            catch (ConnectException)
            {
                while (!QuoteClient.Connected)
                    Thread.Sleep(1);
                var flag = false;
                foreach (var openedOrder in QuoteClient.GetOpenedOrders())
                    if (openedOrder.Ticket == ticket)
                    {
                        flag = true;
                        if (openedOrder.Type == type && openedOrder.Lots == volume && openedOrder.OpenPrice == price && openedOrder.StopLoss == stoploss && openedOrder.TakeProfit == takeprofit && openedOrder.Expiration == expiration)
                            return openedOrder;
                    }

                if (!flag)
                    throw new Exception("Order not found in Opened Orders");
            }
            catch (TradeTimeoutException)
            {
                QuoteClient.ReconnectAsync();
                while (!QuoteClient.Connected)
                    Thread.Sleep(1);
                var flag = false;
                foreach (var openedOrder in QuoteClient.GetOpenedOrders())
                    if (openedOrder.Ticket == ticket)
                    {
                        flag = true;
                        if (openedOrder.Type == type && openedOrder.Lots == volume && openedOrder.OpenPrice == price && openedOrder.StopLoss == stoploss && openedOrder.TakeProfit == takeprofit && openedOrder.Expiration == expiration)
                            return openedOrder;
                    }

                if (!flag)
                    throw new Exception("Order not found in Opened Orders");
            }

        throw new TradeTimeoutException($"Cannot close order in {TradeTimeoutSafe / 1000} seconds");
    }
}
