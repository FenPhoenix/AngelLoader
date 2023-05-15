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
 * The Original Code is mozilla.org code.
 *
 * The Initial Developer of the Original Code is
 * Netscape Communications Corporation.
 * Portions created by the Initial Developer are Copyright (C) 1998
 * the Initial Developer. All Rights Reserved.
 *
 * Contributor(s):
 *           Kohei TAKETA <k-tak@void.in> (Java port)
 *           Rudi Pettazzi <rudi.pettazzi@gmail.com> (C# port)
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

// Escaped charsets state machines

namespace Ude.NetStandard;

internal sealed class HZSMModel : SMModel
{
    private static readonly int[] HZ_cls =
    {
        1,
        0,
        0,
        4096,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        38813696,
        286331153,
        286331153,
        286331153,
        286331153,
        286331153,
        286331153,
        286331153,
        286331153,
        286331153,
        286331153,
        286331153,
        286331153,
        286331153,
        286331153,
        286331153,
        286331153
    };

    private static readonly int[] HZ_st =
    {
        285213456,
        572657937,
        335548706,
        341120533,
        336872468,
        36
    };

    private static readonly int[] HZCharLenTable = { 0, 0, 0, 0, 0, 0 };

    internal HZSMModel() : base(
        HZ_cls,
        6,
        HZ_st,
        HZCharLenTable, Charset.HZ_GB_2312)
    {
    }
}

internal sealed class ISO2022JPSMModel : SMModel
{
    private static readonly int[] ISO2022JP_cls =
    {
        2,
        570425344,
        0,
        4096,
        458752,
        3,
        0,
        0,
        525318,
        1424,
        0,
        0,
        0,
        0,
        0,
        0,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306
    };

    private static readonly int[] ISO2022JP_st =
    {
        304,
        286331136,
        572657937,
        287449634,
        289476945,
        303194385,
        571543825,
        286335249,
        1184017
    };

    private static readonly int[] ISO2022JPCharLenTable = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    internal ISO2022JPSMModel() : base(
        ISO2022JP_cls,
        10,
        ISO2022JP_st,
        ISO2022JPCharLenTable, Charset.ISO_2022_JP)
    {
    }
}

internal sealed class ISO2022KRSMModel : SMModel
{
    private static readonly int[] ISO2022KR_cls =
    {
        2,
        0,
        0,
        4096,
        196608,
        64,
        0,
        0,
        20480,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306
    };

    private static readonly int[] ISO2022KR_st =
    {
        285212976,
        572657937,
        289476898,
        286593297,
        8465
    };

    private static readonly int[] ISO2022KRCharLenTable = { 0, 0, 0, 0, 0, 0 };

    internal ISO2022KRSMModel() : base(
        ISO2022KR_cls,
        6,
        ISO2022KR_st,
        ISO2022KRCharLenTable, Charset.ISO_2022_KR)
    {
    }
}
