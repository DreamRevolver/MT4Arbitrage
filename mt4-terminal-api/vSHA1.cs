namespace TradingAPI.MT4Server;

internal class vSHA1
{
    private int dbCount;
    private readonly byte[] dwBlock = new byte[64];
    private int dwCount;
    private uint dwData;
    private int nBitCount;
    private readonly uint[] Regs = new uint[5];

    public vSHA1()
    {
        nBitCount = 0;
        dwData = 0U;
        dwCount = 0;
        dbCount = 0;
        Regs[0] = 1732584193U;
        Regs[1] = 4023233417U;
        Regs[2] = 2562383102U;
        Regs[3] = 271733878U;
        Regs[4] = 3285377520U;
        Array.Clear(dwBlock, 0, 16);
    }

    public void HashData(byte[] data)
    {
        for (var index = 0; index < data.Length; ++index)
        {
            dwData = (dwData << 8) + data[index];
            nBitCount += 8;
            if (++dbCount >= 4)
            {
                dbCount = 0;
                BitConverter.GetBytes(dwData).CopyTo(dwBlock, dwCount * 4);
                if (++dwCount >= 16)
                {
                    dwCount = 0;
                    Transform(dwBlock);
                }

                dwData = 0U;
            }
        }
    }

    public byte[] FinalizeHash()
    {
        var nBitCount = this.nBitCount;
        dwData = (uint) (((int) dwData << 8) + 128);
        while (true)
        {
            this.nBitCount += 8;
            if (++dbCount >= 4)
            {
                dbCount = 0;
                BitConverter.GetBytes(dwData).CopyTo(dwBlock, dwCount * 4);
                if (++dwCount >= 16)
                {
                    dwCount = 0;
                    Transform(dwBlock);
                }

                dwData = 0U;
            }

            if (dbCount != 0 || dwCount != 14)
                dwData <<= 8;
            else
                break;
        }

        BitConverter.GetBytes(0).CopyTo(dwBlock, dwCount * 4);
        if (++dwCount >= 16)
        {
            dwCount = 0;
            Transform(dwBlock);
        }

        BitConverter.GetBytes(nBitCount).CopyTo(dwBlock, dwCount * 4);
        if (++dwCount >= 16)
        {
            dwCount = 0;
            Transform(dwBlock);
        }

        return new byte[20]
        {
            (byte) (Regs[0] >> 24),
            (byte) (Regs[0] >> 16),
            (byte) (Regs[0] >> 8),
            (byte) Regs[0],
            (byte) (Regs[1] >> 24),
            (byte) (Regs[1] >> 16),
            (byte) (Regs[1] >> 8),
            (byte) Regs[1],
            (byte) (Regs[2] >> 24),
            (byte) (Regs[2] >> 16),
            (byte) (Regs[2] >> 8),
            (byte) Regs[2],
            (byte) (Regs[3] >> 24),
            (byte) (Regs[3] >> 16),
            (byte) (Regs[3] >> 8),
            (byte) Regs[3],
            (byte) (Regs[4] >> 24),
            (byte) (Regs[4] >> 16),
            (byte) (Regs[4] >> 8),
            (byte) Regs[4]
        };
    }

    public byte[] ComputeHash(byte[] data)
    {
        var length = data.Length;
        var num1 = 0;
        if (length >= 64)
        {
            var data1 = new byte[64];
            for (var index1 = 0; index1 < length / 64; ++index1)
            {
                for (var index2 = 0; index2 < 64; ++index2)
                    data1[index2] = data[index1 * 64 + index2];
                Transform(data1);
                num1 += 64;
            }
        }

        var num2 = length % 64;
        if (num2 > 0)
        {
            var data2 = new byte[64];
            for (var index = 0; index < num2; ++index)
                data2[index] = data[num1 + index];
            Transform(data2);
        }

        return new byte[20]
        {
            (byte) Regs[0],
            (byte) (Regs[0] >> 8),
            (byte) (Regs[0] >> 16),
            (byte) (Regs[0] >> 24),
            (byte) Regs[1],
            (byte) (Regs[1] >> 8),
            (byte) (Regs[1] >> 16),
            (byte) (Regs[1] >> 24),
            (byte) Regs[2],
            (byte) (Regs[2] >> 8),
            (byte) (Regs[2] >> 16),
            (byte) (Regs[2] >> 24),
            (byte) Regs[3],
            (byte) (Regs[3] >> 8),
            (byte) (Regs[3] >> 16),
            (byte) (Regs[3] >> 24),
            (byte) Regs[4],
            (byte) (Regs[4] >> 8),
            (byte) (Regs[4] >> 16),
            (byte) (Regs[4] >> 24)
        };
    }

    private uint SHA1Shift(int bits, uint word) => (word << bits) | (word >> (32 - bits));

    private void Transform(byte[] data)
    {
        var numArray = new uint[80];
        for (var index = 0; index < 16; ++index)
            numArray[index] = BitConverter.ToUInt32(data, index * 4);
        for (var index = 16; index < 80; ++index)
            numArray[index] = SHA1Shift(1, numArray[index - 3] ^ numArray[index - 8] ^ numArray[index - 14] ^ numArray[index - 16]);
        var word1 = Regs[0];
        var word2 = Regs[1];
        var num1 = Regs[2];
        var num2 = Regs[3];
        var num3 = Regs[4];
        for (var index = 0; index < 20; ++index)
        {
            var num4 = (int) SHA1Shift(5, word1) + (((int) word2 & (int) num1) | (~(int) word2 & (int) num2)) + (int) num3 + (int) numArray[index] + 1518500249;
            num3 = num2;
            num2 = num1;
            num1 = SHA1Shift(30, word2);
            word2 = word1;
            word1 = (uint) num4;
        }

        for (var index = 20; index < 40; ++index)
        {
            var num5 = (int) SHA1Shift(5, word1) + ((int) word2 ^ (int) num1 ^ (int) num2) + (int) num3 + (int) numArray[index] + 1859775393;
            num3 = num2;
            num2 = num1;
            num1 = SHA1Shift(30, word2);
            word2 = word1;
            word1 = (uint) num5;
        }

        for (var index = 40; index < 60; ++index)
        {
            var num6 = (int) SHA1Shift(5, word1) + (((int) word2 & (int) num1) | ((int) word2 & (int) num2) | ((int) num1 & (int) num2)) + (int) num3 + (int) numArray[index] - 1894007588;
            num3 = num2;
            num2 = num1;
            num1 = SHA1Shift(30, word2);
            word2 = word1;
            word1 = (uint) num6;
        }

        for (var index = 60; index < 80; ++index)
        {
            var num7 = (int) SHA1Shift(5, word1) + ((int) word2 ^ (int) num1 ^ (int) num2) + (int) num3 + (int) numArray[index] - 899497514;
            num3 = num2;
            num2 = num1;
            num1 = SHA1Shift(30, word2);
            word2 = word1;
            word1 = (uint) num7;
        }

        Regs[0] += word1;
        Regs[1] += word2;
        Regs[2] += num1;
        Regs[3] += num2;
        Regs[4] += num3;
    }
}
