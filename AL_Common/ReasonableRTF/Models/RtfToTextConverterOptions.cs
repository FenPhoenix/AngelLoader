/*
 * MIT License
 * 
 * Copyright (c) 2024-2026 Brian Tobin
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
*/

using ReasonableRTF.Enums;

namespace ReasonableRTF.Models;

/// <summary>
/// The options for the conversion.
/// </summary>
public sealed class RtfToTextConverterOptions
{
    internal bool _swapUppercaseAndLowercasePhiSymbols = true;
    internal SymbolFontA0Char _symbolFontA0Char = SymbolFontA0Char.EuroSign;
    internal LineBreakStyle _lineBreakStyle = LineBreakStyle.EnvironmentDefault;
    internal bool _convertHiddenText;
    internal ushort _defaultCodePage = 1252;

    /// <summary>
    /// Gets or sets whether to swap the uppercase and lowercase Greek phi characters in the Symbol font to Unicode
    /// translation table.
    /// <para/>
    /// The Windows Symbol font has these two characters swapped from their nominal positions.
    /// You can disable this by setting this property to <see langword="false"/>.
    /// <para/>
    /// The default value is <see langword="true"/>.
    /// </summary>
    public bool SwapUppercaseAndLowercasePhiSymbols
    {
        get => _swapUppercaseAndLowercasePhiSymbols;
        set => _swapUppercaseAndLowercasePhiSymbols = value;
    }

    /// <summary>
    /// Gets or sets the character at index 0xA0 (160) in the Symbol font to Unicode translation table.
    /// <para/>
    /// This character is nominally the Euro sign, but in older versions of the Symbol font it may have been a
    /// numeric space or undefined.
    /// <para/>
    /// The default value is <see cref="SymbolFontA0Char.EuroSign"/>.
    /// </summary>
    public SymbolFontA0Char SymbolFontA0Char
    {
        get => _symbolFontA0Char;
        set => _symbolFontA0Char = value;
    }

    /// <summary>
    /// Gets or sets the line break style for the converted plain text.
    /// <para/>
    /// The default value is <see cref="LineBreakStyle.EnvironmentDefault"/>.
    /// </summary>
    public LineBreakStyle LineBreakStyle
    {
        get => _lineBreakStyle;
        set => _lineBreakStyle = value;
    }

    /// <summary>
    /// Gets or sets whether to convert text that is marked as hidden. If <see langword="true"/>, this text will
    /// appear in the plain text output; otherwise, it will not.
    /// <para/>
    /// The default value is <see langword="false"/>.
    /// </summary>
    public bool ConvertHiddenText
    {
        get => _convertHiddenText;
        set => _convertHiddenText = value;
    }

    /// <summary>
    /// Gets or sets the code page to use when an rtf file requests the "default code page".
    /// <para/>
    /// If set to 0, the default code page will be determined automatically based on your OS and which version of
    /// .NET you're using.
    /// <br/>
    /// On .NET Framework for Windows, it will normally be your Windows ANSI codepage (1252, for example).
    /// On .NET, it will normally be UTF-8, which is probably not what you want. Hence, it's recommended to set
    /// this property to something other than 0.
    /// <para/>
    /// The default value is 1252.
    /// </summary>
    public ushort DefaultCodePage
    {
        get => _defaultCodePage;
        set => _defaultCodePage = value;
    }
}
