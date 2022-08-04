namespace TradingAPI.MT4Server;

internal class OrderTransaction
{
    private readonly Logger Log;
    private readonly OrderClient OrderClient;
    internal ManualResetEvent GotResult = new(false);
    public Exception OrderException;
    public byte[] OrderRequest;
    public byte[] OrderResult;
    public int RequestID;
    public MT4Status Status = MT4Status.OK_REQUEST;

    public OrderTransaction(OrderClient orderClient, byte[] req)
    {
        OrderClient = orderClient;
        OrderRequest = req;
        Log = orderClient.Log;
    }

    public byte[] CreateTransactionPacket(Connection con)
    {
        var num1 = 0;
        while (num1 < 12 && OrderRequest[num1 + 13] != 0)
            ++num1;
        var now = DateTime.Now;
        var num2 = (uint) now.Ticks;
        for (var index = num1 + 1; index < 12; ++index)
        {
            num2 = (uint) ((int) num2 * 214013 + 2531011);
            OrderRequest[index + 13] = (byte) ((num2 >> 16) & byte.MaxValue);
        }

        var num3 = 0;
        while (num3 < 32 && OrderRequest[num3 + 57] != 0)
            ++num3;
        now = DateTime.Now;
        var num4 = (uint) now.Ticks;
        for (var index = num3 + 1; index < 32; ++index)
        {
            num4 = (uint) ((int) num4 * 214013 + 2531011);
            OrderRequest[index + 57] = (byte) ((num4 >> 16) & byte.MaxValue);
        }

        var crc = (uint) ((ulong) con.Account + ((con.Session >> 8) & byte.MaxValue));
        var numArray1 = new byte[92];
        Array.Copy(OrderRequest, 1, numArray1, 0, 92);
        BitConverter.GetBytes(vCRC32.Calculate(numArray1, crc)).CopyTo(OrderRequest, 93);
        var transactionPacket = new byte[OrderRequest.Length + 4];
        OrderRequest.CopyTo(transactionPacket, 0);
        if (con.ServerBuild > 1101)
            transactionPacket[0] = 209;
        BitConverter.GetBytes(RequestID).CopyTo(transactionPacket, 97);
        if (con.Session == 0U)
            return transactionPacket;
        var num5 = (uint) ((int) (uint) ((ulong) con.Account + ((con.Session >> 8) & byte.MaxValue) + con.CurrentBuild + con.ServerBuild) * 214013 + 2531011);
        var num6 = ((int) (num5 >> 16) & 3) + 16;
        var num7 = (uint) ((int) num5 * 214013 + 2531011);
        var num8 = ((int) (num7 >> 16) & 7) + 8;
        var sourceArray = new byte[transactionPacket.Length + num6 + 20 + 4 + num8];
        transactionPacket.CopyTo(sourceArray, 0);
        for (var index = 0; index < num6; ++index)
        {
            num7 = (uint) ((int) num7 * 214013 + 2531011);
            sourceArray[index + transactionPacket.Length] = (byte) ((num7 >> 16) & byte.MaxValue);
        }

        var index1 = transactionPacket.Length + num6;
        var numArray2 = new byte[index1 - 1];
        Array.Copy(sourceArray, 1, numArray2, 0, index1 - 1);
        var vShA1 = new vSHA1();
        vShA1.HashData(numArray2);
        vShA1.HashData(BitConverter.GetBytes(con.Account));
        vShA1.HashData(MT4Crypt.Decode(con._TransactionKey1, MT4Crypt.GetHardId()));
        vShA1.HashData(MT4Crypt.Decode(con._TransactionKey2, MT4Crypt.GetHardId()));
        vShA1.HashData(MT4Crypt.GetHardId());
        vShA1.FinalizeHash().CopyTo(sourceArray, index1);
        var index2 = index1 + 20;
        BitConverter.GetBytes(con.Session ^ con._ClientExeSize).CopyTo(sourceArray, index2);
        var num9 = index2 + 4;
        for (var index3 = 0; index3 < num8; ++index3)
        {
            num7 = (uint) ((int) num7 * 214013 + 2531011);
            sourceArray[index3 + num9] = (byte) ((num7 >> 16) & byte.MaxValue);
        }

        BitConverter.GetBytes(0U).CopyTo(OrderRequest, 93);
        return sourceArray;
    }

