namespace AngelLoader.Common
{
    internal enum Error
    {
        None,
        NoGamesSpecified,
        BackupPathNotSpecified,
        CamModIniNotFound,
        T1CamModIniNotFound,
        T2CamModIniNotFound,
        SneakyOptionsNoRegKey,
        SneakyOptionsNotFound,
        T3FMInstPathNotFound
    }

    internal enum ImportError
    {
        None,
        NoArchiveDirsFound,
        Unknown
    }

    internal enum StubResponseError
    {
        RootTooLong,
        NameTooLong,
        ModExcludeTooLong,
        LanguageTooLong
    }
}
