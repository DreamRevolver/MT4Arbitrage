namespace TradingAPI.MT4Server;

internal class vAES
{
    private readonly uint[] m_Ke = new uint[64];
    private readonly uint[] m_Ks = new uint[64];
    private int m_nCipherRnd;
    private readonly uint[,] s_tabFT = new uint[4, 256];
    private readonly uint[] s_tabIB = new uint[256];
    private readonly uint[,] s_tabIT = new uint[4, 256];
    private readonly uint[] s_tabSB = new uint[256];

    public vAES()
    {
        m_nCipherRnd = 0;
        Array.Clear(m_Ks, 0, 64);
        Array.Clear(m_Ke, 0, 64);
    }

    private byte[] EncryptBlock(byte[] data)
    {
        var numArray1 = new uint[4];
        var numArray2 = new uint[4];
        numArray1[0] = m_Ks[0] ^ BitConverter.ToUInt32(data, 0);
        numArray1[1] = m_Ks[1] ^ BitConverter.ToUInt32(data, 4);
        numArray1[2] = m_Ks[2] ^ BitConverter.ToUInt32(data, 8);
        numArray1[3] = m_Ks[3] ^ BitConverter.ToUInt32(data, 12);
        numArray2[0] = s_tabFT[0, (int) numArray1[0] & byte.MaxValue] ^ s_tabFT[1, (int) (numArray1[1] >> 8) & byte.MaxValue] ^ s_tabFT[2, (int) (numArray1[2] >> 16) & byte.MaxValue] ^ s_tabFT[3, (int) (numArray1[3] >> 24) & byte.MaxValue] ^ m_Ks[4];
        numArray2[1] = s_tabFT[0, (int) numArray1[1] & byte.MaxValue] ^ s_tabFT[1, (int) (numArray1[2] >> 8) & byte.MaxValue] ^ s_tabFT[2, (int) (numArray1[3] >> 16) & byte.MaxValue] ^ s_tabFT[3, (int) (numArray1[0] >> 24) & byte.MaxValue] ^ m_Ks[5];
        numArray2[2] = s_tabFT[0, (int) numArray1[2] & byte.MaxValue] ^ s_tabFT[1, (int) (numArray1[3] >> 8) & byte.MaxValue] ^ s_tabFT[2, (int) (numArray1[0] >> 16) & byte.MaxValue] ^ s_tabFT[3, (int) (numArray1[1] >> 24) & byte.MaxValue] ^ m_Ks[6];
        numArray2[3] = s_tabFT[0, (int) numArray1[3] & byte.MaxValue] ^ s_tabFT[1, (int) (numArray1[0] >> 8) & byte.MaxValue] ^ s_tabFT[2, (int) (numArray1[1] >> 16) & byte.MaxValue] ^ s_tabFT[3, (int) (numArray1[2] >> 24) & byte.MaxValue] ^ m_Ks[7];
        numArray2.CopyTo(numArray1, 0);
        int num;
        for (num = 0; num < m_nCipherRnd - 2; num += 2)
        {
            numArray2[0] = s_tabFT[0, (int) numArray1[0] & byte.MaxValue] ^ s_tabFT[1, (int) (numArray1[1] >> 8) & byte.MaxValue] ^ s_tabFT[2, (int) (numArray1[2] >> 16) & byte.MaxValue] ^ s_tabFT[3, (int) (numArray1[3] >> 24) & byte.MaxValue] ^
                           m_Ks[num * 4 + 8];
            numArray2[1] = s_tabFT[0, (int) numArray1[1] & byte.MaxValue] ^ s_tabFT[1, (int) (numArray1[2] >> 8) & byte.MaxValue] ^ s_tabFT[2, (int) (numArray1[3] >> 16) & byte.MaxValue] ^ s_tabFT[3, (int) (numArray1[0] >> 24) & byte.MaxValue] ^
                           m_Ks[num * 4 + 9];
            numArray2[2] = s_tabFT[0, (int) numArray1[2] & byte.MaxValue] ^ s_tabFT[1, (int) (numArray1[3] >> 8) & byte.MaxValue] ^ s_tabFT[2, (int) (numArray1[0] >> 16) & byte.MaxValue] ^ s_tabFT[3, (int) (numArray1[1] >> 24) & byte.MaxValue] ^
                           m_Ks[num * 4 + 10];
            numArray2[3] = s_tabFT[0, (int) numArray1[3] & byte.MaxValue] ^ s_tabFT[1, (int) (numArray1[0] >> 8) & byte.MaxValue] ^ s_tabFT[2, (int) (numArray1[1] >> 16) & byte.MaxValue] ^ s_tabFT[3, (int) (numArray1[2] >> 24) & byte.MaxValue] ^
                           m_Ks[num * 4 + 11];
            numArray2.CopyTo(numArray1, 0);
            numArray2[0] = s_tabFT[0, (int) numArray1[0] & byte.MaxValue] ^ s_tabFT[1, (int) (numArray1[1] >> 8) & byte.MaxValue] ^ s_tabFT[2, (int) (numArray1[2] >> 16) & byte.MaxValue] ^ s_tabFT[3, (int) (numArray1[3] >> 24) & byte.MaxValue] ^
                           m_Ks[num * 4 + 12];
            numArray2[1] = s_tabFT[0, (int) numArray1[1] & byte.MaxValue] ^ s_tabFT[1, (int) (numArray1[2] >> 8) & byte.MaxValue] ^ s_tabFT[2, (int) (numArray1[3] >> 16) & byte.MaxValue] ^ s_tabFT[3, (int) (numArray1[0] >> 24) & byte.MaxValue] ^
                           m_Ks[num * 4 + 13];
            numArray2[2] = s_tabFT[0, (int) numArray1[2] & byte.MaxValue] ^ s_tabFT[1, (int) (numArray1[3] >> 8) & byte.MaxValue] ^ s_tabFT[2, (int) (numArray1[0] >> 16) & byte.MaxValue] ^ s_tabFT[3, (int) (numArray1[1] >> 24) & byte.MaxValue] ^
                           m_Ks[num * 4 + 14];
            numArray2[3] = s_tabFT[0, (int) numArray1[3] & byte.MaxValue] ^ s_tabFT[1, (int) (numArray1[0] >> 8) & byte.MaxValue] ^ s_tabFT[2, (int) (numArray1[1] >> 16) & byte.MaxValue] ^ s_tabFT[3, (int) (numArray1[2] >> 24) & byte.MaxValue] ^
                           m_Ks[num * 4 + 15];
            numArray2.CopyTo(numArray1, 0);
        }

        var numArray3 = new byte[16];
        BitConverter.GetBytes((uint) ((int) s_tabSB[(int) numArray1[0] & byte.MaxValue] ^ ((int) s_tabSB[(int) (numArray1[1] >> 8) & byte.MaxValue] << 8) ^ ((int) s_tabSB[(int) (numArray1[2] >> 16) & byte.MaxValue] << 16) ^
                                      ((int) s_tabSB[(int) (numArray1[3] >> 24) & byte.MaxValue] << 24)) ^ m_Ks[num * 4 + 8]).CopyTo(numArray3, 0);
        BitConverter.GetBytes((uint) ((int) s_tabSB[(int) numArray1[1] & byte.MaxValue] ^ ((int) s_tabSB[(int) (numArray1[2] >> 8) & byte.MaxValue] << 8) ^ ((int) s_tabSB[(int) (numArray1[3] >> 16) & byte.MaxValue] << 16) ^
                                      ((int) s_tabSB[(int) (numArray1[0] >> 24) & byte.MaxValue] << 24)) ^ m_Ks[num * 4 + 9]).CopyTo(numArray3, 4);
        BitConverter.GetBytes((uint) ((int) s_tabSB[(int) numArray1[2] & byte.MaxValue] ^ ((int) s_tabSB[(int) (numArray1[3] >> 8) & byte.MaxValue] << 8) ^ ((int) s_tabSB[(int) (numArray1[0] >> 16) & byte.MaxValue] << 16) ^
                                      ((int) s_tabSB[(int) (numArray1[1] >> 24) & byte.MaxValue] << 24)) ^ m_Ks[num * 4 + 10]).CopyTo(numArray3, 8);
        BitConverter.GetBytes((uint) ((int) s_tabSB[(int) numArray1[3] & byte.MaxValue] ^ ((int) s_tabSB[(int) (numArray1[0] >> 8) & byte.MaxValue] << 8) ^ ((int) s_tabSB[(int) (numArray1[1] >> 16) & byte.MaxValue] << 16) ^
                                      ((int) s_tabSB[(int) (numArray1[2] >> 24) & byte.MaxValue] << 24)) ^ m_Ks[num * 4 + 11]).CopyTo(numArray3, 12);
        return numArray3;
    }

