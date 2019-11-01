// TODO: This stub is now 195k because of statically linking the stuff.
// I don't like it, but whatever. It's not really of any consequence, I just like minimalism.
// I can always come back in here if I ever get more to grips with what I'm doing when it comes to C++ strings
// and squash this junk down some. Till then, meh.

/*
 We have NewDark executables call out to this stub program, which provides the NewDark game with data that has
 been passed to it by AngelLoader via a temp file. We do it this way in order to support AngelLoader being
 standalone.

 Note: From the default cam_mod.ini file:

 ---snip---

 FM selection can also be done with command-line options (which override mod.ini)
   -fm        : to start the FM Selector
   -fm=name   : to start game with 'name' as active FM

 ---snip---

 If we just wanted to play FMs and be done with it, we could just pass fm=[name] on the command line and avoid
 having to have this stub altogether. But unfortunately you can't pass anything else on the command line (disabled
 mods etc.) so we have to have this.

 Or at least no other command-line options are listed anywhere that I can find.

 2019/10/10:
 We now use the stub at all times again, due to wanting to pass language stuff. Steam support also requires the
 stub.

 2019/9/28:
 This stub is now in C++ to avoid DLLExport incompatibilities and general hackiness with the .NET version.

 2019/3/31:
 As of this date, we only use this stub if we actually need to pass mod excludes. Otherwise, we just call the
 game and pass it the FM on the command line, as that's much cleaner.
 */

#include "AngelLoader_Stub.h"
#include <string>
#include <fstream>
#include <filesystem>
#include <cstdio>
#include <algorithm>
#include <windows.h>
#define MB_OK 0x00000000L
#define MB_ICONINFORMATION 0x00000040L
using std::string;
namespace fs = std::filesystem;

bool equals_i(string s1, string s2)
{
    std::transform(s1.begin(), s1.end(), s1.begin(), toupper);
    std::transform(s2.begin(), s2.end(), s2.begin(), toupper);
    return s1 == s2;
}

int show_loader_alert()
{
    const string msg1 =
        "AngelLoader is set as the loader for this game.\r\n\r\n";
    const string msg2 =
        "To use a different loader for Thief 1, Thief 2, or System Shock 2, please open cam_mod.ini in your game folder for instructions on how to do so.\r\n\r\n";
    const string msg3 =
        "To use a different loader for Thief 3, please open Sneaky Tweaker and choose another loader in the \"Sneaky Upgrade -> FM Loading\" section.";
    MessageBoxA(nullptr, (msg1 + msg2 + msg3).c_str(), "AngelLoader", MB_OK | MB_ICONINFORMATION);
    return kSelFMRet_ExitGame;
}

