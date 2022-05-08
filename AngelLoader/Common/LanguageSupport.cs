#define FenGen_LanguageSupportSource

using System;
using static AngelLoader.FenGenAttributes;

namespace AngelLoader
{
    // @LANGS: We probably won't need a couple of the things in the genned version.
    // But we won't see if they aren't used because the file is marked Generated and won't be analyzed.
    // So we have to go through manually.
    // @LANGS: We need to change the string.Split() in FMData.Generated.cs to the alloc-free version detailed in FMData.cs
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

        public static bool LanguageIsSupported(string language) => LangStringsToEnums.ContainsKey(language);

        public static string GetTranslatedLanguageName(LanguageIndex index) => LangTranslatedNames[(int)index];

        public static bool TryGetLanguageCodes(string language, out string[] languageCodes)
        {
            if (LangStringsToEnums.TryGetValue(language, out Language result))
            {
                languageCodes = LangCodes[(int)LanguageToLanguageIndex(result)];
                return true;
            }
            else
            {
                languageCodes = Array.Empty<string>();
                return false;
            }
        }
    }
}