    private byte[] DecryptBlock(byte[] data)
    {
        var numArray1 = new uint[4];
        var numArray2 = new uint[4];
        numArray1[0] = m_Ke[0] ^ BitConverter.ToUInt32(data, 0);
        numArray1[1] = m_Ke[1] ^ BitConverter.ToUInt32(data, 4);
        numArray1[2] = m_Ke[2] ^ BitConverter.ToUInt32(data, 8);
        numArray1[3] = m_Ke[3] ^ BitConverter.ToUInt32(data, 12);
        numArray2[0] = s_tabIT[0, (int) numArray1[0] & byte.MaxValue] ^ s_tabIT[1, (int) (numArray1[3] >> 8) & byte.MaxValue] ^ s_tabIT[2, (int) (numArray1[2] >> 16) & byte.MaxValue] ^ s_tabIT[3, (int) (numArray1[1] >> 24) & byte.MaxValue] ^ m_Ke[4];
        numArray2[1] = s_tabIT[0, (int) numArray1[1] & byte.MaxValue] ^ s_tabIT[1, (int) (numArray1[0] >> 8) & byte.MaxValue] ^ s_tabIT[2, (int) (numArray1[3] >> 16) & byte.MaxValue] ^ s_tabIT[3, (int) (numArray1[2] >> 24) & byte.MaxValue] ^ m_Ke[5];
        numArray2[2] = s_tabIT[0, (int) numArray1[2] & byte.MaxValue] ^ s_tabIT[1, (int) (numArray1[1] >> 8) & byte.MaxValue] ^ s_tabIT[2, (int) (numArray1[0] >> 16) & byte.MaxValue] ^ s_tabIT[3, (int) (numArray1[3] >> 24) & byte.MaxValue] ^ m_Ke[6];
        numArray2[3] = s_tabIT[0, (int) numArray1[3] & byte.MaxValue] ^ s_tabIT[1, (int) (numArray1[2] >> 8) & byte.MaxValue] ^ s_tabIT[2, (int) (numArray1[1] >> 16) & byte.MaxValue] ^ s_tabIT[3, (int) (numArray1[0] >> 24) & byte.MaxValue] ^ m_Ke[7];
        numArray2.CopyTo(numArray1, 0);
        int num;
        for (num = 0; num < m_nCipherRnd - 2; num += 2)
        {
            numArray2[0] = s_tabIT[0, (int) numArray1[0] & byte.MaxValue] ^ s_tabIT[1, (int) (numArray1[3] >> 8) & byte.MaxValue] ^ s_tabIT[2, (int) (numArray1[2] >> 16) & byte.MaxValue] ^ s_tabIT[3, (int) (numArray1[1] >> 24) & byte.MaxValue] ^
                           m_Ke[num * 4 + 8];
            numArray2[1] = s_tabIT[0, (int) numArray1[1] & byte.MaxValue] ^ s_tabIT[1, (int) (numArray1[0] >> 8) & byte.MaxValue] ^ s_tabIT[2, (int) (numArray1[3] >> 16) & byte.MaxValue] ^ s_tabIT[3, (int) (numArray1[2] >> 24) & byte.MaxValue] ^
                           m_Ke[num * 4 + 9];
            numArray2[2] = s_tabIT[0, (int) numArray1[2] & byte.MaxValue] ^ s_tabIT[1, (int) (numArray1[1] >> 8) & byte.MaxValue] ^ s_tabIT[2, (int) (numArray1[0] >> 16) & byte.MaxValue] ^ s_tabIT[3, (int) (numArray1[3] >> 24) & byte.MaxValue] ^
                           m_Ke[num * 4 + 10];
            numArray2[3] = s_tabIT[0, (int) numArray1[3] & byte.MaxValue] ^ s_tabIT[1, (int) (numArray1[2] >> 8) & byte.MaxValue] ^ s_tabIT[2, (int) (numArray1[1] >> 16) & byte.MaxValue] ^ s_tabIT[3, (int) (numArray1[0] >> 24) & byte.MaxValue] ^
                           m_Ke[num * 4 + 11];
            numArray2.CopyTo(numArray1, 0);
            numArray2[0] = s_tabIT[0, (int) numArray1[0] & byte.MaxValue] ^ s_tabIT[1, (int) (numArray1[3] >> 8) & byte.MaxValue] ^ s_tabIT[2, (int) (numArray1[2] >> 16) & byte.MaxValue] ^ s_tabIT[3, (int) (numArray1[1] >> 24) & byte.MaxValue] ^
                           m_Ke[num * 4 + 12];
            numArray2[1] = s_tabIT[0, (int) numArray1[1] & byte.MaxValue] ^ s_tabIT[1, (int) (numArray1[0] >> 8) & byte.MaxValue] ^ s_tabIT[2, (int) (numArray1[3] >> 16) & byte.MaxValue] ^ s_tabIT[3, (int) (numArray1[2] >> 24) & byte.MaxValue] ^
                           m_Ke[num * 4 + 13];
            numArray2[2] = s_tabIT[0, (int) numArray1[2] & byte.MaxValue] ^ s_tabIT[1, (int) (numArray1[1] >> 8) & byte.MaxValue] ^ s_tabIT[2, (int) (numArray1[0] >> 16) & byte.MaxValue] ^ s_tabIT[3, (int) (numArray1[3] >> 24) & byte.MaxValue] ^
                           m_Ke[num * 4 + 14];
            numArray2[3] = s_tabIT[0, (int) numArray1[3] & byte.MaxValue] ^ s_tabIT[1, (int) (numArray1[2] >> 8) & byte.MaxValue] ^ s_tabIT[2, (int) (numArray1[1] >> 16) & byte.MaxValue] ^ s_tabIT[3, (int) (numArray1[0] >> 24) & byte.MaxValue] ^
                           m_Ke[num * 4 + 15];
            numArray2.CopyTo(numArray1, 0);
        }

        var numArray3 = new byte[16];
        BitConverter.GetBytes((uint) ((int) s_tabIB[(int) numArray1[0] & byte.MaxValue] ^ ((int) s_tabIB[(int) (numArray1[3] >> 8) & byte.MaxValue] << 8) ^ ((int) s_tabIB[(int) (numArray1[2] >> 16) & byte.MaxValue] << 16) ^
                                      ((int) s_tabIB[(int) (numArray1[1] >> 24) & byte.MaxValue] << 24)) ^ m_Ke[num * 4 + 8]).CopyTo(numArray3, 0);
        BitConverter.GetBytes((uint) ((int) s_tabIB[(int) numArray1[1] & byte.MaxValue] ^ ((int) s_tabIB[(int) (numArray1[0] >> 8) & byte.MaxValue] << 8) ^ ((int) s_tabIB[(int) (numArray1[3] >> 16) & byte.MaxValue] << 16) ^
                                      ((int) s_tabIB[(int) (numArray1[2] >> 24) & byte.MaxValue] << 24)) ^ m_Ke[num * 4 + 9]).CopyTo(numArray3, 4);
        BitConverter.GetBytes((uint) ((int) s_tabIB[(int) numArray1[2] & byte.MaxValue] ^ ((int) s_tabIB[(int) (numArray1[1] >> 8) & byte.MaxValue] << 8) ^ ((int) s_tabIB[(int) (numArray1[0] >> 16) & byte.MaxValue] << 16) ^
                                      ((int) s_tabIB[(int) (numArray1[3] >> 24) & byte.MaxValue] << 24)) ^ m_Ke[num * 4 + 10]).CopyTo(numArray3, 8);
        BitConverter.GetBytes((uint) ((int) s_tabIB[(int) numArray1[3] & byte.MaxValue] ^ ((int) s_tabIB[(int) (numArray1[2] >> 8) & byte.MaxValue] << 8) ^ ((int) s_tabIB[(int) (numArray1[1] >> 16) & byte.MaxValue] << 16) ^
                                      ((int) s_tabIB[(int) (numArray1[0] >> 24) & byte.MaxValue] << 24)) ^ m_Ke[num * 4 + 11]).CopyTo(numArray3, 12);
        return numArray3;
    }

