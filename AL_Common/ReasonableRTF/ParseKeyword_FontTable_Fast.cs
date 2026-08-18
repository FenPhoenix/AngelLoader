using System.Runtime.CompilerServices;
using ReasonableRTF.Enums;
using ReasonableRTF.Extensions;
using ReasonableRTF.Models.Symbols;

namespace ReasonableRTF;

public sealed partial class RtfToTextConverter
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private RtfError ParseKeyword_FontTable_Fast(ref byte bufferRef, out KeywordType fontTableKeyword, out int param)
    {
        param = 0;
        fontTableKeyword = default;

        int startingCurrentPos = _currentPos;

        char ch = (char)GetByteAtPos(ref bufferRef, startingCurrentPos);

        if (!CharExtension.IsAsciiLetter(ch))
        {
            ++_currentPos;

            return HandleControlChar(ref bufferRef, ch);
        }
        else
        {
            ch = (char)GetByteAtPos(ref bufferRef, startingCurrentPos + 1);

            byte keywordCount;
            Symbol? symbol;
            for (keywordCount = 1;
                 keywordCount < _keywordMaxLen + 1 && CharExtension.IsAsciiLetter(ch);
                 keywordCount++,
                 ch = (char)GetByteAtPos(ref bufferRef, startingCurrentPos + keywordCount))
            {
            }
            if (keywordCount > _keywordMaxLen)
            {
                return RtfError.KeywordTooLong;
            }

            int accumulatedPos = startingCurrentPos + keywordCount;

            int negateParam = 0;
            if (ch == '-')
            {
                negateParam = 1;
                accumulatedPos += 1;
                ch = (char)GetByteAtPos(ref bufferRef, accumulatedPos);
            }
            bool hasParam = false;
            if (CharExtension.IsAsciiDigit(ch))
            {
                hasParam = true;
                long longParam = ch - '0';
                ch = (char)GetByteAtPos(ref bufferRef, accumulatedPos + 1);

                int paramLength;
                for (paramLength = 1;
                     paramLength < _paramMaxLen + 1 && CharExtension.IsAsciiDigit(ch);
                     paramLength++,
                     ch = (char)GetByteAtPos(ref bufferRef, accumulatedPos + paramLength))
                {
                    longParam = (longParam * 10) + (ch - '0');
                }
                if (paramLength > _paramMaxLen || longParam > int.MaxValue)
                {
                    return RtfError.ParameterOutOfRange;
                }

                param = (int)longParam;

                accumulatedPos += paramLength;
                /*
                NOTE: Turns out the branches are actually faster than the branchless black magic. On all targets.
                Go figure...
                */
                // This negate is safe, because int max negated is -2147483647, and int min is -2147483648
                if (negateParam == 1) param = -param;
            }

            _currentPos = accumulatedPos + (ch == ' ' ? 1 : 0);

            ref byte keywordRef = ref GetRefAtPos(ref bufferRef, startingCurrentPos);

            // 33% of hit keywords and 97% of hit single-char keywords are \f, so fast-pathing nets substantial
            // performance gain.
            if (keywordCount == 1)
            {
                byte firstChar = keywordRef;

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
                symbol = LookUpControlWord(ref keywordRef, keywordCount);
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
