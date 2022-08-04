namespace TradingAPI.MT4Server;

internal class Decoder
{
    private byte[] Key;
    private int KeyInd;
    private byte Last;

    public Decoder(byte[] key) => Key = key;

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

    public byte[] Decode(byte[] buf)
    {
        var numArray = new byte[buf.Length];
        for (var index = 0; index < buf.Length; ++index)
        {
            KeyInd &= 15;
            numArray[index] = (byte) (buf[index] ^ (Last + (uint) Key[KeyInd]));
            ++KeyInd;
            Last = numArray[index];
        }

        return numArray;
    }
}