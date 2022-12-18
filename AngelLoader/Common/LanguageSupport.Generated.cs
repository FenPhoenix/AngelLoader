// Static analyzer requires this for generated files for whatever reason, even if it's also enabled in the .proj file
#nullable enable

#define FenGen_LanguageSupportDest

using System;
using static AL_Common.Common;
using static AngelLoader.FenGenAttributes;
using static AngelLoader.Utils;

namespace AngelLoader;

[FenGenLanguageSupportDestClass]
public static partial class LanguageSupport
{
    #region Autogenerated language support code

    public static readonly int SupportedLanguageCount = Enum.GetValues(typeof(LanguageIndex)).Length;

    public enum LanguageIndex : uint
    {
        English,
        Czech,
        Dutch,
        French,
        German,
        Hungarian,
        Italian,
        Japanese,
        Polish,
        Russian,
        Spanish
    }

    public static readonly string[] SupportedLanguages =
    {
        "english",
        "czech",
        "dutch",
        "french",
        "german",
        "hungarian",
        "italian",
        "japanese",
        "polish",
        "russian",
        "spanish"
    };

    private static string[]? _fspl;
    public static string[] FSPrefixedLangs
    {
        get
        {
            if (_fspl == null)
            {
                _fspl = new string[11];
                for (int i = 0; i < 11; i++)
                {
                    _fspl[i] = "/" + SupportedLanguages[i];
                }
            }

            return _fspl;
        }
    }

    // Even though we have the perfect hash, this one is required for things that need case-insensitivity
    // in the keys!
    public static readonly DictionaryI<Language> LangStringsToEnums = new(11)
    {
        { "english", Language.English },
        { "czech", Language.Czech },
        { "dutch", Language.Dutch },
        { "french", Language.French },
        { "german", Language.German },
        { "hungarian", Language.Hungarian },
        { "italian", Language.Italian },
        { "japanese", Language.Japanese },
        { "polish", Language.Polish },
        { "russian", Language.Russian },
        { "spanish", Language.Spanish }
    };

    public static readonly string[][] LangCodes =
    {
        new[] { "en" },
        new[] { "cz" },
        new[] { "nl" },
        new[] { "fr" },
        new[] { "de" },
        new[] { "hu" },
        new[] { "it" },
        new[] { "ja", "jp" },
        new[] { "pl" },
        new[] { "ru" },
        new[] { "es" }
    };

    public static readonly string[] LangTranslatedNames =
    {
        "English",
        "Čeština",
        "Nederlands",
        "Français",
        "Deutsch",
        "Magyar",
        "Italiano",
        "日本語",
        "Polski",
        "Русский",
        "Español"
    };

    /// <summary>
    /// Converts a Language to a LanguageIndex. *Narrowing conversion, so make sure the language has been checked for convertibility first!
    /// </summary>
    /// <param name="language"></param>
    public static LanguageIndex LanguageToLanguageIndex(Language language)
    {
        AssertR(language != Language.Default, nameof(language) + " was out of range: " + language);

        return language switch
        {
            Language.English => LanguageIndex.English,
            Language.Czech => LanguageIndex.Czech,
            Language.Dutch => LanguageIndex.Dutch,
            Language.French => LanguageIndex.French,
            Language.German => LanguageIndex.German,
            Language.Hungarian => LanguageIndex.Hungarian,
            Language.Italian => LanguageIndex.Italian,
            Language.Japanese => LanguageIndex.Japanese,
            Language.Polish => LanguageIndex.Polish,
            Language.Russian => LanguageIndex.Russian,
            _ => LanguageIndex.Spanish
        };
    }
    /// <summary>
    /// Converts a LanguageIndex to a Language. Widening conversion, so it will always succeed.
    /// </summary>
    /// <param name="languageIndex"></param>
    public static Language LanguageIndexToLanguage(LanguageIndex languageIndex) => languageIndex switch
    {
        LanguageIndex.English => Language.English,
        LanguageIndex.Czech => Language.Czech,
        LanguageIndex.Dutch => Language.Dutch,
        LanguageIndex.French => Language.French,
        LanguageIndex.German => Language.German,
        LanguageIndex.Hungarian => Language.Hungarian,
        LanguageIndex.Italian => Language.Italian,
        LanguageIndex.Japanese => Language.Japanese,
        LanguageIndex.Polish => Language.Polish,
        LanguageIndex.Russian => Language.Russian,
        _ => Language.Spanish
    };

    public static string GetLanguageString(LanguageIndex index) => SupportedLanguages[(uint)index];

    #endregion
}