extern "C" int FMSELAPI SelectFM(sFMSelectorData * data)
{
    if (!data || static_cast<unsigned int>(data->nStructSize) < sizeof(sFMSelectorData))
    {
        return kSelFMRet_Cancel;
    }

    //string test_msg = string("bForceLanguage: ") + string(data->bForceLanguage == 1 ? "1" : "0") + string("\r\n");
    //test_msg += string("sLanguage: ") + string(data->sLanguage != NULL ? string(data->sLanguage) : "<null>");
    //MessageBoxA(nullptr, test_msg.c_str(), "Test", MB_OK);

    // data->sGameVersion:
    // Might eventually use
    // Note: actually don't think this is of any use, since we already know what game we're running.
    // Maybe for getting the NewDark version?

    // data->sRootPath:
    // Will almost certainly never use
    // If AngelLoader was running by being called directly into by the game as intended, then this would
    // be a really useful way to not have to scan cam_mod.ini for the FM installed path. But as it stands
    // we need to know that path long before even getting here, so it's not very useful.

    // data->sLanguage / data->bForceLanguage:
    // Might use if it will help with multi-language stuff

    bool play_original_game = false;
    bool play_original_game_key_found = false;
    string fm_name;
    string disabled_mods;
    string language;
    string force_language;

    // Note: We can't make this into a char* right here, because of pointer and scope weirdness. We have to convert
    // each time later. Probably a better way to do it but whatever. C# cowboy learning the ropes.
    const fs::path args_file = fs::path(fs::temp_directory_path() / "AngelLoader" / "Stub" / "al_stub_args.tmp");

    const string play_original_game_eq = "PlayOriginalGame=";
    const unsigned int play_original_game_eq_len = play_original_game_eq.length();
    const string fm_name_eq = "SelectedFMName=";
    const unsigned int fm_name_eq_len = fm_name_eq.length();
    const string disabled_mods_eq = "DisabledMods=";
    const unsigned int disabled_mods_eq_len = disabled_mods_eq.length();
    const string language_eq = "Language=";
    const unsigned int language_eq_len = language_eq.length();
    const string force_language_eq = "ForceLanguage=";
    const unsigned int force_language_eq_len = force_language_eq.length();

    // Note: using ifstream instead of fopen bloats the dll up by 10k, but I can't get fopen to work. Reads the
    // encoding wrong I'm guessing, I don't frickin' know. At least this works, and I can come back and shrink it
    // down later when I know better what I'm doing.
    std::ifstream ifs(args_file.string().c_str());
    // This will be false if anything's wrong, including if the file doesn't exist (which is a check we need to make)
    if (!ifs)
    {
        ifs.close();
        return show_loader_alert();
        //return kSelFMRet_Cancel;
    }

    string line;
    while (std::getline(ifs, line))
    {
        if (line.length() > play_original_game_eq_len&&
            line.substr(0, play_original_game_eq_len) == play_original_game_eq)
        {
            play_original_game_key_found = true;
            string val = line.substr(play_original_game_eq_len);
            play_original_game = equals_i(val, "true");
        }
        else if (line.length() > fm_name_eq_len&&
            line.substr(0, fm_name_eq_len) == fm_name_eq)
        {
            fm_name = line.substr(fm_name_eq_len);
        }
        else if (line.length() > disabled_mods_eq_len&&
            line.substr(0, disabled_mods_eq_len) == disabled_mods_eq)
        {
            disabled_mods = line.substr(disabled_mods_eq_len);
        }
        else if (line.length() > language_eq_len&&
            line.substr(0, language_eq_len) == language_eq)
        {
            language = line.substr(language_eq_len);
        }
        else if (line.length() > force_language_eq_len&&
            line.substr(0, force_language_eq_len) == force_language_eq)
        {
            force_language = line.substr(force_language_eq_len);
        }
    }

    ifs.close();

    std::remove(args_file.string().c_str());

    // If no FM folder specified, play the original game
    if (play_original_game_key_found && play_original_game)
    {
        return kSelFMRet_Cancel;
    }
    else if (!play_original_game_key_found || fm_name.empty())
    {
        return show_loader_alert();
    }
    // error conditions; they can be reported through a response file if I decided to ever use that
    else if (fm_name.length() > 30 ||
        disabled_mods.length() > static_cast<unsigned int>(data->nMaxModExcludeLen))
    {
        return kSelFMRet_ExitGame;
    }

    if (data->nMaxNameLen > 0) strncpy_s(data->sName, data->nMaxNameLen, fm_name.c_str(), data->nMaxNameLen);
    if (data->nMaxModExcludeLen > 0) strncpy_s(data->sModExcludePaths, data->nMaxModExcludeLen, disabled_mods.c_str(), data->nMaxModExcludeLen);

    // Leave whatever was in there before if we haven't got a value
    if (!language.empty() && data->nLanguageLen > 0) strncpy_s(data->sLanguage, data->nLanguageLen, language.c_str(), data->nLanguageLen);
    if (!force_language.empty()) data->bForceLanguage = equals_i(force_language, "true") ? 1 : 0;

    // Never call us back; we're standalone and don't need it
    data->bRunAfterGame = 0;

    return kSelFMRet_OK;
}
