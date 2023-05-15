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

internal sealed class UTF8SMModel : SMModel
{
    private static readonly int[] UTF8_cls =
    {
        286331153,
        1118481,
        286331153,
        286327057,
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
        858989090,
        1145324612,
        1145324612,
        1145324612,
        1431655765,
        1431655765,
        1431655765,
        1431655765,
        1717986816,
        1717986918,
        1717986918,
        1717986918,
        -2004318073,
        -2003269496,
        -1145324614,
        16702940
    };

    private static readonly int[] UTF8_st =
    {
        -1408167679,
        878082233,
        286331153,
        286331153,
        572662306,
        572662306,
        290805009,
        286331153,
        290803985,
        286331153,
        293041937,
        286331153,
        293015825,
        286331153,
        295278865,
        286331153,
        294719761,
        286331153,
        298634257,
        286331153,
        297865489,
        286331153,
        287099921,
        286331153,
        285212689,
        286331153
    };

    private static readonly int[] UTF8CharLenTable =
        {0, 1, 0, 0, 0, 0, 2, 3, 3, 3, 4, 4, 5, 5, 6, 6 };

    internal UTF8SMModel() : base(
        UTF8_cls,
        16,
        UTF8_st,
        UTF8CharLenTable, Charset.UTF8)
    {
    }
}

internal sealed class GB18030SMModel : SMModel
{
    private static readonly int[] GB18030_cls =
    {
        286331153,
        1118481,
        286331153,
        286327057,
        286331153,
        286331153,
        858993459,
        286331187,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306,
        1109533218,
        1717986917,
        1717986918,
        1717986918,
        1717986918,
        1717986918,
        1717986918,
        1717986918,
        1717986918,
        1717986918,
        1717986918,
        1717986918,
        1717986918,
        1717986918,
        1717986918,
        1717986918,
        107374182
    };

    private static readonly int[] GB18030_st =
    {
        318767105,
        571543825,
        17965602,
        286326804,
        303109393,
        17
    };

    // To be accurate, the length of class 6 can be either 2 or 4.
    // But it is not necessary to discriminate between the two since
    // it is used for frequency analysis only, and we are validating
    // each code range there as well. So it is safe to set it to be
    // 2 here.
    private static readonly int[] GB18030CharLenTable = { 0, 1, 1, 1, 1, 1, 2 };

    internal GB18030SMModel() : base(
        GB18030_cls,
        7,
        GB18030_st,
        GB18030CharLenTable, Charset.GB_18030)
    {
    }
}

internal sealed class BIG5SMModel : SMModel
{
    private static readonly int[] BIG5_cls =
    {
        286331153,
        1118481,
        286331153,
        286327057,
        286331153,
        286331153,
        286331153,
        286331153,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306,
        304226850,
        1145324612,
        1145324612,
        1145324612,
        1145324612,
        858993460,
        858993459,
        858993459,
        858993459,
        858993459,
        858993459,
        858993459,
        858993459,
        858993459,
        858993459,
        858993459,
        53687091
    };

    private static readonly int[] BIG5_st =
    {
        286339073,
        304226833,
        1
    };

    private static readonly int[] BIG5CharLenTable = { 0, 1, 1, 2, 0 };

    internal BIG5SMModel() : base(
        BIG5_cls,
        5,
        BIG5_st,
        BIG5CharLenTable, Charset.Big5)
    {
    }
}

internal sealed class EUCJPSMModel : SMModel
{
    private static readonly int[] EUCJP_cls =
    {
        1145324612,
        1430537284,
        1145324612,
        1145328708,
        1145324612,
        1145324612,
        1145324612,
        1145324612,
        1145324612,
        1145324612,
        1145324612,
        1145324612,
        1145324612,
        1145324612,
        1145324612,
        1145324612,
        1431655765,
        827675989,
        1431655765,
        1431655765,
        572662309,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306,
        0,
        0,
        0,
        1342177280
    };

    private static readonly int[] EUCJP_st =
    {
        286282563,
        572657937,
        286265378,
        319885329,
        4371
    };

    private static readonly int[] EUCJPCharLenTable = { 2, 2, 2, 3, 1, 0 };

    internal EUCJPSMModel() : base(
        EUCJP_cls,
        6,
        EUCJP_st,
        EUCJPCharLenTable, Charset.EUC_JP)
    {
    }
}

internal sealed class EUCKRSMModel : SMModel
{
    private static readonly int[] EUCKR_cls =
    {
        286331153,
        1118481,
        286331153,
        286327057,
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
        0,
        0,
        0,
        0,
        572662304,
        858923554,
        572662306,
        572662306,
        572662306,
        572662322,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306,
        35791394
    };

    private static readonly int[] EUCKR_st =
    {
        286331649,
        1122850
    };

    private static readonly int[] EUCKRCharLenTable = { 0, 1, 2, 0 };

    internal EUCKRSMModel() : base(
        EUCKR_cls,
        4,
        EUCKR_st,
        EUCKRCharLenTable, Charset.EUC_KR)
    {
    }
}

internal sealed class SJISSMModel : SMModel
{
    private static readonly int[] SJIS_cls =
    {
        286331153,
        1118481,
        286331153,
        286327057,
        286331153,
        286331153,
        286331153,
        286331153,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306,
        304226850,
        858993459,
        858993459,
        858993459,
        858993459,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306,
        572662306,
        858993459,
        1145254707,
        1145324612,
        279620
    };

    private static readonly int[] SJIS_st =
    {
        286339073,
        572657937,
        4386
    };

    private static readonly int[] SJISCharLenTable = { 0, 1, 1, 2, 0, 0 };

    internal SJISSMModel() : base(
        SJIS_cls,
        6,
        SJIS_st,
        SJISCharLenTable, Charset.ShiftJIS)
    {
    }
}
