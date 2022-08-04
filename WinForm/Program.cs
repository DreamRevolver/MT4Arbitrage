using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Autofac;
using LMAXConnector;
using log4net;
using log4net.Config;
using QuickFix;
using QuickFix.Transport;
using QuoteObserver;
using Shared.Extensions;
using Shared.Interfaces;
using Shared.Models;
using Utf8Json.Resolvers;
using Application = System.Windows.Forms.Application;

namespace WinForm;

internal static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        CompositeResolver.RegisterAndSetAsDefault(StandardResolver.AllowPrivateExcludeNull);

        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
        var log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        var logger = log.Logger;
        var containerBuilder = new ContainerBuilder();
        var settings = new SessionSettings("Config/config.cfg");
        containerBuilder.RegisterInstance(settings);
        containerBuilder.RegisterInstance(logger);
        containerBuilder.RegisterInstance(log);
        containerBuilder.RegisterType<FixLMAXBrokerConnector>().As<MessageCracker>().As<IBrokerConnector>().SingleInstance();
        containerBuilder.RegisterType<FileStoreFactory>().As<IMessageStoreFactory>().SingleInstance();
        containerBuilder.RegisterType<ScreenLogFactory>().As<ILogFactory>().SingleInstance();
        containerBuilder.RegisterType<SocketInitiator>().As<IInitiator>().SingleInstance();
        containerBuilder.RegisterType<FixApplication>().As<IApplication>().PropertiesAutowired().SingleInstance();
        containerBuilder.RegisterInstance(LmaxInstrumentParser.ParseFromFile("Config/Pairs.csv")).Named<List<Instrument>>("PAIRS");
        containerBuilder.RegisterType<Observer>().As<IArbitrationObserver>().SingleInstance();
        LogLevels.Register();
        LogManager.GetRepository().RendererMap.Put(typeof(ObserverUpdate), new ObserverUpdateRenderer());
        XmlConfigurator.Configure();
        containerBuilder.RegisterType<LogWindow>().SingleInstance();
        containerBuilder.RegisterType<SpreadWindow>().SingleInstance();
        containerBuilder.RegisterType<MainWindow>().SingleInstance();
        var container = containerBuilder.Build();
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += (sender, args) =>
        {
            Trace.Write($"ThreadException EXCEPTION FROM SENDER: {sender} => Message {args.Exception.Message} | StackTrace {args.Exception.StackTrace}");
            GlobalExceptionLogHandler.LogToFile("ThreadExceptionLogFile", $"ThreadException EXCEPTION FROM SENDER: {sender} => Message {args.Exception.Message} | StackTrace {args.Exception.StackTrace}");
            logger.Error($"ThreadException EXCEPTION FROM SENDER: {sender} => Message {args.Exception.Message} | StackTrace {args.Exception.StackTrace}", "UNHANDLED ThreadException");
        };
        Application.Run(container.Resolve<MainWindow>());
    }
}
