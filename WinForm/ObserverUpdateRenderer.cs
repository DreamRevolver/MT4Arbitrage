using System;
using System.Globalization;
using System.IO;
using log4net.ObjectRenderer;
using Shared.Models;

namespace WinForm;

public class ObserverUpdateRenderer : IObjectRenderer
{
    public void RenderObject(RendererMap rendererMap, object obj, TextWriter writer)
    {
        var update = (ObserverUpdate) obj;
        writer.Write($"Pair:{update.pair} | Spread:{update.spread.ToString()} | Fast {update.fastBroker} (Bid:{update.fastMarketBook.Bid.ToString()} | Ask: {update.fastMarketBook.Ask.ToString()})| Slow {update.slowBroker} (Bid:{update.slowMarketBook.Bid.ToString()} | Ask: {update.slowMarketBook.Ask.ToString()}) | Delta: {update.Delta.ToString("F7")}");
    }
}
