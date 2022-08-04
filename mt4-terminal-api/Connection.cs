using System.Security.Cryptography;
using System.Text;

namespace TradingAPI.MT4Server;

internal class Connection : SecureSocket
{
    private readonly bool IsProxy;
    internal readonly string ProxyHost;
    internal readonly string ProxyPass;
    internal readonly int ProxyPort;
    internal readonly ProxyTypes ProxyType;
    internal readonly string ProxyUser;
    internal readonly QuoteClient QC;
    public uint _ClientExeSize = 2453322706;
    private ulong _DataLoginId;
    private byte[] _HashKey;
    private ulong _LoginId;
    private uint _Seed;
    public byte[] _TransactionKey1 = new byte[16];
    public byte[] _TransactionKey2 = new byte[16];

    public static class Buster
    {
        private static int _next = 1280;
        public static ushort Next => (ushort) Interlocked.Increment(ref _next);
    }

    public Connection(QuoteClient qc)
    {
        IsProxy = false;
        QC = qc;
        //CurrentBuild = Buster.Next;
    }

    public Connection(
        string proxyHost,
        int proxyPort,
        string proxyUser,
        string proxyPass,
        ProxyTypes type,
        QuoteClient qc)
    {
        ProxyHost = proxyHost;
        ProxyPort = proxyPort;
        ProxyUser = proxyUser;
        ProxyPass = proxyPass;
        ProxyType = type;
        IsProxy = true;
        QC = qc;
    }

    public ushort CurrentBuild { get; } = 1357;

    public ushort ServerVersion { get; set; }

    public ushort ServerBuild { get; set; }

    public uint Session { get; set; }

    public int Account { get; set; }

    public void Connect(ref string reason)
    {
        try
        {
            if (IsProxy)
                Connect(QC.Host, QC.Port, ProxyHost, ProxyPort, ProxyUser, ProxyPass, ProxyType);
            else
                Connect(QC.Host, QC.Port, ref reason);
        }
        catch (Exception)
        {
            Close();
            throw;
        }
    }

    public Connection ConnectAndLogin(bool bDataCenter, Logger log, ref string reason)
    {
        try
        {
            if (IsProxy)
                Connect(QC.Host, QC.Port, ProxyHost, ProxyPort, ProxyUser, ProxyPass, ProxyType);
            else
                Connect(QC.Host, QC.Port, ref reason);
            LoginServer(bDataCenter, log, ref reason);
        }
        catch (Exception)
        {
            Close();
            throw;
        }

        return this;
    }

    public void Ping()
    {
        SendEncode(new byte[1]
        {
            2
        });
    }

    public byte[] ReceiveSymbols()
    {
        var buf = new byte[17];
        buf[0] = 8;
        SendEncode(buf);
        var decode = ReceiveDecode(1);
        if (decode[0] == 0)
            return ReceiveCopmressed();
        throw new ServerException(decode[0]);
    }

    public byte[] ReceiveGroups()
    {
        var buf = new byte[17];
        buf[0] = 10;
        SendEncode(buf);
        var decode = ReceiveDecode(1);
        if (decode[0] == 0)
            return ReceiveCopmressed();
        throw new Exception($"Server sent non zero reply {decode[0]:X}");
    }

    public void Disconnect()
    {
        SendEncode(new byte[1] {13});
        Thread.Sleep(100);
        Close();
    }

    public byte[] ReceiveServersList(out int is_demo)
    {
        SendEncode(new byte[1] {16});
        var decode1 = ReceiveDecode(1);
        if (decode1[0] != 0)
            throw new Exception($"Server sent non zero reply {decode1[0]:X}");
        var decode2 = ReceiveDecode(1);
        is_demo = decode2[0];
        return ReceiveCopmressed();
    }

    public byte[] ReceiveAccount()
    {
        var buf = new byte[77];
        buf[0] = 30;
        BitConverter.GetBytes(ConvertTo.Long(DateTime.Now)).CopyTo(buf, 5);
        SendEncode(buf);
        var decode = ReceiveDecode(1);
        if (decode[0] == 0)
            return ReceiveCopmressed();
        throw new Exception($"Server sent non zero reply {decode[0]:X}");
    }

