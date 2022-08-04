namespace TradingAPI.MT4Server;

internal class AutoReconnect
{
    private readonly Logger Log;
    private readonly QuoteClient QC;
    internal bool Run;
    private Thread Thread;

    public AutoReconnect(QuoteClient qc, bool run)
    {
        QC = qc;
        Log = qc.Log;
        Run = run;
    }

    public void Go()
    {
        lock (this)
        {
            if (!Run || Thread is {IsAlive: true})
                return;
            Thread = new Thread(Start);
            Thread.Start();
        }
    }

    private void Start()
    {
        while (Run)
            try
            {
                if (QC.Connected)
                    break;
                Connect();
            }
            catch (Exception ex)
            {
                Log.info($"AutoReconnect: {ex.Message}");
            }
    }

    private void Connect()
    {
        var index1 = -1;
        var num = int.MaxValue;
        Server[] srvList;
        QuoteClient.LoadSrv(QC.LatestSrv, out srvList);
        for (var index2 = 0; index2 < srvList.Length; ++index2)
            srvList[index2].ping = QuoteClient.PingHost(srvList[index2].Host);
        for (var index3 = 0; index3 < srvList.Length; ++index3)
        {
            srvList[index3].ping = QuoteClient.PingHost(srvList[index3].Host);
            if (srvList[index3].ping != -1 && srvList[index3].ping < num && (!(srvList[index3].Host == QC.Host) || DateTime.Now.Subtract(QC.ConnectTime).TotalMinutes >= 5.0))
            {
                num = srvList[index3].ping;
                index1 = index3;
            }
        }

        if (index1 == -1)
            for (var index4 = 0; index4 < srvList.Length; ++index4)
                if (!(srvList[index4].Host == QC.Host) || DateTime.Now.Subtract(QC.ConnectTime).TotalMinutes >= 5.0)
                    index1 = index4;
        if (index1 == -1 && srvList.Length != 0)
            index1 = 0;
        try
        {
            if (index1 == -1)
            {
                QC.ConnectInternal();
                Log.info($"AutoReconnect success {QC.Host}:{QC.Port}");
            }
            else
            {
                QC.Host = srvList[index1].Host;
                QC.Port = srvList[index1].Port;
                QC.ConnectInternal();
                Log.info($"AutoReconnect success {QC.Host}:{QC.Port}");
            }
        }
        catch (Exception ex1)
        {
            Log.info($"AutoReconnect 1st step: {ex1.Message}");
            foreach (var server in srvList)
                try
                {
                    QC.Host = server.Host;
                    QC.Port = server.Port;
                    QC.ConnectInternal();
                    Log.info($"AutoReconnect success {QC.Host}:{QC.Port}");
                    return;
                }
                catch (Exception ex2)
                {
                    Log.info($"AutoReconnect 2nd step: {ex2.Message}");
                }

            throw new Exception("Cannot connect to any server yet");
        }
    }

    internal void Stop()
    {
        lock (this)
        {
            if (Thread is not {IsAlive: true})
                return;
            try
            {
                Thread.Abort();
            }
            catch
            {
            }
        }
    }
}
