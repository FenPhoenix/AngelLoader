using System.Runtime.CompilerServices;
using ReasonableRTF_Displayed.Enums;
using static AL_Common.RTFParserCommon;

namespace ReasonableRTF_Displayed;

public sealed partial class RRTF_RtfDisplayedReadmeParser
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private RtfError ParseKeyword_Slow(ref byte bufferRef)
    {
        char ch = (char)GetByte(IncrementCurrentPos());

        if (!ch.IsAsciiAlpha())
        {
            return HandleControlChar(ch);
        }
        else
        {
            Symbol? symbol;
            ref byte keywordRef = ref GetArrayDataReference(_keyword);

            Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref keywordRef, (nint)0), (byte)ch);
            ch = (char)GetByte(IncrementCurrentPos());

            byte keywordCount;
            for (keywordCount = 1;
                 keywordCount < KeywordMaxLen + 1 && ch.IsAsciiAlpha();
                 keywordCount++, ch = (char)GetByte(IncrementCurrentPos()))
            {
                Unsafe.WriteUnaligned(ref Unsafe.AddByteOffset(ref keywordRef, (nint)keywordCount), (byte)ch);
            }
            if (keywordCount > KeywordMaxLen)
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
            int param = 0;
            if (ch.IsAsciiNumeric())
            {
                hasParam = true;
                long longParam = ch - '0';
                ch = (char)GetByte(IncrementCurrentPos());

                int paramLength;
                for (paramLength = 1;
                     paramLength < ParamMaxLen + 1 && ch.IsAsciiNumeric();
                     paramLength++, ch = (char)GetByte(IncrementCurrentPos()))
                {
                    longParam = (longParam * 10) + (ch - '0');
                }
                if (paramLength > ParamMaxLen || longParam > int.MaxValue)
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
                    symbol = _fontSymbol;
                    _skipDestinationIfUnknown = false;
                    return DispatchKeyword(ref bufferRef, symbol, param, hasParam);
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

            return DispatchKeyword(ref bufferRef, symbol, param, hasParam);
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private RtfError HandleControlChar(char ch)
    {
        /*
        From the spec:
        "A control symbol consists of a backslash followed by a single, non-alphabetical character.
        For example, \~ (backslash tilde) represents a non-breaking space. Control symbols do not have
        delimiters, i.e., a space following a control symbol is treated as text, not a delimiter."
        */
        _skipDestinationIfUnknown = ch == '*';
        return RtfError.OK;
    }
}
