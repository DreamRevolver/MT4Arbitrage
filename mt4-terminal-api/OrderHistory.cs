namespace TradingAPI.MT4Server;

internal class OrderHistory
{
    private Exception Ex;
    private readonly DateTime From;
    private readonly Logger Log;
    private readonly QuoteClient QC;
    private Order[] Result;
    private readonly Server[] Servers;
    private readonly DateTime To;

    public OrderHistory(Server[] servers, DateTime from, DateTime to, Logger log, QuoteClient qc)
    {
        Servers = servers;
        From = from;
        To = to;
        Log = log;
        QC = qc;
    }

    public Order[] get()
    {
        var thread = new Thread(getHistory)
        {
            Name = nameof(OrderHistory)
        };
        thread.Start();
        if (!thread.Join(QC.OrderHistoryTimeout))
        {
            try
            {
                thread.Abort();
            }
            catch (Exception)
            {
            }

            throw new TimeoutException("DownloadOrderHistory timeout");
        }

        if (Ex != null)
            throw Ex;
        return Result;
    }

    private void getHistory()
    {
        try
        {
            getHist();
        }
        catch (Exception ex)
        {
            Ex = ex;
        }
    }

    private void getHist()
    {
        var reason = "";
        Exception exception;
        try
        {
            var con = new Connection(new QuoteClient(QC.User, QC.Pass, QC.Host, QC.Port)
            {
                LoginIdPath = QC.LoginIdPath
            });
            con.ConnectAndLogin(false, Log, ref reason);
            try
            {
                Result = read(con, From, To);
                return;
            }
            finally
            {
                con.Disconnect();
            }
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        if (Servers != null)
            foreach (var server in Servers)
                try
                {
                    var con = new Connection(new QuoteClient(QC.User, QC.Pass, server.Host, server.Port)
                    {
                        LoginIdPath = QC.LoginIdPath
                    });
                    con.ConnectAndLogin(false, Log, ref reason);
                    try
                    {
                        Result = read(con, From, To);
                        return;
                    }
                    finally
                    {
                        con.Disconnect();
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

        if (exception != null)
            throw new Exception($"Cannot get history: {exception.Message}. {reason}");
        throw new Exception($"Cannot get history. {reason}");
    }

    internal static Order[] read(Connection con, DateTime from, DateTime to)
    {
        var buf = new byte[9];
        buf[0] = 34;
        BitConverter.GetBytes(MT4Order.toMtTime(from)).CopyTo(buf, 1);
        BitConverter.GetBytes(MT4Order.toMtTime(to)).CopyTo(buf, 5);
        con.SendEncode(buf);
        var decode = con.ReceiveDecode(1);
        switch (decode[0])
        {
            case 0:
            {
                var copmressed = con.ReceiveCopmressed();
                var orderList = new List<Order>();
                for (var i = 0; i < copmressed.Length; i += 224)
                {
                    var order = MT4Order.read(copmressed, i);
                    orderList.Add(order);
                }

                return orderList.ToArray();
            }
            case 1:
                return new Order[0];
            default:
                throw new ServerException(decode[0]);
        }
    }
}
