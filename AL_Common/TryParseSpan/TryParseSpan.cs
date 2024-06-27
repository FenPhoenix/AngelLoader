// From Framework 4.8, modified to take ReadOnlySpan<char> and otherwise cleaned up

using System;
using System.Globalization;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace AL_Common;

public static class TryParseSpan
{
    // Constants used by number parsing
    internal const int NumberMaxDigits = 50;

    private const int Int32Precision = 10;
    private const int UInt32Precision = Int32Precision;
    private const int Int64Precision = 19;
    private const int UInt64Precision = 20;

    private const string positiveSign = "+";
    private const string negativeSign = "-";

    private const int numberNegativePattern = 1;

    #region Public API

    [PublicAPI]
    public static bool TryParseSByte(ReadOnlySpan<char> s, NumberStyles style, NumberFormatInfo info, out sbyte result)
    {
        result = 0;
        if (!TryParseInt32(s, style, info, out int i))
        {
            return false;
        }

        if ((style & NumberStyles.AllowHexSpecifier) != 0)
        {
            // We are parsing a hexadecimal number
            if (i is < 0 or > byte.MaxValue)
            {
                return false;
            }
            result = (sbyte)i;
            return true;
        }

        if (i is < sbyte.MinValue or > sbyte.MaxValue)
        {
            return false;
        }
        result = (sbyte)i;
        return true;
    }

    [PublicAPI]
    public static bool TryParseByte(ReadOnlySpan<char> s, NumberStyles style, NumberFormatInfo info, out byte result)
    {
        result = 0;
        if (!TryParseInt32(s, style, info, out int i))
        {
            return false;
        }
        if (i is < byte.MinValue or > byte.MaxValue)
        {
            return false;
        }
        result = (byte)i;
        return true;
    }

    [PublicAPI]
    public static bool TryParseInt16(ReadOnlySpan<char> s, NumberStyles style, NumberFormatInfo info, out short result)
    {
        result = 0;
        if (!TryParseInt32(s, style, info, out int i))
        {
            return false;
        }

        // We need this check here since we don't allow signs to specified in hex numbers. So we fixup the result
        // for negative numbers
        if ((style & NumberStyles.AllowHexSpecifier) != 0)
        {
            // We are parsing a hexadecimal number
            if (i is < 0 or > ushort.MaxValue)
            {
                return false;
            }
            result = (short)i;
            return true;
        }

        if (i is < short.MinValue or > short.MaxValue)
        {
            return false;
        }
        result = (short)i;
        return true;
    }

    [PublicAPI]
    public static bool TryParseUInt16(ReadOnlySpan<char> s, NumberStyles style, NumberFormatInfo info, out ushort result)
    {
        result = 0;
        if (!TryParseUInt32(s, style, info, out uint i))
        {
            return false;
        }
        if (i > ushort.MaxValue)
        {
            return false;
        }
        result = (ushort)i;
        return true;
    }

    [PublicAPI]
    public static unsafe bool TryParseInt32(ReadOnlySpan<char> s, NumberStyles style, NumberFormatInfo info, out int result)
    {
        byte* numberBufferBytes = stackalloc byte[NumberBuffer.NumberBufferBytes];
        NumberBuffer number = new(numberBufferBytes);
        result = 0;

        if (!TryStringToNumber(s, style, ref number, info, false))
        {
            return false;
        }

        if ((style & NumberStyles.AllowHexSpecifier) != 0)
        {
            if (!HexNumberToInt32(ref number, ref result))
            {
                return false;
            }
        }
        else
        {
            if (!NumberToInt32(ref number, ref result))
            {
                return false;
            }
        }
        return true;
    }

    [PublicAPI]
    public static unsafe bool TryParseUInt32(ReadOnlySpan<char> s, NumberStyles style, NumberFormatInfo info, out uint result)
    {
        byte* numberBufferBytes = stackalloc byte[NumberBuffer.NumberBufferBytes];
        NumberBuffer number = new(numberBufferBytes);
        result = 0;

        if (!TryStringToNumber(s, style, ref number, info, false))
        {
            return false;
        }

        if ((style & NumberStyles.AllowHexSpecifier) != 0)
        {
            if (!HexNumberToUInt32(ref number, ref result))
            {
                return false;
            }
        }
        else
        {
            if (!NumberToUInt32(ref number, ref result))
            {
                return false;
            }
        }
        return true;
    }

