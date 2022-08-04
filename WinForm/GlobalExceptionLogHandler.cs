using System.IO;
using System.Text;

namespace WinForm;

public static class GlobalExceptionLogHandler
{
    private static FileStream CreateNewFile(string name)
    {
        if (File.Exists(name)) File.Delete(name);
        return File.Create(name);
    }

    public static void LogToFile(string filename, string message)
    {
        using (var writer = CreateNewFile(filename))
        {
            var payload = Encoding.UTF8.GetBytes(message);
            writer.Write(payload, 0, payload.Length);
        }
    }
}