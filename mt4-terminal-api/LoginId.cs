namespace TradingAPI.MT4Server;

internal class LoginId
{
    private int _align;
    private ulong s0;
    private ulong s10;
    private ulong s18;
    private byte[] s20 = new byte[1024];
    private int s420;
    private ulong s428;
    private ulong s8;

    public ulong Decode(byte[] data)
    {
        var pData = new ulong[data.Length / 8];
        for (var index = 0; index < pData.Length; ++index)
            pData[index] = BitConverter.ToUInt64(data, index * 8);
        return Decode(pData, (uint) pData.Length, 0);
    }

    private ulong Decode(ulong[] pData, uint szData, int arg8)
    {
        var numArray = new ulong[53];
        var num46 = numArray[45];
        var num49 = numArray[48];
        var num54 = pData[0];
        s8 = 0UL;
        while (true)
        {
            var num57 = pData[s8];
            ulong num58 = 0;
            ulong num59 = 0;
            for (var index = 0; index < 64; ++index)
            {
                var uint64_1 = Convert.ToUInt64((num57 & GlobalMembers.t00000B98[index]) > 0UL);
                var uint64_2 = Convert.ToUInt64((num54 & GlobalMembers.t00000B98[index]) > 0UL);
                var num60 = uint64_1 ^ uint64_2 ^ num59;
                num59 = Convert.ToBoolean(uint64_1) & Convert.ToBoolean(uint64_2) || Convert.ToBoolean(uint64_1) & Convert.ToBoolean(num59) ? 1UL : uint64_2 & num59;
                var num61 = num60 != 0UL ? ulong.MaxValue : 0UL;
                num58 |= GlobalMembers.t00000B98[index] & num61;
            }

            var num62 = num58;
            var num63 = ~num54 + 1UL;
            ulong num64 = 0;
            ulong num65 = 0;
            for (var index = 0; index < 64; ++index)
            {
                var uint64_3 = Convert.ToUInt64((num62 & GlobalMembers.t00000B98[index]) > 0UL);
                var uint64_4 = Convert.ToUInt64((num63 & GlobalMembers.t00000B98[index]) > 0UL);
                var num66 = (long) uint64_3 ^ (long) uint64_4 ^ (long) num65;
                num65 = Convert.ToBoolean(uint64_3 & uint64_4) || Convert.ToBoolean(uint64_3 & num65) ? 1UL : uint64_4 & num65;
                var num67 = num66 != 0L ? ulong.MaxValue : 0UL;
                num64 |= GlobalMembers.t00000B98[index] & num67;
            }

            ulong num68 = 28;
            var num69 = (uint) ((num64 >> 32) & uint.MaxValue) | ((ulong) (uint) (num64 & uint.MaxValue) << 32);
            var num70 = unchecked((ulong) ~21L) + 1UL;
            ulong num71 = 0;
            ulong num72 = 0;
            for (var index = 0; index < 64; ++index)
            {
                var uint64_5 = Convert.ToUInt64((num68 & GlobalMembers.t00000B98[index]) > 0UL);
                var uint64_6 = Convert.ToUInt64((num70 & GlobalMembers.t00000B98[index]) > 0UL);
                var num73 = uint64_5 ^ uint64_6 ^ num72;
                num72 = Convert.ToBoolean(uint64_5 & uint64_6) || Convert.ToBoolean(num72 & uint64_5) ? 1UL : uint64_6 & num72;
                var num74 = num73 != 0UL ? ulong.MaxValue : 0UL;
                num71 |= GlobalMembers.t00000B98[index] & num74;
            }

            var num75 = ((uint) (num69 & uint.MaxValue) >> 21) | ((ulong) (((uint) ((num69 >> 32) & uint.MaxValue) >> 21) | ((uint) (num69 & uint.MaxValue) << 11)) << 32);
            var num76 = ulong.MaxValue << (int) ((long) (num71 + 1UL) & uint.MaxValue);
            var num77 = (ulong) (((uint) ((num75 >> 32) & uint.MaxValue) | ((long) (uint) (num75 & uint.MaxValue) << 32)) & ~(long) num76);
            ulong num78 = 216;
            var num80 = ~num78 + 1UL;
            ulong num81 = 0;
            ulong num82 = 0;
            for (var index = 0; index < 64; ++index)
            {
                var uint64_7 = Convert.ToUInt64((num77 & GlobalMembers.t00000B98[index]) > 0UL);
                var uint64_8 = Convert.ToUInt64((num80 & GlobalMembers.t00000B98[index]) > 0UL);
                var num83 = uint64_7 ^ uint64_8 ^ num82;
                num82 = Convert.ToBoolean(uint64_7 & uint64_8) || Convert.ToBoolean(num82 & uint64_7) ? 1UL : uint64_8 & num82;
                var num84 = num83 != 0UL ? ulong.MaxValue : 0UL;
                num81 |= GlobalMembers.t00000B98[index] & num84;
            }

            if (num81 != 0UL)
            {
                this.s10 = pData[(int) (uint) ((s8 + 1UL) & uint.MaxValue)];
                var num85 = s8;
                ulong num86 = 2;
                do
                {
                    num86 = num85 ^ num86;
                    num85 = (ulong) (((long) num85 & ~(long) num86) << 1);
                } while (num85 != 0UL);

                s18 = pData[(int) (uint) (num86 & uint.MaxValue)];
                ulong num87 = 28;
                var num88 = (uint) ((this.s10 >> 32) & uint.MaxValue) | ((ulong) (uint) (this.s10 & uint.MaxValue) << 32);
                var num89 = unchecked((ulong) ~21L) + 1UL;
                ulong num90 = 0;
                ulong num91 = 0;
                for (var index = 0; index < 64; ++index)
                {
                    var uint64_9 = Convert.ToUInt64((num87 & GlobalMembers.t00000B98[index]) > 0UL);
                    var uint64_10 = Convert.ToUInt64((num89 & GlobalMembers.t00000B98[index]) > 0UL);
                    var num92 = uint64_9 ^ uint64_10 ^ num91;
                    num91 = Convert.ToBoolean(uint64_9 & uint64_10) || Convert.ToBoolean(num91 & uint64_9) ? 1UL : uint64_10 & num91;
                    var num93 = num92 != 0UL ? ulong.MaxValue : 0UL;
                    num90 |= GlobalMembers.t00000B98[index] & num93;
                }

                var num94 = ((uint) (num88 & uint.MaxValue) >> 21) | ((ulong) (((uint) ((num88 >> 32) & uint.MaxValue) >> 21) | ((uint) (num88 & uint.MaxValue) << 11)) << 32);
                var num95 = ulong.MaxValue << (int) ((long) (num90 + 1UL) & uint.MaxValue);
                if (~((((uint) ((num94 >> 32) & uint.MaxValue) | ((long) (uint) (num94 & uint.MaxValue) << 32)) & ~(long) num95) ^ -246L) == 0L)
                    this.s10 = this.s0;
                ulong num96 = 84;
                var num98 = ~num96 + 1UL;
                ulong num99 = 0;
                ulong num100 = 0;
                for (var index = 0; index < 64; ++index)
                {
                    var uint64_11 = Convert.ToUInt64((num77 & GlobalMembers.t00000B98[index]) > 0UL);
                    var uint64_12 = Convert.ToUInt64((num98 & GlobalMembers.t00000B98[index]) > 0UL);
                    var num101 = uint64_11 ^ uint64_12 ^ num100;
                    num100 = Convert.ToBoolean(uint64_11 & uint64_12) || Convert.ToBoolean(uint64_11 & num100) ? 1UL : num100 & uint64_12;
                    var num102 = num101 != 0UL ? ulong.MaxValue : 0UL;
                    num99 |= GlobalMembers.t00000B98[index] & num102;
                }

                if (num99 == 0UL)
                    this.s0 = this.s10 & s18;
                ulong num103 = 112;
                var num105 = ~num103 + 1UL;
                ulong num106 = 0;
                ulong num107 = 0;
                for (var index = 0; index < 64; ++index)
                {
                    var uint64_13 = Convert.ToUInt64((num77 & GlobalMembers.t00000B98[index]) > 0UL);
                    var uint64_14 = Convert.ToUInt64((num105 & GlobalMembers.t00000B98[index]) > 0UL);
                    var num108 = uint64_13 ^ uint64_14 ^ num107;
                    num107 = Convert.ToBoolean(uint64_13 & uint64_14) || Convert.ToBoolean(uint64_13 & num107) ? 1UL : uint64_14 & num107;
                    var num109 = num108 != 0UL ? ulong.MaxValue : 0UL;
                    num106 |= GlobalMembers.t00000B98[index] & num109;
                }

                if (num106 == 0UL)
                    this.s0 = this.s10 | s18;
                ulong num110 = 145;
                var num112 = ~num110 + 1UL;
                ulong num113 = 0;
                ulong num114 = 0;
                for (var index = 0; index < 64; ++index)
                {
                    var uint64_15 = Convert.ToUInt64((num77 & GlobalMembers.t00000B98[index]) > 0UL);
                    var uint64_16 = Convert.ToUInt64((num112 & GlobalMembers.t00000B98[index]) > 0UL);
                    var num115 = uint64_15 ^ uint64_16 ^ num114;
                    num114 = Convert.ToBoolean(uint64_16 & uint64_15) || Convert.ToBoolean(uint64_15 & num114) ? 1UL : uint64_16 & num114;
                    var num116 = num115 != 0UL ? ulong.MaxValue : 0UL;
                    num113 |= GlobalMembers.t00000B98[index] & num116;
                }

                if (num113 == 0UL)
                    this.s0 = this.s10 ^ s18;
                ulong num117 = 171;
                var num119 = ~num117 + 1UL;
                ulong num120 = 0;
                ulong num121 = 0;
                for (var index = 0; index < 64; ++index)
                {
                    var uint64_17 = Convert.ToUInt64((num77 & GlobalMembers.t00000B98[index]) > 0UL);
                    var uint64_18 = Convert.ToUInt64((num119 & GlobalMembers.t00000B98[index]) > 0UL);
                    var num122 = uint64_17 ^ uint64_18 ^ num121;
                    num121 = Convert.ToBoolean(uint64_17 & uint64_18) || Convert.ToBoolean(uint64_17 & num121) ? 1UL : uint64_18 & num121;
                    var num123 = num122 != 0UL ? ulong.MaxValue : 0UL;
                    num120 |= GlobalMembers.t00000B98[index] & num123;
                }

                if (num120 == 0UL)
                {
                    var num124 = s10;
                    var num125 = s18;
                    do
                    {
                        num125 = num124 ^ num125;
                        num124 = (ulong) (((long) num124 & ~(long) num125) << 1);
                    } while (num124 != 0UL);

                    this.s0 = num125;
                }

                ulong num126 = 169;
                var num128 = ~num126 + 1UL;
                ulong num129 = 0;
                ulong num130 = 0;
                for (var index = 0; index < 64; ++index)
                {
                    var uint64_19 = Convert.ToUInt64((num77 & GlobalMembers.t00000B98[index]) > 0UL);
                    var uint64_20 = Convert.ToUInt64((num128 & GlobalMembers.t00000B98[index]) > 0UL);
                    var num131 = uint64_19 ^ uint64_20 ^ num130;
                    num130 = Convert.ToBoolean(uint64_19 & uint64_20) || Convert.ToBoolean(uint64_19 & num130) ? 1UL : uint64_20 & num130;
                    var num132 = num131 != 0UL ? ulong.MaxValue : 0UL;
                    num129 |= GlobalMembers.t00000B98[index] & num132;
                }

                if (num129 == 0UL)
                {
                    var num133 = s10;
                    var num134 = ~s18 + 1UL;
                    do
                    {
                        num134 = num133 ^ num134;
                        num133 = (ulong) (((long) num133 & ~(long) num134) << 1);
                    } while (num133 != 0UL);

                    this.s0 = num134;
                }

                ulong num135 = 177;
                var num137 = ~num135 + 1UL;
                ulong num138 = 0;
                ulong num139 = 0;
                for (var index = 0; index < 64; ++index)
                {
                    var uint64_21 = Convert.ToUInt64((num77 & GlobalMembers.t00000B98[index]) > 0UL);
                    var uint64_22 = Convert.ToUInt64((num137 & GlobalMembers.t00000B98[index]) > 0UL);
                    var num140 = uint64_21 ^ uint64_22 ^ num139;
                    num139 = Convert.ToBoolean(uint64_21 & uint64_22) || Convert.ToBoolean(uint64_21 & num139) ? 1UL : uint64_22 & num139;
                    var num141 = num140 != 0UL ? ulong.MaxValue : 0UL;
                    num138 |= GlobalMembers.t00000B98[index] & num141;
                }

                if (num138 == 0UL)
                {
                    var num142 = s18 % 24UL;
                    var num143 = s10;
                    ulong num144 = 0;
                    while (true)
                    {
                        var num145 = num144;
                        var num147 = ~num142 + 1UL;
                        ulong num148 = 0;
                        ulong num149 = 1;
                        num46 = 0UL;
                        ulong num150 = 0;
                        ulong num151 = 0;
                        ulong num152 = 0;
                        for (var index = 0; index < 64; ++index)
                        {
                            num150 = Convert.ToUInt64((num145 & GlobalMembers.t00000B98[index]) > 0UL);
                            num151 = Convert.ToUInt64((num142 & GlobalMembers.t00000B98[index]) > 0UL);
                            var uint64 = Convert.ToUInt64((num147 & GlobalMembers.t00000B98[index]) > 0UL);
                            if (num150 != 0UL && num151 == 0UL)
                                num148 = 0UL;
                            if (num150 == 0UL && num151 != 0UL)
                                num148 = 1UL;
                            num49 = num150 ^ uint64 ^ num152;
                            num152 = Convert.ToBoolean(num150 & uint64) || Convert.ToBoolean(num150 & num152) ? 1UL : uint64 & num152;
                            if (num49 != 0UL)
                                if (index <= 7)
                                    num149 = Convert.ToUInt64(num149 == 0UL);
                        }

                        if (num150 == 0UL && num151 != 0UL && num49 != 0UL)
                            num46 = 1UL;
                        if (num150 != 0UL && num151 == 0UL && num49 == 0UL)
                            num46 = 1UL;
                        if (num148 != 0UL)
                        {
                            var num153 = num143;
                            var num154 = num143;
                            ulong num155 = 0;
                            ulong num156 = 0;
                            for (var index = 0; index < 64; ++index)
                            {
                                var uint64_23 = Convert.ToUInt64((num154 & GlobalMembers.t00000B98[index]) > 0UL);
                                var uint64_24 = Convert.ToUInt64((num153 & GlobalMembers.t00000B98[index]) > 0UL);
                                var num157 = uint64_23 ^ uint64_24 ^ num156;
                                num156 = Convert.ToBoolean(uint64_23 & uint64_24) || Convert.ToBoolean(uint64_23 & num156) ? 1UL : uint64_24 & num156;
                                var num158 = num157 != 0UL ? ulong.MaxValue : 0UL;
                                num155 |= GlobalMembers.t00000B98[index] & num158;
                            }

                            num143 = num155;
                            ++num144;
                        }
                        else
                        {
                            break;
                        }
                    }

                    this.s0 = num143;
                }

                if (((long) num77 ^ 200L) == 0L)
                {
                    var num159 = s18 % 24UL;
                    var s10 = this.s10;
                    ulong num160 = 0;
                    ulong index1 = 0;
                    while (true)
                    {
                        var num161 = index1;
                        ulong num162 = 64;
                        var num163 = ~num162 + 1UL;
                        ulong num164 = 0;
                        ulong num165 = 1;
                        num49 = 1UL;
                        ulong num166 = 0;
                        ulong num167 = 0;
                        ulong num168 = 0;
                        for (var index2 = 0; index2 < 64; ++index2)
                        {
                            num166 = Convert.ToUInt64((num161 & GlobalMembers.t00000B98[index2]) > 0UL);
                            num167 = Convert.ToUInt64((num162 & GlobalMembers.t00000B98[index2]) > 0UL);
                            var uint64 = Convert.ToUInt64((num163 & GlobalMembers.t00000B98[index2]) > 0UL);
                            if (num166 != 0UL && num167 == 0UL)
                                num164 = 0UL;
                            if (num166 == 0UL && num167 != 0UL)
                                num164 = 1UL;
                            num46 = num166 ^ uint64 ^ num168;
                            num168 = Convert.ToBoolean(num166 & uint64) || Convert.ToBoolean(num166 & num168) ? 1UL : uint64 & num168;
                            if (num46 != 0UL)
                            {
                                num49 = 0UL;
                                if (index2 <= 7)
                                    num165 = Convert.ToUInt64(num165 == 0UL);
                            }
                        }

                        if (num166 == 0UL && num167 != 0UL)
                            ;
                        if (num166 != 0UL && num167 == 0UL)
                            ;
                        if (num164 != 0UL)
                        {
                            if (((long) s10 & (long) GlobalMembers.t00000B98[index1]) != 0L && index1 >= num159)
                            {
                                var num171 = ~num159 + 1UL;
                                ulong index3 = 0;
                                ulong num172 = 0;
                                for (var index4 = 0; index4 < 64; ++index4)
                                {
                                    var uint64_25 = Convert.ToUInt64((index1 & GlobalMembers.t00000B98[index4]) > 0UL);
                                    var uint64_26 = Convert.ToUInt64((num171 & GlobalMembers.t00000B98[index4]) > 0UL);
                                    var num173 = uint64_25 ^ uint64_26 ^ num172;
                                    num172 = Convert.ToBoolean(uint64_25 & uint64_26) || Convert.ToBoolean(uint64_25 & num172) ? 1UL : uint64_26 & num172;
                                    var num174 = num173 != 0UL ? ulong.MaxValue : 0UL;
                                    index3 |= GlobalMembers.t00000B98[index4] & num174;
                                }

                                num160 ^= GlobalMembers.t00000B98[index3];
                            }

                            ++index1;
                        }
                        else
                        {
                            break;
                        }
                    }

                    this.s0 = num160;
                }

                s8 += 3UL;
            }
            else
            {
                break;
            }
        }

        var s0 = this.s0;
        this.s0 = 0UL;
        return s0;
    }

