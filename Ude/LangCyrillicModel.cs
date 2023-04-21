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

using System;

namespace Ude.NetStandard;

internal abstract class CyrillicModel : SequenceModel
{
    // Model Table:
    // total sequences: 100%
    // first 512 sequences: 97.6601%
    // first 1024 sequences: 2.3389%
    // rest  sequences:      0.1237%
    // negative sequences:   0.0009%
    /* Original uncompressed:
    private static readonly byte[] RUSSIAN_LANG_MODEL = {
        0,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,1,1,3,3,3,3,1,3,3,3,2,3,2,3,3,
        3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,0,3,2,2,2,2,2,0,0,2,
        3,3,3,2,3,3,3,3,3,3,3,3,3,3,2,3,3,0,0,3,3,3,3,3,3,3,3,3,2,3,2,0,
        0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        3,3,3,2,2,3,3,3,3,3,3,3,3,3,2,3,3,0,0,3,3,3,3,3,3,3,3,2,3,3,1,0,
        0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        3,2,3,2,3,3,3,3,3,3,3,3,3,3,3,3,3,0,0,3,3,3,3,3,3,3,3,3,3,3,2,1,
        0,0,0,0,0,0,0,2,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        3,3,3,3,3,3,3,3,3,3,3,3,3,3,2,3,3,0,0,3,3,3,3,3,3,3,3,3,3,3,2,1,
        0,0,0,0,0,1,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        3,3,3,3,3,3,3,3,2,2,2,3,1,3,3,1,3,3,3,3,2,2,3,0,2,2,2,3,3,2,1,0,
        0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,
        3,3,3,3,3,3,2,3,3,3,3,3,2,2,3,2,3,3,3,2,1,2,2,0,1,2,2,2,2,2,2,0,
        0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,
        3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,2,2,2,3,0,2,2,3,3,2,1,2,0,
        0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,1,0,0,2,0,0,0,0,0,0,0,0,0,
        3,3,3,3,3,3,2,3,3,1,2,3,2,2,3,2,3,3,3,3,2,2,3,0,3,2,2,3,1,1,1,0,
        0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        3,3,3,3,3,3,3,3,2,2,3,3,3,3,3,2,3,3,3,3,2,2,2,0,3,3,3,2,2,2,2,0,
        0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        3,3,3,3,3,3,3,3,3,3,2,3,2,3,3,3,3,3,3,2,3,2,2,0,1,3,2,1,2,2,1,0,
        0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,
        3,3,3,3,3,3,3,3,3,3,3,2,1,1,3,0,1,1,1,1,2,1,1,0,2,2,2,1,2,0,1,0,
        0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        3,3,3,3,3,3,2,3,3,2,2,2,2,1,3,2,3,2,3,2,1,2,2,0,1,1,2,1,2,1,2,0,
        0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        3,3,3,3,3,3,3,3,3,3,3,3,2,2,3,2,3,3,3,2,2,2,2,0,2,2,2,2,3,1,1,0,
        0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,
        3,2,3,2,2,3,3,3,3,3,3,3,3,3,1,3,2,0,0,3,3,3,3,2,3,3,3,3,2,3,2,0,
        0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        2,3,3,3,3,3,2,2,3,3,0,2,1,0,3,2,3,2,3,0,0,1,2,0,0,1,0,1,2,1,1,0,
        0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        3,0,3,0,2,3,3,3,3,2,3,3,3,3,1,2,2,0,0,2,3,2,2,2,3,2,3,2,2,3,0,0,
        0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        3,2,3,0,2,3,2,3,0,1,2,3,3,2,0,2,3,0,0,2,3,2,2,0,1,3,1,3,2,2,1,0,
        0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        3,1,3,0,2,3,3,3,3,3,3,3,3,2,1,3,2,0,0,2,2,3,3,3,2,3,3,0,2,2,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        3,3,3,3,3,3,2,2,3,3,2,2,2,3,3,0,0,1,1,1,1,1,2,0,0,1,1,1,1,0,1,0,
        0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        3,3,3,3,3,3,2,2,3,3,3,3,3,3,3,0,3,2,3,3,2,3,2,0,2,1,0,1,1,0,1,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,
        3,3,3,3,3,3,2,3,3,3,2,2,2,2,3,1,3,2,3,1,1,2,1,0,2,2,2,2,1,3,1,0,
        0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,
        2,2,3,3,3,3,3,1,2,2,1,3,1,0,3,0,0,3,0,0,0,1,1,0,1,2,1,0,0,0,0,0,
        0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        3,2,2,1,1,3,3,3,2,2,1,2,2,3,1,1,2,0,0,2,2,1,3,0,0,2,1,1,2,1,1,0,
        0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        3,2,3,3,3,3,1,2,2,2,1,2,1,3,3,1,1,2,1,2,1,2,2,0,2,0,0,1,1,0,1,0,
        0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        2,3,3,3,3,3,2,1,3,2,2,3,2,0,3,2,0,3,0,1,0,1,1,0,0,1,1,1,1,0,1,0,
        0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        3,3,2,3,3,3,2,2,2,3,3,1,2,1,2,1,0,1,0,1,1,0,1,0,0,2,1,1,1,0,1,0,
        0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,
        3,1,1,2,1,2,3,3,2,2,1,2,2,3,0,2,1,0,0,2,2,3,2,1,2,2,2,2,2,3,1,0,
        0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        3,3,3,3,3,1,1,0,1,1,2,2,1,1,3,0,0,1,3,1,1,1,0,0,0,1,0,1,1,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        2,1,3,3,3,2,0,0,0,2,1,0,1,0,2,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        2,0,1,0,0,2,3,2,2,2,1,2,2,2,1,2,1,0,0,1,1,1,0,2,0,1,1,1,0,0,1,1,
        1,0,0,0,0,0,1,2,0,0,0,0,0,1,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,
        2,3,3,3,3,0,0,0,0,1,0,0,0,0,3,0,1,2,1,0,0,0,0,0,0,0,1,1,0,0,1,1,
        1,0,1,0,1,2,0,0,1,1,2,1,0,1,1,1,1,0,1,1,1,1,0,1,0,0,1,0,0,1,1,0,
        2,2,3,2,2,2,3,1,2,2,2,2,2,2,2,2,1,1,1,1,1,1,1,0,1,0,1,1,1,0,2,1,
        1,1,1,1,1,1,1,1,2,1,1,1,1,1,1,1,1,1,1,0,1,0,1,1,0,1,1,1,0,1,1,0,
        3,3,3,2,2,2,2,3,2,2,1,1,2,2,2,2,1,1,3,1,2,1,2,0,0,1,1,0,1,0,2,1,
        1,1,1,1,1,2,1,0,1,1,1,1,0,1,0,0,1,1,0,0,1,0,1,0,0,1,0,0,0,1,1,0,
        2,0,0,1,0,3,2,2,2,2,1,2,1,2,1,2,0,0,0,2,1,2,2,1,1,2,2,0,1,1,0,2,
        1,1,1,1,1,0,1,1,1,2,1,1,1,2,1,0,1,2,1,1,1,1,0,1,1,1,0,0,1,0,0,1,
        1,3,2,2,2,1,1,1,2,3,0,0,0,0,2,0,2,2,1,0,0,0,0,0,0,1,0,0,0,0,1,1,
        1,0,1,1,0,1,0,1,1,0,1,1,0,2,0,0,1,1,0,0,1,0,0,0,0,0,0,0,0,1,1,0,
        2,3,2,3,2,1,2,2,2,2,1,0,0,0,2,0,0,1,1,0,0,0,0,0,0,0,1,1,0,0,2,1,
        1,1,2,1,0,2,0,0,1,0,1,0,0,1,0,0,1,1,0,1,1,0,0,0,0,0,1,0,0,0,0,0,
        3,0,0,1,0,2,2,2,3,2,2,2,2,2,2,2,0,0,0,2,1,2,1,1,1,2,2,0,0,0,1,2,
        1,1,1,1,1,0,1,2,1,1,1,1,1,1,1,0,1,1,1,1,1,1,0,1,1,1,1,1,1,0,0,1,
        2,3,2,3,3,2,0,1,1,1,0,0,1,0,2,0,1,1,3,1,0,0,0,0,0,0,0,1,0,0,2,1,
        1,1,1,1,1,1,1,0,1,0,1,1,1,1,0,1,1,1,0,0,1,1,0,1,0,0,0,0,0,0,1,0,
        2,3,3,3,3,1,2,2,2,2,0,1,1,0,2,1,1,1,2,1,0,1,1,0,0,1,0,1,0,0,2,0,
        0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        2,3,3,3,2,0,0,1,1,2,2,1,0,0,2,0,1,1,3,0,0,1,0,0,0,0,0,1,0,1,2,1,
        1,1,2,0,1,1,1,0,1,0,1,1,0,1,0,1,1,1,1,0,1,0,0,0,0,0,0,1,0,1,1,0,
        1,3,2,3,2,1,0,0,2,2,2,0,1,0,2,0,1,1,1,0,1,0,0,0,3,0,1,1,0,0,2,1,
        1,1,1,0,1,1,0,0,0,0,1,1,0,1,0,0,2,1,1,0,1,0,0,0,1,0,1,0,0,1,1,0,
        3,1,2,1,1,2,2,2,2,2,2,1,2,2,1,1,0,0,0,2,2,2,0,0,0,1,2,1,0,1,0,1,
        2,1,1,1,1,1,1,1,1,1,1,1,1,1,1,0,2,1,1,1,0,1,0,1,1,0,1,1,1,0,0,1,
        3,0,0,0,0,2,0,1,1,1,1,1,1,1,0,1,0,0,0,1,1,1,0,1,0,1,1,0,0,1,0,1,
        1,1,0,0,1,0,0,0,1,0,1,1,0,0,1,0,1,0,1,0,0,0,0,1,0,0,0,1,0,0,0,1,
        1,3,3,2,2,0,0,0,2,2,0,0,0,1,2,0,1,1,2,0,0,0,0,0,0,0,0,1,0,0,2,1,
        0,1,1,0,0,1,1,0,0,0,1,1,0,1,1,0,1,1,0,0,1,0,0,0,0,0,0,0,0,0,1,0,
        2,3,2,3,2,0,0,0,0,1,1,0,0,0,2,0,2,0,2,0,0,0,0,0,1,0,0,1,0,0,1,1,
        1,1,2,0,1,2,1,0,1,1,2,1,1,1,1,1,2,1,1,0,1,0,0,1,1,1,1,1,0,1,1,0,
        1,3,2,2,2,1,0,0,2,2,1,0,1,2,2,0,0,1,0,0,0,0,0,0,0,0,0,1,0,0,1,1,
        0,0,1,1,0,1,1,0,0,1,1,0,1,1,0,0,1,1,0,0,1,0,0,0,0,0,0,0,0,0,0,0,
        1,0,0,1,0,2,3,1,2,2,2,2,2,2,1,1,0,0,0,1,0,1,0,2,1,1,1,0,0,0,0,1,
        1,1,0,1,1,0,1,1,1,1,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,0,0,0,1,0,0,0,
        2,0,2,0,0,1,0,3,2,1,2,1,2,2,0,1,0,0,0,2,1,0,0,2,1,1,1,1,0,2,0,2,
        2,1,1,1,1,1,1,1,1,1,1,1,1,2,1,0,1,1,1,1,0,0,0,1,1,1,1,0,1,0,0,1,
        1,2,2,2,2,1,0,0,1,0,0,0,0,0,2,0,1,1,1,1,0,0,0,0,1,0,1,2,0,0,2,0,
        1,0,1,1,1,2,1,0,1,0,1,1,0,0,1,0,1,1,1,0,1,0,0,0,1,0,0,1,0,1,1,0,
        2,1,2,2,2,0,3,0,1,1,0,0,0,0,2,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,1,
        0,0,0,1,1,1,0,0,1,0,1,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,0,1,0,0,
        1,2,2,3,2,2,0,0,1,1,2,0,1,2,1,0,1,0,1,0,0,1,0,0,0,0,0,0,0,0,0,1,
        0,1,1,0,0,1,1,0,0,1,1,0,0,1,1,0,1,1,0,0,1,0,0,0,0,0,0,0,0,1,1,0,
        2,2,1,1,2,1,2,2,2,2,2,1,2,2,0,1,0,0,0,1,2,2,2,1,2,1,1,1,1,1,2,1,
        1,1,1,1,1,1,1,1,1,1,0,0,1,1,1,0,1,1,1,0,0,0,0,1,1,1,0,1,1,0,0,1,
        1,2,2,2,2,0,1,0,2,2,0,0,0,0,2,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,2,0,
        0,0,1,0,0,1,0,0,0,0,1,0,1,1,0,0,1,1,0,0,1,0,0,0,0,0,0,0,0,0,0,0,
        0,0,2,0,0,0,0,0,0,0,0,0,0,0,0,0,2,0,0,0,0,0,0,0,0,0,0,1,0,0,0,0,
        0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        1,2,2,2,2,0,0,0,2,2,2,0,1,0,1,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,1,1,
        0,1,1,0,0,1,1,0,0,0,1,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        1,2,2,2,2,0,0,0,0,1,0,0,1,1,2,0,0,0,0,1,0,1,0,0,1,0,0,2,0,0,0,1,
        0,0,1,0,0,1,0,0,0,1,1,0,0,0,0,0,1,0,0,1,1,0,0,0,0,0,0,0,0,0,0,0,
        1,2,2,2,1,1,2,0,2,1,1,1,1,0,2,2,0,0,0,0,0,0,0,0,0,1,1,0,0,0,1,1,
        0,0,1,0,1,1,0,0,0,0,1,0,0,0,0,0,1,1,0,0,1,0,0,0,0,0,0,0,0,0,0,0,
        1,0,2,1,2,0,0,0,0,0,1,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,0,1,0,0,0,0,
        0,0,1,0,1,1,0,0,0,0,1,0,0,0,0,0,1,0,0,0,1,0,0,0,0,0,0,0,0,0,1,0,
        1,0,0,0,0,2,0,1,2,1,0,1,1,1,0,1,0,0,0,1,0,1,0,0,1,0,1,0,0,0,0,1,
        0,0,0,0,0,1,0,0,1,1,0,0,1,1,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,1,
        2,2,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,
        1,0,0,0,1,0,0,0,1,1,0,0,0,0,0,0,0,1,0,0,0,0,0,1,0,0,1,0,0,0,0,0,
        2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,
        1,1,1,0,1,0,1,0,0,1,1,1,1,0,0,0,1,0,0,0,0,1,0,0,0,1,0,1,0,0,0,0,
        1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,
        1,1,0,1,1,0,1,0,1,0,0,0,0,1,1,0,1,1,0,0,0,0,0,1,0,1,1,0,1,0,0,0,
        0,1,1,1,1,0,0,0,0,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,
        0,0,0,0,0,1,0,0,0,0,1,0,0,0,0,0,1,0,0,0,0,0,0,0,0,0,0,0,0,1,0,0
    };
    */
    private static readonly byte[] RUSSIAN_LANG_MODEL_COMPRESSED =
    {
        141, 87, 139, 130, 195, 32, 8, 35, 248, 255, 255, 124, 18, 30, 162, 103, 215, 186, 219, 102, 59, 145, 16,
        2, 246, 100, 28, 3, 224, 231, 124, 235, 124, 61, 12, 25, 106, 67, 68, 185, 174, 198, 156, 138, 180, 171,
        185, 194, 71, 77, 206, 97, 203, 244, 110, 63, 47, 240, 110, 127, 162, 108, 254, 231, 22, 88, 246, 184,
        219, 111, 67, 239, 246, 248, 137, 223, 87, 206, 40, 224, 188, 89, 64, 98, 215, 203, 253, 105, 143, 195,
        94, 99, 15, 78, 20, 147, 89, 104, 48, 124, 181, 215, 195, 126, 15, 194, 253, 115, 163, 155, 61, 254, 219,
        27, 213, 154, 254, 137, 223, 62, 128, 15, 252, 151, 211, 21, 136, 225, 246, 175, 79, 249, 95, 106, 169,
        201, 140, 159, 52, 148, 255, 35, 121, 215, 248, 117, 74, 87, 38, 102, 204, 137, 241, 63, 163, 127, 178,
        63, 253, 171, 163, 5, 65, 56, 255, 115, 151, 7, 254, 30, 240, 175, 252, 89, 220, 74, 65, 124, 192, 175,
        93, 255, 176, 138, 89, 60, 126, 168, 159, 82, 206, 76, 57, 188, 26, 166, 55, 202, 157, 60, 188, 198, 63,
        95, 90, 30, 225, 53, 173, 140, 196, 100, 240, 110, 111, 90, 179, 15, 24, 135, 98, 38, 145, 191, 25, 203,
        163, 254, 155, 61, 194, 191, 103, 208, 34, 214, 104, 42, 162, 143, 86, 205, 190, 226, 103, 197, 9, 5,
        192, 248, 77, 189, 159, 243, 159, 16, 132, 82, 176, 64, 166, 49, 30, 205, 46, 245, 51, 60, 227, 195, 178,
        174, 212, 223, 140, 38, 219, 199, 179, 125, 120, 134, 175, 182, 238, 99, 6, 76, 94, 55, 248, 193, 191,
        178, 99, 155, 220, 233, 218, 182, 132, 101, 1, 61, 255, 191, 250, 167, 187, 55, 185, 143, 17, 178, 87,
        54, 204, 47, 252, 105, 37, 206, 88, 179, 63, 33, 115, 7, 255, 63, 234, 39, 184, 51, 16, 22, 116, 242,
        174, 191, 242, 215, 250, 39, 17, 103, 252, 150, 54, 163, 84, 189, 125, 142, 143, 249, 55, 79, 32, 145,
        98, 194, 37, 109, 30, 197, 235, 80, 146, 207, 137, 31, 19, 31, 52, 187, 217, 51, 214, 65, 254, 153, 3,
        50, 103, 183, 131, 68, 135, 175, 91, 20, 13, 24, 249, 175, 123, 163, 233, 38, 237, 225, 237, 128, 220,
        242, 58, 136, 229, 207, 36, 203, 136, 210, 24, 240, 193, 240, 33, 121, 73, 45, 161, 255, 230, 91, 35,
        251, 60, 101, 232, 246, 195, 251, 38, 215, 105, 26, 47, 175, 244, 236, 238, 233, 223, 102, 222, 124, 179,
        225, 178, 241, 123, 15, 142, 13, 36, 0, 168, 55, 181, 240, 238, 219, 13, 7, 237, 189, 202, 250, 46, 26,
        71, 1, 50, 0, 135, 168, 155, 170, 32, 217, 240, 221, 46, 22, 20, 127, 225, 84, 23, 102, 148, 46, 252,
        115, 240, 233, 192, 89, 140, 3, 79, 29, 171, 50, 111, 129, 127, 241, 186, 125, 89, 215, 100, 223, 140,
        120, 108, 50, 90, 150, 139, 117, 89, 201, 235, 133, 9, 169, 242, 45, 186, 20, 139, 228, 111, 231, 135,
        203, 131, 165, 35, 94, 3, 177, 185, 7, 146, 42, 90, 32, 210, 185, 205, 201, 159, 21, 157, 74, 202, 22,
        174, 196, 196, 31, 132, 85, 89, 75, 148, 23, 111, 153, 90, 66, 124, 76, 187, 248, 86, 164, 78, 26, 113,
        201, 131, 110, 2, 20, 68, 218, 23, 77, 178, 224, 202, 210, 201, 186, 17, 194, 200, 55, 76, 191, 116, 234,
        101, 134, 117, 230, 67, 36, 185, 12, 177, 34, 119, 89, 75, 52, 79, 105, 135, 206, 87, 24, 71, 19, 84, 15,
        37, 34, 209, 44, 131, 148, 230, 112, 233, 41, 181, 173, 251, 230, 233, 124, 255, 218, 154, 153, 203, 38,
        235, 55, 59, 151, 211, 212, 10, 32, 123, 218, 102, 156, 136, 89, 129, 222, 246, 121, 171, 18, 199, 114,
        106, 67, 215, 78, 85, 206, 81, 58, 144, 149, 136, 20, 143, 119, 183, 176, 107, 9, 65, 226, 102, 62, 249,
        180, 24, 26, 249, 247, 152, 158, 249, 204, 188, 237, 33, 176, 128, 248, 184, 33, 197, 179, 96, 231, 176,
        72, 107, 28, 214, 175, 96, 128, 113, 90, 100, 252, 209, 136, 143, 174, 151, 221, 116, 227, 53, 226, 103,
        11, 184, 225, 215, 82, 194, 6, 69, 206, 53, 215, 171, 47, 199, 143, 255, 159, 32, 89, 127, 231, 230, 232,
        250, 189, 238, 154, 246, 145, 204, 0, 42, 217, 60, 80, 248, 87, 215, 219, 206, 69, 146, 5, 77, 193, 44,
        248, 89, 51, 117, 144, 162, 110, 239, 250, 213, 118, 184, 157, 217, 13, 56, 221, 126, 11, 33, 84, 145,
        37, 214, 91, 139, 236, 70, 69, 195, 65, 144, 238, 207, 89, 231, 88, 101, 211, 186, 114, 241, 34, 175,
        199, 125, 246, 110, 233, 37, 184, 80, 190, 38, 121, 29, 96, 129, 166, 210, 128, 186, 135, 206, 199, 125,
        151, 13, 248, 118, 255, 15
    };
    private static readonly byte[] RUSSIAN_LANG_MODEL;

