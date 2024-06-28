using System;
using System.Text;

namespace AngelLoader;

public static partial class Utils
{
    /// <summary>30</summary>
    private const int _maxDarkInstDirLength = 30;

    internal readonly struct InstDirNameContext()
    {
        internal readonly StringBuilder SB = new(_maxDarkInstDirLength);

        // Static analyzer assistance to make sure I don't call this by accident
        // ReSharper disable once UnusedMember.Global
        public new static void ToString() { }
    }

    /// <summary>
    /// Format an FM archive name to conform to NewDarkLoader's FM install directory name requirements.
    /// </summary>
    /// <param name="archiveName">Filename without path or extension.</param>
    /// <param name="context"></param>
    /// <param name="truncate">Whether to truncate the name to <inheritdoc cref="_maxDarkInstDirLength" path="//summary"/> characters or less.</param>
    /// <returns></returns>
    internal static string ToInstDirNameNDL(this string archiveName, InstDirNameContext context, bool truncate)
    {
        return ToInstDirName(archiveName, "+.~ ", truncate, context);
    }

    /// <summary>
    /// Format an FM archive name to conform to FMSel's FM install directory name requirements.
    /// </summary>
    /// <param name="archiveName">Filename without path or extension.</param>
    /// <param name="context"></param>
    /// <param name="truncate">Whether to truncate the name to <inheritdoc cref="_maxDarkInstDirLength" path="//summary"/> characters or less.</param>
    /// <returns></returns>
    internal static string ToInstDirNameFMSel(this string archiveName, InstDirNameContext context, bool truncate)
    {
        return ToInstDirName(archiveName, "+;:.,<>?*~| ", truncate, context);
    }

    private static string ToInstDirName(string archiveName, string illegalChars, bool truncate, InstDirNameContext context)
    {
        int count = archiveName.LastIndexOf('.');
        if (truncate)
        {
            if (count is -1 or > _maxDarkInstDirLength)
            {
                count = Math.Min(archiveName.Length, _maxDarkInstDirLength);
            }
        }
        else
        {
            if (count == -1)
            {
                count = archiveName.Length;
            }
        }

        context.SB.Clear();
        context.SB.Append(archiveName, 0, count);
        for (int i = 0; i < illegalChars.Length; i++)
        {
            context.SB.Replace(illegalChars[i], '_');
        }

        return context.SB.ToString();
    }
}