    public byte[] ReceiveMailHistory()
    {
        SendEncode(new byte[1] {33});
        if (ReceiveDecode(1)[0] != 0)
            return null;
        var int32 = BitConverter.ToInt32(ReceiveDecode(4), 0);
        return int32 is < 1 or > 128 ? null : ReceiveDecode(int32 * 68);
    }

    public void ChangePassword(string password, bool bInvestor)
    {
        var buf = new byte[21];
        buf[0] = 4;
        buf[1] = bInvestor ? (byte) 1 : (byte) 0;
        Encoding.ASCII.GetBytes(password).CopyTo(buf, 5);
        SendEncode(buf);
        var decode = ReceiveDecode(1);
        if (decode[0] != 0)
            throw new Exception($"Server sent non zero reply {decode[0]:X}");
    }

    private byte[] CreateLoginRequest(byte[] key, bool bDataCenter)
    {
        var loginRequest = new byte[28];
        var numArray1 = MT4Crypt.Decode(key, key);
        ushort num = 400;
        loginRequest[0] = bDataCenter ? (byte) 11 : (byte) 0;
        loginRequest[1] = (byte) (loginRequest[0] + (uint) QC.User + CurrentBuild + loginRequest[27]);
        numArray1.CopyTo(loginRequest, 3);
        BitConverter.GetBytes(QC.User).CopyTo(loginRequest, 19);
        BitConverter.GetBytes(num).CopyTo(loginRequest, 23);
        BitConverter.GetBytes(CurrentBuild).CopyTo(loginRequest, 25);
        return loginRequest;
    }

    private void LoginServer(bool bDataCenter, Logger log, ref string reason)
    {
        reason += ", sending login";
        log.trace("Sending login");
        var cryptoServiceProvider = new MD5CryptoServiceProvider();
        var str = QC.Pass;
        if (bDataCenter)
            str = "a61D3f6-j65D49ed";
        else if (QC.Pass.Length > 15)
            str = str.Substring(0, 15);
        var bytes = Encoding.ASCII.GetBytes(str);
        var hash = cryptoServiceProvider.ComputeHash(bytes);
        SetNewKey(hash);
        SendEnrypt(CreateLoginRequest(hash, bDataCenter));
        var numArray1 = Receive(1);
        if (numArray1[0] != 0)
            throw new ServerException(numArray1[0]);
        log.trace($"CurrentBuild - {CurrentBuild}");
        ResetDecoder();
        ResetEncoder();
        ReceiveDecode(1);
        ServerBuild = BitConverter.ToUInt16(ReceiveDecode(2), 0);
        log.trace($"ServerBuild - {ServerBuild}");
        Session = BitConverter.ToUInt32(ReceiveDecode(4), 0);
        Account = QC.User;
        if (ServerBuild < 1010)
        {
            log.trace($"{Account} old version of server");
        }
        else
        {
            if (bDataCenter || Session == 0U)
                return;
            var decode = ReceiveDecode(64);
            var num = (uint) ((ulong) ((sbyte) hash[3] + (sbyte) hash[2] + (sbyte) hash[1] + (sbyte) hash[0]) + ((Session >> 8) & byte.MaxValue) + (ulong) Account + CurrentBuild);
            var numArray2 = hash;
            reason += ", modify hash";
            for (var index = 0; index < 8; ++index)
            {
                num = (uint) ((int) num * 214013 + 2531011);
                numArray2 = ((int) (num >> 16) & 15) switch
                {
                    0 => ModifyKeySHA256_MTComplex(decode, numArray2),
                    1 => ModifyKeySHA256_MTLogin(numArray2),
                    2 => ModifyKeySHA256_MTType(numArray2),
                    3 => ModifyKeyMD5_MTRandom(decode, numArray2),
                    4 => ModifyKeySwapPair(numArray2),
                    5 => ModifyKeySHA1_MTType(numArray2),
                    6 => ModifyKeyNotEven(numArray2),
                    7 => ModifyKeyNotOdd(numArray2),
                    8 => ModifyKeySHA256_MTBuild(numArray2),
                    9 => ModifyKeyCryptKey(numArray2),
                    10 => ModifyKeySwapAllBytes(numArray2),
                    11 => ModifyKeyMD5_MTComplex(decode, numArray2),
                    12 => ModifyKeySHA256_MTRandom(decode, numArray2),
                    13 => ModifyKeyNotBytes(numArray2),
                    14 => ModifyKeySHA1_MTComplex(decode, numArray2),
                    15 => ModifyKeySHA256_MTSession(numArray2),
                    _ => numArray2
                };
            }

            reason += ", login ID";
            LoginIdentification(numArray2, ref reason);
            SetNewKey(numArray2);
            reason += ", terminal identification";
            TerminalIdentification(numArray2);
        }
    }

