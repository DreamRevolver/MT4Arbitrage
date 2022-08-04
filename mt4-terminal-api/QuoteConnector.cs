namespace TradingAPI.MT4Server;

internal class QuoteConnector
{
    internal Thread ConnectThread;
    private Exception Exception;
    private readonly Logger Log;
    private readonly QuoteClient QC;
    private string Reason = "";

    public QuoteConnector(QuoteClient qc)
    {
        QC = qc;
        Log = qc.Log;
    }

    public void Run(int msTimeout)
    {
        lock (this)
        {
            while (ConnectThread != null)
                Thread.Sleep(1);
            if (QC.Connected)
                return;
            ConnectThread = new Thread(ConnectToAccount);
        }

        if (QC.CmdHandler != null)
            QC.CmdHandler.stop();
        QC.Connection.Close();
        Exception = null;
        Reason += "Starting thread";
        ConnectThread.Start();
        if (ConnectThread.Join(msTimeout))
        {
            Reason += ", thread completed";
            if (Exception == null)
            {
                try
                {
                    ++QC.ConenectCount;
                    Thread.Sleep(500);
                    lock (QC.Subscriber.Quotes)
                    {
                        if (QC.Subscriber.Quotes.Count > 0)
                            QC.Subscribe(QC.Subscriber.Quotes.Keys.ToArray());
                    }

                    ConnectMonitor.Add(QC);
                }
                finally
                {
                    ConnectThread = null;
                }

                QC.onConnect(null);
            }
            else
            {
                ConnectThread = null;
                QC.onConnect(Exception);
                if (Logger.DebugInfo)
                    throw new Exception(nameof(QuoteConnector), Exception);
                throw Exception;
            }
        }
        else
        {
            try
            {
                ConnectThread.Interrupt();
            }
            catch
            {
            }

            try
            {
                ConnectThread.Abort();
            }
            catch
            {
            }

            Exception exception = new TimeoutException($"Not connected in {msTimeout} ms: {Reason}");
            ConnectThread = null;
            QC.onConnect(exception);
            throw exception;
        }
    }

    public void runAsync(int msTimeout)
    {
        new Thread((ThreadStart) (() =>
        {
            try
            {
                Run(msTimeout);
            }
            catch (Exception)
            {
            }
        })).Start();
    }

    private void ConnectToAccount()
    {
        try
        {
            LoadAccount();
            QC.ConnectTime = DateTime.Now;
        }
        catch (Exception ex)
        {
            Exception = ex;
        }
    }

    private void LoadAccount()
    {
        var con = QC.Connection.ConnectAndLogin(QC.DataCenter, QC.Log, ref Reason);
        try
        {
            if (!QC.DataCenter)
            {
                Reason += ", getting servers";
                Log.trace("Getting servers");
                var is_demo = 0;
                var serversList = con.ReceiveServersList(out is_demo);
                QC.Connection.ServerVersion = BitConverter.ToUInt16(serversList, 0);
                QC.Connection.ServerBuild = BitConverter.ToUInt16(serversList, 2);
                if (QC.PathForSavingSrv != null)
                    QC.SrvInfo = ServerList.WriteServers(serversList, is_demo, QC.PathForSavingSrv);
                var memoryStream = new MemoryStream();
                ServerList.WriteServers(serversList, is_demo, memoryStream);
                QC.LatestSrv = memoryStream.ToArray();
                Reason += ", getting symbols";
                Log.trace("Getting symbols");
                QC.MT4Symbol.LoadSymbols(con, QC.PathForSavingSym);
                Reason += ", getting groups";
                Log.trace("Getting groups");
                var groups = con.ReceiveGroups();
                var conSymbolGroupArray = new ConSymbolGroup[32];
                for (var index = 0; index < 32; ++index)
                    conSymbolGroupArray[index] = UDT.ReadStruct<ConSymbolGroup>(groups, index * 80);
                QC._Groups = conSymbolGroupArray;
                Reason += ", getting mail history";
                Log.trace("Getting mail history");
                con.ReceiveMailHistory();
                Reason += ", getting orders history";
                Log.trace("Getting orders history");
                QC.ClosedOrders.Clear();
                QC.ClosedOrders.AddRange(OrderHistory.read(con, DateTime.Now.AddDays(-QC.OrderHistoryForLastNDays), DateTime.Now.AddDays(1.0)));
                QC.Connection.CreateTransactionKey();
            }

            Reason += ", getting account";
            Log.trace("Getting account");
            var account = con.ReceiveAccount();
            QC._Account = UDT.ReadStruct<ConGroup>(account, 88);
            if (!QC.DataCenter)
                UpdateSymbolsMargin();
            // QC.readOrders(account, QC.Connection.ServerBuild);
            QC.AccountType = (AccountType) BitConverter.ToInt32(account, 0);
            QC.Name = MT4Order.getString(account, 4);
            QC.Leverage = BitConverter.ToInt32(account, 68);
            QC.Balance = BitConverter.ToDouble(account, 72);
            QC.Credit = BitConverter.ToDouble(account, 80);
            QC._AccountMode = BitConverter.ToInt32(account, 13784);
            Reason += ", start command handler";
            QC.UpdateSymbolsMargin();
            QC.CalculateTradePropertiesAsync();
            QC.CmdHandler = new QuoteCmdHandler(QC);
            QC.CmdHandler.start(con);
        }
        catch (Exception)
        {
            con.Close();
            throw;
        }
    }

    private void UpdateSymbolsMargin()
    {
        for (var index = 0; index < QC.Account.secmargins.Length; ++index)
        {
            var secmargin = QC.Account.secmargins[index];
            var str = UDT.readStringASCII(secmargin.symbol, 0, secmargin.symbol.Length);
            if (str != "")
                str.ToString();
            foreach (var symbol in QC.Symbols)
            {
                var ex = QC.GetSymbolInfo(symbol).Ex;
                var num = ex.symbol.SequenceEqual(secmargin.symbol) ? 1 : 0;
                UDT.readStringASCII(ex.symbol, 0, ex.symbol.Length);
                if (num != 0)
                {
                    if (secmargin.margin_divider != 0.0)
                        ex.margin_divider = secmargin.margin_divider;
                    if (secmargin.swap_long != 0.0)
                        ex.swap_long = secmargin.swap_long;
                    if (secmargin.swap_short != 0.0)
                        ex.swap_short = secmargin.swap_short;
                }
            }
        }
    }
}
