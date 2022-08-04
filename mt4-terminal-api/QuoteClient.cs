using System.Collections;
using System.Net.NetworkInformation;
using System.Text;

namespace TradingAPI.MT4Server;

public class QuoteClient
{
    private static readonly Dictionary<string, QuoteClient> Clients = new();
    internal readonly Dictionary<int, Order> Orders = new();
    internal ConGroup _Account;
    internal int _AccountMode;
    internal ConSymbolGroup[] _Groups;
    internal int _SleepTime = 5;
    internal bool AutoReconnect;
    internal AutoReconnect AutoReconnecter;
    internal double Balance;
    internal BarHistory BarHistory;
    public bool CalculateTradeProps = true;
    internal QuoteCmdHandler CmdHandler;
    internal int ConenectCount;
    internal Connection Connection;
    public int ConnectTimeout = 30000;
    internal double Credit;
    internal bool DataCenter;
    internal QuoteClient DataQC;
    public int DisconnectTimeout = 1000;
    public int DownloadQuoteHistoryTimeout = 60000;
    internal double Equity;
    internal double FreeMargin;
    private DateTime LastCalculateTradeProperties = DateTime.Now;
    public DateTime LastServerMessageTime = DateTime.Now;
    public byte[] LatestSrv;
    internal int Leverage;
    internal Logger Log;
    public string LoginIdPath = "http://a.mtapi.su:4400/loginid";
    public int LoginIdWebServerTimeout = 15000;
    internal double Margin;
    internal MT4Symbol MT4Symbol = new();
    internal string Name;
    public int NoServerMessagesTimeout = 30000;
    public int OrderHistoryForLastNDays = 1;
    public int OrderHistoryTimeout = 30000;
    internal string Pass;
    public string PathForSavingSrv;
    public string PathForSavingSym;
    public ProcessEvents ProcessEvents = ProcessEvents.ThreadPool;
    internal double Profit;
    internal QuoteConnector QuoteConnector;
    private readonly Dictionary<string, List<Bar>> QuoteHistory = new();
    internal ServerInfo SrvInfo;
    internal Subscriber Subscriber;
    internal DateTime Time;

    public QuoteClient()
    {
    }

    public QuoteClient(int user, string password, string host, int port)
    {
        ConvertTo.DateTime(1611756349L);
        Log = new Logger(this);
        Log.trace("Initializing quote client");
        Trial.check();
        User = user;
        Host = host;
        Port = port;
        Pass = password;
        Connection = new Connection(this);
        Subscriber = new Subscriber(this, Log);
        BarHistory = new BarHistory(this);
        AutoReconnecter = new AutoReconnect(this, AutoReconnect);
        QuoteConnector = new QuoteConnector(this);
    }

    public QuoteClient(
        int user,
        string password,
        string host,
        int port,
        DateTime closedOrdersFrom,
        DateTime closedOrdersTo)
    {
        Log = new Logger(this);
        Log.trace("Initializing quote client");
        Trial.check();
        User = user;
        Host = host;
        Port = port;
        Pass = password;
        Connection = new Connection(this);
        Subscriber = new Subscriber(this, Log);
        BarHistory = new BarHistory(this);
        AutoReconnecter = new AutoReconnect(this, AutoReconnect);
        QuoteConnector = new QuoteConnector(this);
    }

    public QuoteClient(
        int user,
        string password,
        string host,
        int port,
        string proxyHost,
        int proxyPort,
        string proxyUser,
        string proxyPass,
        ProxyTypes type)
    {
        Log = new Logger(this);
        Log.trace("Initializing quote client");
        Trial.check();
        User = user;
        Host = host;
        Port = port;
        Pass = password;
        Connection = new Connection(proxyHost, proxyPort, proxyUser, proxyPass, type, this);
        Subscriber = new Subscriber(this, Log);
        BarHistory = new BarHistory(this);
        AutoReconnecter = new AutoReconnect(this, AutoReconnect);
        QuoteConnector = new QuoteConnector(this);
    }

    public OrderClient OrderClient { get; internal set; }

    public int User { get; internal set; }

    public string Host { get; set; }

    public int SleepTime
    {
        get => _SleepTime;
        set
        {
            var num = value;
            if (num <= 0)
                num = 1;
            if (num > 20)
                num = 20;
            _SleepTime = num;
        }
    }

    public double AccountBalance
    {
        get
        {
            if (!Connected && QuoteConnector.ConnectThread == null)
                Connect();
            return Balance;
        }
    }

    public double AccountCredit
    {
        get
        {
            if (!Connected && QuoteConnector.ConnectThread == null)
                Connect();
            return Credit;
        }
    }

    public double AccountProfit
    {
        get
        {
            if (!Connected && QuoteConnector.ConnectThread == null)
                Connect();
            var accountProfit = 0.0;
            foreach (var openedOrder in GetOpenedOrders())
                accountProfit += openedOrder.Profit + openedOrder.Ex.commission + openedOrder.Ex.storage + openedOrder.Ex.taxes;
            return accountProfit;
        }
    }

    public double AccountEquity
    {
        get
        {
            if (!Connected && QuoteConnector.ConnectThread == null)
                Connect();
            return AccountBalance + AccountProfit;
        }
    }

    public double AccountMargin
    {
        get
        {
            if (!Connected && QuoteConnector.ConnectThread == null)
                Connect();
            return !CalculateTradeProps ? 0.0 : Margin;
        }
    }

