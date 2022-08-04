using System.Runtime.InteropServices;

namespace TradingAPI.MT4Server;

internal class ServerList
{
    private static readonly Dictionary<string, object> Locks = new();

    public static MainServer ReadServers(byte[] buf, out Server[] dataSrv)
    {
        try
        {
            using Stream stream = new MemoryStream(buf);
            var length1 = 352;
            var buf1 = new byte[length1];
            stream.Read(buf1, 0, length1);
            var mainServer = UDT.ReadStruct<MainServer>(buf1, 0);
            if (mainServer.count != 0)
            {
                var num = Marshal.SizeOf(typeof(Server));
                var length2 = mainServer.count * num;
                var buf2 = new byte[length2];
                stream.Read(buf2, 0, length2);
                var buf3 = MT4Crypt.Decrypt(buf2);
                dataSrv = new Server[mainServer.count];
                for (var index = 0; index < mainServer.count; ++index)
                    dataSrv[index] = UDT.ReadStruct<Server>(buf3, index * num);
            }
            else
            {
                dataSrv = new Server[0];
            }

            return mainServer;
        }
        catch (Exception ex)
        {
            throw new Exception($"Unable to read server list: {ex.Message}");
        }
    }

    public static MainServer ReadServers(string serverFilePath, out Server[] dataSrv)
    {
        if (!File.Exists(serverFilePath))
            throw new Exception($"{serverFilePath} not found");
        try
        {
            serverFilePath = serverFilePath.Trim();
            lock (Locks)
            {
                if (!Locks.ContainsKey(serverFilePath))
                    Locks.Add(serverFilePath, new object());
            }

            lock (Locks[serverFilePath])
            {
                using var fileStream = new FileStream(serverFilePath, (FileMode) 3, (FileAccess) 1);
                var length1 = 352;
                var buf1 = new byte[length1];
                fileStream.Read(buf1, 0, length1);
                var mainServer = UDT.ReadStruct<MainServer>(buf1, 0);
                if (mainServer.count != 0)
                {
                    var num = Marshal.SizeOf(typeof(Server));
                    var length2 = mainServer.count * num;
                    var buf2 = new byte[length2];
                    fileStream.Read(buf2, 0, length2);
                    var buf3 = MT4Crypt.Decrypt(buf2);
                    dataSrv = new Server[mainServer.count];
                    for (var index = 0; index < mainServer.count; ++index)
                        dataSrv[index] = UDT.ReadStruct<Server>(buf3, index * num);
                }
                else
                {
                    dataSrv = new Server[0];
                }

                return mainServer;
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Unable to read server list: {ex.Message}");
        }
    }

    private static string ValidFileName(string s)
    {
        var str = s;
        foreach (var invalidFileNameChar in Path.GetInvalidFileNameChars())
            str = str.Replace(invalidFileNameChar, '_');
        return str;
    }

    public static ServerInfo WriteServers(byte[] data, int is_demo, string path)
    {
        ServerInfo serverInfo;
        try
        {
            path = path.Trim();
            if (path != "")
                path += "\\";
            path.Replace("\\\\", "\\");
            var mainServer = new MainServer
            {
                is_demo = is_demo
            };
            var num = Marshal.SizeOf(typeof(Server));
            mainServer.count = (data.Length - 196) / num;
            var memoryStream = new MemoryStream(data, 4, data.Length - 4);
            var length1 = mainServer.count * num;
            var buf1 = new byte[length1];
            memoryStream.Read(buf1, 0, length1);
            mainServer.host_addr = UDT.ReadStruct<Server>(data, 4).server;
            var numArray = MT4Crypt.Encrypt(buf1);
            var length2 = 192;
            var buf2 = new byte[length2];
            memoryStream.Read(buf2, 0, length2);
            serverInfo = UDT.ReadStruct<ServerInfo>(buf2, 0);
            mainServer.name = serverInfo.name;
            mainServer.comment = serverInfo.comment;
            memoryStream.Close();
            var key = $"{path}{ValidFileName(mainServer.name)}.srv";
            lock (Locks)
            {
                if (!Locks.ContainsKey(key))
                    Locks.Add(key, new object());
            }

            lock (Locks[key])
            {
                using var fs = new FileStream(key, (FileMode) 2, (FileAccess) 2);
                UDT.WriteStruct(fs, mainServer);
                fs.Write(numArray, 0, length1);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Unable to write server list: {ex.Message}");
        }

        return serverInfo;
    }

    public static ServerInfo WriteServers(byte[] data, int is_demo, Stream stream)
    {
        ServerInfo serverInfo;
        try
        {
            var mainServer = new MainServer
            {
                is_demo = is_demo
            };
            var num = Marshal.SizeOf(typeof(Server));
            mainServer.count = (data.Length - 196) / num;
            var memoryStream = new MemoryStream(data, 4, data.Length - 4);
            var length1 = mainServer.count * num;
            var buf1 = new byte[length1];
            memoryStream.Read(buf1, 0, length1);
            mainServer.host_addr = UDT.ReadStruct<Server>(data, 4).server;
            var numArray = MT4Crypt.Encrypt(buf1);
            var length2 = 192;
            var buf2 = new byte[length2];
            memoryStream.Read(buf2, 0, length2);
            serverInfo = UDT.ReadStruct<ServerInfo>(buf2, 0);
            mainServer.name = serverInfo.name;
            mainServer.comment = serverInfo.comment;
            memoryStream.Close();
            UDT.WriteStruct(stream, mainServer);
            stream.Write(numArray, 0, length1);
        }
        catch (Exception ex)
        {
            throw new Exception($"Unable to write server list: {ex.Message}");
        }

        return serverInfo;
    }
}
