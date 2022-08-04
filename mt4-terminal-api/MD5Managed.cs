namespace TradingAPI.MT4Server;

internal class MD5Managed
{
    private const int S11 = 7;
    private const int S12 = 12;
    private const int S13 = 17;
    private const int S14 = 22;
    private const int S21 = 5;
    private const int S22 = 9;
    private const int S23 = 14;
    private const int S24 = 20;
    private const int S31 = 4;
    private const int S32 = 11;
    private const int S33 = 16;
    private const int S34 = 23;
    private const int S41 = 6;
    private const int S42 = 10;
    private const int S43 = 15;
    private const int S44 = 21;
    private static readonly byte[] PADDING = new byte[64];
    private readonly MD5_CTX _context = new();
    private readonly byte[] _digest = new byte[16];
    private bool _hashCoreCalled;
    private bool _hashFinalCalled;

    static MD5Managed() => PADDING[0] = 128;

    public MD5Managed()
    {
        InitializeVariables(true);
    }

    public byte[] Hash
    {
        get
        {
            if (!_hashCoreCalled)
                throw new NullReferenceException();
            if (!_hashFinalCalled)
                throw new Exception("Hash must be finalized before the hash value is retrieved.");
            return _digest;
        }
    }

    public int HashSize => _digest.Length * 8;

    public void Initialize(bool clearConetext)
    {
        InitializeVariables(clearConetext);
    }

    private void InitializeVariables(bool clearConetext)
    {
        MD5Init(_context, clearConetext);
        _hashCoreCalled = false;
        _hashFinalCalled = false;
    }

    public void HashCore(byte[] array, int ibStart, int cbSize)
    {
        if (array == null)
            throw new ArgumentNullException(nameof(array));
        if (_hashFinalCalled)
            throw new Exception("Hash not valid for use in specified state.");
        _hashCoreCalled = true;
        MD5Update(_context, array, (uint) ibStart, (uint) cbSize);
    }

    public byte[] HashFinal()
    {
        _hashFinalCalled = true;
        MD5Final(_digest, _context);
        return Hash;
    }

    private static uint F(uint x, uint y, uint z) => (uint) (((int) x & (int) y) | (~(int) x & (int) z));

    private static uint G(uint x, uint y, uint z) => (uint) (((int) x & (int) z) | ((int) y & ~(int) z));

    private static uint H(uint x, uint y, uint z) => x ^ y ^ z;

    private static uint I(uint x, uint y, uint z) => y ^ (x | ~z);

    private static uint ROTATE_LEFT(uint x, int n) => (x << n) | (x >> (32 - n));

    private static void FF(ref uint a, uint b, uint c, uint d, uint x, int s, uint ac)
    {
        a += F(b, c, d) + x + ac;
        a = ROTATE_LEFT(a, s);
        a += b;
    }

    private static void GG(ref uint a, uint b, uint c, uint d, uint x, int s, uint ac)
    {
        a += G(b, c, d) + x + ac;
        a = ROTATE_LEFT(a, s);
        a += b;
    }

    private static void HH(ref uint a, uint b, uint c, uint d, uint x, int s, uint ac)
    {
        a += H(b, c, d) + x + ac;
        a = ROTATE_LEFT(a, s);
        a += b;
    }

    private static void II(ref uint a, uint b, uint c, uint d, uint x, int s, uint ac)
    {
        a += I(b, c, d) + x + ac;
        a = ROTATE_LEFT(a, s);
        a += b;
    }

    private static void MD5Init(MD5_CTX context, bool clearConetext)
    {
        context.count[0] = context.count[1] = 0U;
        if (!clearConetext)
            return;
        context.state[0] = 1732584193U;
        context.state[1] = 4023233417U;
        context.state[2] = 2562383102U;
        context.state[3] = 271733878U;
    }