    public double AccountFreeMargin
    {
        get
        {
            if (!Connected && QuoteConnector.ConnectThread == null)
                Connect();
            return !CalculateTradeProps ? 0.0 : FreeMargin;
        }
    }

    public string AccountName => Name;

    public int AccountLeverage => Leverage;

    public ConGroup Account => _Account;

    public ConSymbolGroup[] Groups => _Groups;

    public DateTime ServerTime => Time;

    public int ServerBuild => Connection.ServerBuild;

    public bool IsInvestor => _AccountMode == 1;

    public DateTime ConnectTime { get; internal set; }

    public int Port { get; set; }

    public AccountType AccountType { get; internal set; }

    public SymbolInfo[] SymbolsInfo
    {
        get
        {
            if (MT4Symbol == null)
                throw new Exception("Not connected");
            var array = new SymbolInfo[MT4Symbol.Info.Values.Count];
            MT4Symbol.Info.Values.CopyTo(array, 0);
            return array;
        }
    }

    public List<Order> ClosedOrders { get; internal set; } = new();

    public string[] Symbols => MT4Symbol != null ? MT4Symbol.Names : throw new Exception("Not connected");

    public ConGroupSec[] GroupParameters => Account.secgroups;

    public bool Connected => CmdHandler is {Stop: false} && Connection.Client is {Connected: true} && DateTime.Now.Subtract(LastServerMessageTime).TotalMilliseconds <= NoServerMessagesTimeout && CmdHandler.Running;

    public int BytesCountInRecieveQueue
    {
        get
        {
            try
            {
                return Connection.Available;
            }
            catch (Exception)
            {
                return -1;
            }
        }
    }

    public void ChangePassword(string password, bool investor)
    {
        var connection = new Connection(this);
        var reason = "";
        connection.ConnectAndLogin(false, Log, ref reason);
        var buf = new byte[21];
        buf[0] = 4;
        buf[1] = investor ? (byte) 1 : (byte) 0;
        var bytes = Encoding.ASCII.GetBytes(password);
        if (bytes.Length > 16)
            Array.Resize(ref bytes, 16);
        bytes.CopyTo(buf, 5);
        connection.SendEncode(buf);
        var decode = connection.ReceiveDecode(1);
        if (decode[0] != 0)
            throw new ServerException(decode[0]);
        connection.Disconnect();
    }

    public static int PingHost(string nameOrAddress)
    {
        Ping ping = null;
        try
        {
            ping = new Ping();
            var pingReply = ping.Send(nameOrAddress, 500);
            if (pingReply.Status == IPStatus.Success)
                return (int) pingReply.RoundtripTime;
        }
        catch (PingException)
        {
        }
        finally
        {
            ping?.Dispose();
        }

        return -1;
    }

    public void ReconnectAsync()
    {
        Log.info("Reconnect");
        AutoReconnecter.Run = true;
        if (CmdHandler != null)
            CmdHandler.stop();
        AutoReconnecter.Go();
    }

    public event SymbolsUpdateEventHandler OnSymbolsUpdate;

    public event OrderUpdateEventHandler OnOrderUpdate;

    public event QuoteHistoryEventHandler OnQuoteHistory;

    public event DisconnectEventHandler OnDisconnect;

    public event ConnectEventHandler OnConnect;

    public event QuoteEventHandler OnQuote;

    public void SetAutoReconnect(bool b)
    {
        AutoReconnect = b;
        if (b)
            return;
        AutoReconnecter.Run = false;
    }

    internal void InitSymbols(QuoteClient qc)
    {
        MT4Symbol = qc.MT4Symbol;
    }

    public static QuoteClient GetQuoteClient(
        long user,
        string password,
        string host,
        int port,
        string loginIdPath = null) => GetQuoteClient((int) user, password, host, port, loginIdPath);

    public static void DeleteQuoteClient(int user, string password, string host, int port)
    {
        lock (Clients)
        {
            var key = user + password + host + port;
            if (!Clients.ContainsKey(key))
                return;
            try
            {
                Clients[key].Disconnect();
            }
            catch (Exception)
            {
            }

            Clients.Remove(key);
        }
    }

    public static QuoteClient GetQuoteClient(
        int user,
        string password,
        string host,
        int port,
        string loginIdPath = null)
    {
        lock (Clients)
        {
            var key = user + password + host + port;
            if (Clients.ContainsKey(key))
            {
                if (Clients[key].Connected)
                    return Clients[key];
                Clients.Remove(key);
                var quoteClient = new QuoteClient(user, password, host, port);
                if (loginIdPath != null)
                    quoteClient.LoginIdPath = loginIdPath;
                quoteClient.Connect();
                Clients.Add(key, quoteClient);
                return quoteClient;
            }

            var quoteClient1 = new QuoteClient(user, password, host, port);
            if (loginIdPath != null)
                quoteClient1.LoginIdPath = loginIdPath;
            quoteClient1.Connect();
            Clients.Add(key, quoteClient1);
            return quoteClient1;
        }
    }

    public double GetAskTickValue(QuoteEventArgs quote)
    {
        var symbolInfo = GetSymbolInfo(quote.Symbol);
        new OrderProfit(this).UpdateSymbolTick(symbolInfo.Ex, quote.Bid, quote.Ask);
        return symbolInfo.Ex.ask_tickvalue;
    }

    public double GetBidTickValue(QuoteEventArgs quote)
    {
        var symbolInfo = GetSymbolInfo(quote.Symbol);
        new OrderProfit(this).UpdateSymbolTick(symbolInfo.Ex, quote.Bid, quote.Ask);
        return symbolInfo.Ex.bid_tickvalue;
    }