    static CyrillicModel()
    {
        RUSSIAN_LANG_MODEL = Utils.Decompress(RUSSIAN_LANG_MODEL_COMPRESSED, 4096);
        RUSSIAN_LANG_MODEL_COMPRESSED = Array.Empty<byte>();
    }

    protected CyrillicModel(byte[] charToOrderMap, Charset name)
        : base(charToOrderMap, RUSSIAN_LANG_MODEL, 0.976601f, name)
    {
    }
}

internal sealed class Koi8rModel : CyrillicModel
{
    private static readonly byte[] KOI8R_CHAR_TO_ORDER_MAP = {
        255,255,255,255,255,255,255,255,255,255,254,255,255,254,255,255,  //00
        255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,  //10
        +253,253,253,253,253,253,253,253,253,253,253,253,253,253,253,253,  //20
        252,252,252,252,252,252,252,252,252,252,253,253,253,253,253,253,  //30
        253,142,143,144,145,146,147,148,149,150,151,152, 74,153, 75,154,  //40
        155,156,157,158,159,160,161,162,163,164,165,253,253,253,253,253,  //50
        253, 71,172, 66,173, 65,174, 76,175, 64,176,177, 77, 72,178, 69,  //60
        67,179, 78, 73,180,181, 79,182,183,184,185,253,253,253,253,253,  //70
        191,192,193,194,195,196,197,198,199,200,201,202,203,204,205,206,  //80
        207,208,209,210,211,212,213,214,215,216,217,218,219,220,221,222,  //90
        223,224,225, 68,226,227,228,229,230,231,232,233,234,235,236,237,  //a0
        238,239,240,241,242,243,244,245,246,247,248,249,250,251,252,253,  //b0
        27,  3, 21, 28, 13,  2, 39, 19, 26,  4, 23, 11,  8, 12,  5,  1,  //c0
        15, 16,  9,  7,  6, 14, 24, 10, 17, 18, 20, 25, 30, 29, 22, 54,  //d0
        59, 37, 44, 58, 41, 48, 53, 46, 55, 42, 60, 36, 49, 38, 31, 34,  //e0
        35, 43, 45, 32, 40, 52, 56, 33, 61, 62, 51, 57, 47, 63, 50, 70   //f0
    };

