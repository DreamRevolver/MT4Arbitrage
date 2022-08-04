using log4net.Core;
using QuickFix;
using Shared.Extensions;
using Shared.Models;

namespace LMAXConnector;

public class FixApplication : IApplication
{

    private readonly ILogger _logger;
    protected SessionSettings _settings;
    private readonly Lazy<MessageCracker> _cracker;
    public FixApplication(SessionSettings settings, ILogger logger, Lazy<MessageCracker> cracker)
    {
        _settings = settings;
        _logger = logger;
        _cracker = cracker;
    }
    private void Crack(Message message, SessionID sessionId) => _cracker.Value.Crack(message, sessionId);

    public void OnCreate(SessionID id)
    {
        try
        {
            Session.LookupSession(id);
            _logger.Debug($"OnCreate {id.BeginString}","FixApplication");
        }
        catch (Exception ex)
        {
            _logger.Error($"OnCreate {ex.Message}","FixApplication");
        }
        
    }

    public void OnLogon(SessionID id)
    {
        _logger.Debug($"OnLogon {id.BeginString}","FixApplication");
        try
        {
            Crack(new OnConnectionMsg()
            {
                Event = BrokerEvent.SessionLogon
            }, id);
        }
        catch (Exception ex)
        {
            _logger.Error($"OnLogon {ex.Message}","FixApplication");
        }
       
    }

    public void OnLogout(SessionID id)
    {
        _logger.Debug($"OnLogout {id.BeginString}","FixApplication");
        try
        {
            Crack(new OnConnectionMsg()
            {
                Event = BrokerEvent.SessionLogout
            }, id);
        }
        catch (Exception ex)
        {
            _logger.Error($"OnLogout {ex.Message}","FixApplication");
        }
       
    }

    public void ToAdmin(Message msg, SessionID id)
    {
        _logger.Debug($"ToAdmin {msg}","FixApplication");
        try
        {
            Crack(msg, id);
        }
        catch (Exception ex)
        {
            _logger.Error($"ToAdmin {ex.Message}","FixApplication");
        }
    }

    public void ToApp(Message msg, SessionID id)
    {
        _logger.Debug($"ToApp {msg}","FixApplication");
        try
        {
            Crack(msg, id);
        }
        catch (Exception ex)
        {
            _logger.Error($"ToApp {ex.Message}","FixApplication");
        }
    }

    public void FromAdmin(Message msg, SessionID id)
    {
        _logger.Debug($"FromAdmin {msg}", "FixApplication");
        try
        {
            Crack(msg, id);
        }
        catch (Exception ex)
        {
            _logger.Error($"FromAdmin {ex.Message}","FixApplication");
        }
        
    }

    public void FromApp(Message msg, SessionID id)
    {
        _logger.Debug($"FromApp {msg}", "FixApplication");
        try
        {
            Crack(msg, id);
        }
        catch (Exception ex)
        {
            _logger.Error($"FromApp {ex.Message}","FixApplication");
        }
    }
}
