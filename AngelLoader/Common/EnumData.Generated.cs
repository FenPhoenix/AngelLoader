#define FenGen_EnumDataDest

using static AL_Common.FenGenAttributes;

namespace AngelLoader;

[FenGenEnumDataDestClass]
public static partial class Misc
{
    public const int ColumnCount = 14;
    public const int TDMColumnCount = 8;
    public const int HideableFilterControlsCount = 10;
    public const int SettingsTabCount = 4;
    public static readonly string[] CustomResourcesNames =
    {
        "None",
        "Map",
        "Automap",
        "Scripts",
        "Textures",
        "Sounds",
        "Objects",
        "Creatures",
        "Motions",
        "Movies",
        "Subtitles"
    };
    public const int CustomResourcesCount = 11;
    public const int DifficultyCount = 4;
}
