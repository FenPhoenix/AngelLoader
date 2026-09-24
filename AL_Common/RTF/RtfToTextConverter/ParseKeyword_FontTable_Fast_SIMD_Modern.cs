using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using static AL_Common.RTF.RtfCommon;

namespace AL_Common.RTF;

public sealed partial class RtfToTextConverter
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private RtfError ParseKeyword_FontTable_Fast_Vector128(ref byte bufferRef, out KeywordType fontTableKeyword, out int param)
    {
        param = 0;
        fontTableKeyword = default;

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
                    _skipDestinationIfUnknown = false;
                    // \f default param is 0 but param will already be 0 if we didn't parse any, so no need to set it
                    fontTableKeyword = KeywordType.F;
                    return RtfError.OK;
                }
                else
                {
                    symbol = LookUpControlWord_LengthOne(firstChar);
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

            fontTableKeyword = symbol.KeywordType;
            return fontTableKeyword < KeywordType.F
                ? DispatchKeyword(ref bufferRef, symbol, param, hasParam)
                : RtfError.OK;
        }
    }
}