    [PublicAPI]
    public static unsafe bool TryParseInt64(ReadOnlySpan<char> s, NumberStyles style, NumberFormatInfo info, out long result)
    {
        byte* numberBufferBytes = stackalloc byte[NumberBuffer.NumberBufferBytes];
        NumberBuffer number = new(numberBufferBytes);
        result = 0;

        if (!TryStringToNumber(s, style, ref number, info, false))
        {
            return false;
        }

        if ((style & NumberStyles.AllowHexSpecifier) != 0)
        {
            if (!HexNumberToInt64(ref number, ref result))
            {
                return false;
            }
        }
        else
        {
            if (!NumberToInt64(ref number, ref result))
            {
                return false;
            }
        }
        return true;
    }

    [PublicAPI]
    public static unsafe bool TryParseUInt64(ReadOnlySpan<char> s, NumberStyles style, NumberFormatInfo info, out ulong result)
    {
        byte* numberBufferBytes = stackalloc byte[NumberBuffer.NumberBufferBytes];
        NumberBuffer number = new(numberBufferBytes);
        result = 0;

        if (!TryStringToNumber(s, style, ref number, info, false))
        {
            return false;
        }

        if ((style & NumberStyles.AllowHexSpecifier) != 0)
        {
            if (!HexNumberToUInt64(ref number, ref result))
            {
                return false;
            }
        }
        else
        {
            if (!NumberToUInt64(ref number, ref result))
            {
                return false;
            }
        }
        return true;
    }

    #endregion

    #region Utils

    private static bool IsWhite(char ch) => (ch == 0x20) || (ch >= 0x09 && ch <= 0x0D);

    private static bool TrailingZeros(ReadOnlySpan<char> s, int index)
    {
        // For compatibility, we need to allow trailing zeros at the end of a number string
        for (int i = index; i < s.Length; i++)
        {
            if (s[i] != '\0')
            {
                return false;
            }
        }
        return true;
    }

    #endregion

    #region Hex

    private static bool HexNumberToInt32(ref NumberBuffer number, ref int value)
    {
        uint passedValue = 0;
        bool returnValue = HexNumberToUInt32(ref number, ref passedValue);
        value = (int)passedValue;
        return returnValue;
    }

    private static unsafe bool HexNumberToUInt32(ref NumberBuffer number, ref uint value)
    {
        int i = number.scale;
        if (i > UInt32Precision || i < number.precision)
        {
            return false;
        }
        char* p = number.digits;

        uint n = 0;
        while (--i >= 0)
        {
            if (n > (uint)0xFFFFFFFF / 16)
            {
                return false;
            }
            n *= 16;
            if (*p != '\0')
            {
                uint newN = n;
                if (*p != '\0')
                {
                    if (*p >= '0' && *p <= '9')
                    {
                        newN += (uint)(*p - '0');
                    }
                    else
                    {
                        if (*p >= 'A' && *p <= 'F')
                        {
                            newN += (uint)((*p - 'A') + 10);
                        }
                        else
                        {
                            newN += (uint)((*p - 'a') + 10);
                        }
                    }
                    p++;
                }

                // Detect an overflow here...
                if (newN < n)
                {
                    return false;
                }
                n = newN;
            }
        }
        value = n;
        return true;
    }

    private static bool HexNumberToInt64(ref NumberBuffer number, ref long value)
    {
        ulong passedValue = 0;
        bool returnValue = HexNumberToUInt64(ref number, ref passedValue);
        value = (long)passedValue;
        return returnValue;
    }

