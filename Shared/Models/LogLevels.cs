using log4net;

namespace Shared.Models;

public static class LogLevels
{
    public static readonly log4net.Core.Level Observer = new(1, "Observer");

    public static void Register()
    {
        LogManager.GetRepository().LevelMap.Add(Observer);
    }
}