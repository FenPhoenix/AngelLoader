using System;

namespace AngelLoader;

internal enum Error
{
    None,
#if false
    NoGamesSpecified,
    BackupPathNotSpecified,
    T1CamModIniNotFound,
    T2CamModIniNotFound,
    SS2CamModIniNotFound,
    CamModIniNotFound,
    CamModIniCouldNotBeRead,
    SneakyOptionsNoRegKey,
    T3FMInstPathNotFound,
#endif
    GeneralSneakyOptionsIniError,
    SneakyOptionsNotFound,
    GameExeNotSpecified,
    GameExeNotFound,
    SneakyDllNotFound,
    GameVersionNotFound,
}

[Flags]
internal enum SetGameDataError
{
    None = 0,
    SneakyOptionsNotFound = 1,
    GameDirNotWriteable = 2,
}

internal enum MissFlagError
{
    None,
    NoValidlyNamedMisFiles,
}

internal enum ImportError
{
    None,
    NoArchiveDirsFound,
    Unknown,
}

internal enum ConvertAudioError
{
    None,
    FFmpegNotFound,
}

#if false
internal enum StubResponseError
{
    RootTooLong,
    NameTooLong,
    ModExcludeTooLong,
    LanguageTooLong,
}
#endif