    private byte[] ModifyKeyCryptKey(byte[] hashKey)
    {
        byte num = 0;
        var numArray = new byte[16];
        for (var index = 0; index < 16; ++index)
        {
            numArray[index] = (byte) (hashKey[index] ^ (MT4Crypt.CryptKey[index] + (uint) num));
            num = numArray[index];
        }

        return numArray;
    }

    private byte[] ModifyKeySwapAllBytes(byte[] hashKey)
    {
        var numArray = new byte[16];
        for (var index = 0; index < 16; ++index)
            numArray[index] = hashKey[15 - index];
        return numArray;
    }

    private byte[] ModifyKeyMD5_MTComplex(byte[] rcvData, byte[] hashKey)
    {
        var buffer = new byte[97];
        buffer[0] = 0;
        Encoding.ASCII.GetBytes("MTComplex").CopyTo(buffer, 1);
        BitConverter.GetBytes(CurrentBuild).CopyTo(buffer, 11);
        hashKey.CopyTo(buffer, 13);
        rcvData.CopyTo(buffer, 29);
        BitConverter.GetBytes(Account).CopyTo(buffer, 93);
        return new MD5CryptoServiceProvider().ComputeHash(buffer);
    }

    private byte[] ModifyKeyMD5_MTRandom(byte[] rcvData, byte[] hashKey)
    {
        var buffer = new byte[89];
        hashKey.CopyTo(buffer, 0);
        rcvData.CopyTo(buffer, 16);
        Encoding.ASCII.GetBytes("MTRandom").CopyTo(buffer, 80);
        return new MD5CryptoServiceProvider().ComputeHash(buffer);
    }

    private byte[] ModifyKeySwapPair(byte[] hashKey)
    {
        var numArray = new byte[16];
        for (var index = 0; index < 16; index += 2)
        {
            numArray[index] = hashKey[index + 1];
            numArray[index + 1] = hashKey[index];
        }

        return numArray;
    }

    private byte[] ModifyKeyNotBytes(byte[] hashKey)
    {
        var numArray = new byte[16];
        for (var index = 0; index < 16; ++index)
            numArray[index] = (byte) ~hashKey[index];
        return numArray;
    }

    private byte[] ModifyKeySHA1_MTComplex(byte[] rcvData, byte[] hashKey)
    {
        var data = new byte[97];
        data[0] = 0;
        BitConverter.GetBytes(CurrentBuild).CopyTo(data, 1);
        rcvData.CopyTo(data, 3);
        hashKey.CopyTo(data, 67);
        BitConverter.GetBytes(Account).CopyTo(data, 83);
        Encoding.ASCII.GetBytes("MTComplex").CopyTo(data, 87);
        return GetKeySHA1(data);
    }

    private byte[] ModifyKeySHA1_MTType(byte[] hashKey)
    {
        var data = new byte[24];
        Encoding.ASCII.GetBytes("MTType").CopyTo(data, 0);
        hashKey.CopyTo(data, 7);
        data[23] = 0;
        return GetKeySHA1(data);
    }

    private byte[] ModifyKeyNotEven(byte[] hashKey)
    {
        var numArray = new byte[16];
        for (var index = 0; index < 16; index += 2)
        {
            numArray[index] = (byte) ~hashKey[index];
            numArray[index + 1] = hashKey[index + 1];
        }

        return numArray;
    }

    private byte[] ModifyKeyNotOdd(byte[] hashKey)
    {
        var numArray = new byte[16];
        for (var index = 0; index < 16; index += 2)
        {
            numArray[index] = hashKey[index];
            numArray[index + 1] = (byte) ~hashKey[index + 1];
        }

        return numArray;
    }

    private byte[] ModifyKeySHA256_MTComplex(byte[] rcvData, byte[] hashKey)
    {
        var data = new byte[97];
        data[0] = 0;
        Encoding.ASCII.GetBytes("MTComplex").CopyTo(data, 1);
        rcvData.CopyTo(data, 11);
        BitConverter.GetBytes(CurrentBuild).CopyTo(data, 75);
        hashKey.CopyTo(data, 77);
        BitConverter.GetBytes(Account).CopyTo(data, 93);
        return GetKeySHA256(data);
    }