    private uint upr(uint x) => (x << 8) | (x >> 24);

    public void GenerateTables()
    {
        var numArray1 = new byte[256];
        var numArray2 = new byte[256];
        numArray1[0] = 0;
        byte num1 = 1;
        for (var index1 = 0; index1 < 256; ++index1)
        {
            uint index2 = num1;
            numArray1[(int) index2] = (byte) index1;
            numArray2[index1] = num1;
            num1 ^= (byte) ((index2 << 1) ^ ((num1 & 128) != 0 ? 27L : 0L));
        }

        numArray2[byte.MaxValue] = 0;
        for (var index3 = 0; index3 < 256; ++index3)
        {
            var num2 = numArray2[byte.MaxValue - numArray1[index3]];
            var index4 = (byte) ((((((((num2 >> 1) ^ num2) >> 1) ^ num2) >> 1) ^ num2) >> 4) ^ (((((((num2 << 1) ^ num2) << 1) ^ num2) << 1) ^ num2) << 1) ^ num2 ^ 99);
            s_tabSB[index3] = index4;
            s_tabIB[index4] = (uint) index3;
        }

        for (var index5 = 0; index5 < 256; ++index5)
        {
            var num3 = (byte) s_tabSB[index5];
            var num4 = (byte) ((uint) num3 << 1);
            if ((num3 & 128) != 0)
                num4 ^= 27;
            var x1 = (uint) (((num3 ^ num4) << 24) | (num3 << 16) | (num3 << 8)) | num4;
            s_tabFT[0, index5] = x1;
            var x2 = upr(x1);
            s_tabFT[1, index5] = x2;
            var x3 = upr(x2);
            s_tabFT[2, index5] = x3;
            var num5 = upr(x3);
            s_tabFT[3, index5] = num5;
            uint x4 = 0;
            var index6 = (byte) s_tabIB[index5];
            if (index6 != 0)
                x4 = (uint) ((numArray2[(numArray1[index6] + 104) % byte.MaxValue] << 24) ^ (numArray2[(numArray1[index6] + 238) % byte.MaxValue] << 16) ^ (numArray2[(numArray1[index6] + 199) % byte.MaxValue] << 8)) ^
                     numArray2[(numArray1[index6] + 223) % byte.MaxValue];
            s_tabIT[0, index5] = x4;
            var x5 = upr(x4);
            s_tabIT[1, index5] = x5;
            var x6 = upr(x5);
            s_tabIT[2, index5] = x6;
            var num6 = upr(x6);
            s_tabIT[3, index5] = num6;
        }
    }

