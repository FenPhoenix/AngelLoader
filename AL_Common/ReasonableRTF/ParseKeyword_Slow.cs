using System.Runtime.CompilerServices;
using ReasonableRTF.Enums;
using ReasonableRTF.Extensions;
using ReasonableRTF.Models.Symbols;

namespace ReasonableRTF;

public sealed partial class RtfToTextConverter
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private RtfError ParseKeyword_Slow(ref byte bufferRef)
    {
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
            int param = 0;
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
    private RtfError HandleControlChar(ref byte bufferRef, char ch)
    {
        /*
        From the spec:
        "A control symbol consists of a backslash followed by a single, non-alphabetical character.
        For example, \~ (backslash tilde) represents a non-breaking space. Control symbols do not have
        delimiters, i.e., a space following a control symbol is treated as text, not a delimiter."
        */

        // Fast path for destination marker - claws us back a small amount of perf
        if (ch == '*')
        {
            _skipDestinationIfUnknown = true;
            return RtfError.OK;
        }

        char symbol = LookUpControlSymbol((byte)ch);

        if (symbol == 0)
        {
            /*
            NOTE(Control symbol skippable destination check):
            Technically, only control words (not control symbols) can be destinations, so we don't necessarily
            have to check for a skippable destination here by spec. LibreOffice skips unknown control symbol
            "destinations", while RichEdit fails the whole read. So we'd be within reason to assume this will
            never happen. But if we do, then any text inside a skippable control word "destination" group WILL
            be output. It's a vanishingly unlikely scenario, but the perf loss from this check is also tiny.
            So let's just leave it in for now.
            */
            if (_skipDestinationIfUnknown)
            {
                _skipDestinationIfUnknown = false;
                SkipDest(ref bufferRef);
            }
            return RtfError.OK;
        }

        _skipDestinationIfUnknown = false;

        return DispatchControlSymbol(ref bufferRef, symbol);
    }
}