    private byte[] ModifyKeySHA256_MTLogin(byte[] hashKey)
    {
        var data = new byte[28];
        BitConverter.GetBytes(Account).CopyTo(data, 0);
        hashKey.CopyTo(data, 4);
        Encoding.ASCII.GetBytes("MTLogin").CopyTo(data, 20);
        return GetKeySHA256(data);
    }

    private byte[] ModifyKeySHA256_MTType(byte[] hashKey)
    {
        var data = new byte[24];
        data[0] = 0;
        hashKey.CopyTo(data, 1);
        Encoding.ASCII.GetBytes("MTType").CopyTo(data, 17);
        return GetKeySHA256(data);
    }

    private byte[] ModifyKeySHA256_MTBuild(byte[] hashKey)
    {
        var data = new byte[26];
        BitConverter.GetBytes(CurrentBuild).CopyTo(data, 0);
        hashKey.CopyTo(data, 2);
        Encoding.ASCII.GetBytes("MTBuild").CopyTo(data, 18);
        return GetKeySHA256(data);
    }

    private byte[] ModifyKeySHA256_MTRandom(byte[] rcvData, byte[] hashKey)
    {
        var data = new byte[89];
        Encoding.ASCII.GetBytes("MTRandom").CopyTo(data, 0);
        hashKey.CopyTo(data, 9);
        rcvData.CopyTo(data, 25);
        return GetKeySHA256(data);
    }

    private byte[] ModifyKeySHA256_MTSession(byte[] hashKey)
    {
        var data = new byte[30];
        BitConverter.GetBytes(Session).CopyTo(data, 0);
        hashKey.CopyTo(data, 4);
        Encoding.ASCII.GetBytes("MTSession").CopyTo(data, 20);
        return GetKeySHA256(data);
    }

    private byte[] GetKeySHA1(byte[] data)
    {
        var hash = new vSHA1().ComputeHash(data);
        var keyShA1 = new byte[16];
        Array.Copy(hash, keyShA1, 16);
        return keyShA1;
    }

    private byte[] GetKeySHA256(byte[] data)
    {
        var hash = SHA256.Create().ComputeHash(data);
        var keyShA256 = new byte[16];
        Array.Copy(hash, keyShA256, 16);
        return keyShA256;
    }

    private void LoginIdentification(byte[] hashKey, ref string reason)
    {
        _LoginId = 0UL;
        if (ServerBuild <= 1101)
            return;
        var hdr1 = new PacketHdr();
        var decode = ReceiveDecode(8);
        hdr1.SizeData = BitConverter.ToUInt16(decode, 0);
        hdr1.PackType = BitConverter.ToUInt16(decode, 2);
        hdr1.DataType = BitConverter.ToUInt16(decode, 4);
        hdr1.Random = BitConverter.ToUInt16(decode, 6);
        var sourceArray = DecryptPacket(ReceiveDecode(hdr1.SizeData), hashKey, hdr1);
        var sourceIndex1 = 0;
        while (sourceIndex1 < sourceArray.Length && sourceArray.Length - sourceIndex1 >= 8)
        {
            var numArray1 = new byte[8];
            Array.Copy(sourceArray, sourceIndex1, numArray1, 0, 8);
            var sourceIndex2 = sourceIndex1 + 8;
            var hdr2 = DecryptPacketHdr(numArray1, hashKey);
            if (sourceArray.Length - sourceIndex2 < hdr2.SizeData)
                break;
            var numArray2 = new byte[hdr2.SizeData];
            Array.Copy(sourceArray, sourceIndex2, numArray2, 0, hdr2.SizeData);
            sourceIndex1 = sourceIndex2 + hdr2.SizeData;
            var data = DecryptPacket(numArray2, hashKey, hdr2);
            switch (hdr2.PackType)
            {
                case 9:
                    _LoginId = new LoginId().Decode(data);
                    _LoginId ^= ((ulong) Account << 32) + Session;
                    _LoginId ^= 371664536245528874UL;
                    break;
                case 11:
                    _DataLoginId = new DataLoginId().Decode(data);
                    reason += ", got data login id";
                    _DataLoginId ^= ((ulong) Account << 32) + Session;
                    _DataLoginId ^= 371664536245528874UL;
                    break;
            }
        }
    }

