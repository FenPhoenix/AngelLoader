#define GAME_VERSIONS

#if GAME_VERSIONS
using System;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using static AngelLoader.GameSupport;
using static AngelLoader.GameSupport.GameIndex;
using static AngelLoader.Logger;
using static AngelLoader.Misc;

namespace AngelLoader
{
    internal static class GameVersions
    {
        // UTF-16-encoded ASCII: first byte is an ASCII character, second byte is always null
        private static readonly byte[] ProductVersionBytes = Encoding.ASCII.GetBytes(new[]
        {
            'P', '\0', 'r', '\0', 'o', '\0', 'd', '\0', 'u', '\0', 'c', '\0', 't', '\0',
            'V', '\0', 'e', '\0', 'r', '\0', 's', '\0', 'i', '\0', 'o', '\0', 'n', '\0', '\0', '\0'
        });

        private static int IndexOfByteSequence(this byte[] input, byte[] pattern)
        {
            byte firstByte = pattern[0];
            int index = Array.IndexOf(input, firstByte);

            while (index > -1)
            {
                for (int i = 0; i < pattern.Length; i++)
                {
                    if (index + i >= input.Length) return -1;
                    if (pattern[i] != input[index + i])
                    {
                        if ((index = Array.IndexOf(input, firstByte, index + i)) == -1) return -1;
                        break;
                    }

                    if (i == pattern.Length - 1) return index;
                }
            }

            return index;
        }

        [PublicAPI]
        internal static Error TryGetGameVersion(GameIndex game, out string version)
        {
            version = "";

            string gameExe = Config.GetGameExe(game);
            string exeToSearch;

            if (gameExe.IsWhiteSpace()) return Error.GameExeNotSpecified;
            if (!File.Exists(gameExe)) return Error.GameExeNotFound;

            if (GameIsDark(game))
            {
                exeToSearch = gameExe;
            }
            else
            {
                if (!TryCombineFilePathAndCheckExistence(Config.GetGamePath(Thief3), Paths.SneakyDll, out exeToSearch))
                {
                    return Error.SneakyDllNotFound;
                }
            }

            BinaryReader? br = null;
            try
            {
                br = new BinaryReader(new FileStream(exeToSearch, FileMode.Open, FileAccess.Read, FileShare.Read),
                                      Encoding.ASCII, leaveOpen: false);

                long streamLen = br.BaseStream.Length;

                if (streamLen > int.MaxValue) return Error.ExeIsLargerThanInt;

                // Search starting at 88% through the file: 91% (average location) plus some wiggle room (fastest)
                long pos = GetValueFromPercent(88.0d, streamLen);
                long byteCount = streamLen - pos;
                br.BaseStream.Position = pos;
                byte[] bytes = new byte[byteCount];
                br.Read(bytes, 0, (int)byteCount);
                int verIndex = bytes.IndexOfByteSequence(ProductVersionBytes);

                // Fallback: search the whole file - still fast, but not as fast
                if (verIndex == -1)
                {
                    br.BaseStream.Position = 0;
                    bytes = new byte[streamLen];
                    br.Read(bytes, 0, (int)streamLen);
                    verIndex = bytes.IndexOfByteSequence(ProductVersionBytes);
                    if (verIndex == -1) return Error.GameVersionNotFound;
                }

                // Init with non-null values so we don't start out with two nulls and early-out before we do
                // anything.
                // UTF-16, so two null bytes = one null char.
                byte[] null2 = { 255, 255 };
                for (int i = verIndex + ProductVersionBytes.Length; i < bytes.Length; i++)
                {
                    if (null2[0] == '\0' && null2[1] == '\0') break;
                    null2[0] = null2[1];
                    null2[1] = bytes[i];
                    if (bytes[i] > 0) version += ((char)bytes[i]).ToString();
                }
            }
            catch (Exception ex)
            {
                Log("Exception reading/searching game exe for version string", ex);
                version = "";
                return Error.GameExeReadFailed;
            }
            finally
            {
                br?.Dispose();
            }

            return Error.None;
        }
    }
}
#endif
