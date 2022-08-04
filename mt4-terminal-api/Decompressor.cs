namespace TradingAPI.MT4Server;

internal class Decompressor
{
    public static unsafe byte[] decompress(byte[] src, int maxSize)
    {
        var length = maxSize;
        var numArray1 = new byte[length + 2048];
        if (numArray1 is not {Length: not 0}) throw new Exception();
        fixed (byte* pDst = &numArray1[0])
        {
            fixed (byte* pSrc = src)
            {
                if (DecompressData(pSrc, src.Length, pDst, &length) == 0)
                    throw new Exception();
                var numArray3 = new byte[length];
                for (var index = 0; index < length; ++index)
                    numArray3[index] = pDst[index];
                return numArray3;
            }
        }
    }

    private static unsafe int DecompressData(byte* pSrc, int szSrc, byte* pDst, int* pszDst)
    {
        if ((IntPtr) pSrc == IntPtr.Zero || szSrc < 3 || (IntPtr) pDst == IntPtr.Zero || (IntPtr) pszDst == IntPtr.Zero || *pszDst < 1)
            return 0;
        var numPtr1 = pDst;
        var numPtr2 = pSrc + szSrc;
        var numPtr3 = pDst + *pszDst;
        *pszDst = 0;
        if (numPtr2[-1] != 0 || numPtr2[-2] != 0 || numPtr2[-3] != 17)
            return 0;
        uint num1 = *pSrc;
        uint num2;
        if (num1 > 17U)
        {
            num2 = num1 - 17U;
            ++pSrc;
            if (num2 >= 4U)
            {
                if (numPtr3 - pDst < num2 || numPtr2 - pSrc < num2 + 1U)
                    return 0;
                while (num2-- > 0U)
                    *pDst++ = *pSrc++;
                goto label_25;
            }

            goto label_30;
        }

        label_10:
        uint num3 = *pSrc++;
        if (num3 < 16U)
        {
            if (num3 == 0U)
            {
                if (pSrc > numPtr2)
                    return 0;
                while (*pSrc == 0)
                {
                    num3 += byte.MaxValue;
                    if (++pSrc > numPtr2)
                        return 0;
                }

                num3 += *pSrc++ + 15U;
            }

            if (numPtr3 - pDst < num3 + 3U || numPtr2 - pSrc < num3 + 4U)
                return 0;
            *(int*) pDst = (int) *(uint*) pSrc;
            pDst += 4;
            pSrc += 4;
            uint num4;
            if ((num4 = num3 - 1U) != 0U)
                for (; num4 >= 4U; num4 -= 4U)
                {
                    *(int*) pDst = (int) *(uint*) pSrc;
                    pDst += 4;
                    pSrc += 4;
                }

            while (num4-- > 0U)
                *pDst++ = *pSrc++;
        }
        else
        {
            goto label_35;
        }

        label_25:
        num3 = *pSrc++;
        if (num3 < 16U)
        {
            var numPtr4 = pDst - *pSrc++ * 4 - num3 / 4U - 2049;
            if (numPtr4 < numPtr1 || numPtr3 - pDst < 3L)
                return 0;
            var numPtr5 = pDst++;
            var numPtr7 = numPtr4 + 1;
            int num5 = *numPtr4;
            *numPtr5 = (byte) num5;
            var numPtr8 = pDst++;
            var numPtr10 = numPtr7 + 1;
            int num6 = *numPtr7;
            *numPtr8 = (byte) num6;
            var numPtr11 = pDst++;
            int num7 = *numPtr10;
            *numPtr11 = (byte) num7;
        }
        else
        {
            goto label_35;
        }

        label_29:
        num2 = pSrc[-2] & 3U;
        if (num2 == 0U)
            goto label_10;
        label_30:
        if (numPtr3 - pDst < num2 || numPtr2 - pSrc < num2 + 1U)
            return 0;
        while (num2-- > 0U)
            *pDst++ = *pSrc++;
        num3 = *pSrc++;
        label_35:
        if (num3 >= 64U)
        {
            var numPtr14 = pDst - *pSrc++ * 8 - ((num3 / 4U) & 7U) - 1;
            var num8 = num3 / 32U - 1U;
            if (numPtr14 < numPtr1 || numPtr3 - pDst < num8 + 2U)
                return 0;
            var numPtr15 = pDst++;
            var numPtr17 = numPtr14 + 1;
            int num9 = *numPtr14;
            *numPtr15 = (byte) num9;
            var numPtr18 = pDst++;
            var numPtr20 = numPtr17 + 1;
            int num10 = *numPtr17;
            *numPtr18 = (byte) num10;
            while (num8-- > 0U)
                *pDst++ = *numPtr20++;
            goto label_29;
        }

        if (num3 >= 32U)
        {
            var num11 = num3 & 31U;
            if (num11 == 0U)
            {
                if (pSrc > numPtr2)
                    return 0;
                while (*pSrc == 0)
                {
                    num11 += byte.MaxValue;
                    if (++pSrc > numPtr2)
                        return 0;
                }

                num11 += *pSrc++ + 31U;
            }

            var numPtr21 = pDst - *(ushort*) pSrc / 4 - 1;
            pSrc += 2;
            if (numPtr21 < numPtr1 || numPtr3 - pDst < num11 + 2U)
                return 0;
            byte* numPtr22;
            if (num11 >= 6U && pDst - numPtr21 >= 4L)
            {
                *(int*) pDst = (int) *(uint*) numPtr21;
                pDst += 4;
                numPtr22 = numPtr21 + 4;
                for (num11 -= 2U; num11 >= 4U; num11 -= 4U)
                {
                    *(int*) pDst = (int) *(uint*) numPtr22;
                    pDst += 4;
                    numPtr22 += 4;
                }
            }
            else
            {
                var numPtr23 = pDst++;
                var numPtr25 = numPtr21 + 1;
                int num12 = *numPtr21;
                *numPtr23 = (byte) num12;
                var numPtr26 = pDst++;
                numPtr22 = numPtr25 + 1;
                int num13 = *numPtr25;
                *numPtr26 = (byte) num13;
            }

            while (num11-- > 0U)
                *pDst++ = *numPtr22++;
            goto label_29;
        }

        if (num3 >= 16U)
        {
            var numPtr28 = pDst - (uint) (((int) num3 & 8) << 11);
            var num14 = num3 & 7U;
            if (num14 == 0U)
            {
                if (pSrc > numPtr2)
                    return 0;
                while (*pSrc == 0)
                {
                    num14 += byte.MaxValue;
                    if (++pSrc > numPtr2)
                        return 0;
                }

                num14 += *pSrc++ + 7U;
            }

            var numPtr29 = numPtr28 - *(ushort*) pSrc / 4;
            pSrc += 2;
            if (numPtr29 == pDst)
            {
                *pszDst = (int) (pDst - numPtr1);
                return pSrc != numPtr2 ? 0 : 1;
            }

            var numPtr30 = numPtr29 - 16384;
            if (numPtr30 < numPtr1 || numPtr3 - pDst < num14 + 2U)
                return 0;
            byte* numPtr31;
            if (num14 >= 6U && pDst - numPtr30 >= 4L)
            {
                *(int*) pDst = (int) *(uint*) numPtr30;
                pDst += 4;
                numPtr31 = numPtr30 + 4;
                for (num14 -= 2U; num14 >= 4U; num14 -= 4U)
                {
                    *(int*) pDst = (int) *(uint*) numPtr31;
                    pDst += 4;
                    numPtr31 += 4;
                }
            }
            else
            {
                var numPtr32 = pDst++;
                var numPtr34 = numPtr30 + 1;
                int num15 = *numPtr30;
                *numPtr32 = (byte) num15;
                var numPtr35 = pDst++;
                numPtr31 = numPtr34 + 1;
                int num16 = *numPtr34;
                *numPtr35 = (byte) num16;
            }

            while (num14-- > 0U)
                *pDst++ = *numPtr31++;
            goto label_29;
        }

        var numPtr37 = pDst - *pSrc++ * 4 - num3 / 4U - 1;
        var numPtr38 = pDst++;
        var numPtr40 = numPtr37 + 1;
        int num17 = *numPtr37;
        *numPtr38 = (byte) num17;
        var numPtr41 = pDst++;
        var numPtr43 = numPtr40 + 1;
        int num18 = *numPtr40;
        *numPtr41 = (byte) num18;
        if (numPtr43 < numPtr1 || numPtr3 - pDst < 2L)
            return 0;
        goto label_29;
    }

