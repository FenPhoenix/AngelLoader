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

using AL_Common;

namespace Ude.NetStandard;

public enum ProbingState
{
    Detecting = 0, // no sure answer yet, but caller can ask for confidence
    FoundIt = 1,   // positive answer
    NotMe = 2      // negative answer
}

internal abstract class CharsetProber
{
    protected const float SHORTCUT_THRESHOLD = 0.95F;

    protected ProbingState _state;

    // ASCII codes
    private const byte SPACE = 0x20;
    private const byte LESS_THAN = 0x3C;
    private const byte GREATER_THAN = 0x3E;

    /// <summary>
    /// Feed data to the prober
    /// </summary>
    /// <param name="buf">a buffer</param>
    /// <param name="offset">offset into buffer</param>
    /// <param name="len">number of bytes available into buffer</param>
    /// <param name="memoryStream"></param>
    /// <returns>
    /// A <see cref="ProbingState"/>
    /// </returns>
    internal abstract ProbingState HandleData(byte[] buf, int offset, int len, MemoryStreamFast? memoryStream);

    /// <summary>
    /// Reset prober state
    /// </summary>
    internal abstract void Reset();

    internal abstract Charset GetCharsetName();

    internal abstract float GetConfidence();

    internal virtual ProbingState GetState()
    {
        return _state;
    }

    /// <summary>
    /// Helper functions used in the Latin1 and Group probers
    /// </summary>
    /// <returns>filtered buffer</returns>
    protected static byte[] FilterWithoutEnglishLetters(byte[] buf, int offset, int len)
    {
        var ms = new MemoryStreamFast(buf.Length);
        bool meetMSB = false;
        int max = offset + len;
        int prev = offset;
        int cur = offset;

        while (cur < max)
        {
            byte b = buf[cur];

            if ((b & 0x80) != 0)
            {
                meetMSB = true;
            }
            else if (!b.IsAsciiAlpha())
            {
                if (meetMSB && cur > prev)
                {
                    ms.Write(buf, prev, cur - prev);
                    ms.WriteByte(SPACE);
                    meetMSB = false;
                }
                prev = cur + 1;
            }
            cur++;
        }

        if (meetMSB && cur > prev)
        {
            ms.Write(buf, prev, cur - prev);
        }

        ms.SetLength(ms.Position);

        return ms.ToArray();
    }

    /// <summary>
    /// Do filtering to reduce load to probers (Remove ASCII symbols,
    /// collapse spaces). This filter applies to all scripts which contain
    /// both English characters and upper ASCII characters.
    /// </summary>
    /// <returns>a filtered copy of the input buffer</returns>
    protected static MemoryStreamFast FilterWithEnglishLetters(byte[] buf, int offset, int len, MemoryStreamFast? memoryStream)
    {
        var ms = memoryStream ?? new MemoryStreamFast(buf.Length);
        ms.SetLength(0);
        bool inTag = false;
        int max = offset + len;
        int prev = offset;
        int cur = offset;

        while (cur < max)
        {
            byte b = buf[cur];

            if (b == GREATER_THAN)
            {
                inTag = false;
            }
            else if (b == LESS_THAN)
            {
                inTag = true;
            }

            // it's ascii, but it's not a letter
            if ((b & 0x80) == 0 && !b.IsAsciiAlpha())
            {
                if (cur > prev && !inTag)
                {
                    ms.Write(buf, prev, cur - prev);
                    ms.WriteByte(SPACE);
                }
                prev = cur + 1;
            }
            cur++;
        }

        // If the current segment contains more than just a symbol
        // and it is not inside a tag then keep it.
        if (!inTag && cur > prev)
        {
            ms.Write(buf, prev, cur - prev);
        }

        ms.SetLength(ms.Position);

        return ms;
    }
}