    public void OrderNotify(Connection con, byte[] buf)
    {
        var code = buf[5];
        var tradeType = buf[4];
        switch (code)
        {
            case 0:
                processTrade(tradeType, con);
                break;
            case 1:
                code = 0;
                break;
            case 138:
                Log.trace("Notification requote");
                buf = con.ReceiveDecode(16);
                var bid = BitConverter.ToDouble(buf, 0);
                var ask = BitConverter.ToDouble(buf, 8);
                OrderException = new RequoteException(code, bid, ask);
                GotResult.Set();
                OrderClient.onProgress(new byte[0], ProgressType.Rejected, RequestID, OrderException);
                break;
            case 142:
                Log.trace("Notification request was accepted");
                OrderClient.onProgress(new byte[0], ProgressType.Accepted, RequestID, null);
                break;
            case 143:
                Log.trace("Notification request in process");
                OrderClient.onProgress(new byte[0], ProgressType.InProcess, RequestID, null);
                break;
            default:
                try
                {
                    OrderException = new ServerException(code);
                    GotResult.Set();
                    OrderClient.onProgress(new byte[0], ProgressType.Rejected, RequestID, OrderException);
                    break;
                }
                catch (Exception)
                {
                    OrderException = new Exception($"Unknown notification {code:X}.");
                    GotResult.Set();
                    OrderClient.onProgress(new byte[0], ProgressType.Rejected, RequestID, OrderException);
                    throw OrderException;
                }
        }

        Status = (MT4Status) code;
    }

    private void processTrade(byte tradeType, Connection con)
    {
        switch (tradeType)
        {
            case 0:
                Log.trace("Trade price reading");
                OrderClient.onProgress(con.ReceiveDecode(16), ProgressType.Price, RequestID, null);
                break;
            case 64:
            case 65:
            case 66:
            case 67:
                Log.trace($"Trade opened {tradeType:X}");
                var copmressed1 = con.ReceiveCopmressed();
                OrderResult = copmressed1;
                GotResult.Set();
                OrderClient.onProgress(copmressed1, ProgressType.Opened, RequestID, null);
                break;
            case 68:
            case 69:
            case 70:
                Log.trace("Trade close");
                con.ReceiveDecode(8);
                con.ReceiveDecode(8);
                var copmressed2 = con.ReceiveCopmressed();
                OrderResult = copmressed2;
                GotResult.Set();
                OrderClient.onProgress(copmressed2, ProgressType.Closed, RequestID, null);
                break;
            case 71:
                Log.trace("Trade modify");
                var copmressed3 = con.ReceiveCopmressed();
                OrderResult = copmressed3;
                GotResult.Set();
                OrderClient.onProgress(copmressed3, ProgressType.Modified, RequestID, null);
                break;
            case 72:
                Log.trace("Trade pending delete");
                OrderResult = new byte[0];
                GotResult.Set();
                OrderClient.onProgress(new byte[0], ProgressType.PendingDeleted, RequestID, null);
                break;
            case 73:
                Log.trace("Trade close by");
                con.ReceiveDecode(8);
                con.ReceiveDecode(8);
                con.ReceiveCopmressed();
                OrderResult = new byte[0];
                GotResult.Set();
                OrderClient.onProgress(new byte[0], ProgressType.ClosedBy, RequestID, null);
                break;
            case 74:
                Log.trace("Trade multiple close by");
                OrderResult = new byte[0];
                GotResult.Set();
                OrderClient.onProgress(new byte[0], ProgressType.MultipleClosedBy, RequestID, null);
                break;
            default:
                OrderException = new Exception($"Unknown trade type {tradeType:X}.");
                GotResult.Set();
                OrderClient.onProgress(new byte[0], ProgressType.Rejected, RequestID, OrderException);
                throw OrderException;
        }
    }
}