    public static unsafe byte[] compress(byte[] src)
    {
        var szSrc = src.Length;
        var numArray1 = new byte[szSrc];
        var length = 0;
        fixed (byte* pSrc = src)
        fixed (byte* pDst = numArray1)
        {
            var numPtr1 = pDst;
            if (szSrc > 13)
            {
                szSrc = CompressData(pSrc, szSrc, pDst, &length);
                numPtr1 = pDst + length;
            }

            if (szSrc > 0)
            {
                var numPtr2 = pSrc + src.Length - szSrc;
                if (numPtr1 == pDst && szSrc <= 238)
                    *numPtr1++ = (byte) (szSrc + 17);
                else
                    switch (szSrc)
                    {
                        case <= 3:
                        {
                            var numPtr3 = numPtr1 + -2;
                            *numPtr3 = (byte) (*numPtr3 | (uint) (byte) szSrc);
                            break;
                        }
                        case <= 18:
                            *numPtr1++ = (byte) (szSrc - 3);
                            break;
                        default:
                        {
                            var numPtr4 = numPtr1 + 1;
                            var num1 = szSrc - 18;
                            if (num1 > byte.MaxValue)
                            {
                                var num2 = (int) ((2155905153UL * (ulong) (num1 - 1)) >> 39);
                                numPtr4 += num2;
                                for (var index = 0; index < num2; ++index)
                                    num1 -= byte.MaxValue;
                            }

                            var numPtr5 = numPtr4;
                            numPtr1 = numPtr5 + 1;
                            int num3 = (byte) num1;
                            *numPtr5 = (byte) num3;
                            break;
                        }
                    }

                for (var index = 0; index < szSrc; ++index)
                    *numPtr1++ = *numPtr2++;
            }

            var numPtr6 = numPtr1;
            var numPtr7 = numPtr6 + 1;
            *numPtr6 = 17;
            length = (int) (numPtr7 + 2 - pDst);
        }

        var numArray2 = new byte[length];
        for (var index = 0; index < length; ++index)
            numArray2[index] = numArray1[index];
        return numArray2;
    }

