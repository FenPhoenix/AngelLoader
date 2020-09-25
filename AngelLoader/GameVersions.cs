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
        private static int IndexOfByteSequence(this byte[] input, byte[] pattern)
        {
            var firstByte = pattern[0];
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

        // Used for detecting NewDark version of NewDark executables
        private static readonly byte[] ProductVersionBytes = Encoding.ASCII.GetBytes(new[]
        {
            'P', '\0', 'r', '\0', 'o', '\0', 'd', '\0', 'u', '\0', 'c', '\0', 't', '\0',
            'V', '\0', 'e', '\0', 'r', '\0', 's', '\0', 'i', '\0', 'o', '\0', 'n', '\0', '\0', '\0'
        });

        internal static string CreateTitle()
        {
            string ret = "";
            for (int i = 0; i < SupportedGameCount; i++)
            {
                string gameExe = Config.GetGameExe((GameIndex)i);
                if (!gameExe.IsWhiteSpace() && File.Exists(gameExe))
                {
                    if (ret.Length > 0) ret += ", ";
                    ret += i switch
                    {
                        0 => "T1: ",
                        1 => "T2: ",
                        2 => "T3: ",
                        _ => "SS2: "
                    };
                    Error error = TryGetGameVersion((GameIndex)i, out string version);
                    ret += error == Error.None ? version : "unknown";
                }
            }

            return ret;
        }

        [PublicAPI]
        internal static Error TryGetGameVersion(GameIndex game, out string version)
        {
            version = "";

            string gameExe = GameIsDark(game) ? Config.GetGameExe(game) : Path.Combine(Config.GetGamePath(Thief3), "Sneaky.dll");

            if (gameExe.IsWhiteSpace()) return Error.GameExeNotSpecified;
            if (!File.Exists(gameExe)) return Error.GameExeNotFound;

            BinaryReader? br = null;
            try
            {
                br = new BinaryReader(new FileStream(gameExe, FileMode.Open, FileAccess.Read), Encoding.ASCII, leaveOpen: false);

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

                // Init with non-null values so we don't start out with two nulls and early-out before we do anything
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