    internal Koi8rModel() : base(KOI8R_CHAR_TO_ORDER_MAP, Charset.KOI8_R)
    {
    }
}

internal sealed class Win1251Model : CyrillicModel
{
    private static readonly byte[] WIN1251_CHAR_TO_ORDER_MAP = {
        255,255,255,255,255,255,255,255,255,255,254,255,255,254,255,255,  //00
        255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,  //10
        +253,253,253,253,253,253,253,253,253,253,253,253,253,253,253,253,  //20
        252,252,252,252,252,252,252,252,252,252,253,253,253,253,253,253,  //30
        253,142,143,144,145,146,147,148,149,150,151,152, 74,153, 75,154,  //40
        155,156,157,158,159,160,161,162,163,164,165,253,253,253,253,253,  //50
        253, 71,172, 66,173, 65,174, 76,175, 64,176,177, 77, 72,178, 69,  //60
        67,179, 78, 73,180,181, 79,182,183,184,185,253,253,253,253,253,  //70
        191,192,193,194,195,196,197,198,199,200,201,202,203,204,205,206,
        207,208,209,210,211,212,213,214,215,216,217,218,219,220,221,222,
        223,224,225,226,227,228,229,230,231,232,233,234,235,236,237,238,
        239,240,241,242,243,244,245,246, 68,247,248,249,250,251,252,253,
        37, 44, 33, 46, 41, 48, 56, 51, 42, 60, 36, 49, 38, 31, 34, 35,
        45, 32, 40, 52, 53, 55, 58, 50, 57, 63, 70, 62, 61, 47, 59, 43,
        3, 21, 10, 19, 13,  2, 24, 20,  4, 23, 11,  8, 12,  5,  1, 15,
        9,  7,  6, 14, 39, 26, 28, 22, 25, 29, 54, 18, 17, 30, 27, 16
    };