    private static unsafe int CompressData(byte* pSrc, int szSrc, byte* pDst, int* pszDst)
    {
        var numPtrArray = new byte*[16384];
        var numPtr1 = pDst;
        var numPtr2 = pSrc + 4;
        var numPtr3 = pSrc;
        var numPtr4 = pSrc + szSrc;
        do
        {
            var num1 = (((((numPtr2[3] << 6) ^ numPtr2[2]) << 5) ^ numPtr2[1]) << 5) ^ *numPtr2;
            var index1 = ((num1 + (num1 << 5)) >> 5) & 16383;
            var numPtr5 = numPtrArray[index1];
            if (numPtr5 >= pSrc)
            {
                szSrc = (int) (numPtr2 - numPtr5);
                if (szSrc is not 0 and <= 49151)
                {
                    if (szSrc > 2048 && numPtr5[3] != numPtr2[3])
                    {
                        index1 = (index1 & 2047) ^ 8223;
                        numPtr5 = numPtrArray[index1];
                        if (numPtr5 >= pSrc)
                        {
                            szSrc = (int) (numPtr2 - numPtr5);
                            if (szSrc is 0 or > 49151 || (szSrc > 2048 && numPtr5[3] != numPtr2[3]))
                                goto label_46;
                        }
                        else
                        {
                            goto label_46;
                        }
                    }

                    if (*numPtr5 == *numPtr2 && numPtr5[1] == numPtr2[1] && numPtr5[2] == numPtr2[2])
                    {
                        numPtrArray[index1] = numPtr2;
                        var num2 = (int) (numPtr2 - numPtr3);
                        if (num2 != 0)
                        {
                            switch (num2)
                            {
                                case <= 3:
                                {
                                    var numPtr6 = numPtr1 + -2;
                                    *numPtr6 = (byte) (*numPtr6 | (uint) (byte) num2);
                                    break;
                                }
                                case <= 18:
                                    *numPtr1++ = (byte) (num2 - 3);
                                    break;
                                default:
                                {
                                    var numPtr7 = numPtr1;
                                    var numPtr8 = numPtr7 + 1;
                                    *numPtr7 = 0;
                                    var num3 = num2 - 18;
                                    if (num3 > byte.MaxValue)
                                    {
                                        var num4 = (int) ((2155905153UL * (ulong) (num3 - 1)) >> 39);
                                        numPtr8 += num4;
                                        for (var index2 = 0; index2 < num4; ++index2)
                                            num3 -= byte.MaxValue;
                                    }

                                    var numPtr9 = numPtr8;
                                    numPtr1 = numPtr9 + 1;
                                    int num5 = (byte) num3;
                                    *numPtr9 = (byte) num5;
                                    break;
                                }
                            }

                            for (var index3 = 0; index3 < num2; ++index3)
                                *numPtr1++ = *numPtr3++;
                        }

                        var numPtr10 = numPtr2 + 3;
                        numPtr2 = numPtr10 + 1;
                        if (*numPtr10 == numPtr5[3] && *numPtr2++ == numPtr5[4] && *numPtr2++ == numPtr5[5] && *numPtr2++ == numPtr5[6] && *numPtr2++ == numPtr5[7] && *numPtr2++ == numPtr5[8])
                        {
                            for (var numPtr11 = numPtr5 + 9; numPtr2 < numPtr4 && *numPtr11 == *numPtr2; ++numPtr2)
                                ++numPtr11;
                            var num6 = (int) (numPtr2 - numPtr3);
                            byte* numPtr12;
                            if (szSrc <= 16384)
                            {
                                --szSrc;
                                if (num6 <= 33)
                                {
                                    numPtr12 = numPtr1 + 1;
                                    int num7 = (byte) ((num6 - 2) | 32);
                                    *numPtr1 = (byte) num7;
                                }
                                else
                                {
                                    var numPtr15 = numPtr1 + 1;
                                    *numPtr1 = 32;
                                    var num8 = num6 - 33;
                                    if (num8 > byte.MaxValue)
                                    {
                                        var num9 = (int) ((2155905153UL * (ulong) (num8 - 1)) >> 39);
                                        numPtr15 += num9;
                                        for (var index4 = 0; index4 < num9; ++index4)
                                            num8 -= byte.MaxValue;
                                    }

                                    var numPtr16 = numPtr15;
                                    numPtr12 = numPtr16 + 1;
                                    int num10 = (byte) num8;
                                    *numPtr16 = (byte) num10;
                                }
                            }
                            else
                            {
                                szSrc -= 16384;
                                if (num6 <= 9)
                                {
                                    numPtr12 = numPtr1 + 1;
                                    int num11 = (byte) (((szSrc >> 11) & 8) | (num6 - 2) | 16);
                                    *numPtr1 = (byte) num11;
                                }
                                else
                                {
                                    var numPtr19 = numPtr1 + 1;
                                    int num12 = (byte) (((szSrc >> 11) & 8) | 16);
                                    *numPtr1 = (byte) num12;
                                    var num13 = num6 - 9;
                                    if (num13 > byte.MaxValue)
                                    {
                                        var num14 = (int) ((2155905153UL * (ulong) (num13 - 1)) >> 39);
                                        numPtr19 += num14;
                                        for (var index5 = 0; index5 < num14; ++index5)
                                            num13 -= byte.MaxValue;
                                    }

                                    var numPtr20 = numPtr19;
                                    numPtr12 = numPtr20 + 1;
                                    int num15 = (byte) num13;
                                    *numPtr20 = (byte) num15;
                                }
                            }

                            var numPtr21 = numPtr12;
                            var numPtr22 = numPtr21 + 1;
                            int num16 = (byte) (szSrc << 2);
                            *numPtr21 = (byte) num16;
                            numPtr1 = numPtr22 + 1;
                            int num17 = (byte) (szSrc >> 6);
                            *numPtr22 = (byte) num17;
                        }
                        else
                        {
                            var num18 = (int) (--numPtr2 - numPtr3);
                            switch (szSrc)
                            {
                                case <= 2048:
                                {
                                    --szSrc;
                                    var numPtr24 = numPtr1;
                                    var numPtr25 = numPtr24 + 1;
                                    int num19 = (byte) (((num18 + 7) << 5) | ((szSrc & 7) << 2));
                                    *numPtr24 = (byte) num19;
                                    numPtr1 = numPtr25 + 1;
                                    int num20 = (byte) (szSrc >> 3);
                                    *numPtr25 = (byte) num20;
                                    break;
                                }
                                case <= 16384:
                                {
                                    var numPtr27 = numPtr1;
                                    var numPtr28 = numPtr27 + 1;
                                    int num21 = (byte) ((num18 - 2) | 32);
                                    *numPtr27 = (byte) num21;
                                    --szSrc;
                                    var numPtr30 = numPtr28 + 1;
                                    int num22 = (byte) (szSrc << 2);
                                    *numPtr28 = (byte) num22;
                                    numPtr1 = numPtr30 + 1;
                                    int num23 = (byte) (szSrc >> 6);
                                    *numPtr30 = (byte) num23;
                                    break;
                                }
                                default:
                                {
                                    szSrc -= 16384;
                                    var numPtr32 = numPtr1;
                                    var numPtr33 = numPtr32 + 1;
                                    int num24 = (byte) (((szSrc >> 11) & 8) | (num18 - 2) | 16);
                                    *numPtr32 = (byte) num24;
                                    var numPtr35 = numPtr33 + 1;
                                    int num25 = (byte) (szSrc << 2);
                                    *numPtr33 = (byte) num25;
                                    numPtr1 = numPtr35 + 1;
                                    int num26 = (byte) (szSrc >> 6);
                                    *numPtr35 = (byte) num26;
                                    break;
                                }
                            }
                        }

                        numPtr3 = numPtr2;
                        goto label_47;
                    }
                }
            }

            label_46:
            numPtrArray[index1] = numPtr2++;
            label_47: ;
        } while (numPtr2 < numPtr4 - 13);

        *pszDst = (int) (numPtr1 - pDst);
        return (int) (numPtr4 - numPtr3);
    }
}