    public static class GlobalMembers
    {
        public static ulong[] t00000B98 = new ulong[64]
        {
            1UL,
            2UL,
            4UL,
            8UL,
            16UL,
            32UL,
            64UL,
            128UL,
            256UL,
            512UL,
            1024UL,
            2048UL,
            4096UL,
            8192UL,
            16384UL,
            32768UL,
            65536UL,
            131072UL,
            262144UL,
            524288UL,
            1048576UL,
            2097152UL,
            4194304UL,
            8388608UL,
            16777216UL,
            33554432UL,
            67108864UL,
            134217728UL,
            268435456UL,
            536870912UL,
            1073741824UL,
            2147483648UL,
            4294967296UL,
            8589934592UL,
            17179869184UL,
            34359738368UL,
            68719476736UL,
            137438953472UL,
            274877906944UL,
            549755813888UL,
            1099511627776UL,
            2199023255552UL,
            4398046511104UL,
            8796093022208UL,
            17592186044416UL,
            35184372088832UL,
            70368744177664UL,
            140737488355328UL,
            281474976710656UL,
            562949953421312UL,
            1125899906842624UL,
            2251799813685248UL,
            4503599627370496UL,
            9007199254740992UL,
            18014398509481984UL,
            36028797018963968UL,
            72057594037927936UL,
            144115188075855872UL,
            288230376151711744UL,
            576460752303423488UL,
            1152921504606846976UL,
            2305843009213693952UL,
            4611686018427387904UL,
            9223372036854775808UL
        };
    }
}
