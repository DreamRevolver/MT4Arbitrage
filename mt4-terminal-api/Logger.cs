namespace TradingAPI.MT4Server;

public class Logger
{
    public delegate void OnMsgHandler(object sender, string msg, MsgType type);

    public enum MsgType
    {
        Trace,
        Debug,
        Info,
        Warn,
        Error,
        Exception
    }

    public static bool DebugInfo;
    private readonly object Parent;

    public Logger(object parent) => Parent = parent;

    public static event OnMsgHandler OnMsg;

    public void trace(string msg)
    {
        if (OnMsg == null)
            return;
        onMsg(new object[2]
        {
            msg,
            MsgType.Trace
        });
    }

    private void onMsg(object obj)
    {
        try
        {
            var objArray = (object[]) obj;
            OnMsg(Parent, (string) objArray[0], (MsgType) objArray[1]);
        }
        catch (Exception)
        {
        }
    }

    public void debug(string msg)
    {
        if (OnMsg == null)
            return;
        onMsg(new object[2]
        {
            msg,
            MsgType.Debug
        });
    }

    public void info(string msg)
    {
        if (OnMsg == null)
            return;
        onMsg(new object[2]
        {
            msg,
            MsgType.Info
        });
    }

    public void warn(string msg)
    {
        if (OnMsg == null)
            return;
        onMsg(new object[2]
        {
            msg,
            MsgType.Warn
        });
    }

    public void error(string msg)
    {
        if (OnMsg == null)
            return;
        onMsg(new object[2]
        {
            msg,
            MsgType.Error
        });
    }

    public void exception(Exception ex)
    {
        if (OnMsg == null)
            return;
        var str = ex.Message;
        if (DebugInfo)
        {
            if (ex.StackTrace != null)
                str = $"{str}\n{ex.StackTrace}";
            while ((ex = ex.InnerException) != null)
            {
                str = $"{str}\nInner Exc: {ex.Message}";
                if (ex.StackTrace != null)
                    str = $"{str}\n{ex.StackTrace}";
            }
        }

        onMsg(new object[2]
        {
            str,
            MsgType.Exception
        });
    }
}
