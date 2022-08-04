using JetBrains.Annotations;

namespace Shared;
[PublicAPI]
public class ConnectorException : Exception
{
    public ConnectorException(string msg) => throw new Exception($"CONNECTOR EXCEPTION: {msg}");
}