    private void TerminalIdentification(byte[] hashKey)
    {
        var array = new byte[1];
        _Seed = (uint) ((ulong) Account + Session);
        var rcvData = new byte[64];
        for (var index = 0; index < 64; ++index)
        {
            _Seed = (uint) ((int) _Seed * 214013 + 2531011);
            rcvData[index] = (byte) ((_Seed >> 16) & byte.MaxValue);
        }

        _HashKey = ModifyKeySHA1_MTComplex(rcvData, hashKey);
        var hashKey1 = new byte[_HashKey.Length];
        _HashKey.CopyTo(hashKey1, 0);
        var flag = ServerBuild <= 1101;
        var packCodesArray1 = new PackCodes[9];
        packCodesArray1[0] = DataPacket;
        packCodesArray1[1] = HashPacket;
        var num1 = 2;
        if (!flag)
            packCodesArray1[num1++] = LoginIdPacket;
        var index1 = num1;
        var num2 = index1 + 1;
        PackCodes packCodes1 = ModifyKey;
        packCodesArray1[index1] = packCodes1;
        var num3 = num2 + 1;
        PackCodes packCodes2 = RandomPacket;
        packCodesArray1[num2] = packCodes2;
        var num4 = num3 + 1;
        PackCodes packCodes3 = ModifyKey;
        packCodesArray1[num3] = packCodes3;
        var num5 = num4 + 1;
        PackCodes packCodes4 = RandomPacket;
        packCodesArray1[num4] = packCodes4;
        var num6 = num5 + 1;
        PackCodes packCodes5 = ModifyKey;
        packCodesArray1[num5] = packCodes5;
        PackCodes packCodes6 = RandomPacket;
        packCodesArray1[num6] = packCodes6;
        _Seed = (uint) ((int) _Seed * 214013 + 2531011);
        var num8 = ((int) (_Seed >> 16) & 3) + (flag ? 4 : 5);
        for (var index7 = 0; index7 < 128; ++index7)
        {
            _Seed = (uint) ((int) _Seed * 214013 + 2531011);
            var index8 = ((_Seed >> 16) & (uint) short.MaxValue) % num8;
            _Seed = (uint) ((int) _Seed * 214013 + 2531011);
            var index9 = ((_Seed >> 16) & (uint) short.MaxValue) % num8;
            var packCodes7 = packCodesArray1[index8];
            packCodesArray1[index8] = packCodesArray1[index9];
            packCodesArray1[index9] = packCodes7;
        }

        var index10 = 0;
        for (var index11 = 0; index11 < num8; ++index11)
        {
            var numArray = packCodesArray1[index11]();
            Array.Resize(ref array, index10 + numArray.Length);
            numArray.CopyTo(array, index10);
            index10 += numArray.Length;
        }

        var hdr = new byte[8];
        BitConverter.GetBytes((ushort) array.Length).CopyTo(hdr, 0);
        hdr[2] = 2;
        hdr[4] = 1;
        _Seed = (uint) ((int) _Seed * 214013 + 2531011);
        BitConverter.GetBytes((ushort) ((_Seed >> 16) & (uint) short.MaxValue)).CopyTo(hdr, 6);
        var numArray1 = EncryptPacket(hdr, array, hashKey1);
        var buf = new byte[numArray1.Length + 1];
        buf[0] = 24;
        numArray1.CopyTo(buf, 1);
        Send(buf);
    }