    private static void MD5Update(
        MD5_CTX context,
        byte[] input,
        uint inputIndex,
        uint inputLen)
    {
        var dstOffset = (context.count[0] >> 3) & 63U;
        if ((context.count[0] += inputLen << 3) < inputLen << 3)
            ++context.count[1];
        context.count[1] += inputLen >> 29;
        var count = 64U - dstOffset;
        uint num = 0;
        if (inputLen >= count)
        {
            Buffer.BlockCopy(input, (int) inputIndex, context.buffer, (int) dstOffset, (int) count);
            MD5Transform(context.state, context.buffer, 0U);
            for (num = count; num + 63U < inputLen; num += 64U)
                MD5Transform(context.state, input, inputIndex + num);
            dstOffset = 0U;
        }

        Buffer.BlockCopy(input, (int) inputIndex + (int) num, context.buffer, (int) dstOffset, (int) inputLen - (int) num);
    }

    private static void MD5Final(byte[] digest, MD5_CTX context)
    {
        var numArray = new byte[8];
        Encode(numArray, context.count, 8U);
        var num = (context.count[0] >> 3) & 63U;
        var inputLen = num < 56U ? 56U - num : 120U - num;
        MD5Update(context, PADDING, 0U, inputLen);
        MD5Update(context, numArray, 0U, 8U);
        Encode(digest, context.state, 16U);
    }

    private static void MD5Transform(uint[] state, byte[] block, uint blockIndex)
    {
        var a1 = state[0];
        var a2 = state[1];
        var a3 = state[2];
        var a4 = state[3];
        var output = new uint[16];
        Decode(output, block, blockIndex, 64U);
        FF(ref a1, a2, a3, a4, output[0], 7, 3614090360U);
        FF(ref a4, a1, a2, a3, output[1], 12, 3905402710U);
        FF(ref a3, a4, a1, a2, output[2], 17, 606105819U);
        FF(ref a2, a3, a4, a1, output[3], 22, 3250441966U);
        FF(ref a1, a2, a3, a4, output[4], 7, 4118548399U);
        FF(ref a4, a1, a2, a3, output[5], 12, 1200080426U);
        FF(ref a3, a4, a1, a2, output[6], 17, 2821735955U);
        FF(ref a2, a3, a4, a1, output[7], 22, 4249261313U);
        FF(ref a1, a2, a3, a4, output[8], 7, 1770035416U);
        FF(ref a4, a1, a2, a3, output[9], 12, 2336552879U);
        FF(ref a3, a4, a1, a2, output[10], 17, 4294925233U);
        FF(ref a2, a3, a4, a1, output[11], 22, 2304563134U);
        FF(ref a1, a2, a3, a4, output[12], 7, 1804603682U);
        FF(ref a4, a1, a2, a3, output[13], 12, 4254626195U);
        FF(ref a3, a4, a1, a2, output[14], 17, 2792965006U);
        FF(ref a2, a3, a4, a1, output[15], 22, 1236535329U);
        GG(ref a1, a2, a3, a4, output[1], 5, 4129170786U);
        GG(ref a4, a1, a2, a3, output[6], 9, 3225465664U);
        GG(ref a3, a4, a1, a2, output[11], 14, 643717713U);
        GG(ref a2, a3, a4, a1, output[0], 20, 3921069994U);
        GG(ref a1, a2, a3, a4, output[5], 5, 3593408605U);
        GG(ref a4, a1, a2, a3, output[10], 9, 38016083U);
        GG(ref a3, a4, a1, a2, output[15], 14, 3634488961U);
        GG(ref a2, a3, a4, a1, output[4], 20, 3889429448U);
        GG(ref a1, a2, a3, a4, output[9], 5, 568446438U);
        GG(ref a4, a1, a2, a3, output[14], 9, 3275163606U);
        GG(ref a3, a4, a1, a2, output[3], 14, 4107603335U);
        GG(ref a2, a3, a4, a1, output[8], 20, 1163531501U);
        GG(ref a1, a2, a3, a4, output[13], 5, 2850285829U);
        GG(ref a4, a1, a2, a3, output[2], 9, 4243563512U);
        GG(ref a3, a4, a1, a2, output[7], 14, 1735328473U);
        GG(ref a2, a3, a4, a1, output[12], 20, 2368359562U);
        HH(ref a1, a2, a3, a4, output[5], 4, 4294588738U);
        HH(ref a4, a1, a2, a3, output[8], 11, 2272392833U);
        HH(ref a3, a4, a1, a2, output[11], 16, 1839030562U);
        HH(ref a2, a3, a4, a1, output[14], 23, 4259657740U);
        HH(ref a1, a2, a3, a4, output[1], 4, 2763975236U);
        HH(ref a4, a1, a2, a3, output[4], 11, 1272893353U);
        HH(ref a3, a4, a1, a2, output[7], 16, 4139469664U);
        HH(ref a2, a3, a4, a1, output[10], 23, 3200236656U);
        HH(ref a1, a2, a3, a4, output[13], 4, 681279174U);
        HH(ref a4, a1, a2, a3, output[0], 11, 3936430074U);
        HH(ref a3, a4, a1, a2, output[3], 16, 3572445317U);
        HH(ref a2, a3, a4, a1, output[6], 23, 76029189U);
        HH(ref a1, a2, a3, a4, output[9], 4, 3654602809U);
        HH(ref a4, a1, a2, a3, output[12], 11, 3873151461U);
        HH(ref a3, a4, a1, a2, output[15], 16, 530742520U);
        HH(ref a2, a3, a4, a1, output[2], 23, 3299628645U);
        II(ref a1, a2, a3, a4, output[0], 6, 4096336452U);
        II(ref a4, a1, a2, a3, output[7], 10, 1126891415U);
        II(ref a3, a4, a1, a2, output[14], 15, 2878612391U);
        II(ref a2, a3, a4, a1, output[5], 21, 4237533241U);
        II(ref a1, a2, a3, a4, output[12], 6, 1700485571U);
        II(ref a4, a1, a2, a3, output[3], 10, 2399980690U);
        II(ref a3, a4, a1, a2, output[10], 15, 4293915773U);
        II(ref a2, a3, a4, a1, output[1], 21, 2240044497U);
        II(ref a1, a2, a3, a4, output[8], 6, 1873313359U);
        II(ref a4, a1, a2, a3, output[15], 10, 4264355552U);
        II(ref a3, a4, a1, a2, output[6], 15, 2734768916U);
        II(ref a2, a3, a4, a1, output[13], 21, 1309151649U);
        II(ref a1, a2, a3, a4, output[4], 6, 4149444226U);
        II(ref a4, a1, a2, a3, output[11], 10, 3174756917U);
        II(ref a3, a4, a1, a2, output[2], 15, 718787259U);
        II(ref a2, a3, a4, a1, output[9], 21, 3951481745U);
        state[0] += a1;
        state[1] += a2;
        state[2] += a3;
        state[3] += a4;
        Array.Clear(output, 0, output.Length);
    }

