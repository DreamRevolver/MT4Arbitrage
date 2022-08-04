namespace TradingAPI.MT4Server;

internal class IAsyncProxyResult : IAsyncResult
{
    private ManualResetEvent m_WaitHandle;

    internal IAsyncProxyResult(object stateObject = null)
    {
        AsyncState = stateObject;
        IsCompleted = false;
        if (m_WaitHandle == null)
            return;
        m_WaitHandle.Reset();
    }

    public bool IsCompleted { get; private set; }

    public bool CompletedSynchronously => false;

    public object AsyncState { get; private set; }

    public WaitHandle AsyncWaitHandle
    {
        get
        {
            if (m_WaitHandle == null)
                m_WaitHandle = new ManualResetEvent(false);
            return m_WaitHandle;
        }
    }

    internal void Reset()
    {
        AsyncState = null;
        IsCompleted = true;
        if (m_WaitHandle == null)
            return;
        m_WaitHandle.Set();
    }
}