    private uint bKs(int index)
    {
        var k = m_Ks[index / 4];
        return (index % 4) switch
        {
            0 => k & byte.MaxValue,
            1 => (k >> 8) & byte.MaxValue,
            2 => (k >> 16) & byte.MaxValue,
            3 => (k >> 24) & byte.MaxValue,
            _ => 0
        };
    }

    private void EncodeKey(byte[] key, int szKey)
    {
        if (szKey > 256)
            return;
        if (s_tabSB[0] == 0U)
            GenerateTables();
        for (var index = 0; index < szKey / 32; ++index)
            m_Ks[index] = BitConverter.ToUInt32(key, index * 4);
        byte num1 = 1;
        int num2;
        switch (szKey)
        {
            case 128:
                for (var index = 0; index < 2; ++index)
                {
                    m_Ks[index * 20 + 4] = (uint) ((((((int) s_tabSB[(int) bKs(index * 80 + 12)] << 8) ^ (int) s_tabSB[(int) bKs(index * 80 + 15)]) << 8) ^ (int) s_tabSB[(int) bKs(index * 80 + 14)]) << 8) ^ s_tabSB[(int) bKs(index * 80 + 13)] ^
                                           m_Ks[index * 20] ^ num1;
                    m_Ks[index * 20 + 5] = m_Ks[index * 20 + 1] ^ m_Ks[index * 20 + 4];
                    m_Ks[index * 20 + 6] = m_Ks[index * 20 + 1] ^ m_Ks[index * 20 + 2] ^ m_Ks[index * 20 + 4];
                    m_Ks[index * 20 + 7] = m_Ks[index * 20 + 3] ^ m_Ks[index * 20 + 6];
                    var num3 = (byte) (((uint) num1 << 1) ^ ((num1 & 128) != 0 ? 27L : 0L));
                    m_Ks[index * 20 + 8] = (uint) ((((((int) s_tabSB[(int) bKs(index * 80 + 28)] << 8) ^ (int) s_tabSB[(int) bKs(index * 80 + 31)]) << 8) ^ (int) s_tabSB[(int) bKs(index * 80 + 30)]) << 8) ^ s_tabSB[(int) bKs(index * 80 + 29)] ^
                                           m_Ks[index * 20 + 4] ^ num3;
                    m_Ks[index * 20 + 9] = m_Ks[index * 20 + 5] ^ m_Ks[index * 20 + 8];
                    m_Ks[index * 20 + 10] = m_Ks[index * 20 + 5] ^ m_Ks[index * 20 + 6] ^ m_Ks[index * 20 + 8];
                    m_Ks[index * 20 + 11] = m_Ks[index * 20 + 7] ^ m_Ks[index * 20 + 10];
                    var num4 = (byte) (((uint) num3 << 1) ^ ((num3 & 128) != 0 ? 27L : 0L));
                    m_Ks[index * 20 + 12] = (uint) ((((((int) s_tabSB[(int) bKs(index * 80 + 44)] << 8) ^ (int) s_tabSB[(int) bKs(index * 80 + 47)]) << 8) ^ (int) s_tabSB[(int) bKs(index * 80 + 46)]) << 8) ^ s_tabSB[(int) bKs(index * 80 + 45)] ^
                                            m_Ks[index * 20 + 8] ^ num4;
                    m_Ks[index * 20 + 13] = m_Ks[index * 20 + 9] ^ m_Ks[index * 20 + 12];
                    m_Ks[index * 20 + 14] = m_Ks[index * 20 + 9] ^ m_Ks[index * 20 + 10] ^ m_Ks[index * 20 + 12];
                    m_Ks[index * 20 + 15] = m_Ks[index * 20 + 11] ^ m_Ks[index * 20 + 14];
                    var num5 = (byte) (((uint) num4 << 1) ^ ((num4 & 128) != 0 ? 27L : 0L));
                    m_Ks[index * 20 + 16] = (uint) ((((((int) s_tabSB[(int) bKs(index * 80 + 60)] << 8) ^ (int) s_tabSB[(int) bKs(index * 80 + 63)]) << 8) ^ (int) s_tabSB[(int) bKs(index * 80 + 62)]) << 8) ^ s_tabSB[(int) bKs(index * 80 + 61)] ^
                                            m_Ks[index * 20 + 12] ^ num5;
                    m_Ks[index * 20 + 17] = m_Ks[index * 20 + 13] ^ m_Ks[index * 20 + 16];
                    m_Ks[index * 20 + 18] = m_Ks[index * 20 + 13] ^ m_Ks[index * 20 + 14] ^ m_Ks[index * 20 + 16];
                    m_Ks[index * 20 + 19] = m_Ks[index * 20 + 15] ^ m_Ks[index * 20 + 18];
                    var num6 = (byte) (((uint) num5 << 1) ^ ((num5 & 128) != 0 ? 27L : 0L));
                    m_Ks[index * 20 + 20] = (uint) ((((((int) s_tabSB[(int) bKs(index * 80 + 76)] << 8) ^ (int) s_tabSB[(int) bKs(index * 80 + 79)]) << 8) ^ (int) s_tabSB[(int) bKs(index * 80 + 78)]) << 8) ^ s_tabSB[(int) bKs(index * 80 + 77)] ^
                                            m_Ks[index * 20 + 16] ^ num6;
                    m_Ks[index * 20 + 21] = m_Ks[index * 20 + 17] ^ m_Ks[index * 20 + 20];
                    m_Ks[index * 20 + 22] = m_Ks[index * 20 + 17] ^ m_Ks[index * 20 + 18] ^ m_Ks[index * 20 + 20];
                    m_Ks[index * 20 + 23] = m_Ks[index * 20 + 19] ^ m_Ks[index * 20 + 22];
                    num1 = (byte) (((uint) num6 << 1) ^ ((num6 & 128) != 0 ? 27L : 0L));
                }

                m_nCipherRnd = 10;
                num2 = 160;
                break;
            case 196:
                for (var index = 0; index < 2; ++index)
                {
                    m_Ks[index * 24 + 6] = (uint) ((((((int) s_tabSB[(int) bKs(index * 96 + 20)] << 8) ^ (int) s_tabSB[(int) bKs(index * 96 + 23)]) << 8) ^ (int) s_tabSB[(int) bKs(index * 96 + 22)]) << 8) ^ s_tabSB[(int) bKs(index * 96 + 21)] ^
                                           m_Ks[index * 24] ^ num1;
                    m_Ks[index * 24 + 7] = m_Ks[index * 24 + 1] ^ m_Ks[index * 24 + 6];
                    m_Ks[index * 24 + 8] = m_Ks[index * 24 + 1] ^ m_Ks[index * 24 + 2] ^ m_Ks[index * 24 + 6];
                    m_Ks[index * 24 + 9] = m_Ks[index * 24 + 3] ^ m_Ks[index * 24 + 8];
                    m_Ks[index * 24 + 10] = m_Ks[index * 24 + 3] ^ m_Ks[index * 24 + 4] ^ m_Ks[index * 24 + 8];
                    m_Ks[index * 24 + 11] = m_Ks[index * 24 + 5] ^ m_Ks[index * 24 + 10];
                    var num7 = (byte) ((uint) num1 << 1);
                    m_Ks[index * 24 + 12] = (uint) ((((((int) s_tabSB[(int) bKs(index * 96 + 44)] << 8) ^ (int) s_tabSB[(int) bKs(index * 96 + 47)]) << 8) ^ (int) s_tabSB[(int) bKs(index * 96 + 46)]) << 8) ^ s_tabSB[(int) bKs(index * 96 + 45)] ^
                                            m_Ks[index * 24 + 6] ^ num7;
                    m_Ks[index * 24 + 13] = m_Ks[index * 24 + 7] ^ m_Ks[index * 24 + 12];
                    m_Ks[index * 24 + 14] = m_Ks[index * 24 + 7] ^ m_Ks[index * 24 + 8] ^ m_Ks[index * 24 + 12];
                    m_Ks[index * 24 + 15] = m_Ks[index * 24 + 9] ^ m_Ks[index * 24 + 14];
                    m_Ks[index * 24 + 16] = m_Ks[index * 24 + 9] ^ m_Ks[index * 24 + 10] ^ m_Ks[index * 24 + 14];
                    m_Ks[index * 24 + 17] = m_Ks[index * 24 + 11] ^ m_Ks[index * 24 + 16];
                    var num8 = (byte) ((uint) num7 << 1);
                    m_Ks[index * 24 + 18] = (uint) ((((((int) s_tabSB[(int) bKs(index * 96 + 68)] << 8) ^ (int) s_tabSB[(int) bKs(index * 96 + 71)]) << 8) ^ (int) s_tabSB[(int) bKs(index * 96 + 70)]) << 8) ^ s_tabSB[(int) bKs(index * 96 + 69)] ^
                                            m_Ks[index * 24 + 12] ^ num8;
                    m_Ks[index * 24 + 19] = m_Ks[index * 24 + 13] ^ m_Ks[index * 24 + 18];
                    m_Ks[index * 24 + 20] = m_Ks[index * 24 + 13] ^ m_Ks[index * 24 + 14] ^ m_Ks[index * 24 + 18];
                    m_Ks[index * 24 + 21] = m_Ks[index * 24 + 15] ^ m_Ks[index * 24 + 20];
                    m_Ks[index * 24 + 22] = m_Ks[index * 24 + 15] ^ m_Ks[index * 24 + 16] ^ m_Ks[index * 24 + 20];
                    m_Ks[index * 24 + 23] = m_Ks[index * 24 + 17] ^ m_Ks[index * 24 + 22];
                    var num9 = (byte) ((uint) num8 << 1);
                    m_Ks[index * 24 + 24] = (uint) ((((((int) s_tabSB[(int) bKs(index * 96 + 92)] << 8) ^ (int) s_tabSB[(int) bKs(index * 96 + 95)]) << 8) ^ (int) s_tabSB[(int) bKs(index * 96 + 94)]) << 8) ^ s_tabSB[(int) bKs(index * 96 + 93)] ^
                                            m_Ks[index * 24 + 18] ^ num9;
                    m_Ks[index * 24 + 25] = m_Ks[index * 24 + 19] ^ m_Ks[index * 24 + 18];
                    m_Ks[index * 24 + 26] = m_Ks[index * 24 + 19] ^ m_Ks[index * 24 + 20] ^ m_Ks[index * 24 + 18];
                    m_Ks[index * 24 + 27] = m_Ks[index * 24 + 21] ^ m_Ks[index * 24 + 26];
                    m_Ks[index * 24 + 28] = m_Ks[index * 24 + 21] ^ m_Ks[index * 24 + 22] ^ m_Ks[index * 24 + 26];
                    m_Ks[index * 24 + 29] = m_Ks[index * 24 + 23] ^ m_Ks[index * 24 + 28];
                    num1 = (byte) ((uint) num9 << 1);
                }

                m_nCipherRnd = 12;
                num2 = 192;
                break;
            case 256:
                for (var index = 0; index < 7; ++index)
                {
                    m_Ks[index * 8 + 8] = (uint) ((((((int) s_tabSB[(int) bKs(index * 32 + 28)] << 8) ^ (int) s_tabSB[(int) bKs(index * 32 + 31)]) << 8) ^ (int) s_tabSB[(int) bKs(index * 32 + 30)]) << 8) ^ s_tabSB[(int) bKs(index * 32 + 29)] ^
                                          m_Ks[index * 8] ^ num1;
                    m_Ks[index * 8 + 9] = m_Ks[index * 8 + 1] ^ m_Ks[index * 8 + 8];
                    m_Ks[index * 8 + 10] = m_Ks[index * 8 + 1] ^ m_Ks[index * 8 + 2] ^ m_Ks[index * 8 + 8];
                    m_Ks[index * 8 + 11] = m_Ks[index * 8 + 3] ^ m_Ks[index * 8 + 10];
                    num1 <<= 1;
                    m_Ks[index * 8 + 12] = (uint) ((((((int) s_tabSB[(int) bKs(index * 32 + 44)] << 8) ^ (int) s_tabSB[(int) bKs(index * 32 + 47)]) << 8) ^ (int) s_tabSB[(int) bKs(index * 32 + 46)]) << 8) ^ s_tabSB[(int) bKs(index * 32 + 45)] ^
                                           m_Ks[index * 8 + 4];
                    m_Ks[index * 8 + 13] = m_Ks[index * 8 + 5] ^ m_Ks[index * 8 + 12];
                    m_Ks[index * 8 + 14] = m_Ks[index * 8 + 5] ^ m_Ks[index * 8 + 6] ^ m_Ks[index * 8 + 12];
                    m_Ks[index * 8 + 15] = m_Ks[index * 8 + 7] ^ m_Ks[index * 8 + 14];
                }

                m_nCipherRnd = 14;
                num2 = 224;
                break;
            default:
                m_nCipherRnd = 0;
                return;
        }

        m_Ke[0] = m_Ks[num2 / 4];
        m_Ke[1] = m_Ks[num2 / 4 + 1];
        m_Ke[2] = m_Ks[num2 / 4 + 2];
        m_Ke[3] = m_Ks[num2 / 4 + 3];
        var num10 = 0;
        var num11 = (m_nCipherRnd - 1) * 4;
        while (num11 > 0)
        {
            num2 += ((num11 & 3) != 0 ? 1 : -7) * 4;
            m_Ke[num10 + 4] = s_tabIT[3, (int) s_tabSB[(int) bKs(num2 + 15)]] ^ s_tabIT[2, (int) s_tabSB[(int) bKs(num2 + 14)]] ^ s_tabIT[1, (int) s_tabSB[(int) bKs(num2 + 13)]] ^ s_tabIT[0, (int) s_tabSB[(int) bKs(num2 + 12)]];
            --num11;
            ++num10;
        }

        m_Ke[num10 + 4] = m_Ks[num2 / 4 - 4];
        m_Ke[num10 + 5] = m_Ks[num2 / 4 - 3];
        m_Ke[num10 + 6] = m_Ks[num2 / 4 - 2];
        m_Ke[num10 + 7] = m_Ks[num2 / 4 - 1];
    }

