namespace TradingAPI.MT4Server;

internal class vRSA
{
    private readonly ulong D;
    private readonly ulong M;
    private readonly ulong N;
    private ulong P;
    private ulong Q;
    private readonly ulong Y;

    public vRSA(ulong p)
    {
        P = p ^ 354255445UL;
        Q = p ^ 659475456UL;
        M = 465992738619895673UL;
        N = 14757395258967641292UL;
        D = 32882249924737UL;
        Y = 23407534262244673UL;
    }

    public ulong ComputePacketKey(byte[] data) => ExpMod64(PrepareKey(data) % N, N, M);

    public ulong ComputeFileKey(byte[] data) => ExpMod64(PrepareKey(data) % M, D, M);

    public bool CheckKey(byte[] data)
    {
        if (data.Length < 8)
            return false;
        var length = data.Length - 8;
        var numArray = new byte[length];
        Array.Copy(data, numArray, length);
        var num = PrepareKey(numArray);
        return (long) ExpMod64(BitConverter.ToUInt64(data, length), Y, M) == (long) (num % M);
    }

    private ulong PrepareKey(byte[] data)
    {
        if (data.Length < 1)
            return 0;
        ulong num1 = 0;
        ulong num2 = 4886718345;
        for (var index = 0; index < data.Length / 8; ++index)
        {
            var uint64 = BitConverter.ToUInt64(data, index * 8);
            num1 ^= uint64;
            var num3 = (uint) uint64;
            var num4 = (uint) (uint64 >> 32);
            var num5 = ((int) num4 & int.MinValue) != 0 ? 18446744069414584320UL : 0UL;
            var num6 = (((ulong) (num4 ^ num3) << 32) | (num3 ^ num4)) + num5 + (((ulong) num3 << 32) | num4 | num5) + num2 + uint64;
            num2 ^= num6;
        }

        var num7 = data.Length & 7;
        if (num7 != 0)
        {
            long num8 = 0;
            long num9 = 0;
            var num10 = 1234567890123;
            var num11 = 0;
            if (num7 >= 2)
            {
                var num12 = (num7 - 2) / 2 + 1;
                num11 = num12 * 2;
                var num13 = data.Length - num7;
                var num14 = 0;
                var num15 = 0;
                while (num15 < num12)
                {
                    num8 += (long) (sbyte) data[num15 * 2 + num13] << num14;
                    num9 += (long) (sbyte) data[num15 * 2 + 1 + num13] << (num14 + 8);
                    ++num15;
                    num14 += 16;
                }
            }

            if (num11 < num7)
                num10 += (long) (sbyte) data[data.Length - 1] << (num11 << 8);
            var num16 = (ulong) (num8 + num9 + num10);
            num1 ^= num16;
            var num17 = (uint) num16;
            var num18 = (uint) (num16 >> 32);
            var num19 = ((int) num18 & int.MinValue) != 0 ? 18446744069414584320UL : 0UL;
            var num20 = (((ulong) (num18 ^ num17) << 32) | (num17 ^ num18)) + num19 + (((ulong) num17 << 32) | num18 | num19) + num2 + num16;
            num2 ^= num20;
        }

        return (ulong) (((data.Length * 4294967297L) ^ (long) num2 ^ (long) num1) & 17592186044415L);
    }

    private ulong ExpMod64(ulong rem, ulong n, ulong m)
    {
        ulong k = 1;
        var num = rem;
        for (var index = 0; index < 64; ++index)
        {
            if (((long) (n >> index) & 1L) != 0L)
                k = MulMod64(k, num, m);
            num = MulMod64(num, num, m);
        }

        return k;
    }

    private ulong MulMod64(ulong k, ulong n, ulong m)
    {
        ulong num1 = 0;
        var num2 = k;
        for (var index = 0; index < 64; ++index)
        {
            if (((long) (n >> index) & 1L) != 0L)
                num1 = (num1 + num2) % m;
            num2 = num2 * 2UL % m;
        }

        return num1;
    }
}
