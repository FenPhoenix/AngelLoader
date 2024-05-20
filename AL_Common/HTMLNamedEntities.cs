using System.Collections.Generic;

namespace AL_Common;

public static class HTML
{
    /*
    GarrettLoader only supports the HTML 4.01 set, so we can save a ton of space and trouble (no multi-byte chars!).
    Some of these keywords can appear with no trailing semicolon and even work if there are other letters directly
    after them with no space. We'd need regex-like behavior to correctly read these and we don't do it at the moment.
    */

    // List of keywords that can appear with no trailing semicolon:
    /*
    Aacute
    aacute
    Acirc
    acirc
    acute
    AElig
    aelig
    Agrave
    agrave
    AMP
    amp
    Aring
    aring
    Atilde
    atilde
    Auml
    auml
    brvbar
    Ccedil
    ccedil
    cedil
    cent
    COPY
    copy
    curren
    deg
    divide
    Eacute
    eacute
    Ecirc
    ecirc
    Egrave
    egrave
    ETH
    eth
    Euml
    euml
    frac12
    frac14
    frac34
    GT
    gt
    Iacute
    iacute
    Icirc
    icirc
    iexcl
    Igrave
    igrave
    iquest
    Iuml
    iuml
    laquo
    LT
    lt
    macr
    micro
    middot
    nbsp
    not
    Ntilde
    ntilde
    Oacute
    oacute
    Ocirc
    ocirc
    Ograve
    ograve
    ordf
    ordm
    Oslash
    oslash
    Otilde
    otilde
    Ouml
    ouml
    para
    plusmn
    pound
    QUOT
    quot
    raquo
    REG
    reg
    sect
    shy
    sup1
    sup2
    sup3
    szlig
    THORN
    thorn
    times
    Uacute
    uacute
    Ucirc
    ucirc
    Ugrave
    ugrave
    uml
    Uuml
    uuml
    Yacute
    yacute
    yen
    yuml
    */
    private static Dictionary<string, string>? _html401NamedEntities;
    public static Dictionary<string, string> HTML401NamedEntities => _html401NamedEntities ??= new Dictionary<string, string>
    {
        { "QUOT", "34" },
        { "quot", "34" },
        { "AMP", "38" },
        { "amp", "38" },
        { "apos", "39" },
        { "GT", "62" },
        { "gt", "62" },
        { "LT", "60" },
        { "lt", "60" },
        { "nbsp", "160" },
        { "iexcl", "161" },
        { "cent", "162" },
        { "pound", "163" },
        { "curren", "164" },
        { "yen", "165" },
        { "brvbar", "166" },
        { "sect", "167" },
        { "COPY", "169" },
        { "copy", "169" },
        { "REG", "174" },
        { "reg", "174" },
        { "Aacute", "193" },
        { "aacute", "225" },
        { "Acirc", "194" },
        { "acirc", "226" },
        { "acute", "180" },
        { "AElig", "198" },
        { "aelig", "230" },
        { "Agrave", "192" },
        { "agrave", "224" },
        { "alefsym", "8501" },
        { "Alpha", "913" },
        { "alpha", "945" },
        { "and", "8743" },
        { "ang", "8736" },
        { "Aring", "197" },
        { "aring", "229" },
        { "asymp", "8776" },
        { "Atilde", "195" },
        { "atilde", "227" },
        { "Auml", "196" },
        { "auml", "228" },
        { "bdquo", "8222" },
        { "Beta", "914" },
        { "beta", "946" },
        { "bull", "8226" },
        { "cap", "8745" },
        { "Ccedil", "199" },
        { "ccedil", "231" },
        { "cedil", "184" },
        { "Chi", "935" },
        { "chi", "967" },
        { "circ", "710" },
        { "clubs", "9827" },
        { "cong", "8773" },
        { "crarr", "8629" },
        { "cup", "8746" },
        { "Dagger", "8225" },
        { "dagger", "8224" },
        { "dArr", "8659" },
        { "darr", "8595" },
        { "deg", "176" },
        { "Delta", "916" },
        { "delta", "948" },
        { "diams", "9830" },
        { "divide", "247" },
        { "Eacute", "201" },
        { "eacute", "233" },
        { "Ecirc", "202" },
        { "ecirc", "234" },
        { "Egrave", "200" },
        { "egrave", "232" },
        { "empty", "8709" },
        { "emsp", "8195" },
        { "ensp", "8194" },
        { "Epsilon", "917" },
        { "epsilon", "949" },
        { "equiv", "8801" },
        { "Eta", "919" },
        { "eta", "951" },
        { "ETH", "208" },
        { "eth", "240" },
        { "Euml", "203" },
        { "euml", "235" },
        { "euro", "8364" },
        { "exist", "8707" },
        { "fnof", "402" },
        { "forall", "8704" },
        { "frac12", "189" },
        { "frac14", "188" },
        { "frac34", "190" },
        { "frasl", "8260" },
        { "Gamma", "915" },
        { "gamma", "947" },
        { "ge", "8805" },
        { "hArr", "8660" },
        { "harr", "8596" },
        { "hearts", "9829" },
        { "hellip", "8230" },
        { "Iacute", "205" },
        { "iacute", "237" },
        { "Icirc", "206" },
        { "icirc", "238" },
        { "Igrave", "204" },
        { "igrave", "236" },
        { "image", "8465" },
        { "infin", "8734" },
        { "int", "8747" },
        { "Iota", "921" },
        { "iota", "953" },
        { "iquest", "191" },
        { "isin", "8712" },
        { "Iuml", "207" },
        { "iuml", "239" },
        { "Kappa", "922" },
        { "kappa", "954" },
        { "Lambda", "923" },
        { "lambda", "955" },
        { "lang", "10216" },
        { "laquo", "171" },
        { "lArr", "8656" },
        { "larr", "8592" },
        { "lceil", "8968" },
        { "ldquo", "8220" },
        { "le", "8804" },
        { "lfloor", "8970" },
        { "lowast", "8727" },
        { "loz", "9674" },
        { "lrm", "8206" },
        { "lsaquo", "8249" },
        { "lsquo", "8216" },
        { "macr", "175" },
        { "mdash", "8212" },
        { "micro", "181" },
        { "middot", "183" },
        { "minus", "8722" },
        { "Mu", "924" },
        { "mu", "956" },
        { "nabla", "8711" },
        { "ndash", "8211" },
        { "ne", "8800" },
        { "ni", "8715" },
        { "not", "172" },
        { "notin", "8713" },
        { "nsub", "8836" },
        { "Ntilde", "209" },
        { "ntilde", "241" },
        { "Nu", "925" },
        { "nu", "957" },
        { "Oacute", "211" },
        { "oacute", "243" },
        { "Ocirc", "212" },
        { "ocirc", "244" },
        { "OElig", "338" },
        { "oelig", "339" },
        { "Ograve", "210" },
        { "ograve", "242" },
        { "oline", "8254" },
        { "Omega", "937" },
        { "omega", "969" },
        { "Omicron", "927" },
        { "omicron", "959" },
        { "oplus", "8853" },
        { "or", "8744" },
        { "ordf", "170" },
        { "ordm", "186" },
        { "Oslash", "216" },
        { "oslash", "248" },
        { "Otilde", "213" },
        { "otilde", "245" },
        { "otimes", "8855" },
        { "Ouml", "214" },
        { "ouml", "246" },
        { "para", "182" },
        { "part", "8706" },
        { "permil", "8240" },
        { "perp", "8869" },
        { "Phi", "934" },
        { "phi", "966" },
        { "Pi", "928" },
        { "pi", "960" },
        { "piv", "982" },
        { "plusmn", "177" },
        { "Prime", "8243" },
        { "prime", "8242" },
        { "prod", "8719" },
        { "prop", "8733" },
        { "Psi", "936" },
        { "psi", "968" },
        { "radic", "8730" },
        { "rang", "10217" },
        { "raquo", "187" },
        { "rArr", "8658" },
        { "rarr", "8594" },
        { "rceil", "8969" },
        { "rdquo", "8221" },
        { "real", "8476" },
        { "rfloor", "8971" },
        { "Rho", "929" },
        { "rho", "961" },
        { "rlm", "8207" },
        { "rsaquo", "8250" },
        { "rsquo", "8217" },
        { "sbquo", "8218" },
        { "Scaron", "352" },
        { "scaron", "353" },
        { "sdot", "8901" },
        { "shy", "173" },
        { "Sigma", "931" },
        { "sigma", "963" },
        { "sigmaf", "962" },
        { "sim", "8764" },
        { "spades", "9824" },
        { "sub", "8834" },
        { "sube", "8838" },
        { "sum", "8721" },
        { "sup", "8835" },
        { "sup1", "185" },
        { "sup2", "178" },
        { "sup3", "179" },
        { "supe", "8839" },
        { "szlig", "223" },
        { "Tau", "932" },
        { "tau", "964" },
        { "there4", "8756" },
        { "Theta", "920" },
        { "theta", "952" },
        { "thetasym", "977" },
        { "thinsp", "8201" },
        { "THORN", "222" },
        { "thorn", "254" },
        { "tilde", "732" },
        { "times", "215" },
        { "TRADE", "8482" },
        { "trade", "8482" },
        { "Uacute", "218" },
        { "uacute", "250" },
        { "uArr", "8657" },
        { "uarr", "8593" },
        { "Ucirc", "219" },
        { "ucirc", "251" },
        { "Ugrave", "217" },
        { "ugrave", "249" },
        { "uml", "168" },
        { "upsih", "978" },
        { "Upsilon", "933" },
        { "upsilon", "965" },
        { "Uuml", "220" },
        { "uuml", "252" },
        { "weierp", "8472" },
        { "Xi", "926" },
        { "xi", "958" },
        { "Yacute", "221" },
        { "yacute", "253" },
        { "Yuml", "376" },
        { "yuml", "255" },
        { "Zeta", "918" },
        { "zeta", "950" },
        { "zwj", "8205" },
        { "zwnj", "8204" },
    };
}