    public double GetTickValue(QuoteEventArgs quote)
    {
        var symbolInfo = GetSymbolInfo(quote.Symbol);
        new OrderProfit(this).UpdateSymbolTick(symbolInfo.Ex, quote.Bid, quote.Ask);
        return (symbolInfo.Ex.bid_tickvalue + symbolInfo.Ex.ask_tickvalue) / 2.0;
    }

    public void Init(int user, string password, string host, int port)
    {
        Log = new Logger(this);
        Log.trace("Initializing quote client");
        Trial.check();
        User = user;
        Host = host;
        Port = port;
        Pass = password;
        Connection = new Connection(this);
        Subscriber = new Subscriber(this, Log);
        BarHistory = new BarHistory(this);
        AutoReconnecter = new AutoReconnect(this, AutoReconnect);
        QuoteConnector = new QuoteConnector(this);
    }

    public void Init(int user, string password, string serverFilePath)
    {
        Log = new Logger(this);
        Log.trace("Initializing quote client");
        Trial.check();
        User = user;
        Pass = password;
        var temp = HostAndPort.parse(serverFilePath);
        var key = temp.Key;
        var value = temp.Value;
        Host = key;
        Port = value;
        Connection = new Connection(this);
        Subscriber = new Subscriber(this, Log);
        BarHistory = new BarHistory(this);
        AutoReconnecter = new AutoReconnect(this, AutoReconnect);
        QuoteConnector = new QuoteConnector(this);
    }

    public static MainServer LoadSrv(string serverFilePath, out Server[] srvList) => ServerList.ReadServers(serverFilePath, out srvList);

    public static MainServer LoadSrv(byte[] srv, out Server[] srvList) => ServerList.ReadServers(srv, out srvList);

    public static MainServer LoadSrv(string serverFilePath) => ServerList.ReadServers(serverFilePath, out _);

    public ServerInfo GetServerInfo()
    {
        if (!Connected)
            throw new Exception("Not connected");
        return SrvInfo;
    }

    public static DemoAccount GetDemo(
        string serverFilePath,
        int leverage,
        double balance,
        string name,
        string accountType,
        string country,
        string city,
        string state,
        string zip,
        string address,
        string phone,
        string email,
        string terminalCompany) => Demo.get(serverFilePath, leverage, balance, name, accountType, country, city, state, zip, address, phone, email, terminalCompany);

    public static DemoAccount GetDemo(
        string host,
        int port,
        int leverage,
        double balance,
        string name,
        string accountType,
        string country,
        string city,
        string state,
        string zip,
        string address,
        string phone,
        string email,
        string terminalCompany) => Demo.get(host, port, leverage, balance, name, accountType, country, city, state, zip, address, phone, email, terminalCompany);

    public SymbolInfo GetSymbolInfo(string symbol) => MT4Symbol.getInfo(symbol);

    public ConGroupSec GetSymbolGroupParams(string symbol) => GroupParameters[MT4Symbol.getInfo(symbol).Ex.type];

    public ConSymbolGroup GetSymbolGroup(string symbol) => Groups[MT4Symbol.getInfo(symbol).Ex.type];

    public bool IsSubscribed(string symbol) => Connected && MT4Symbol.exist(symbol) && Subscriber.subscribed(symbol);

    public void Subscribe(string symbol)
    {
        Log.trace(nameof(Subscribe));
        if (!Connected)
            Connect();
        if (!MT4Symbol.exist(symbol))
            throw new Exception($"{symbol} not exist");
        Subscriber.subscribe(symbol);
    }

    public void Unsubscribe(string symbol)
    {
        Log.trace(nameof(Unsubscribe));
        if (!Connected)
            throw new ConnectException("Not connected");
        if (CalculateTradeProps)
            foreach (var openedOrder in GetOpenedOrders())
                if (openedOrder.Type is Op.Buy or Op.Sell && openedOrder.Symbol == symbol)
                    return;
        Subscriber.unsubscribe(symbol);
    }

    public void Subscribe(string[] symbols)
    {
        Log.trace(nameof(Subscribe));
        if (!Connected)
            Connect();
        foreach (var symbol in symbols)
            if (!MT4Symbol.exist(symbol))
                throw new Exception($"{symbol} not exist");
        Subscriber.subscribe(symbols);
    }

    public QuoteEventArgs GetQuote(string symbol)
    {
        if (!Connected)
            Connect();
        return Subscriber.getQuote(symbol);
    }

    public void Connect()
    {
        if (Connected)
            return;
        if (AutoReconnect)
            AutoReconnecter.Run = true;
        QuoteConnector.Run(ConnectTimeout);
    }

    internal void ConnectInternal()
    {
        QuoteConnector.Run(ConnectTimeout);
    }

    public void ConnectAsync()
    {
        Log.trace("Connecting MT4 quote client async");
        AutoReconnecter.Run = true;
        Time = new DateTime();
        AutoReconnecter.Run = true;
        QuoteConnector.runAsync(ConnectTimeout);
    }

    public Order[] GetOpenedOrders()
    {
        Log.trace(nameof(GetOpenedOrders));
        Order[] array;
        lock (Orders)
        {
            array = new Order[Orders.Count];
            Orders.Values.CopyTo(array, 0);
        }

        return array;
    }

    public Order GetOpenedOrder(int ticket)
    {
        lock (Orders)
        {
            return Orders.ContainsKey(ticket) ? Orders[ticket] : null;
        }
    }

