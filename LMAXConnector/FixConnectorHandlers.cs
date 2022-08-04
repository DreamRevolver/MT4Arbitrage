using System.Globalization;
using QuickFix;
using QuickFix.Fields;
using QuickFix.FIX44;
using QuickFix.Util;
using Shared.Extensions;
using Shared.Models;

namespace LMAXConnector;

public partial class FixLMAXBrokerConnector
{
    public void OnMessage(Heartbeat heartBeat, SessionID id)
    {
        _logger.Debug($"Heartbeat {heartBeat}", "FixConnectorHandlers.Heartbeat");
    }

    public void OnMessage(TradingSessionStatus report, SessionID id)
    {
        _logger.Debug($"{report.Text}", "FixConnectorHandlers.TradingSessionStatus");
    }

    public void OnMessage(Logon msg, SessionID id)
    {
        if (_settings.Get(id).Has("Username"))
        {
            var User = _settings.Get(id).GetString("Username");
            msg.SetField(new Username(User));
        }

        if (_settings.Get(id).Has("Password"))
        {
            var Pwd = _settings.Get(id).GetString("Password");
            msg.SetField(new Password(Pwd));
        }
        _logger.Debug($"Logon msg: {msg}", "FixConnectorHandlers.Logon");
    }

    public void OnMessage(Logout msg, SessionID id)
    {
        _logger.Debug($"Logout msg: {msg}", "FixConnectorHandlers.Logout");
    }

    public void OnMessage(MarketDataRequest report, SessionID id)
    {
        _logger.Debug($"MarketDataRequest {report}", "FixConnectorHandlers.MarketDataRequest");
    }

    public void OnMessage(TestRequest report, SessionID id)
    {
        _logger.Debug($"TestRequest {report}", "FixConnectorHandlers.TestRequest");
    }

    public void OnMessage(MarketDataSnapshotFullRefresh msg, SessionID sess)
    {
        _logger.Debug($"MarketDataSnapshotFullRefresh {msg}", "FixConnectorHandlers.MarketDataSnapshotFullRefresh");
        var entries = msg.GetString(NoMDEntries.TAG); //symbol
        if (entries is "0")
        {
            return;
        }
        var count = int.Parse(entries);
        var topOfmarket = new MarketBook() {Ask = 0.0, Bid = 0.0};
        topOfmarket.Time=DateTime.Now;
        for (var i = 0; i < count; i++)
        {
            var group = msg.GetGroup(i + 1, NoMDEntries.TAG);
            var type = (EntryType) char.Parse(group.GetString(MDEntryType.TAG));
            var price = double.Parse(group.GetString(MDEntryPx.TAG));
            var volume = uint.TryParse(group.GetString(MDEntrySize.TAG), out var resultVolume);
            
            //Parse(group.GetString(271)); //MDEntrySize
           
            topOfmarket.Pair = Instruments.FirstOrDefault(i => i.ConId == int.Parse(msg.GetString(SecurityID.TAG))).Symbol;
            if (type == EntryType.Bid)
            {
                topOfmarket.Bid = price;
                topOfmarket.BidVolume = volume ? resultVolume : default;
            }
            else if (type == EntryType.Offer)
            {
                topOfmarket.Ask = price;
                topOfmarket.AskVolume = volume ? resultVolume : default;
            }
        }
        OnTick(this,topOfmarket);
    }

    public void OnMessage(MarketDataIncrementalRefresh report, SessionID sess)
    {
        _logger.Debug($"MarketDataIncrementalRefresh {report.MDReqID}", "FixConnectorHandlers.MarketDataIncrementalRefresh");
    }

    public void OnMessage(MarketDataRequestReject report, SessionID sess)
    {
        _logger.Debug($"MarketDataRequestReject {report.Text}", "FixConnectorHandlers.MarketDataRequestReject");
    }

    public void OnMessage(OnConnectionMsg msg, SessionID sess)
    {
        if (msg == BrokerEvent.SessionLogon)
        {
            _logger.Info($"Logon", "FixLMAXBrokerConnector");
        }
        else if (msg == BrokerEvent.SessionLogout)
        {
            _logger.Info($"Logout", "FixLMAXBrokerConnector");
        }
        OnChannelEvent(msg);
    }

    public void OnMessage(QuickFix.FIX44.ExecutionReport report, SessionID sess)
    {
        _logger.Debug($"ExecutionReport {report}", "FixConnectorHandlers.ExecutionReport");
    }

    public void OnMessage(Message msg, SessionID sess)
    {
        _logger.Debug($"Messege {msg} | {sess}", "FixConnectorHandlers.Message");
    }
}
