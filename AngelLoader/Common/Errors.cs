using JetBrains.Annotations;

namespace AngelLoader
{
    [PublicAPI]
    internal enum Error
    {
        None,
        NoGamesSpecified,
        BackupPathNotSpecified,
        CamModIniNotFound,
        T1CamModIniNotFound,
        T2CamModIniNotFound,
        SS2CamModIniNotFound,
        SneakyOptionsNoRegKey,
        SneakyOptionsNotFound,
        T3FMInstPathNotFound
    }

    [PublicAPI]
    internal enum ImportError
    {
        None,
        NoArchiveDirsFound,
        Unknown
    }

    [PublicAPI]
    internal enum StubResponseError
    {
        RootTooLong,
        NameTooLong,
        ModExcludeTooLong,
        LanguageTooLong
    }
}
