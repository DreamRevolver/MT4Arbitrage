namespace TradingAPI.MT4Server;

internal class ErrorDescription
{
    private static readonly Dictionary<int, int> keys = new();
    private static readonly Dictionary<int, string> strings;

    static ErrorDescription()
    {
        keys.Add(0, 5711368);
        keys.Add(1, 5711364);
        keys.Add(2, 5711348);
        keys.Add(3, 5711328);
        keys.Add(4, 5711312);
        keys.Add(5, 5711300);
        keys.Add(6, 5711284);
        keys.Add(7, 5711264);
        keys.Add(8, 5711240);
        keys.Add(13, 5711232);
        keys.Add(14, 5711228);
        keys.Add(64, 5711220);
        keys.Add(65, 5711204);
        keys.Add(128, 5711188);
        keys.Add(129, 5711172);
        keys.Add(130, 5711152);
        keys.Add(131, 5711136);
        keys.Add(132, 5711116);
        keys.Add(133, 5711096);
        keys.Add(134, 5711076);
        keys.Add(135, 5711056);
        keys.Add(136, 5711044);
        keys.Add(137, 5711028);
        keys.Add(138, 5711020);
        keys.Add(139, 5711004);
        keys.Add(140, 5710972);
        keys.Add(141, 5710952);
        keys.Add(142, 5710932);
        keys.Add(143, 5710912);
        keys.Add(144, 5710884);
        keys.Add(145, 5710812);
        keys.Add(146, 5710860);
        keys.Add(147, 5710768);
        keys.Add(148, 5710752);
        keys.Add(149, 5710736);
        keys.Add(150, 5710720);
        strings = new Dictionary<int, string>();
        strings.Add(5710720, "Prohibited by FIFO rule");
        strings.Add(5710736, "Hedge is prohibited");
        strings.Add(5710752, "Too many open orders");
        strings.Add(5710768, "Expiration for pending orders is disabled");
        strings.Add(5710860, "Trade context is busy");
        strings.Add(5710812, "Modification denied. Order too close to market");
        strings.Add(5710884, "Request canceled by client");
        strings.Add(5710912, "Order is in process");
        strings.Add(5710932, "Order is accepted");
        strings.Add(5710952, "Too many requests");
        strings.Add(5710972, "Only long positions are allowed");
        strings.Add(5711004, "Order is locked");
        strings.Add(5711020, "Requote");
        strings.Add(5711028, "Broker is busy");
        strings.Add(5711044, "Off quotes");
        strings.Add(5711056, "Price is changed");
        strings.Add(5711076, "Not enough money");
        strings.Add(5711096, "Trade is disabled");
        strings.Add(5711116, "Market is closed");
        strings.Add(5711136, "Invalid volume");
        strings.Add(5711152, "Invalid S/L or T/P");
        strings.Add(5711172, "Invalid prices");
        strings.Add(5711188, "Trade timeout");
        strings.Add(5711204, "Invalid account");
        strings.Add(5711220, "Account disabled");
        strings.Add(5711228, "Invalid one-time password");
        strings.Add(5711232, "Secret key for one-time password is required");
        strings.Add(5711240, "Too frequent requests");
        strings.Add(5711264, "Not enough rights");
        strings.Add(5711284, "No connection");
        strings.Add(5711300, "Old version");
        strings.Add(5711312, "Server is busy");
        strings.Add(5711328, "Invalid parameters");
        strings.Add(5711348, "Common error");
        strings.Add(5711364, "OK");
        strings.Add(5711368, "Done");
    }

    public static string get(int code)
    {
        try
        {
            var key = keys[code];
            return strings[key];
        }
        catch (Exception)
        {
            return $"Unknown server reply {code:X}";
        }
    }
}
