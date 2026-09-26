using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using static AL_Common.RTF.RtfCommon;

namespace AL_Common.RTF;

public sealed partial class RtfDisplayedReadmeParser
{
    private static readonly Vector128<byte> _hex20_128 = Vector128.Create((byte)0x20);
    private static readonly Vector128<byte> _all_a_128 = Vector128.Create((byte)'a');
    private static readonly Vector128<byte> _z_minus_a_128 = Vector128.Create((byte)('z' - 'a'));

    /*
    TODO: Interestingly, Vector128 is faster (on my Ryzen 5600) than Vector256 here.
    Is it due to less overlap in loading each keyword and so less duplicate data loaded / less waste?
    If we were smarter about it and parsed all found complete keywords in each vector, would Vector256 be faster
    again?
    */
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private RtfError ParseKeyword_Fast_Vector128(ref byte bufferRef)
    {
        int startingCurrentPos = _currentPos;

        char ch = (char)GetByteAtPos(ref bufferRef, startingCurrentPos);

        if (!char.IsAsciiLetter(ch))
        {
            ++_currentPos;

            return HandleControlChar(ref bufferRef, ch);
        }
        else
        {
            Symbol? symbol;
            Vector128<byte> keyword = Vector128.LoadUnsafe(ref GetRefAtPos(ref bufferRef, startingCurrentPos));
            Vector128<byte> asciiLetters = Vector128.GreaterThan((keyword | _hex20_128) - _all_a_128, _z_minus_a_128);

            uint notEqualsElements = asciiLetters.ExtractMostSignificantBits();
            byte keywordCount = (byte)BitOperations.TrailingZeroCount(notEqualsElements);

            // 99.9% of keywords in the test set (849,098 out of 849,948) are less than 16 chars long, so this
            // slightly inefficient fallback path will hardly ever be hit.
            if (keywordCount >= Vector128<byte>.Count)
            {
                return RtfError.KeywordTooLong;
            }

            int accumulatedPos = startingCurrentPos + keywordCount;

            byte firstChar = (byte)ch;

            ch = (char)GetByteAtPos(ref bufferRef, accumulatedPos);

            int negateParam = 0;
            if (ch == '-')
            {
                negateParam = 1;
                accumulatedPos += 1;
                ch = (char)GetByteAtPos(ref bufferRef, accumulatedPos);
            }
            bool hasParam = false;
            int param = 0;
            if (char.IsAsciiDigit(ch))
            {
                hasParam = true;
                long longParam = ch - '0';
                ch = (char)GetByteAtPos(ref bufferRef, accumulatedPos + 1);

                int paramLength;
                for (paramLength = 1;
                     paramLength < ParamMaxLen + 1 && char.IsAsciiDigit(ch);
                     paramLength++,
                     ch = (char)GetByteAtPos(ref bufferRef, accumulatedPos + paramLength))
                {
                    longParam = (longParam * 10) + (ch - '0');
                }
                if (paramLength > ParamMaxLen || longParam > int.MaxValue)
                {
                    return RtfError.ParameterOutOfRange;
                }

                param = (int)longParam;

                accumulatedPos += paramLength;
                // This negate is safe, because int max negated is -2147483647, and int min is -2147483648
                if (negateParam == 1) param = -param;
            }

            _currentPos = accumulatedPos + (ch == ' ' ? 1 : 0);

            // 33% of hit keywords and 97% of hit single-char keywords are \f, so fast-pathing nets substantial
            // performance gain.
            if (keywordCount == 1)
            {
                if (firstChar == (byte)'f')
                {
                    symbol = _fontSymbol;
                    _skipDestinationIfUnknown = false;
                    return DispatchKeyword(ref bufferRef, symbol, param, hasParam);
                }
                else
                {
                    symbol = null;
                }
            }
            else
            {
                ref byte keywordRef = ref GetRefAtPos(ref bufferRef, startingCurrentPos);
                symbol = LookUpControlWord_Vector128(keyword, ref keywordRef, keywordCount, firstChar);
            }

            if (symbol == null)
            {
                if (_skipDestinationIfUnknown)
                {
                    _skipDestinationIfUnknown = false;
                    SkipDest(ref bufferRef);
                }
                return RtfError.OK;
            }

            _skipDestinationIfUnknown = false;

            return DispatchKeyword(ref bufferRef, symbol, param, hasParam);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Symbol? LookUpControlWord_Vector128(Vector128<byte> keyword, ref byte keywordRef, byte len, byte firstChar)
    {
        /*
        Min word length is 1, and we're guaranteed to always be at least 1, so no need to check for >= min.
        Max keyword length on this path is 16 (Vector128<byte>.Count), while MAX_WORD_LENGTH is 18, so no need
        to check for that either.
        */
        int key = len;

        // We handle 1-length before we get here, so we know we're at least 2.
        // NOTE: This logic is optimized to do the same thing as the gperf generated code, but more efficiently.
        // This logic has to match the logic in the regular non-vectorized version.
        key += asso_values[Unsafe.AddByteOffset(ref keywordRef, len - 1)];
        if (len > 2) key += asso_values[Unsafe.AddByteOffset(ref keywordRef, 2)];
        key += asso_values[Unsafe.AddByteOffset(ref keywordRef, 0)];

        if (key <= MAX_HASH_VALUE)
        {
            ushort firstCharAndLength = _symbolFirstCharTable[key];
            ushort incomingFirstCharAndLength = (ushort)((ushort)(firstChar << 8) + len);
            if (incomingFirstCharAndLength != firstCharAndLength)
            {
                return null;
            }

            Vector128<byte> keywordVectorFromTable = _vectorKeywordTable[key];

            // Only do the masking operation if we've got a keyword hit. This saves doing it for the majority of
            // encountered keywords that will never reach this point.
            Vector128<byte> keywordMask = Vector128.GreaterThan(Vector128.Create(len), IndexVec_128);
            keyword = Vector128.BitwiseAnd(keyword, keywordMask);

            if (Vector128.EqualsAll(keyword, keywordVectorFromTable))
            {
                return _symbolTable[key]!;
            }
        }

        return null;
    }
}
