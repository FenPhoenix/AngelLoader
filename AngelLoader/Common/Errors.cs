﻿namespace AngelLoader
{
    internal enum Error
    {
        None,
        /*
        NoGamesSpecified,
        BackupPathNotSpecified,
        CamModIniNotFound,
        T1CamModIniNotFound,
        T2CamModIniNotFound,
        SS2CamModIniNotFound,
        */
        SneakyOptionsNoRegKey,
        SneakyOptionsNotFound,
        T3FMInstPathNotFound,
        GameExeNotSpecified,
        GameExeNotFound,
        SneakyDllNotFound,
        GameVersionNotFound
    }

    internal enum ImportError
    {
        None,
        NoArchiveDirsFound,
        Unknown
    }

    /*
    internal enum StubResponseError
    {
        RootTooLong,
        NameTooLong,
        ModExcludeTooLong,
        LanguageTooLong
    }
    */
}
