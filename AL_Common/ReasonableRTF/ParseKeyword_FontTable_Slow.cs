using System.Runtime.CompilerServices;
using ReasonableRTF.Enums;
using ReasonableRTF.Extensions;
using ReasonableRTF.Models.Symbols;

namespace ReasonableRTF;

public sealed partial class RtfToTextConverter
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private RtfError ParseKeyword_FontTable_Slow(ref byte bufferRef, out KeywordType fontTableKeyword, out int param)
    {
        param = 0;
        fontTableKeyword = default;

        char ch = (char)GetByte(IncrementCurrentPos());

        if (!CharExtension.IsAsciiLetter(ch))
        {
            return HandleControlChar(ref bufferRef, ch);
        }
        else
        {
            Symbol? symbol;
            ref byte keywordRef = ref GetArrayDataReference(_keyword);

            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref keywordRef, (nint)0), (byte)ch);
            ch = (char)GetByte(IncrementCurrentPos());

            byte keywordCount;
            for (keywordCount = 1;
                 keywordCount < _keywordMaxLen + 1 && CharExtension.IsAsciiLetter(ch);
                 keywordCount++, ch = (char)GetByte(IncrementCurrentPos()))
            {
                Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref keywordRef, (nint)keywordCount), (byte)ch);
            }
            if (keywordCount > _keywordMaxLen)
            {
                return RtfError.KeywordTooLong;
            }

            int negateParam = 0;
            if (ch == '-')
            {
                negateParam = 1;
                ch = (char)GetByte(IncrementCurrentPos());
            }
            bool hasParam = false;
            if (CharExtension.IsAsciiDigit(ch))
            {
                hasParam = true;
                long longParam = ch - '0';
                ch = (char)GetByte(IncrementCurrentPos());

                int paramLength;
                for (paramLength = 1;
                     paramLength < _paramMaxLen + 1 && CharExtension.IsAsciiDigit(ch);
                     paramLength++, ch = (char)GetByte(IncrementCurrentPos()))
                {
                    longParam = (longParam * 10) + (ch - '0');
                }
                if (paramLength > _paramMaxLen || longParam > int.MaxValue)
                {
                    return RtfError.ParameterOutOfRange;
                }

                param = (int)longParam;

                /*
                NOTE: Turns out the branches are actually faster than the branchless black magic. On all targets.
                Go figure...
                */
                // This negate is safe, because int max negated is -2147483647, and int min is -2147483648
                if (negateParam == 1) param = -param;
            }

            if (ch != ' ') --_currentPos;

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
