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
 *          Kohei TAKETA <k-tak@void.in> (Java port)
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

internal sealed class BitPackage
{
    private const int INDEX_SHIFT_4BITS = 3;

    private const int SHIFT_MASK_4BITS = 7;

    private const int BIT_SHIFT_4BITS = 2;

    private const int UNIT_MASK_4BITS = 0x0000000F;

    private readonly int[] _data;

    internal BitPackage(int[] data) => _data = data;

    private static int Pack16bits(int a, int b)
    {
        return b << 16 | a;
    }

    private static int Pack8bits(int a, int b, int c, int d)
    {
        return Pack16bits(b << 8 | a, d << 8 | c);
    }

    internal static int Pack4bits(int a, int b, int c, int d,
        int e, int f, int g, int h)
    {
        return Pack8bits(b << 4 | a, d << 4 | c,
            f << 4 | e, h << 4 | g);
    }

    internal int Unpack(int i)
    {
        return _data[i >> INDEX_SHIFT_4BITS] >>
                ((i & SHIFT_MASK_4BITS) << BIT_SHIFT_4BITS) & UNIT_MASK_4BITS;
    }
}