    internal Win1251Model() : base(WIN1251_CHAR_TO_ORDER_MAP, Charset.Windows1251)
    {
    }
}

internal sealed class Latin5Model : CyrillicModel
{
    private static readonly byte[] LATIN5_CHAR_TO_ORDER_MAP = {
        255,255,255,255,255,255,255,255,255,255,254,255,255,254,255,255,  //00
        255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,  //10
        +253,253,253,253,253,253,253,253,253,253,253,253,253,253,253,253,  //20
        252,252,252,252,252,252,252,252,252,252,253,253,253,253,253,253,  //30
        253,142,143,144,145,146,147,148,149,150,151,152, 74,153, 75,154,  //40
        155,156,157,158,159,160,161,162,163,164,165,253,253,253,253,253,  //50
        253, 71,172, 66,173, 65,174, 76,175, 64,176,177, 77, 72,178, 69,  //60
        67,179, 78, 73,180,181, 79,182,183,184,185,253,253,253,253,253,  //70
        191,192,193,194,195,196,197,198,199,200,201,202,203,204,205,206,
        207,208,209,210,211,212,213,214,215,216,217,218,219,220,221,222,
        223,224,225,226,227,228,229,230,231,232,233,234,235,236,237,238,
        37, 44, 33, 46, 41, 48, 56, 51, 42, 60, 36, 49, 38, 31, 34, 35,
        45, 32, 40, 52, 53, 55, 58, 50, 57, 63, 70, 62, 61, 47, 59, 43,
        3, 21, 10, 19, 13,  2, 24, 20,  4, 23, 11,  8, 12,  5,  1, 15,
        9,  7,  6, 14, 39, 26, 28, 22, 25, 29, 54, 18, 17, 30, 27, 16,
        239, 68,240,241,242,243,244,245,246,247,248,249,250,251,252,255
    };

