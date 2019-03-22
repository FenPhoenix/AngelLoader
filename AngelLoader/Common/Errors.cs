namespace AngelLoader.Common
{
    internal enum Error
    {
        None,
        CamModIniNotFound
    }

    internal enum ImportError
    {
        None,
        NoArchiveDirsFound
    }

    internal enum StubResponseError
    {
        RootTooLong,
        NameTooLong,
        ModExcludeTooLong,
        LanguageTooLong
    }
}
