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

 UPDATE: As of 2019/3/31, we only use this stub if we actually need to pass mod excludes. Otherwise, we just call
 the game and pass it the FM on the command line, as that's much cleaner.

 UPDATE 2019/9/28: This stub is now in C++ to avoid DLLExport incompatibilities and general hackiness with the
 .NET version.
*/

#include "AngelLoader_Stub.h"
#include <string>
#include <fstream>
#include <filesystem>
#include <cstdio>
using std::string;
namespace fs = std::filesystem;

extern "C" int FMSELAPI SelectFM(sFMSelectorData * data)
{
    if (!data || static_cast<unsigned int>(data->nStructSize) < sizeof(sFMSelectorData))
    {
        return kSelFMRet_Cancel;
    }

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

    string fm_name, disabled_mods;

    // Note: We can't make this into a char* right here, because of pointer and scope weirdness. We have to convert
    // each time later. Probably a better way to do it but whatever. C# cowboy learning the ropes.
    const fs::path args_file = fs::path(fs::temp_directory_path() / "AngelLoader" / "Stub" / "al_stub_args.tmp");

    const string fm_name_eq = "SelectedFMName=";
    const unsigned int fm_name_eq_len = fm_name_eq.length();
    const string disabled_mods_eq = "DisabledMods=";
    const unsigned int disabled_mods_eq_len = disabled_mods_eq.length();

    // Note: using ifstream instead of fopen bloats the dll up by 10k, but I can't get fopen to work. Reads the
    // encoding wrong I'm guessing, I don't frickin' know. At least this works, and I can come back and shrink it
    // down later when I know better what I'm doing.
    std::ifstream ifs(args_file.string().c_str());
    if (!ifs.is_open())
    {
        ifs.close();
        return kSelFMRet_Cancel;
    }

    string line;
    while (std::getline(ifs, line))
    {
        if (line.length() > fm_name_eq_len &&
            line.substr(0, fm_name_eq_len) == fm_name_eq)
        {
            fm_name = line.substr(fm_name_eq_len, string::npos);
        }
        else if (line.length() > disabled_mods_eq_len &&
            line.substr(0, disabled_mods_eq_len) == disabled_mods_eq)
        {
            disabled_mods = line.substr(disabled_mods_eq_len, string::npos);
        }
    }

    ifs.close();

    std::remove(args_file.string().c_str());

    // If no FM folder specified, play the original game
    if (fm_name.empty())
    {
        return kSelFMRet_Cancel;
    }
    // error conditions; they can be reported through a response file if I decided to ever use that
    else if (fm_name.length() > 30 ||
        disabled_mods.length() > static_cast<unsigned int>(data->nMaxModExcludeLen))
    {
        return kSelFMRet_ExitGame;
    }

    if (data->nMaxNameLen > 0) strncpy_s(data->sName, data->nMaxNameLen, fm_name.c_str(), data->nMaxNameLen);
    if (data->nMaxModExcludeLen > 0) strncpy_s(data->sModExcludePaths, data->nMaxModExcludeLen, disabled_mods.c_str(), data->nMaxModExcludeLen);

    // Never call us back; we're standalone and don't need it
    data->bRunAfterGame = 0;

    return kSelFMRet_OK;
}
