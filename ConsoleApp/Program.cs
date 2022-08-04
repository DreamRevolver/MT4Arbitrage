using System.Globalization;
using Autofac;
using LMAXConnector;
using QuickFix;
using QuickFix.Transport;
using Shared.Interfaces;
using TradingAPI.MT4Server;
using Utf8Json.Resolvers;

namespace ConsoleApp;

internal class Program
{
    private static void Main()
    {

        var containerBuilder = new ContainerBuilder();

        CompositeResolver.RegisterAndSetAsDefault(StandardResolver.AllowPrivateExcludeNull);
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
        var settings = new SessionSettings("config.cfg");

        containerBuilder.RegisterInstance(settings);

        containerBuilder.RegisterType<FixLMAXBrokerConnector>().As<MessageCracker>().As<IBrokerConnector>().SingleInstance();
        containerBuilder.RegisterType<FileStoreFactory>().As<IMessageStoreFactory>().SingleInstance();
        containerBuilder.RegisterType<ScreenLogFactory>().As<ILogFactory>().SingleInstance();
        containerBuilder.RegisterType<SocketInitiator>().As<IInitiator>().SingleInstance();
        containerBuilder.RegisterType<FixApplication>().As<IApplication>().PropertiesAutowired().SingleInstance();
        var container = containerBuilder.Build();
        var connector = container.Resolve<IBrokerConnector>();
        connector.Start();
        
    }

    private static void QcOnOnQuoteHistory(object sender, QuoteHistoryEventArgs args)
    {
        foreach (var i in args.Bars) Console.WriteLine($"Low: {i.Low} High: {i.High} Open: {i.Open} Close: {i.Close} ");

        Console.WriteLine($"-----");
    }

    private static void qc_OnDisc(object sender, DisconnectEventArgs args)
    {
        Console.WriteLine("Disc, " + args.Exception);
    }

    private static void qc_OnQuote(object sender, QuoteEventArgs args)
    {
        Console.WriteLine($"{args.Time} {args.Symbol} {args.Bid} {args.Ask}");
    }
}