    private byte[] EncryptPacket(byte[] hdr, byte[] data, byte[] hashKey)
    {
        var numArray1 = new byte[data.Length];
        var sourceArray1 = new byte[hdr.Length];
        uint uint16 = BitConverter.ToUInt16(hdr, 0);
        var num = uint16 < 1U ? 0 : (uint) data.Length > 0U ? 1 : 0;
        if (num != 0)
            switch (BitConverter.ToUInt16(hdr, 4))
            {
                case 0:
                    numArray1 = MT4Crypt.Encode(data, hashKey);
                    break;
                case 1:
                    var vAes = new vAES();
                    var length = (int) (uint16 & -16L);
                    var destinationArray1 = new byte[length];
                    Array.Copy(data, destinationArray1, length);
                    var sourceArray2 = vAes.EncryptData(destinationArray1, hashKey);
                    var numArray2 = new byte[uint16 - length];
                    Array.Copy(data, length, numArray2, 0L, uint16 - length);
                    var sourceArray3 = MT4Crypt.Encode(numArray2, hashKey);
                    numArray1 = new byte[sourceArray2.Length + sourceArray3.Length];
                    Array.Copy(sourceArray2, numArray1, sourceArray2.Length);
                    Array.Copy(sourceArray3, 0, numArray1, sourceArray2.Length, sourceArray3.Length);
                    break;
            }

        if (BitConverter.ToUInt16(hdr, 2) == 2)
            hdr.CopyTo(sourceArray1, 0);
        else
            sourceArray1 = MT4Crypt.Encode(hdr, hashKey);
        if (num == 0)
            return sourceArray1;
        var destinationArray2 = new byte[sourceArray1.Length + numArray1.Length];
        Array.Copy(sourceArray1, destinationArray2, sourceArray1.Length);
        Array.Copy(numArray1, 0, destinationArray2, sourceArray1.Length, numArray1.Length);
        return destinationArray2;
    }

    protected PacketHdr DecryptPacketHdr(byte[] data, byte[] key)
    {
        var numArray = MT4Crypt.Decode(data, key);
        return new PacketHdr
        {
            SizeData = BitConverter.ToUInt16(numArray, 0),
            PackType = BitConverter.ToUInt16(numArray, 2),
            DataType = BitConverter.ToUInt16(numArray, 4),
            Random = BitConverter.ToUInt16(numArray, 6)
        };
    }

    protected byte[] DecryptPacket(byte[] data, byte[] key, PacketHdr hdr)
    {
        if (hdr.DataType == 0)
            return MT4Crypt.Decode(data, key);
        if (hdr.DataType != 1)
            return data;
        var vAes = new vAES();
        var length1 = hdr.SizeData & -16;
        var length2 = hdr.SizeData - length1;
        var destinationArray = new byte[length1];
        Array.Copy(data, destinationArray, length1);
        var array = vAes.DecryptData(destinationArray, key);
        var numArray1 = new byte[length2];
        Array.Copy(data, length1, numArray1, 0, length2);
        var numArray2 = MT4Crypt.Decode(numArray1, key);
        var length3 = array.Length;
        Array.Resize(ref array, length3 + numArray2.Length);
        numArray2.CopyTo(array, length3);
        return array;
    }

    private byte[] DataPacket()
    {
        var hdr = new byte[8];
        hdr[0] = 60;
        hdr[4] = 1;
        _Seed = (uint) ((int) _Seed * 214013 + 2531011);
        BitConverter.GetBytes((ushort) ((_Seed >> 16) & (uint) short.MaxValue)).CopyTo(hdr, 6);
        var array = new byte[60];
        MT4Crypt.GetHardId().CopyTo(array, 0);
        BitConverter.GetBytes(_ClientExeSize).CopyTo(array, 20);
        BitConverter.GetBytes(3447060032U).CopyTo(array, 24);
        if (ServerBuild > 1294)
        {
            hdr[0] = 68;
            Array.Resize(ref array, 68);
            BitConverter.GetBytes(4911006713307073302L).CopyTo(array, 60);
        }

        return EncryptPacket(hdr, array, _HashKey);
    }

    private byte[] HashPacket()
    {
        var vShA1_1 = new vSHA1();
        vShA1_1.HashData(MT4Crypt.GetHardId());
        vShA1_1.HashData(BitConverter.GetBytes(Account));
        vShA1_1.HashData(GetKey());
        vShA1_1.HashData(_HashKey);
        var key = vShA1_1.FinalizeHash();
        _HashKey.CopyTo(_TransactionKey1, 0);
        _TransactionKey1 = MT4Crypt.Encode(_TransactionKey1, key);
        _TransactionKey1 = MT4Crypt.Encode(_TransactionKey1, MT4Crypt.GetHardId());
        var vShA1_2 = new vSHA1();
        vShA1_2.HashData(BitConverter.GetBytes(Account));
        vShA1_2.HashData(GetKey());
        vShA1_2.HashData(BitConverter.GetBytes(Session));
        var data = vShA1_2.FinalizeHash();
        var hdr = new byte[8];
        hdr[0] = 20;
        hdr[2] = 1;
        _Seed = (uint) ((int) _Seed * 214013 + 2531011);
        BitConverter.GetBytes((ushort) ((_Seed >> 16) & (uint) short.MaxValue)).CopyTo(hdr, 6);
        return EncryptPacket(hdr, data, _HashKey);
    }

