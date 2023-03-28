/* ***** BEGIN LICENSE BLOCK *****
 * Version: MPL 1.1/GPL 2.0/LGPL 2.1
 *
 * The contents of this file are subject to the Mozilla Public License Version
 * 1.1 (the "License"); you may not use this file except in compliance with
 * the License. You may obtain a copy of the License at
 * http://www.mozilla.org/MPL/
 *
 * Software distributed under the License is distributed on an "AS IS" basis,
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
 * for the specific language governing rights and limitations under the
 * License.
 *
 * The Original Code is Mozilla Universal charset detector code.
 *
 * The Initial Developer of the Original Code is
 * Netscape Communications Corporation.
 * Portions created by the Initial Developer are Copyright (C) 2001
 * the Initial Developer. All Rights Reserved.
 *
 * Contributor(s):
 *          Shy Shalom <shooshX@gmail.com>
 *          Rudi Pettazzi <rudi.pettazzi@gmail.com> (C# port)
 *
 * Alternatively, the contents of this file may be used under the terms of
 * either the GNU General Public License Version 2 or later (the "GPL"), or
 * the GNU Lesser General Public License Version 2.1 or later (the "LGPL"),
 * in which case the provisions of the GPL or the LGPL are applicable instead
 * of those above. If you wish to allow use of your version of this file only
 * under the terms of either the GPL or the LGPL, and not to allow others to
 * use your version of this file under the terms of the MPL, indicate your
 * decision by deleting the provisions above and replace them with the notice
 * and other provisions required by the GPL or the LGPL. If you do not delete
 * the provisions above, a recipient may use your version of this file under
 * the terms of any one of the MPL, the GPL or the LGPL.
 *
 * ***** END LICENSE BLOCK ***** */

namespace Ude.NetStandard;

public enum Charset
{
    Null,
    ASCII,
    Big5,
    EUC_JP,
    EUC_KR,
    HZ_GB_2312,
    IBM855,
    IBM866,
    ISO_2022_JP,
    ISO_2022_KR,
    ISO_8859_5,
    ISO_8859_7,
    ISO_8859_8, // Visual Hebrew
    KOI8_R,
    ShiftJIS,
    UTF16BE,
    UTF16LE,
    UTF32BE,
    UTF32LE,
    UTF8,
    GB_18030,
    Windows1251,
    Windows1252,
    Windows1253,
    Windows1255, // Logical Hebrew
    X_MAC_CYRILLIC
}

/// <summary>
/// <para>
/// Default implementation of charset detection interface.
/// The detector can be fed by a System.IO.Stream:
/// <example>
/// <code>
/// using (FileStream fs = File.OpenRead(filename)) {
///    CharsetDetector cdet = new CharsetDetector();
///    cdet.Feed(fs);
///    cdet.DataEnd();
///    Console.WriteLine("{0}, {1}", cdet.Charset, cdet.Confidence);
/// </code>
/// </example>
/// </para>
/// <para> or by a byte a array:</para>
/// <para>
/// <example>
/// <code>
/// byte[] buff = new byte[1024];
/// int read;
/// while ((read = stream.Read(buff, 0, buff.Length)) > 0 && !done)
///     Feed(buff, 0, read);
/// cdet.DataEnd();
/// Console.WriteLine("{0}, {1}", cdet.Charset, cdet.Confidence);
/// </code>
/// </example>
/// </para>
/// </summary>
public sealed class CharsetDetector : UniversalDetector
{
    // @Ude: Static array, probably we want to keep it static because we use it all the time
    // IMPORTANT: These must be in the exact same order as the Ude.Charset enum members! (Done for perf)
    public static readonly string[] CharsetToName =
    {
        "",
        "ASCII",
        "Big5",
        "EUC-JP",
        "EUC-KR",
        "HZ-GB-2312",
        "IBM855",
        "IBM866",
        "ISO-2022-JP",
        "ISO-2022-KR",
        "ISO-8859-5",
        "ISO-8859-7",
        "ISO-8859-8", // Visual Hebrew
        "KOI8-R",
        "Shift-JIS",
        "UTF-16BE",
        "UTF-16LE",
        "UTF-32BE",
        "UTF-32LE",
        "UTF-8",
        "gb18030",
        "windows-1251",
        "windows-1252",
        "windows-1253",
        "windows-1255", // Logical Hebrew
        "x-mac-cyrillic"
    };

    public static readonly int CharsetCount = CharsetToName.Length;

    private Charset _charset;

    private float _confidence;

#if false
    public void Feed(Stream stream)
    {
        byte[] buff = new byte[1024];
        int read;
        while ((read = stream.Read(buff, 0, buff.Length)) > 0 && !_done)
        {
            Feed(buff, 0, read);
        }
    }
#endif

    public bool IsDone() => _done;

    public override void Reset()
    {
        this._charset = Charset.Null;
        this._confidence = 0.0f;
        base.Reset();
    }

    public Charset Charset => _charset;

    public float Confidence => _confidence;

    protected override void Report(Charset charset, float confidence)
    {
        this._charset = charset;
        this._confidence = confidence;
    }
}
