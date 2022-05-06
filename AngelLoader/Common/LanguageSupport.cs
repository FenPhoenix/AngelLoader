#define FenGen_LanguageSupportSource

using System;
using static AngelLoader.FenGenAttributes;

namespace AngelLoader
{
    public static partial class LanguageSupport
    {
        [Flags, FenGenLanguageEnum(languageIndexEnumName: "LanguageIndex")]
        public enum Language : uint
        {
            [FenGenIgnore]
            Default = 0,
            [FenGenLanguage("en", "English")]
            English = 1,
            [FenGenLanguage("cz", "Čeština")]
            Czech = 2,
            [FenGenLanguage("nl", "Nederlands")]
            Dutch = 4,
            [FenGenLanguage("fr", "Français")]
            French = 8,
            [FenGenLanguage("de", "Deutsch")]
            German = 16,
            [FenGenLanguage("hu", "Magyar")]
            Hungarian = 32,
            [FenGenLanguage("it", "Italiano")]
            Italian = 64,
            [FenGenLanguage("ja,jp", "日本語")]
            Japanese = 128,
            [FenGenLanguage("pl", "Polski")]
            Polish = 256,
            [FenGenLanguage("ru", "Русский")]
            Russian = 512,
            [FenGenLanguage("es", "Español")]
            Spanish = 1024
        }

        // @LANGS: Temporary! Eventually we won't need this old Supported string array.
        // This is for passing to the game via the stub to match FMSel's behavior (Dark only)
        // Immediate use, so don't bother lazy-loading
        public static readonly string[]
        Supported =
        {
            "english", // en, eng (must be first)
            "czech", // cz
            "dutch", // nl
            "french", // fr
            "german", // de
            "hungarian", // hu
            "italian", // it
            "japanese", // ja, jp
            "polish", // pl
            "russian", // ru
            "spanish" // es
        };
    }
}
