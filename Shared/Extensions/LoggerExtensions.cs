using log4net;
using log4net.Core;
using Shared.Models;

namespace Shared.Extensions;

public static class LoggerExtensions
{
    public static void Log(this object obj) => Console.WriteLine(obj.ToString());
    public static void Observer(this ILogger logger, ObserverUpdate obj) => logger.Log(typeof(ILogger), LogLevels.Observer, obj, null);
    public static void Observer(this ILog logger, ObserverUpdate obj) => logger.Logger.Observer(obj);

    public static void Error(this ILogger logger, Exception exception, string source) => logger.Log(new LoggingEvent(new LoggingEventData
        {Message = $"Error: [Message: {exception.Message}, StackTrace: {exception.StackTrace}]", Identity = source, Level = Level.Error, TimeStamp = DateTime.Now}));

    public static void Error(this ILogger logger, string message, string source) => logger.Log(new LoggingEvent(new LoggingEventData {Message = message, Identity = source, Level = Level.Error, TimeStamp = DateTime.Now}));
    public static void Warning(this ILogger logger, string message, string source) => logger.Log(new LoggingEvent(new LoggingEventData {Message = message, Identity = source, Level = Level.Warn, TimeStamp = DateTime.Now}));
    public static void Info(this ILogger logger, string message, string source) => logger.Log(new LoggingEvent(new LoggingEventData {Message = message, Identity = source, Level = Level.Info, TimeStamp = DateTime.Now}));
    public static void Debug(this ILogger logger, string message, string source) => logger.Log(new LoggingEvent(new LoggingEventData {Message = message, Identity = source, Level = Level.Debug, TimeStamp = DateTime.Now}));
    public static void Trace(this ILogger logger, string message, string source) => logger.Log(new LoggingEvent(new LoggingEventData {Message = message, Identity = source, Level = Level.Trace, TimeStamp = DateTime.Now}));
    public static void Benchmark(this ILogger logger, string message, string source) => logger.Log(new LoggingEvent(new LoggingEventData {Message = message, Identity = source, TimeStamp = DateTime.Now}));
}
