using System.Runtime.CompilerServices;
using AngelLoader.DataClasses;
using static AL_Common.LanguageSupport;
using static AngelLoader.GameSupport;

namespace AngelLoader;

public static partial class Utils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool HasFlagFast(this FinishedState @enum, FinishedState flag) => (@enum & flag) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool HasFlagFast(this Game @enum, Game flag) => (@enum & flag) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool HasFlagFast(this Language @enum, Language flag) => (@enum & flag) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool HasFlagFast(this Difficulty @enum, Difficulty flag) => (@enum & flag) != 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool HasFlagFast(this SetGameDataError @enum, SetGameDataError flag) => (@enum & flag) != 0;
}
