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
    GameVersionNotFound
}

internal enum MissFlagError
{
    None,
    NoValidlyNamedMisFiles
}

internal enum ImportError
{
    None,
    NoArchiveDirsFound,
    Unknown
}

#if false
internal enum StubResponseError
{
    RootTooLong,
    NameTooLong,
    ModExcludeTooLong,
    LanguageTooLong
}
#endif