    internal Latin5Model() : base(LATIN5_CHAR_TO_ORDER_MAP, Charset.ISO_8859_5)
    {
    }
}

internal sealed class MacCyrillicModel : CyrillicModel
{
    private static readonly byte[] MACCYRILLIC_CHAR_TO_ORDER_MAP = {
        255,255,255,255,255,255,255,255,255,255,254,255,255,254,255,255,  //00
        255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,  //10
        +253,253,253,253,253,253,253,253,253,253,253,253,253,253,253,253,  //20
        252,252,252,252,252,252,252,252,252,252,253,253,253,253,253,253,  //30
        253,142,143,144,145,146,147,148,149,150,151,152, 74,153, 75,154,  //40
        155,156,157,158,159,160,161,162,163,164,165,253,253,253,253,253,  //50
        253, 71,172, 66,173, 65,174, 76,175, 64,176,177, 77, 72,178, 69,  //60
        67,179, 78, 73,180,181, 79,182,183,184,185,253,253,253,253,253,  //70
        37, 44, 33, 46, 41, 48, 56, 51, 42, 60, 36, 49, 38, 31, 34, 35,
        45, 32, 40, 52, 53, 55, 58, 50, 57, 63, 70, 62, 61, 47, 59, 43,
        191,192,193,194,195,196,197,198,199,200,201,202,203,204,205,206,
        207,208,209,210,211,212,213,214,215,216,217,218,219,220,221,222,
        223,224,225,226,227,228,229,230,231,232,233,234,235,236,237,238,
        239,240,241,242,243,244,245,246,247,248,249,250,251,252, 68, 16,
        3, 21, 10, 19, 13,  2, 24, 20,  4, 23, 11,  8, 12,  5,  1, 15,
        9,  7,  6, 14, 39, 26, 28, 22, 25, 29, 54, 18, 17, 30, 27,255
    };

