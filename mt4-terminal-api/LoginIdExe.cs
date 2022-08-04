using System.Diagnostics;
using System.Text;

namespace TradingAPI.MT4Server;

internal class LoginIdExe
{
    public static ulong Get(byte[] data, string path)
    {
        var processStartInfo = new ProcessStartInfo
        {
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            FileName = path,
            WorkingDirectory = new FileInfo(path).DirectoryName
        };
        var process = new Process();
        process.StartInfo = processStartInfo;
        process.Start();
        using (var baseStream = process.StandardInput.BaseStream)
        {
            baseStream.Write(BitConverter.GetBytes(data.Length), 0, 4);
            baseStream.Write(data, 0, data.Length);
            baseStream.Flush();
        }

        using (var baseStream = process.StandardOutput.BaseStream)
        {
            var numArray1 = new byte[4];
            if (baseStream.Read(numArray1, 0, numArray1.Length) < 4)
                throw new Exception("Cannot read header");
            var numArray2 = new byte[BitConverter.ToInt32(numArray1, 0)];
            var num = baseStream.Read(numArray2, 0, numArray2.Length);
            if (num != numArray2.Length)
                throw new Exception("Wrong input");
            if (num != 8)
                throw new Exception(Encoding.UTF8.GetString(numArray2));
            return BitConverter.ToUInt64(numArray2, 0);
        }
    }
}
