//@LangUpdate: Comment out for final
#define TESTING

using System;

namespace AngelLoader;

internal static class LangUpdate
{
    public sealed class LanguageUpdateInfo
    {
        // @LangUpdate: App version needs to be a range
        public readonly Version AppVersion;
        public readonly uint Revision;
        public Uri? DownloadUrl;

        public LanguageUpdateInfo(Version appVersion, uint revision)
        {
            AppVersion = appVersion;
            Revision = revision;
        }
    }

#if TESTING
    private const string _langUpdatesRepoDir = "lang_updates_testing";
#else
    private const string _langUpdatesRepoDir = "lang_updates";
#endif

    // @LangUpdate: We have "app version" and "revision" so we need something more complicated here I guess
    private const string _langLatestVersionFile = "https://fenphoenix.github.io/AngelLoaderUpdates/" + _langUpdatesRepoDir + "/latest_version.txt";
    private const string _langVersionsFile = "https://fenphoenix.github.io/AngelLoaderUpdates/" + _langUpdatesRepoDir + "/versions.ini";
}
