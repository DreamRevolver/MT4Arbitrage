using System.Text;
using static TradingAPI.MT4Server.UpdateAction;

namespace TradingAPI.MT4Server;

internal class QuoteCmdHandler
{
    private readonly Logger Log;
    private readonly QuoteClient QuoteClient;
    private readonly Thread Thread;
    private bool HighRiskAccepted;
    internal bool Stop;
    public Dictionary<int, OrderTransaction> Transactions = new();

    public QuoteCmdHandler(QuoteClient quoteClient)
    {
        QuoteClient = quoteClient;
        Log = quoteClient.Log;
        Thread = new Thread(run)
        {
            Name = nameof(QuoteCmdHandler),
            IsBackground = true
        };
    }

    public bool Running => Thread.IsAlive && !Stop;

    public void start(Connection con)
    {
        Thread.Start(con);
    }

    public void stop()
    {
        Stop = true;
        try
        {
            if (Thread.Join(QuoteClient.DisconnectTimeout))
                return;
            Thread.Abort();
        }
        catch (Exception)
        {
        }
    }

    private void AcceptHightRisk(Connection con)
    {
        var str = "high risk investment warning has been accepted";
        var buf = new byte[9 + str.Length];
        buf[0] = 159;
        BitConverter.GetBytes(str.Length).CopyTo(buf, 1);
        BitConverter.GetBytes(0).CopyTo(buf, 5);
        Encoding.ASCII.GetBytes("high risk investment warning has been accepted").CopyTo(buf, 9);
        con.SendEncode(buf);
    }

    private void run(object obj)
    {
        var connection = (Connection) obj;
        var dateTime1 = DateTime.Now.AddSeconds(4.0);
        var dateTime2 = DateTime.Now.AddSeconds(8.0);
        byte num1 = 0;
        try
        {
            while (!Stop)
            {
                var now1 = DateTime.Now;
                DateTime now2;
                if (now1 > dateTime2)
                {
                    Log.trace("Ping2");
                    connection.Ping();
                    now2 = DateTime.Now;
                    dateTime2 = now2.AddSeconds(8.0);
                }

                QuoteClient.Subscriber.send(connection);
                QuoteClient.BarHistory.send(connection);
                if (now1 > dateTime1)
                {
                    Log.trace("Ping1");
                    connection.Ping();
                    now2 = DateTime.Now;
                    now2.AddSeconds(4.0);
                }

                var decode1 = connection.ReceiveDecode(1);
                QuoteClient.LastServerMessageTime = DateTime.Now;
                now2 = DateTime.Now;
                dateTime1 = now2.AddSeconds(4.0);
                var num2 = num1;
                num1 = decode1[0];
                switch (num1)
                {
                    case 2:
                        Log.trace("Cmd Ping");
                        break;
                    case 13:
                        Log.trace("Cmd Disconnect");
                        connection.Disconnect();
                        throw new Exception("Server decided to disconnect");
                    case 151:
                        Log.trace("Quote Cmd");
                        var decode2 = connection.ReceiveDecode(1);
                        var count1 = decode2[0] * 14;
                        Log.trace("Quote ReceiveDecode");
                        var decode3 = connection.ReceiveDecode(count1);
                        Log.trace("Quote Parse");
                        QuoteClient.Subscriber.parse(decode3);
                        break;
                    case 152:
                        Log.trace("Cmd IncomeNews");
                        ReceiveNews(connection);
                        break;
                    case 153:
                        Log.trace("Cmd IncomeMail");
                        connection.ReceiveCopmressed();
                        break;
                    case 154:
                        Log.trace("Cmd SymbolUpdate");
                        QuoteClient.MT4Symbol.update(connection.ReceiveCopmressed());
                        QuoteClient.UpdateSymbolsMargin();
                        QuoteClient.CalculateTradePropertiesAsync();
                        QuoteClient.onSymbolsUpdate();
                        break;
                    case 155:
                        Log.trace("Cmd OrderUpdate");
                        UpdateOrders(connection);
                        break;
                    case 156:
                        Log.trace("Cmd QuoteHistory");
                        var decode4 = connection.ReceiveDecode(24);
                        var symbol = MT4Order.getString(decode4, 0);
                        var uint16 = (Timeframe) BitConverter.ToUInt16(decode4, 12);
                        var count2 = BitConverter.ToInt32(decode4, decode4.Length - 4) * 28;
                        var decode5 = connection.ReceiveDecode(count2);
                        QuoteClient.BarHistory.parse(symbol, uint16, decode5);
                        break;
                    case 157:
                        Log.trace("Cmd NewsUpdate");
                        if (connection.ReceiveDecode(1)[0] == 0) ReceiveNews(connection);
                        break;
                    case 171:
                        Log.trace("Cmd ConGroupUpdate");
                        UpdateConGroup(connection.ReceiveCopmressed());
                        QuoteClient.UpdateSymbolsMargin();
                        QuoteClient.CalculateTradePropertiesAsync();
                        break;
                    case 190:
                    case 209:
                        Log.trace("Cmd OrderNotify");
                        var decode6 = connection.ReceiveDecode(6);
                        if (BitConverter.ToUInt32(decode6, 0) == 0U)
                            throw new ServerException(decode6[5]);
                        ReceiveOrderNotify(connection, decode6);
                        break;
                    default:
                        var numArray = connection.Receive(connection.Available);
                        var str = "";
                        foreach (var num4 in numArray)
                            str = $"{str}{num4:X} ";
                        throw new Exception($"Previous command {num2:X}. Unknown command {num1:X} {str}_{str.Length} ^{QuoteClient.ConenectCount}");
                }

                if (connection.Available == 0)
                    Thread.Sleep(QuoteClient.SleepTime);
            }

            connection.Disconnect();
        }
        catch (InvalidOperationException ex)
        {
            var message = $"Quote command = {num1:X}";
            Log.exception(Logger.DebugInfo ? new Exception(message, ex) : new Exception($"{ex.Message} in {message}"));
            Stop = true;
            QuoteClient.onDisconnect(ex);
            connection.Close();
        }
        catch (Exception ex)
        {
            var message = $"Quote command = {num1:X}";
            Log.exception(Logger.DebugInfo ? new Exception(message, ex) : new Exception($"{ex.Message} in {message}"));
            Stop = true;
            QuoteClient.onDisconnect(ex);
            connection.Close();
        }
    }

