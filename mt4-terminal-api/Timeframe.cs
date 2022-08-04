namespace TradingAPI.MT4Server;

public enum Timeframe
{
    M1 = 1,
    M5 = 5,
    M15 = 15, // 0x0000000F
    M30 = 30, // 0x0000001E
    H1 = 60, // 0x0000003C
    H4 = 240, // 0x000000F0
    D1 = 1440, // 0x000005A0
    W1 = 10080, // 0x00002760
    MN1 = 43200 // 0x0000A8C0
}