    private static void Encode(byte[] output, uint[] input, uint len)
    {
        uint index1 = 0;
        for (uint index2 = 0; index2 < len; index2 += 4U)
        {
            output[(int) index2] = (byte) (input[(int) index1] & byte.MaxValue);
            output[(int) index2 + 1] = (byte) ((input[(int) index1] >> 8) & byte.MaxValue);
            output[(int) index2 + 2] = (byte) ((input[(int) index1] >> 16) & byte.MaxValue);
            output[(int) index2 + 3] = (byte) ((input[(int) index1] >> 24) & byte.MaxValue);
            ++index1;
        }
    }

    private static void Decode(uint[] output, byte[] input, uint inputIndex, uint len)
    {
        uint index1 = 0;
        for (uint index2 = 0; index2 < len; index2 += 4U)
        {
            output[(int) index1] = (uint) (input[(int) inputIndex + (int) index2] | (input[(int) inputIndex + (int) index2 + 1] << 8) | (input[(int) inputIndex + (int) index2 + 2] << 16) | (input[(int) inputIndex + (int) index2 + 3] << 24));
            ++index1;
        }
    }

    private class MD5_CTX
    {
        public readonly byte[] buffer;
        public readonly uint[] count;
        public readonly uint[] state;

        public MD5_CTX()
        {
            state = new uint[4];
            count = new uint[2];
            buffer = new byte[64];
        }

        public void Clear()
        {
            Array.Clear(state, 0, state.Length);
            Array.Clear(count, 0, count.Length);
            Array.Clear(buffer, 0, buffer.Length);
        }
    }
}
