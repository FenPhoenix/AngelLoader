﻿using System.Net.Http;

namespace AL_Common;

public static partial class Common
{
    /*
    @TDM(Http):
    On new .NETs there's a System.Net.Http.SocketsHttpHandler that apparently results in "a significant performance
    improvement" compared to HttpClientHandler which Framework uses.
    It's only in new .NETs, but there's a backport here: https://github.com/TalAloni/StandardSocketsHttpHandler
    We can test this out later once we know what we're doing and see if we get better perf.

    2023-10-28: Perf was the same with the above nuget package, so we're not using it for now. If we do an in-app
    downloader we can revisit it and test again.
    */

    // Only one instance is supposed to exist app-wide
    private static HttpClient? _globalHttpClient;
    public static HttpClient GlobalHttpClient => _globalHttpClient ??= new HttpClient();
}