    private static unsafe bool HexNumberToUInt64(ref NumberBuffer number, ref ulong value)
    {
        int i = number.scale;
        if (i > UInt64Precision || i < number.precision)
        {
            return false;
        }
        char* p = number.digits;

        ulong n = 0;
        while (--i >= 0)
        {
            if (n > 0xFFFFFFFFFFFFFFFF / 16)
            {
                return false;
            }
            n *= 16;
            if (*p != '\0')
            {
                ulong newN = n;
                if (*p != '\0')
                {
                    if (*p >= '0' && *p <= '9')
                    {
                        newN += (ulong)(*p - '0');
                    }
                    else
                    {
                        if (*p >= 'A' && *p <= 'F')
                        {
                            newN += (ulong)((*p - 'A') + 10);
                        }
                        else
                        {
                            newN += (ulong)((*p - 'a') + 10);
                        }
                    }
                    p++;
                }

                // Detect an overflow here...
                if (newN < n)
                {
                    return false;
                }
                n = newN;
            }
        }
        value = n;
        return true;
    }

    #endregion

    #region Normal number (non-hex)

    private static unsafe bool NumberToInt32(ref NumberBuffer number, ref int value)
    {
        int i = number.scale;
        if (i > Int32Precision || i < number.precision)
        {
            return false;
        }
        char* p = number.digits;
        int n = 0;
        while (--i >= 0)
        {
            if ((uint)n > 0x7FFFFFFF / 10)
            {
                return false;
            }
            n *= 10;
            if (*p != '\0')
            {
                n += (int)(*p++ - '0');
            }
        }
        if (number.sign)
        {
            n = -n;
            if (n > 0)
            {
                return false;
            }
        }
        else
        {
            if (n < 0)
            {
                return false;
            }
        }
        value = n;
        return true;
    }

    private static unsafe bool NumberToUInt32(ref NumberBuffer number, ref uint value)
    {
        int i = number.scale;
        if (i > UInt32Precision || i < number.precision || number.sign)
        {
            return false;
        }
        char* p = number.digits;
        uint n = 0;
        while (--i >= 0)
        {
            if (n > 0xFFFFFFFF / 10)
            {
                return false;
            }
            n *= 10;
            if (*p != '\0')
            {
                uint newN = n + (uint)(*p++ - '0');
                // Detect an overflow here...
                if (newN < n)
                {
                    return false;
                }
                n = newN;
            }
        }
        value = n;
        return true;
    }

    private static unsafe bool NumberToInt64(ref NumberBuffer number, ref long value)
    {
        int i = number.scale;
        if (i > Int64Precision || i < number.precision)
        {
            return false;
        }
        char* p = number.digits;
        long n = 0;
        while (--i >= 0)
        {
            if ((ulong)n > 0x7FFFFFFFFFFFFFFF / 10)
            {
                return false;
            }
            n *= 10;
            if (*p != '\0')
            {
                n += (int)(*p++ - '0');
            }
        }
        if (number.sign)
        {
            n = -n;
            if (n > 0)
            {
                return false;
            }
        }
        else
        {
            if (n < 0)
            {
                return false;
            }
        }
        value = n;
        return true;
    }

    private static unsafe bool NumberToUInt64(ref NumberBuffer number, ref ulong value)
    {
        int i = number.scale;
        if (i > UInt64Precision || i < number.precision || number.sign)
        {
            return false;
        }
        char* p = number.digits;
        ulong n = 0;
        while (--i >= 0)
        {
            if (n > 0xFFFFFFFFFFFFFFFF / 10)
            {
                return false;
            }
            n *= 10;
            if (*p != '\0')
            {
                ulong newN = n + (ulong)(*p++ - '0');
                // Detect an overflow here...
                if (newN < n)
                {
                    return false;
                }
                n = newN;
            }
        }
        value = n;
        return true;
    }

    #endregion

    private static unsafe bool TryStringToNumber(ReadOnlySpan<char> str, NumberStyles options, ref NumberBuffer number, NumberFormatInfo numFormat, bool parseDecimal)
    {
        fixed (char* stringPointer = &MemoryMarshal.GetReference(str))
        {
            char* p = stringPointer;
            if (!ParseNumber(ref p, options, ref number, numFormat, parseDecimal)
                || (p - stringPointer < str.Length && !TrailingZeros(str, (int)(p - stringPointer))))
            {
                return false;
            }
        }

        return true;
    }

    private static unsafe char* MatchChars(char* p, string? str)
    {
        fixed (char* stringPointer = str)
        {
            return MatchChars(p, stringPointer);
        }
    }

