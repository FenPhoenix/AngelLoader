#pragma once

#define FMSELAPI __declspec(dllexport)

#pragma pack(4)
typedef struct sFMSelectorData
{
    // sizeof(sFMSelectorData)
    int nStructSize;

    // game version string as returned by AppName() (ie. in the form "Thief 2 Final 1.19")
    const char* sGameVersion;

    // supplied initial FM root path (the FM selector may change this)
    char* sRootPath;
    int nMaxRootLen;

    // buffer to copy the selected FM name
    char* sName;
    int nMaxNameLen;

    // set to non-zero when selector is invoked after game exit (if requested during game start)
    int bExitedGame;
    // FM selector should set this to non-zero if it wants to be invoked after game exits (only done for FMs)
    int bRunAfterGame;

    // optional list of paths to exclude from mod_path/uber_mod_path in + separated format and like the config
    // vars, or if "*" all mod paths are excluded (leave buffer empty for no excludes)
    // the specified exclude paths work as if they had a "*\" wildcard prefix
    char* sModExcludePaths;
    int nMaxModExcludeLen;

    // language setting for FM (set by the FM selector when an FM is selected), may be empty if FM has no
    // language specific resources
    // when 'bForceLanguage' is 0 this is used to ensure an FM runs correctly even if it doesn't support
    // the game's current language setting (set by the "language" config var)
    // when 'bForceLanguage' is 1 this is used to force a language (that must be supported by the FM) other
    // than the game's current language
    // the initial values are usually an empty string for 'sLanguage' and 0 for 'bForceLanguage', unless "fm_language"
    // and/or "fm_language_forced" happen to be set in cam_mod.ini (which they normally shouldn't be, that's only
    // something that's useful when testing an FM for another language than the game's current language)
    char* sLanguage;
    int nLanguageLen;
    int bForceLanguage;
} sFMSelectorData;
#pragma pack()

typedef enum eFMSelReturn  // NOLINT(performance-enum-size)
{
    kSelFMRet_OK = 0,       // run selected FM 'data->sName' (0-len string to run without an FM)
    kSelFMRet_Cancel = -1,  // cancel FM selection and start game as-is (no FM or if defined in cam_mod.ini use that)
    kSelFMRet_ExitGame = 1  // abort and quit game
} eFMSelReturn;

extern "C" int FMSELAPI SelectFM(sFMSelectorData * data);
