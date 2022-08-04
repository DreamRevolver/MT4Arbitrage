// Decompiled with JetBrains decompiler
// Type: Shared.Models.MarketBook
// Assembly: Shared, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 0543B696-6DAE-4CA5-9153-B54F54037F5B
// Assembly location: C:\Users\ellen\RiderProjects\lmaxTestSolution\LmaxFixConnector\bin\Debug\Shared.dll

using Shared.Interfaces;

namespace Shared.Models;

public struct MarketBook : ITickData
{
    public uint BidVolume;
    public uint AskVolume;
    public double Trade;
    public uint TradeVolume;
    public DateTime Time;
    public double Bid { get; set; }
    public double Ask { get; set; }
    public string Pair { get; set; }
}