    private static unsafe char* MatchChars(char* p, char* str)
    {
        if (*str == '\0')
        {
            return null;
        }
        for (; *str != '\0'; p++, str++)
        {
            if (*p != *str)
            {
                //We only hurt the failure case
                if ((*str == '\u00A0') && (*p == '\u0020'))
                {
                    // This fix is for French or Kazakh cultures. Since a user cannot type 0xA0 as a
                    // space character we use 0x20 space character instead to mean the same.
                    continue;
                }
                return null;
            }
        }
        return p;
    }

    private static unsafe bool ParseNumber(ref char* str, NumberStyles options, ref NumberBuffer number, NumberFormatInfo numFormat, bool parseDecimal)
    {
        const int StateSign = 0x0001;
        const int StateParens = 0x0002;
        const int StateDigits = 0x0004;
        const int StateNonZero = 0x0008;
        const int StateDecimal = 0x0010;
        const int StateCurrency = 0x0020;

        number.scale = 0;
        number.sign = false;
        string decSep;                  // decimal separator from NumberFormatInfo.
        string groupSep;                // group separator from NumberFormatInfo.
        string? currSymbol = null;       // currency symbol from NumberFormatInfo.

        // The alternative currency symbol used in ANSI codepage, that can not roundtrip between ANSI and Unicode.
        // Currently, only ja-JP and ko-KR has non-null values (which is U+005c, backslash)
        string? ansiCurrSymbol = null;   // currency symbol from NumberFormatInfo.
        string? altDecSep = null;        // decimal separator from NumberFormatInfo as a decimal
        string? altGroupSep = null;      // group separator from NumberFormatInfo as a decimal

        bool parsingCurrency = false;
        if ((options & NumberStyles.AllowCurrencySymbol) != 0)
        {
            currSymbol = numFormat.CurrencySymbol;

            // The idea here is to match the currency separators and on failure match the number separators to keep the perf of VB's IsNumeric fast.
            // The values of decSep are setup to use the correct relevant separator (currency in the if part and decimal in the else part).
            altDecSep = numFormat.NumberDecimalSeparator;
            altGroupSep = numFormat.NumberGroupSeparator;
            decSep = numFormat.CurrencyDecimalSeparator;
            groupSep = numFormat.CurrencyGroupSeparator;
            parsingCurrency = true;
        }
        else
        {
            decSep = numFormat.NumberDecimalSeparator;
            groupSep = numFormat.NumberGroupSeparator;
        }

        int state = 0;
        bool signFlag = false; // Cache the results of "options & PARSE_LEADINGSIGN && !(state & STATE_SIGN)" to avoid doing this twice
        int maxParseDigits = NumberMaxDigits;

        char* p = str;
        char ch = *p;
        char* next;

        while (true)
        {
            // Eat whitespace unless we've found a sign which isn't followed by a currency symbol.
            // "-Kr 1231.47" is legal but "- 1231.47" is not.
            if (IsWhite(ch) && ((options & NumberStyles.AllowLeadingWhite) != 0) && (((state & StateSign) == 0) || (((state & StateSign) != 0) && (((state & StateCurrency) != 0) || numberNegativePattern == 2))))
            {
                // Do nothing here. We will increase p at the end of the loop.
            }
            else if ((signFlag = ((options & NumberStyles.AllowLeadingSign) != 0) && ((state & StateSign) == 0)) && ((next = MatchChars(p, positiveSign)) != null))
            {
                state |= StateSign;
                p = next - 1;
            }
            else if (signFlag && (next = MatchChars(p, negativeSign)) != null)
            {
                state |= StateSign;
                number.sign = true;
                p = next - 1;
            }
            else if (ch == '(' && ((options & NumberStyles.AllowParentheses) != 0) && ((state & StateSign) == 0))
            {
                state |= StateSign | StateParens;
                number.sign = true;
            }
            else if ((currSymbol != null && (next = MatchChars(p, currSymbol)) != null) || (ansiCurrSymbol != null && (next = MatchChars(p, ansiCurrSymbol)) != null))
            {
                state |= StateCurrency;
                currSymbol = null;
                ansiCurrSymbol = null;
                // We already found the currency symbol. There should not be more currency symbols. Set
                // currSymbol to NULL so that we won't search it again in the later code path.
                p = next - 1;
            }
            else
            {
                break;
            }
            ch = *++p;
        }
        int digCount = 0;
        int digEnd = 0;
        while (true)
        {
            // @vNext: Switch to fast version for 0-9 (in all places)
            if (ch is >= '0' and <= '9' || (((options & NumberStyles.AllowHexSpecifier) != 0) && ((ch >= 'a' && ch <= 'f') || (ch >= 'A' && ch <= 'F'))))
            {
                state |= StateDigits;

                if (ch != '0' || (state & StateNonZero) != 0)
                {
                    if (digCount < maxParseDigits)
                    {
                        number.digits[digCount++] = ch;
                        if (ch != '0' || parseDecimal)
                        {
                            digEnd = digCount;
                        }
                    }
                    if ((state & StateDecimal) == 0)
                    {
                        number.scale++;
                    }
                    state |= StateNonZero;
                }
                else if ((state & StateDecimal) != 0)
                {
                    number.scale--;
                }
            }
            else if (((options & NumberStyles.AllowDecimalPoint) != 0) && ((state & StateDecimal) == 0) && ((next = MatchChars(p, decSep)) != null || ((parsingCurrency) && (state & StateCurrency) == 0) && (next = MatchChars(p, altDecSep)) != null))
            {
                state |= StateDecimal;
                p = next - 1;
            }
            else if (((options & NumberStyles.AllowThousands) != 0) && ((state & StateDigits) != 0) && ((state & StateDecimal) == 0) && ((next = MatchChars(p, groupSep)) != null || ((parsingCurrency) && (state & StateCurrency) == 0) && (next = MatchChars(p, altGroupSep)) != null))
            {
                p = next - 1;
            }
            else
            {
                break;
            }
            ch = *++p;
        }

        bool negExp = false;
        number.precision = digEnd;
        number.digits[digEnd] = '\0';
        if ((state & StateDigits) != 0)
        {
            if ((ch is 'E' or 'e') && ((options & NumberStyles.AllowExponent) != 0))
            {
                char* temp = p;
                ch = *++p;
                if ((next = MatchChars(p, positiveSign)) != null)
                {
                    ch = *(p = next);
                }
                else if ((next = MatchChars(p, negativeSign)) != null)
                {
                    ch = *(p = next);
                    negExp = true;
                }
                if (ch is >= '0' and <= '9')
                {
                    int exp = 0;
                    do
                    {
                        exp = exp * 10 + (ch - '0');
                        ch = *++p;
                        if (exp > 1000)
                        {
                            exp = 9999;
                            while (ch is >= '0' and <= '9')
                            {
                                ch = *++p;
                            }
                        }
                    } while (ch is >= '0' and <= '9');
                    if (negExp)
                    {
                        exp = -exp;
                    }
                    number.scale += exp;
                }
                else
                {
                    p = temp;
                    ch = *p;
                }
            }
            while (true)
            {
                if (IsWhite(ch) && ((options & NumberStyles.AllowTrailingWhite) != 0))
                {
                }
                else if ((signFlag = (((options & NumberStyles.AllowTrailingSign) != 0) && ((state & StateSign) == 0))) && (next = MatchChars(p, positiveSign)) != null)
                {
                    state |= StateSign;
                    p = next - 1;
                }
                else if (signFlag && (next = MatchChars(p, negativeSign)) != null)
                {
                    state |= StateSign;
                    number.sign = true;
                    p = next - 1;
                }
                else if (ch == ')' && ((state & StateParens) != 0))
                {
                    state &= ~StateParens;
                }
                else if ((currSymbol != null && (next = MatchChars(p, currSymbol)) != null) || (ansiCurrSymbol != null && (next = MatchChars(p, ansiCurrSymbol)) != null))
                {
                    currSymbol = null;
                    ansiCurrSymbol = null;
                    p = next - 1;
                }
                else
                {
                    break;
                }
                ch = *++p;
            }
            if ((state & StateParens) == 0)
            {
                if ((state & StateNonZero) == 0)
                {
                    if (!parseDecimal)
                    {
                        number.scale = 0;
                    }
                    if ((state & StateDecimal) == 0)
                    {
                        number.sign = false;
                    }
                }
                str = p;
                return true;
            }
        }
        str = p;
        return false;
    }
}