    private byte[] LoginIdPacket()
    {
        var hdr1 = new byte[8];
        hdr1[0] = 8;
        hdr1[2] = 10;
        _Seed = (uint) ((int) _Seed * 214013 + 2531011);
        BitConverter.GetBytes((ushort) ((_Seed >> 16) & (uint) short.MaxValue)).CopyTo(hdr1, 6);
        var data1 = new byte[8];
        BitConverter.GetBytes(_LoginId ^ 371664536245528874UL).CopyTo(data1, 0);
        var array = EncryptPacket(hdr1, data1, _HashKey);
        if (CurrentBuild > 1321)
        {
            var hdr2 = new byte[8];
            hdr2[0] = 8;
            hdr2[2] = 12;
            _Seed = (uint) ((int) _Seed * 214013 + 2531011);
            BitConverter.GetBytes((ushort) ((_Seed >> 16) & (uint) short.MaxValue)).CopyTo(hdr2, 6);
            var data2 = new byte[8];
            BitConverter.GetBytes(_DataLoginId ^ 371664536245528874UL).CopyTo(data2, 0);
            var numArray = EncryptPacket(hdr2, data2, _HashKey);
            var length = array.Length;
            Array.Resize(ref array, array.Length + numArray.Length);
            numArray.CopyTo(array, length);
        }

        return array;
    }

    private byte[] ModifyKey()
    {
        var hdr = new byte[8];
        _Seed = (uint) ((int) _Seed * 214013 + 2531011);
        var length = ((int) (_Seed >> 16) & 31) + 8;
        BitConverter.GetBytes((ushort) length).CopyTo(hdr, 0);
        hdr[2] = 3;
        _Seed = (uint) ((int) _Seed * 214013 + 2531011);
        BitConverter.GetBytes((ushort) ((_Seed >> 16) & (uint) short.MaxValue)).CopyTo(hdr, 6);
        var numArray1 = new byte[length];
        for (var index = 0; index < length; ++index)
        {
            _Seed = (uint) ((int) _Seed * 214013 + 2531011);
            numArray1[index] = (byte) ((_Seed >> 16) & byte.MaxValue);
        }

        var numArray2 = EncryptPacket(hdr, numArray1, _HashKey);
        _HashKey = MT4Crypt.Encode(_HashKey, numArray1);
        return numArray2;
    }

    private byte[] RandomPacket()
    {
        var hdr = new byte[8];
        _Seed = (uint) ((int) _Seed * 214013 + 2531011);
        var length = ((int) (_Seed >> 16) & 31) + 8;
        BitConverter.GetBytes((ushort) length).CopyTo(hdr, 0);
        hdr[2] = 4;
        _Seed = (uint) ((int) _Seed * 214013 + 2531011);
        BitConverter.GetBytes((ushort) ((_Seed >> 16) & (uint) short.MaxValue)).CopyTo(hdr, 6);
        var data = new byte[length];
        for (var index = 0; index < length; ++index)
        {
            _Seed = (uint) ((int) _Seed * 214013 + 2531011);
            data[index] = (byte) ((_Seed >> 16) & byte.MaxValue);
        }

        return EncryptPacket(hdr, data, _HashKey);
    }

    public void CreateTransactionKey()
    {
        var buf = new byte[16];
        var num = (uint) ((ulong) Account + Session);
        for (var index = 0; index < 16; ++index)
        {
            num = (uint) ((int) num * 214013 + 2531011);
            buf[index] = (byte) ((num >> 16) & byte.MaxValue);
        }

        var buffer = new byte[36];
        buf.CopyTo(buffer, 0);
        MT4Crypt.GetHardId().CopyTo(buffer, 16);
        BitConverter.GetBytes(_ClientExeSize).CopyTo(buffer, 32);
        var hash = new MD5CryptoServiceProvider().ComputeHash(buffer);
        _TransactionKey2 = MT4Crypt.Encode(buf, hash);
        _TransactionKey2 = MT4Crypt.Encode(_TransactionKey2, MT4Crypt.GetHardId());
    }

    internal class PacketHdr
    {
        public ushort DataType;
        public ushort PackType;
        public ushort Random;
        public ushort SizeData;
    }

    private delegate byte[] PackCodes();
}