    internal MacCyrillicModel() : base(MACCYRILLIC_CHAR_TO_ORDER_MAP,
        Charset.X_MAC_CYRILLIC)
    {
    }
}

internal sealed class Ibm855Model : CyrillicModel
{
    private static readonly byte[] IBM855_BYTE_TO_ORDER_MAP = {
        255,255,255,255,255,255,255,255,255,255,254,255,255,254,255,255,  //00
        255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,  //10
        +253,253,253,253,253,253,253,253,253,253,253,253,253,253,253,253,  //20
        252,252,252,252,252,252,252,252,252,252,253,253,253,253,253,253,  //30
        253,142,143,144,145,146,147,148,149,150,151,152, 74,153, 75,154,  //40
        155,156,157,158,159,160,161,162,163,164,165,253,253,253,253,253,  //50
        253, 71,172, 66,173, 65,174, 76,175, 64,176,177, 77, 72,178, 69,  //60
        67,179, 78, 73,180,181, 79,182,183,184,185,253,253,253,253,253,  //70
        191,192,193,194, 68,195,196,197,198,199,200,201,202,203,204,205,
        206,207,208,209,210,211,212,213,214,215,216,217, 27, 59, 54, 70,
        3, 37, 21, 44, 28, 58, 13, 41,  2, 48, 39, 53, 19, 46,218,219,
        220,221,222,223,224, 26, 55,  4, 42,225,226,227,228, 23, 60,229,
        230,231,232,233,234,235, 11, 36,236,237,238,239,240,241,242,243,
        8, 49, 12, 38,  5, 31,  1, 34, 15,244,245,246,247, 35, 16,248,
        43,  9, 45,  7, 32,  6, 40, 14, 52, 24, 56, 10, 33, 17, 61,249,
        250, 18, 62, 20, 51, 25, 57, 30, 47, 29, 63, 22, 50,251,252,255
    };

