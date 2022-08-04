namespace TradingAPI.MT4Server;

internal class Encoder
{
    private byte[] Key;
    private int KeyInd;
    private byte Last;

    public Encoder(byte[] key) => Key = key;

    public void ChangeKey(byte[] key)
    {
        Key = key;
    }

    public byte[] GetKey() => Key;

    public void Reset()
    {
        Last = 0;
        KeyInd = 0;
    }

    public byte[] Encode(byte[] buf)
    {
        var numArray = new byte[buf.Length];
        for (var index = 0; index < buf.Length; ++index)
        {
            KeyInd &= 15;
            numArray[index] = (byte) (buf[index] ^ (Last + (uint) Key[KeyInd]));
            ++KeyInd;
            Last = buf[index];
        }

        return numArray;
    }
}