    public byte[] EncryptData(byte[] data, byte[] key)
    {
        EncodeKey(key, key.Length * 8);
        var destinationArray = new byte[(data.Length + 15) & -16];
        var numArray = new byte[16];
        var destinationIndex = 0;
        var num = 0;
        while (num < data.Length / 16)
        {
            for (var index = 0; index < 16; ++index)
                numArray[index] ^= data[destinationIndex + index];
            numArray = EncryptBlock(numArray);
            Array.Copy(numArray, 0, destinationArray, destinationIndex, 16);
            ++num;
            destinationIndex += 16;
        }

        if ((data.Length & 15) != 0)
        {
            for (var index = 0; index < (data.Length & 15); ++index)
                numArray[index] ^= data[destinationIndex + index];
            Array.Copy(EncryptBlock(numArray), 0, destinationArray, destinationIndex, 16);
        }

        return destinationArray;
    }

    public byte[] DecryptData(byte[] data, byte[] key)
    {
        EncodeKey(key, key.Length * 8);
        var destinationArray1 = new byte[data.Length];
        var destinationArray2 = new byte[16];
        var destinationArray3 = new byte[16];
        var numArray1 = new byte[16];
        var numArray2 = new byte[16];
        var sourceArray = new byte[16];
        var num1 = 0;
        var num2 = data.Length / 16;
        while (num2 > 0)
        {
            if ((num2 & 1) == 0)
            {
                Array.Copy(data, num1, destinationArray2, 0, 16);
                Array.Copy(data, num1, numArray1, 0, 16);
                numArray1 = DecryptBlock(numArray1);
                for (var index = 0; index < 16; ++index)
                    sourceArray[index] = (byte) (destinationArray3[index] ^ (uint) numArray1[index]);
            }
            else
            {
                Array.Copy(data, num1, destinationArray3, 0, 16);
                Array.Copy(data, num1, numArray2, 0, 16);
                numArray2 = DecryptBlock(numArray2);
                for (var index = 0; index < 16; ++index)
                    sourceArray[index] = (byte) (destinationArray2[index] ^ (uint) numArray2[index]);
            }

            Array.Copy(sourceArray, 0, destinationArray1, num1, 16);
            --num2;
            num1 += 16;
        }

        return destinationArray1;
    }
}
