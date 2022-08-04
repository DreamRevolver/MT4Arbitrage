namespace Shared.Interfaces;

public interface ITickData
{
    
    double Bid { get; set; }
    double Ask { get; set; }
    string Pair { get; set; }
}