    public void RequestQuoteHistory(string symbol, Timeframe tf, DateTime from, short count)
    {
        Log.trace("DownloadQuoteHistory");
        if (!Connected)
            throw new Exception("Not connected");
        BarHistory.request(symbol, tf, from, count);
    }

    public Bar[] DownloadQuoteHistory(string symbol, Timeframe tf, DateTime from, int count)
    {
        if (!MT4Symbol.exist(symbol))
            throw new Exception($"{symbol} not exist");
        Log.trace(nameof(DownloadQuoteHistory));
        if (!Connected)
            throw new Exception("Not connected");
        var key = symbol + tf;
        if (QuoteHistory.ContainsKey(key))
            throw new Exception($"Prevoius request for {key} still running");
        QuoteHistory.Add(key, new List<Bar>());
        var barList = QuoteHistory[key];
        OnQuoteHistory += QuoteClient_OnQuoteHistory;
        var now = DateTime.Now;
        try
        {
            BarHistory.request(symbol, tf, from, (short) count);
            while (DateTime.Now.Subtract(now).TotalMilliseconds <= DownloadQuoteHistoryTimeout)
            {
                lock (barList)
                {
                    try
                    {
                        for (var index = 0; index < barList.Count; ++index)
                        {
                            var bar = barList[index];
                            if (bar.Time == new DateTime(1970, 1, 1) || bar.Time == new DateTime())
                            {
                                barList.RemoveAt(index);
                                return barList.ToArray();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.exception(ex);
                    }
                }

                Thread.Sleep(250);
            }

            throw new Exception($"Cannot DownloadQuoteHistory in {DownloadQuoteHistoryTimeout} ms");
        }
        finally
        {
            QuoteHistory.Remove(key);
            OnQuoteHistory -= QuoteClient_OnQuoteHistory;
        }
    }

    private void QuoteClient_OnQuoteHistory(object sender, QuoteHistoryEventArgs args)
    {
        var key = args.Symbol + args.Timeframe;
        if (!QuoteHistory.ContainsKey(key))
            return;
        lock (QuoteHistory[key])
        {
            QuoteHistory[key].AddRange(args.Bars);
        }
    }

    public Order[] DownloadOrderHistory(DateTime from, DateTime to)
    {
        Server[] srvList = null;
        if (Connected)
            LoadSrv(LatestSrv, out srvList);
        var collection = new OrderHistory(srvList, from, to, Log, this).get();
        ClosedOrders.AddRange(collection);
        return collection;
    }

    internal void onQuoteHistory(string symbol, Timeframe tf, Bar[] bars)
    {
        ExecuteEvent(onQuoteHist, new QuoteHistoryEventArgs
        {
            Symbol = symbol,
            Bars = bars,
            Timeframe = tf
        });
    }

    private void onQuoteHist(object args)
    {
        try
        {
            if (OnQuoteHistory == null)
                return;
            OnQuoteHistory(this, (QuoteHistoryEventArgs) args);
        }
        catch (Exception ex)
        {
            Log.exception(ex);
        }
    }

    internal void onDisconnect(Exception exception)
    {
        var args = new DisconnectEventArgs
        {
            Exception = exception
        };
        AutoReconnecter.Go();
        ExecuteEvent(onDiscon, args);
    }

    private void onDiscon(object args)
    {
        try
        {
            if (OnDisconnect == null)
                return;
            OnDisconnect(this, (DisconnectEventArgs) args);
        }
        catch (Exception ex)
        {
            Log.exception(ex);
        }
    }

    internal void onConnect(Exception exception)
    {
        ExecuteEvent(onCon, new ConnectEventArgs
        {
            Exception = exception
        });
    }

    private void onCon(object args)
    {
        try
        {
            if (OnConnect == null)
                return;
            OnConnect(this, (ConnectEventArgs) args);
        }
        catch (Exception ex)
        {
            Log.exception(ex);
        }
    }

    internal void readOrders(byte[] buf, int serverBuild)
    {
        lock (Orders)
        {
            Orders.Clear();
            var i = 13824;
            if (serverBuild >= 1290)
                i = 13856;
            for (; i < buf.Length; i += 224)
            {
                var order = MT4Order.read(buf, i);
                Orders.Add(order.Ticket, order);
                if (CalculateTradeProps && order.Type is Op.Buy or Op.Sell)
                    Subscriber.subscribe(order.Symbol);
            }
        }
    }

    public void Disconnect()
    {
        AutoReconnecter.Run = false;
        AutoReconnecter.Stop();
        if (CmdHandler != null)
            CmdHandler.stop();
        Connection.Close();
        ConnectMonitor.Delete(this);
    }

    internal void onOrderUpdate(OrderUpdateEventArgs update, double balance, double credit)
    {
        if (balance != 0.0)
            Balance = balance;
        if (credit != 0.0)
            Credit = credit;
        try
        {
            lock (Orders)
            {
                if (Orders.ContainsKey(update.Order.Ticket))
                {
                    switch (update.Action)
                    {
                        case UpdateAction.PositionModify:
                        {
                            var type1 = Orders[update.Order.Ticket].Type;
                            var type2 = update.Order.Type;
                            switch (type1)
                            {
                                case Op.BuyLimit or Op.BuyStop when type2 == Op.Buy:
                                case Op.SellLimit or Op.SellStop when type2 == Op.Sell:
                                    update.Action = UpdateAction.PendingFill;
                                    break;
                            }

                            Orders[update.Order.Ticket] = update.Order;
                            break;
                        }
                        case UpdateAction.PositionClose:
                        {
                            var order1 = update.Order;
                            var order2 = Orders[order1.Ticket];
                            Orders.Remove(order2.Ticket);
                            if (order1.Comment.StartsWith("to #"))
                            {
                                var num = int.Parse(order1.Comment.Substring("to #".Length));
                                order2.Ticket = num;
                                order2.Lots -= order1.Lots;
                                Orders.Add(order2.Ticket, order2);
                            }

                            break;
                        }
                        case UpdateAction.PendingClose:
                            Orders.Remove(update.Order.Ticket);
                            break;
                        default:
                            Orders[update.Order.Ticket] = update.Order;
                            break;
                    }
                }
                else
                {
                    if (update.Action is not UpdateAction.PositionOpen and not UpdateAction.PendingOpen) goto label_29;
                    Orders.Add(update.Order.Ticket, update.Order);
                    if (CalculateTradeProps)
                    {
                        if (update.Order.Type is not Op.Buy and not Op.Sell) goto label_29;
                        Subscriber.subscribe(update.Order.Symbol);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.exception(ex);
        }

        label_29:
        ExecuteEvent(onUpdate, update);
    }

    private void ExecuteEvent(WaitCallback callback, object args)
    {
        switch (ProcessEvents)
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

    private void onUpdate(object args)
    {
        var update = (OrderUpdateEventArgs) args;
        try
        {
            lock (ClosedOrders)
            {
                if (update.Action is not UpdateAction.PositionClose and not UpdateAction.PendingClose) goto label_9;
                ClosedOrders.Add(update.Order);
            }
        }
        catch (Exception ex)
        {
            Log.exception(ex);
        }

        label_9:
        try
        {
            if (update.Action is not UpdateAction.PositionClose and not UpdateAction.PendingClose)
                lock (Orders)
                {
                    switch (update.Order.Type)
                    {
                        case Op.Buy:
                        {
                            if (Subscriber.subscribed(update.Order.Symbol))
                            {
                                var quote = GetQuote(update.Order.Symbol);
                                if (quote != null)
                                    update.Order.ClosePrice = quote.Bid;
                            }

                            break;
                        }
                        case Op.Sell:
                        {
                            if (Subscriber.subscribed(update.Order.Symbol))
                            {
                                var quote = GetQuote(update.Order.Symbol);
                                if (quote != null)
                                    update.Order.ClosePrice = quote.Ask;
                            }

                            break;
                        }
                    }
                }
        }
        catch (Exception ex)
        {
            Log.exception(ex);
        }

        try
        {
            CalculateTradeProperties();
        }
        catch (Exception ex)
        {
            Log.exception(ex);
        }

        try
        {
            if (OnOrderUpdate == null)
                return;
            OnOrderUpdate(this, update);
        }
        catch (Exception ex)
        {
            Log.exception(ex);
        }
    }

    internal void onSymbolsUpdate()
    {
        ExecuteEvent(onSymbolsUpdate, null);
    }

    private void onSymbolsUpdate(object args)
    {
        try
        {
            if (OnSymbolsUpdate == null)
                return;
            OnSymbolsUpdate(this);
        }
        catch (Exception ex)
        {
            Log.exception(ex);
        }
    }

    internal void onQuote(QuoteEventArgs quote)
    {
        try
        {
            Time = quote.Time;
            var spreadDiff = GetSymbolGroupParams(quote.Symbol).spread_diff;
            var point = GetSymbolInfo(quote.Symbol).Point;
            if (spreadDiff != 0)
            {
                quote.Bid -= spreadDiff * point / 2.0;
                quote.Ask += spreadDiff * point / 2.0;
            }

            if (CalculateTradeProps)
            {
                ThreadPool.QueueUserWorkItem(CalcProfit, quote);
                CalculateTradePropertiesAsync();
            }

            ExecuteEvent(onquote, quote);
        }
        catch (Exception ex)
        {
            Log.exception(ex);
        }
    }

    private void CalcProfit(object args)
    {
        try
        {
            var quoteEventArgs = (QuoteEventArgs) args;
            GetSymbolInfo(quoteEventArgs.Symbol);
            foreach (var openedOrder in GetOpenedOrders())
                if (openedOrder.Symbol == quoteEventArgs.Symbol)
                {
                    switch (openedOrder.Type)
                    {
                        case Op.Buy:
                            openedOrder.ClosePrice = quoteEventArgs.Bid;
                            break;
                        case Op.Sell:
                            openedOrder.ClosePrice = quoteEventArgs.Ask;
                            break;
                    }

                    if (openedOrder.Type is Op.Buy or Op.Sell)
                        new OrderProfit(this).Update(openedOrder, quoteEventArgs.Bid, quoteEventArgs.Ask);
                }
                else if (openedOrder.Type is Op.Buy or Op.Sell && !IsSubscribed(openedOrder.Symbol))
                {
                    Subscribe(openedOrder.Symbol);
                }
        }
        catch (Exception ex)
        {
            if (ex is ConnectException)
                return;
            Log.exception(ex);
        }
    }

    private void onquote(object args)
    {
        try
        {
            if (OnQuote == null)
                return;
            OnQuote(this, (QuoteEventArgs) args);
        }
        catch (Exception ex)
        {
            Log.exception(ex);
        }
    }

    internal void UpdateSymbolsMargin()
    {
        for (var index = 0; index < _Account.secmargins_total; ++index)
            MT4Symbol.UpdateMargin(_Account.secmargins[index]);
    }

    internal bool GetRate(string currency, out double[] rate)
    {
        rate = new double[2];
        var quote = GetQuote(currency);
        if (quote == null)
            return false;
        rate[0] = quote.Bid;
        rate[1] = quote.Ask;
        return true;
    }

    internal double CalculateOrderProfit(Order order, SymbolInfoEx sym, double bid, double ask)
    {
        var orderProfit = order.Profit;
        switch (sym.profit_mode)
        {
            case 2 when sym.tick_value <= 0.0 || sym.tick_size <= 0.0:
                return orderProfit;
            case 0 when _Account.currency == vUTF.toString(sym.symbol, 0).Substring(0, 3):
                sym.bid_tickvalue = sym.contract_size / bid;
                sym.ask_tickvalue = sym.contract_size / ask;
                break;
            case 0 when _Account.currency == vUTF.toString(sym.symbol, 0).Substring(3, 3):
                sym.bid_tickvalue = sym.contract_size;
                sym.ask_tickvalue = sym.contract_size;
                break;
            case 0:
            {
                double[] rate;
                if (GetRate(_Account.currency.Substring(0, 3) + vUTF.toString(sym.symbol, 0).Substring(3), out rate))
                {
                    sym.bid_tickvalue = sym.contract_size / rate[0];
                    sym.ask_tickvalue = sym.contract_size / rate[1];
                }
                else if (GetRate(vUTF.toString(sym.symbol, 0).Substring(3, 3) + _Account.currency.Substring(0, 3) + vUTF.toString(sym.symbol, 0).Substring(6), out rate))
                {
                    sym.bid_tickvalue = sym.contract_size * rate[0];
                    sym.ask_tickvalue = sym.contract_size * rate[1];
                }
                else
                {
                    if (GetRate($"USD{vUTF.toString(sym.symbol, 0).Substring(3)}", out rate))
                    {
                        sym.bid_tickvalue = sym.contract_size / rate[0];
                        sym.ask_tickvalue = sym.contract_size / rate[1];
                    }
                    else
                    {
                        if (!GetRate($"{vUTF.toString(sym.symbol, 0).Substring(0, 3)}USD{vUTF.toString(sym.symbol, 0).Substring(6)}", out rate))
                            return orderProfit;
                        sym.bid_tickvalue = sym.contract_size * rate[0];
                        sym.ask_tickvalue = sym.contract_size * rate[1];
                    }

                    if (GetRate($"USD{_Account.currency.Substring(0, 3)}{vUTF.toString(sym.symbol, 0).Substring(6)}", out rate))
                    {
                        sym.bid_tickvalue *= rate[0];
                        sym.ask_tickvalue *= rate[1];
                    }
                    else
                    {
                        if (!GetRate($"{_Account.currency.Substring(0, 3)}USD{vUTF.toString(sym.symbol, 0).Substring(6)}", out rate))
                            return orderProfit;
                        sym.bid_tickvalue /= rate[0];
                        sym.ask_tickvalue /= rate[1];
                    }
                }

                break;
            }
            case 1:
            case 2:
            {
                var num = sym.profit_mode == 2 ? sym.tick_value : sym.contract_size;
                sym.bid_tickvalue = num;
                sym.ask_tickvalue = num;
                if (sym.currency != _Account.currency)
                {
                    double[] rate;
                    if (GetRate(_Account.currency.Substring(0, 3) + sym.currency.Substring(0, 3), out rate))
                    {
                        sym.bid_tickvalue /= rate[0];
                        sym.ask_tickvalue = sym.bid_tickvalue;
                    }
                    else if (GetRate(sym.currency.Substring(0, 3) + _Account.currency.Substring(0, 3), out rate))
                    {
                        sym.bid_tickvalue *= rate[0];
                        sym.ask_tickvalue = sym.bid_tickvalue;
                    }
                    else
                    {
                        if (GetRate($"USD{sym.currency.Substring(0, 3)}", out rate))
                        {
                            sym.bid_tickvalue = sym.contract_size / rate[0];
                            sym.ask_tickvalue = sym.contract_size / rate[1];
                        }
                        else
                        {
                            if (!GetRate($"{sym.currency.Substring(0, 3)}USD", out rate))
                                return orderProfit;
                            sym.bid_tickvalue = sym.contract_size * rate[0];
                            sym.ask_tickvalue = sym.contract_size * rate[1];
                        }

                        if (GetRate($"USD{_Account.currency.Substring(0, 3)}", out rate))
                        {
                            sym.bid_tickvalue *= rate[0];
                            sym.ask_tickvalue *= rate[1];
                        }
                        else
                        {
                            if (!GetRate($"{_Account.currency.Substring(0, 3)}USD", out rate))
                                return orderProfit;
                            sym.bid_tickvalue /= rate[0];
                            sym.ask_tickvalue /= rate[1];
                        }
                    }
                }

                break;
            }
        }

        if (sym.bid_tickvalue is <= 0.0 or >= double.MaxValue || sym.ask_tickvalue is <= 0.0 or >= double.MaxValue)
            return orderProfit;
        if (sym.profit_mode == 0)
        {
            switch (order.Type)
            {
                case Op.Buy:
                {
                    var num = order.Ex.volume * sym.bid_tickvalue / 100.0;
                    orderProfit = Math.Round(order.ClosePrice * num, 2) - Math.Round(order.OpenPrice * num, 2);
                    break;
                }
                case Op.Sell:
                {
                    var num = order.Ex.volume * sym.ask_tickvalue / 100.0;
                    orderProfit = Math.Round(order.OpenPrice * num, 2) - Math.Round(order.ClosePrice * num, 2);
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
                    num = (order.ClosePrice - order.OpenPrice) * sym.bid_tickvalue;
                    break;
                case Op.Sell:
                    num = (order.OpenPrice - order.ClosePrice) * sym.ask_tickvalue;
                    break;
            }

            orderProfit = num * order.Ex.volume / 100.0;
            if (sym.profit_mode == 2)
                orderProfit /= sym.tick_size;
        }

        return Math.Round(orderProfit, 2);
    }

    private double CalculateMargin(
        SymbolInfoEx sym,
        int leverage,
        bool bInitialMargin,
        double dAllVolume,
        double dTradeVolume,
        double dAllOpenPrice,
        double dAllMarginRate,
        double dBuyVolume,
        double dBuyOpenPrice,
        double dBuyMarginRate,
        double dSellVolume,
        double dSellOpenPrice,
        double dSellMarginRate)
    {
        var num1 = 0.0;
        var num2 = dAllVolume > 0.0 ? dAllOpenPrice / dAllVolume : 0.0;
        var num3 = dAllVolume > 0.0 ? dAllMarginRate / dAllVolume : 0.0;
        var num4 = Math.Abs(dTradeVolume);
        double num5 = leverage;
        dTradeVolume = Math.Round(dTradeVolume, 8);
        switch (sym.margin_mode)
        {
            case 0:
                if (dTradeVolume != 0.0)
                    num1 = (sym.margin_initial > 0.0 ? sym.margin_initial : sym.contract_size) * (num4 / num5 / sym.margin_divider);
                if (dAllVolume > num4)
                {
                    if (Account.s34E8 == 0)
                    {
                        num1 += (dAllVolume - num4) * sym.margin_hedged / num5 / sym.margin_divider;
                        break;
                    }

                    var num6 = sym.margin_initial > 0.0 ? sym.margin_initial : sym.contract_size;
                    return Math.Max(num6 * dBuyVolume / num5 / sym.margin_divider * (dBuyMarginRate / dBuyVolume), num6 * dSellVolume / num5 / sym.margin_divider * (dSellMarginRate / dSellVolume));
                }

                break;
            case 1:
                if (dTradeVolume != 0.0)
                    num1 = (sym.margin_initial > 0.0 ? sym.margin_initial : num2 * sym.contract_size) * (num4 / sym.margin_divider);
                if (dAllVolume > num4)
                {
                    if (Account.s34E8 == 0)
                    {
                        var num7 = dAllVolume - num4;
                        if (sym.margin_initial <= 0.0)
                            num7 *= num2;
                        num1 += num7 * sym.margin_hedged / sym.margin_divider;
                        break;
                    }

                    double num8;
                    double num9;
                    if (sym.margin_initial > 0.0)
                    {
                        num8 = dBuyVolume * sym.margin_initial;
                        num9 = dSellVolume * sym.margin_initial;
                    }
                    else
                    {
                        num8 = dBuyOpenPrice * sym.contract_size;
                        num9 = dSellOpenPrice * sym.contract_size;
                    }

                    return Math.Max(num8 / sym.margin_divider * (dBuyMarginRate / dBuyVolume), num9 / sym.margin_divider * (dSellMarginRate / dSellVolume));
                }

                break;
            case 2:
                if (dTradeVolume != 0.0)
                    num1 = ((sym.margin_initial > 0.0) & bInitialMargin ? sym.margin_initial : sym.margin_maintenance) * (num4 / sym.margin_divider);
                if (dAllVolume > num4)
                {
                    if (Account.s34E8 == 0)
                    {
                        num1 += (dAllVolume - num4) * sym.margin_hedged / sym.margin_divider;
                        break;
                    }

                    var num10 = (sym.margin_initial > 0.0) & bInitialMargin ? sym.margin_initial : sym.margin_maintenance;
                    return Math.Max(num10 * dBuyVolume / sym.margin_divider * (dBuyMarginRate / dBuyVolume), num10 * dSellVolume / sym.margin_divider * (dSellMarginRate / dSellVolume));
                }

                break;
            case 3:
                if (sym.tick_size != 0.0 && sym.tick_value != 0.0)
                {
                    if (dTradeVolume != 0.0)
                        num1 = (sym.margin_initial > 0.0 ? sym.margin_initial : num2 * sym.contract_size / sym.tick_size * sym.tick_value) * (num4 / sym.margin_divider);
                    if (dAllVolume > num4)
                    {
                        if (Account.s34E8 == 0)
                        {
                            var num11 = dAllVolume - num4;
                            if (sym.margin_initial > 0.0)
                            {
                                num1 += num11 * sym.margin_hedged / sym.margin_divider;
                                break;
                            }

                            num1 += num11 * num2 * sym.margin_hedged / sym.tick_size * sym.tick_value / sym.margin_divider;
                            break;
                        }

                        double num12;
                        double num13;
                        if (sym.margin_initial > 0.0)
                        {
                            num12 = dBuyVolume * sym.margin_initial;
                            num13 = dSellVolume * sym.margin_initial;
                        }
                        else
                        {
                            num12 = dBuyOpenPrice * sym.contract_size / sym.tick_size * sym.tick_value;
                            num13 = dSellOpenPrice * sym.contract_size / sym.tick_size * sym.tick_value;
                        }

                        return Math.Max(num12 / sym.margin_divider * (dBuyMarginRate / dBuyVolume), num13 / sym.margin_divider * (dSellMarginRate / dSellVolume));
                    }
                }

                break;
            case 4:
                if (dTradeVolume != 0.0)
                    num1 = (sym.margin_initial > 0.0 ? sym.margin_initial : num2 * sym.contract_size) * (num4 / num5 / sym.margin_divider);
                if (dAllVolume > num4)
                {
                    if (Account.s34E8 == 0)
                    {
                        var num14 = dAllVolume - num4;
                        if (sym.margin_initial <= 0.0)
                            num14 *= num2;
                        num1 += num14 * sym.margin_hedged / num5 / sym.margin_divider;
                        break;
                    }

                    double num15;
                    double num16;
                    if (sym.margin_initial > 0.0)
                    {
                        num15 = dBuyVolume * sym.margin_initial;
                        num16 = dSellVolume * sym.margin_initial;
                    }
                    else
                    {
                        num15 = dBuyOpenPrice * sym.contract_size;
                        num16 = dSellOpenPrice * sym.contract_size;
                    }

                    return Math.Max(num15 / num5 / sym.margin_divider * (dBuyMarginRate / dBuyVolume), num16 / num5 / sym.margin_divider * (dSellMarginRate / dSellVolume));
                }

                break;
        }

        return num1 * num3;
    }

    internal void CalculateTradePropertiesAsync()
    {
        if (DateTime.Now.Subtract(LastCalculateTradeProperties).TotalSeconds <= 1.0)
            return;
        LastCalculateTradeProperties = DateTime.Now;
        ThreadPool.QueueUserWorkItem((WaitCallback) (obj => CalculateTradeProperties()));
    }

    internal void CalculateTradeProperties()
    {
        if (!CalculateTradeProps)
            return;
        UpdateProfit();
        var num = 0.0;
        Equity = 0.0;
        Margin = 0.0;
        var openedOrders = GetOpenedOrders();
        Array.Sort(openedOrders, new OrderComparer());
        for (var index = 0; index < openedOrders.Length; ++index)
        {
            var order1 = openedOrders[index];
            if (order1.Type is Op.Buy or Op.Sell)
            {
                var ex = GetSymbolInfo(order1.Symbol).Ex;
                var dTradeVolume = 0.0;
                var dAllVolume = 0.0;
                var dAllOpenPrice = 0.0;
                var dAllMarginRate = 0.0;
                var dBuyVolume = 0.0;
                var dBuyOpenPrice = 0.0;
                var dBuyMarginRate = 0.0;
                var dSellVolume = 0.0;
                var dSellOpenPrice = 0.0;
                var dSellMarginRate = 0.0;
                var symbol = order1.Symbol;
                for (; index < openedOrders.Length; ++index)
                {
                    var order2 = openedOrders[index];
                    if (order2.Type is Op.Buy or Op.Sell)
                    {
                        if (!(symbol != order2.Symbol))
                        {
                            double volume = order2.Ex.volume;
                            if (order2.Type == Op.Buy)
                            {
                                dBuyVolume += volume;
                                dTradeVolume += volume;
                                dBuyOpenPrice += order2.Ex.open_price * volume;
                                dBuyMarginRate += order2.Ex.margin_rate * volume;
                            }
                            else
                            {
                                dSellVolume += volume;
                                dTradeVolume -= volume;
                                dSellOpenPrice += order2.Ex.open_price * volume;
                                dSellMarginRate += order2.Ex.margin_rate * volume;
                            }

                            dAllVolume += volume;
                            dAllOpenPrice += order2.Ex.open_price * volume;
                            dAllMarginRate += order2.Ex.margin_rate * volume;
                            num += order2.Profit + order2.Ex.commission + order2.Ex.storage + order2.Ex.taxes;
                        }
                        else
                        {
                            break;
                        }
                    }
                }

                Margin += CalculateMargin(ex, Leverage, false, dAllVolume, dTradeVolume, dAllOpenPrice, dAllMarginRate, dBuyVolume, dBuyOpenPrice, dBuyMarginRate, dSellVolume, dSellOpenPrice, dSellMarginRate);
                --index;
            }
        }

        if (num != 0.0 || Margin == 0.0)
            Profit = num;
        Margin /= 100.0;
        Margin = Math.Round(Margin, 2);
        Equity = Profit + Balance + Credit;
        FreeMargin = Balance + Credit - Margin;
        switch (_Account.margin_mode)
        {
            case 1:
            case 2 when Profit > 0.0:
                FreeMargin += Profit;
                break;
            default:
            {
                if (_Account.margin_mode != 3 || Profit >= 0.0)
                    return;
                FreeMargin += Profit;
                break;
            }
        }
    }

    internal void UpdateProfit()
    {
        if (!CalculateTradeProps)
            return;
        var num = 0.0;
        foreach (var openedOrder in GetOpenedOrders())
            if (openedOrder.Type is Op.Buy or Op.Sell)
                num += openedOrder.Profit + openedOrder.Ex.commission + openedOrder.Ex.storage + openedOrder.Ex.taxes;
        if (num != 0.0)
            Profit = num;
        Equity = Profit + Balance + Credit;
        FreeMargin = Balance + Credit - Margin;
        switch (_Account.margin_mode)
        {
            case 1:
            case 2 when Profit > 0.0:
                FreeMargin += Profit;
                break;
            default:
            {
                if (_Account.margin_mode != 3 || Profit >= 0.0)
                    return;
                FreeMargin += Profit;
                break;
            }
        }
    }

    internal class OrderComparer : IComparer
    {
        public int Compare(object x, object y) => ((Order) x).Symbol.CompareTo(((Order) y).Symbol);
    }
}
