#include "AngelLoader_Stub.h"
#include <cstdio>
#include <string>
#include <filesystem>
using std::string;
namespace fs = std::filesystem;

extern "C" int FMSELAPI SelectFM(sFMSelectorData * data)
{
    if (!data || static_cast<unsigned int>(data->nStructSize) < sizeof(sFMSelectorData))
    {
        return kSelFMRet_Cancel;
    }

    string fm_name;
    string disabled_mods;

    FILE* f;
    const char* args_file = fs::path(fs::temp_directory_path() / "AngelLoader" / "Stub" / "al_stub_args.tmp").generic_u8string().c_str();
    const errno_t err = fopen_s(&f, args_file, "r");
    if (f == nullptr || err != 0) return kSelFMRet_Cancel;

    char line[8192];
    const string fm_name_eq = "SelectedFMName=";
    const int fm_name_eq_len = fm_name_eq.length();
    const string disabled_mods_eq = "DisabledMods=";
    const int disabled_mods_eq_len = disabled_mods_eq.length();

    while (true)
    {
        if (!fgets(line, sizeof(line) - 1, f)) break;
        string line_s(line);

        if (line_s.substr(0, fm_name_eq_len) == fm_name_eq)
        {
            fm_name = line_s.substr(fm_name_eq_len, string::npos);
        }
        else if (line_s.substr(0, disabled_mods_eq_len) == disabled_mods_eq)
        {
            disabled_mods = line_s.substr(disabled_mods_eq_len, string::npos);
        }
    }

    if (fm_name.length() > 30 ||
        disabled_mods.length() > static_cast<unsigned int>(data->nMaxModExcludeLen))
    {
        return kSelFMRet_ExitGame;
    }

    if (fm_name.empty())
    {
        return kSelFMRet_Cancel;
    }

    fm_name.copy(data->sName, fm_name.length());
    disabled_mods.copy(data->sModExcludePaths, disabled_mods.length());

    return kSelFMRet_OK;
}
