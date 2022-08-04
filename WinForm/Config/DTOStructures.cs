using Shared.Models;

namespace WinForm.Config;

public readonly struct ConfigDTO
{
    public readonly PairDTO[] pairs;
    public readonly BrokerDTO[] fastBrokers;
    public readonly BrokerDTO[] slowBrokers;
}

public readonly struct BrokerDTO
{
    public readonly string srvFilePath;
    public readonly int user;
    public readonly string password;
}

public readonly struct PairDTO
{
    public readonly string name;
    public readonly double spread;
    public static implicit operator Pair(PairDTO pairDto) => new Pair(pairDto.name, pairDto.spread);
}
