using System.Security.Cryptography;

namespace TradingAPI.MT4Server;

internal class MT4Crypt
{
    public static byte[] CryptKey = new byte[16]
    {
        65,
        182,
        127,
        88,
        56,
        12,
        240,
        45,
        123,
        57,
        8,
        254,
        33,
        187,
        65,
        88
    };

    private static byte[] _HardId = new byte[16];

    public static byte[] Encrypt(byte[] buf)
    {
        var num = 0;
        var numArray = new byte[buf.Length];
        for (var index = 0; index < buf.Length; ++index)
        {
            numArray[index] = (byte) (buf[index] ^ ((uint) num + CryptKey[index & 15]));
            num = numArray[index];
        }

        return numArray;
    }

    public static byte[] Decrypt(byte[] buf)
    {
        var num = 0;
        var numArray = new byte[buf.Length];
        for (var index = 0; index < buf.Length; ++index)
        {
            numArray[index] = (byte) (buf[index] ^ ((uint) num + CryptKey[index & 15]));
            num = buf[index];
        }

        return numArray;
    }

    public static byte[] Encode(byte[] buf, byte[] key)
    {
        var num = 0;
        var numArray = new byte[buf.Length];
        for (var index = 0; index < buf.Length; ++index)
        {
            numArray[index] = (byte) (buf[index] ^ ((uint) num + key[index % key.Length]));
            num = numArray[index];
        }

        return numArray;
    }

    public static byte[] Decode(byte[] buf, byte[] key)
    {
        var num = 0;
        var numArray = new byte[buf.Length];
        for (var index = 0; index < buf.Length; ++index)
        {
            numArray[index] = (byte) (buf[index] ^ ((uint) num + key[index % key.Length]));
            num = buf[index];
        }

        return numArray;
    }

    private static void CreateHardId()
    {
        var num = (uint) DateTime.Now.Ticks;
        var buffer = new byte[256];
        for (var index = 0; index < 256; ++index)
        {
            num = (uint) ((int) num * 214013 + 2531011);
            buffer[index] = (byte) ((num >> 16) & byte.MaxValue);
        }

        _HardId = new MD5CryptoServiceProvider().ComputeHash(buffer);
        _HardId[0] = 0;
        for (var index = 1; index < 16; ++index)
            _HardId[0] += _HardId[index];
    }

    public static byte[] GetHardId()
    {
        lock (_HardId)
        {
            if (_HardId[0] != 0)
                if (_HardId[15] != 0)
                    goto label_7;
            CreateHardId();
        }

        label_7:
        return _HardId;
    }
}
