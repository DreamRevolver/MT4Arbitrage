using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Autofac;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository.Hierarchy;
using QuoteObserver;
using Shared.Extensions;
using Shared.Interfaces;
using Shared.Models;
using TradingAPI.MT4Server;
using WeifenLuo.WinFormsUI.Docking;
using WinForm.Config;
using ILogger = log4net.Core.ILogger;
using static System.Drawing.ColorTranslator;

namespace WinForm;

public partial class SpreadWindow : DockContent, IAppender
{
    private enum MsgColumnType
    {
        Time,
        Pair,
        Spread,
        FastBroker,
        FastBid,
        FastAsk,
        SlowBroker,
        SlowBid,
        SlowAsk,
        Delta
    }
    private ImmutableDictionary<string, Arbitration> _arbitrations = ImmutableDictionary<string, Arbitration>.Empty;
    private readonly List<LoggingEvent> _messages = new();
    private readonly int _maxMsgCount = 500;
    private bool _update;
    private ILogger _logger;
    private IArbitrationObserver _observer;
    private ConfigLoader _loader;
    private IContainer container;
    private IBrokerConnector _connector;
    private static readonly string[] ACTIVE_COLORS = { "#006400", "#007800", "#008C00", "#00A600", "#00B500", "#00C800", "#00D700", "#00E600", "#00F500", "#00FF00"};
    private static readonly string[] UNACTIVE_COLORS = { "#FF0000", "#E90000", "#DE0000", "#C80000", "#B40000", "#A10000", "#8C0000", "#780000", "#640000", "#500000"};
    private readonly Color[] activeColors = ACTIVE_COLORS.Select(FromHtml).ToArray();
    private readonly Color[]  unactiveColors = UNACTIVE_COLORS.Select(FromHtml).ToArray();
    public SpreadWindow(ILogger logger, IBrokerConnector connector, IArbitrationObserver observer)
    {
        _logger = logger;
        _loader = new ConfigLoader(_logger);
        _connector = connector;
        _observer = observer;
        _observer.OnArbitrationUpdate += LogHandler;
        InitializeComponent();
    }

    private void LogHandler(object? _,  Arbitration update)
    {
        ImmutableInterlocked.AddOrUpdate(ref _arbitrations, $"{update.Pair.Symbol}|{update.FastName}|{update.SlowName}", update, (_, arbitration) => arbitration);
        _logger.Observer(new ObserverUpdate()
        {
            pair = update.Pair.Symbol,
            spread = update.Pair.Spread,
            fastBroker = update.FastName,
            slowBroker = update.SlowName,
            fastMarketBook = update.FastLast.Value,
            slowMarketBook = update.SlowLast.Value
        });
    }

    public void DoAppend(LoggingEvent loggingEvent)
    {
        if (loggingEvent.MessageObject is not ObserverUpdate log) return;
        if (InvokeRequired)
        {
            BeginInvoke(new Action<LoggingEvent>(DoAppend), loggingEvent);
        }
        else
        {
            _messages.Insert(0, loggingEvent);
            if (_messages.Count > _maxMsgCount) _messages.RemoveAt(_maxMsgCount);
            _update = true;
        }
    }

