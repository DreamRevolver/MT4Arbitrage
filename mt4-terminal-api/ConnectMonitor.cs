namespace TradingAPI.MT4Server;

public class ConnectMonitor
{
    private static ConnectMonitor Monitor;
    private static Thread Monitoring;
    private static readonly HashSet<QuoteClient> Clients = new();
    private static bool Run;

    public static void Add(QuoteClient qc)
    {
    }

    public static void Delete(QuoteClient qc)
    {
        lock (Clients)
        {
            if (Clients.Contains(qc))
                Clients.Remove(qc);
            if (Clients.Count != 0)
                return;
            Run = false;
            Monitoring = null;
        }
    }

    private void run()
    {
        while (Run)
        {
            lock (Clients)
            {
                foreach (var client in Clients)
                {
                    if (client.Connected && DateTime.Now.Subtract(client.LastServerMessageTime).TotalMilliseconds > client.NoServerMessagesTimeout)
                    {
                        client.CmdHandler.stop();
                        client.onDisconnect(new Exception($"No messages from server for {client.NoServerMessagesTimeout} ms"));
                    }

                    if (client.AutoReconnecter.Run && !client.Connected)
                        client.ReconnectAsync();
                }
            }

            Thread.Sleep(1000);
        }
    }
}
