namespace TradingAPI.MT4Server;

public class DataLoginId
{
    private int _align;
    private byte[] s0 = new byte[1024];
    private int s400;
    private ulong s408;

    public DataLoginId()
    {
        s400 = 0;
        s408 = 0UL;
    }

    public ulong Decode(byte[] data)
    {
        var pData = new ulong[data.Length / 8];
        for (var index = 0; index < pData.Length; ++index)
            pData[index] = BitConverter.ToUInt64(data, index * 8);
        return Decode(pData, (uint) pData.Length);
    }

    public ulong Decode(ulong[] pData, uint szData)
    {
        ulong num1 = 0;
        for (uint index = 0; index < szData; index += 3U)
        {
            var num2 = (byte) (pData[(int) index] >> 14);
            var num3 = pData[(int) index + 2];
            if ((((long) pData[(int) index + 1] & 4177920L) ^ 1359872L) != 0L)
                num1 = pData[(int) index + 1];
            switch (num2)
            {
                case 18:
                    num1 |= num3;
                    break;
                case 58:
                    num1 -= num3;
                    break;
                case 64:
                    num1 >>= (int) (num3 % 24UL);
                    break;
                case 121:
                    num1 ^= num3;
                    break;
                case 139:
                    num1 += num3;
                    break;
                case 181:
                    num1 = (uint) (num1 & uint.MaxValue) | ((ulong) (uint) ((num3 >> 32) & uint.MaxValue) << 32);
                    break;
                case 194:
                    num1 <<= (int) (num3 % 24UL);
                    break;
                case 225:
                    num1 &= num3;
                    break;
                default:
                    return 0;
            }
        }

        return num1;
    }
}