    private void InfoDataGridView_CellValueNeeded(object _, DataGridViewCellValueEventArgs e)
    {
        if (e.RowIndex < 0 || e.RowIndex >= _arbitrations.Count) return;

        var item = _arbitrations.ElementAt(e.RowIndex).Value;
        switch (e.ColumnIndex)
        {
            case (int) MsgColumnType.Time:
                e.Value = item.FastLast.Value.Time;
                break;
            case (int) MsgColumnType.Pair:
                e.Value = item.Pair.Symbol;
                break;
            case (int) MsgColumnType.Spread:
                e.Value = item.Pair.Spread.ToString();
                break;
            case (int) MsgColumnType.FastBroker:
                e.Value = item.FastName;
                break;
            case (int) MsgColumnType.FastBid:
                e.Value = item.FastLast.Value.Bid.ToString();
                break;
            case (int) MsgColumnType.FastAsk:
                e.Value = item.FastLast.Value.Ask.ToString();
                break;
            case (int) MsgColumnType.SlowBroker:
                e.Value = item.SlowName;
                break;
            case (int) MsgColumnType.SlowBid:
                e.Value = item.SlowLast.Value.Bid.ToString();
                break;
            case (int) MsgColumnType.SlowAsk:
                e.Value = item.SlowLast.Value.Ask.ToString();
                break;
            case (int) MsgColumnType.Delta:
                e.Value = item.Delta.ToString("F7");
                break;
        }

        SetRowColor(item, e.RowIndex);
    }
    private void SetRowColor(Arbitration item, int rowIndex)
    {
        int value = (int)(item.Delta / item.Pair.Spread);
        switch (value) 
        {
            case > 0:
                Color activeColor = value >= activeColors.Length ? activeColors.Last() : activeColors[value];
                InfoDataGridView.Rows[rowIndex].DefaultCellStyle.BackColor = activeColor;
                break;
            case 0:
                InfoDataGridView.Rows[rowIndex].DefaultCellStyle.BackColor = Color.Yellow;
                break;
            case < 0:
                Color unactiveColor = -value >= unactiveColors.Length ? unactiveColors.Last() : unactiveColors[-value];
                InfoDataGridView.Rows[rowIndex].DefaultCellStyle.BackColor = unactiveColor;
                break;
        }
        if (item.IsActive)
        {
            if (item.Flag)
            {
                InfoDataGridView.Rows[rowIndex].Cells[(int) MsgColumnType.FastBid].Style.BackColor = Color.Aqua;
                InfoDataGridView.Rows[rowIndex].Cells[(int) MsgColumnType.SlowAsk].Style.BackColor = Color.Aqua;
            }
            else
            {
                InfoDataGridView.Rows[rowIndex].Cells[(int) MsgColumnType.SlowBid].Style.BackColor = Color.Aquamarine;
                InfoDataGridView.Rows[rowIndex].Cells[(int) MsgColumnType.FastAsk].Style.BackColor = Color.Aquamarine;
            }
        }
        else
        {
            if (item.Flag)
            {
                InfoDataGridView.Rows[rowIndex].Cells[(int) MsgColumnType.FastBid].Style.BackColor = InfoDataGridView.Rows[rowIndex].DefaultCellStyle.BackColor;
                InfoDataGridView.Rows[rowIndex].Cells[(int) MsgColumnType.SlowAsk].Style.BackColor = InfoDataGridView.Rows[rowIndex].DefaultCellStyle.BackColor;
            }
            else
            {
                InfoDataGridView.Rows[rowIndex].Cells[(int) MsgColumnType.SlowBid].Style.BackColor = InfoDataGridView.Rows[rowIndex].DefaultCellStyle.BackColor;
                InfoDataGridView.Rows[rowIndex].Cells[(int) MsgColumnType.FastAsk].Style.BackColor = InfoDataGridView.Rows[rowIndex].DefaultCellStyle.BackColor;
            }
        }
    }
    private void timer1_Tick(object sender, EventArgs e)
    {
        //if (_update == false) return;
        InfoDataGridView.RowCount = _arbitrations.Count;
        InfoDataGridView.Refresh();
        //_update = false;
    }

    private void SpreadWindow_Load(object sender, EventArgs e)
    {
        ((Hierarchy) LogManager.GetRepository()).Root.AddAppender(this);
        timer1.Enabled = true;
        timer1.Start();
        _loader.LoadFromFile();
        _observer.AddPairs(_loader.Pairs);
        foreach (var broker in _loader.FastBrokers)
        {
            QuoteClient quoteClient = new QuoteClient();
            quoteClient.Init(broker.user, broker.password, broker.srvFilePath);
            var QCconnector = new QuoteClientConnector(quoteClient, _logger);
            QCconnector.UploadPairs(_loader.Pairs);
            QCconnector.Start();
            _observer.AddConnector(QCconnector, BrokerType.Fast);
        }
        foreach (var broker in _loader.SlowBrokers)
        {
            QuoteClient quoteClient = new QuoteClient();
            quoteClient.Init(broker.user, broker.password, broker.srvFilePath);
            var QCconnector = new QuoteClientConnector(quoteClient, _logger);
            QCconnector.UploadPairs(_loader.Pairs);
            QCconnector.Start();
            _observer.AddConnector(QCconnector, BrokerType.Slow);
        }
        _connector.UploadPairs(_loader.Pairs);
        _connector.Start();
        _observer.AddConnector(_connector, BrokerType.Fast);
        // _connector.GetPairsFromConfig(_loader.Pairs);
        // _connector.Start();
    }

    private void SpreadWindow_FormClosing(object sender, FormClosingEventArgs e)
    {
        _connector.Stop();
        timer1.Stop();
        timer1.Dispose();
    }
    private void InfoDataGridView_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
    {
        if (InfoDataGridView.Rows[e.RowIndex].Selected)
        {
            InfoDataGridView.ClearSelection();
        }
    }
}
