using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net.Core;
using Shared.Extensions;
using Shared.Models;
using static Utf8Json.JsonSerializer;

namespace WinForm.Config;

public class ConfigLoader
{
    private readonly ILogger _logger;
    private readonly List<Pair> _pairs = new List<Pair>();
    private IEnumerable<BrokerDTO> _fastBrokers = Enumerable.Empty<BrokerDTO>();
    private IEnumerable<BrokerDTO> _slowBrokers = Enumerable.Empty<BrokerDTO>();
    public List<Pair> Pairs => _pairs;
    public IEnumerable<BrokerDTO> FastBrokers => _fastBrokers;
    public IEnumerable<BrokerDTO> SlowBrokers => _slowBrokers;
    public ConfigLoader(ILogger logger) => _logger = logger;

    public void LoadFromFile()
    {
        try
        {
            if (File.Exists("Config/config.json"))
            {
                _logger.Info("Loading config.json", "Config Loader");
                using (var fs = new FileStream("Config/config.json", FileMode.Open, FileAccess.Read))
                {
                    var conf = Deserialize<ConfigDTO>(fs);
                    foreach (var pair in conf.pairs)
                    {
                        _pairs.Add(pair);
                        _logger.Info($"READ {pair.name}, Spread: {pair.spread}", "Config Loader");
                    }
                    _fastBrokers = conf.fastBrokers;
                    _slowBrokers = conf.slowBrokers;
                }
                _logger.Info("config.json loaded", "Config Loader");
            }
            else
            {
                _logger.Warning("Cant find config.json", "Config Loader");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "ConfigLoader.LoadFromFile");
        }
    }
}
