using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;

namespace TradingAPI.MT4Server;

public class Broker
{
    public static IList<Company> Search(string company, bool mt5 = false)
    {
        var str1 = "mt4";
        if (mt5)
            str1 = nameof(mt5);
        var str2 = $"company={company}&code={str1}";
        var signature = GetSignature(Encoding.Default.GetBytes(str2));
        var str3 = $"{str2}&signature=";
        foreach (var num in signature)
        {
            var str4 = num.ToString("X").ToLower();
            if (str4.Length == 1)
                str4 = $"0{str4}";
            str3 += str4;
        }

        var str5 = $"{str3}&ver=2";
        ServicePointManager.Expect100Continue = true;
        ServicePointManager.SecurityProtocol = (SecurityProtocolType) 3072;
        var httpWebRequest = (HttpWebRequest) WebRequest.Create($"https://updates.metaquotes.net/public/{str1}/network");
        var bytes = Encoding.ASCII.GetBytes(str5);
        httpWebRequest.Method = "POST";
        httpWebRequest.Accept = "*/*";
        httpWebRequest.Headers.Add("Accept-Encoding", "gzip, deflate");
        httpWebRequest.Headers.Add("Accept-Language", "en");
        httpWebRequest.UserAgent = "MetaTrader 4 Terminal/4.1340 (Windows NT 6.2.9200; x64)";
        httpWebRequest.Headers.Add("Cookie", "_fz_uniq=4911006713307073302;uniq=4911006713307073302;age=6341585;tid=1F6378CF05FAA259E;sid=l4chrpji4d5ewu2mjifa1ob4;");
        httpWebRequest.ContentType = "application/x-www-form-urlencoded";
        httpWebRequest.ContentLength = bytes.Length;
        using (var requestStream = httpWebRequest.GetRequestStream())
        {
            requestStream.Write(bytes, 0, bytes.Length);
        }

        var end = new StreamReader(httpWebRequest.GetResponse().GetResponseStream()).ReadToEnd();
        return ReadToObject(end.Substring(end.IndexOf("{"))).result;
    }

    public static Companies ReadToObject(string json)
    {
        var companies1 = new Companies();
        var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        var companies2 = new DataContractJsonSerializer(companies1.GetType()).ReadObject(memoryStream) as Companies;
        memoryStream.Close();
        return companies2;
    }

    private static byte[] GetSignature(byte[] data)
    {
        var md5Managed = new MD5Managed();
        md5Managed.HashCore(data, 0, data.Length);
        var array1 = md5Managed.HashFinal();
        md5Managed.Initialize(true);
        md5Managed.HashCore(array1, 0, 16);
        var array2 = new byte[32]
        {
            61,
            123,
            21,
            22,
            214,
            234,
            187,
            52,
            217,
            214,
            99,
            227,
            98,
            62,
            27,
            215,
            251,
            220,
            174,
            244,
            87,
            59,
            223,
            53,
            127,
            168,
            207,
            11,
            190,
            173,
            146,
            127
        };
        md5Managed.HashCore(array2, 0, 32);
        return md5Managed.HashFinal();
    }

    public class Result
    {
        public string name { get; set; }

        public IList<string> access { get; set; }
    }

    public class Company
    {
        public string company { get; set; }

        public IList<Result> results { get; set; }
    }

    public class Companies
    {
        public IList<Company> result { get; set; }
    }
}
