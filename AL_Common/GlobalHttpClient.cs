using System.Net.Http;

namespace AL_Common;

public static partial class Common
{
    // Only one instance is supposed to exist app-wide
    private static HttpClient? _globalHttpClient;
    public static HttpClient GlobalHttpClient => _globalHttpClient ??= new HttpClient();
}