    internal Ibm855Model() : base(IBM855_BYTE_TO_ORDER_MAP, Charset.IBM855)
    {
    }
}

internal sealed class Ibm866Model : CyrillicModel
{
    private static readonly byte[] IBM866_CHAR_TO_ORDER_MAP = {
        255,255,255,255,255,255,255,255,255,255,254,255,255,254,255,255,  //00
        255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,255,  //10
        +253,253,253,253,253,253,253,253,253,253,253,253,253,253,253,253,  //20
        252,252,252,252,252,252,252,252,252,252,253,253,253,253,253,253,  //30
        253,142,143,144,145,146,147,148,149,150,151,152, 74,153, 75,154,  //40
        155,156,157,158,159,160,161,162,163,164,165,253,253,253,253,253,  //50
        253, 71,172, 66,173, 65,174, 76,175, 64,176,177, 77, 72,178, 69,  //60
        67,179, 78, 73,180,181, 79,182,183,184,185,253,253,253,253,253,  //70
        37, 44, 33, 46, 41, 48, 56, 51, 42, 60, 36, 49, 38, 31, 34, 35,
        45, 32, 40, 52, 53, 55, 58, 50, 57, 63, 70, 62, 61, 47, 59, 43,
        3, 21, 10, 19, 13,  2, 24, 20,  4, 23, 11,  8, 12,  5,  1, 15,
        191,192,193,194,195,196,197,198,199,200,201,202,203,204,205,206,
        207,208,209,210,211,212,213,214,215,216,217,218,219,220,221,222,
        223,224,225,226,227,228,229,230,231,232,233,234,235,236,237,238,
        9,  7,  6, 14, 39, 26, 28, 22, 25, 29, 54, 18, 17, 30, 27, 16,
        239, 68,240,241,242,243,244,245,246,247,248,249,250,251,252,255
    };

    internal Ibm866Model() : base(IBM866_CHAR_TO_ORDER_MAP, Charset.IBM866)
    {
    }
}
