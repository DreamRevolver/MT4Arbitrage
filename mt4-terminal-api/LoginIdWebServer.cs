using System.Net;
using System.Text;

namespace TradingAPI.MT4Server;

internal class LoginIdWebServer
{
    private byte[] Bytes;
    private bool Data;
    private string Url;

    public ulong Decode(string url, byte[] bytes, int timeout, bool data)
    {
        Url = url;
        Bytes = bytes;
        Data = data;
        var thread = new Thread(ThreadStart);
        var parameter = new Result();
        thread.Start(parameter);
        if (!thread.Join(timeout))
            throw new ConnectException($"No reply from login id web server({url}) in {timeout}ms");
        return parameter.Ex == null ? parameter.Id : throw parameter.Ex;
    }

    private void ThreadStart(object param)
    {
        var result1 = (Result) param;
        string end;
        try
        {
            var httpWebRequest = (HttpWebRequest) WebRequest.Create(Url);
            var str = Convert.ToBase64String(Bytes);
            if (Data)
                str = $"loginiddata{str}";
            var bytes = Encoding.ASCII.GetBytes(str);
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentType = "application/text";
            httpWebRequest.ContentLength = bytes.Length;
            using (var requestStream = httpWebRequest.GetRequestStream())
            {
                requestStream.Write(bytes, 0, bytes.Length);
            }

            end = new StreamReader(httpWebRequest.GetResponse().GetResponseStream()).ReadToEnd();
        }
        catch (Exception ex)
        {
            result1.Ex = new ConnectException($"LoginIdWebServer({Url}): {ex.Message}");
            return;
        }

        ulong result2;
        if (ulong.TryParse(end, out result2))
            result1.Id = result2;
        else
            result1.Ex = new ConnectException($"LoginIdWebServer response({Url}): {end}");
    }

    private class Result
    {
        public Exception Ex;
        public ulong Id;
    }
}