    private void UpdateOrders(Connection con)
    {
        var copmressed = con.ReceiveCopmressed();
        for (var index = 0; index < copmressed.Length; index += 272)
        {
            var balance = BitConverter.ToDouble(copmressed, index + 24);
            var credit = BitConverter.ToDouble(copmressed, index + 32);
            var order = MT4Order.read(copmressed, index + 48);
            var type = order.Type;
            var updateAction = PositionOpen;
            switch (copmressed[index + 4])
            {
                case 0:
                    updateAction = type is Op.Buy or Op.Sell ? PositionOpen : PendingOpen;
                    break;
                case 1:
                    switch (type)
                    {
                        case Op.Buy:
                        case Op.Sell:
                            updateAction = PositionClose;
                            break;
                        case Op.Balance:
                            updateAction = Balance;
                            break;
                        case Op.Credit:
                            updateAction = Credit;
                            break;
                        default:
                            updateAction = PendingClose;
                            break;
                    }

                    break;
                case 2:
                    updateAction = type is Op.Buy or Op.Sell ? PositionModify : PendingModify;
                    break;
            }

            QuoteClient.onOrderUpdate(new OrderUpdateEventArgs
            {
                Action = updateAction,
                Order = order
            }, balance, credit);
        }
    }

    private void UpdateConGroup(byte[] buf)
    {
        var num = buf.Length / 13712;
        for (var index = 0; index < num; ++index)
            if (BitConverter.ToUInt32(buf, index * 13712 + 4) == 2U)
            {
                var conGroup = UDT.ReadStruct<ConGroup>(buf, index * 13712 + 16);
                if (QuoteClient._Account.group == conGroup.group)
                    QuoteClient._Account = conGroup;
            }
    }

    private void ReceiveNews(SecureSocket sock)
    {
        var int32 = BitConverter.ToInt32(sock.ReceiveDecode(6), 0);
        if (int32 > 8388608)
            return;
        sock.ReceiveDecode(int32);
    }

    internal void SendOrderTransaction(Connection con, OrderTransaction item)
    {
        var array = new byte[1];
        var index = 0;
        item.Status = MT4Status.ORDER_ACCEPTED;
        var transactionPacket = item.CreateTransactionPacket(con);
        Array.Resize(ref array, index + transactionPacket.Length);
        transactionPacket.CopyTo(array, index);
        if (array.Length <= 1)
            return;
        con.SendEncode(array);
    }

    private void ReceiveOrderNotify(Connection con, byte[] buf)
    {
        Transactions[BitConverter.ToInt32(buf, 0)].OrderNotify(con, buf);
    }
}
