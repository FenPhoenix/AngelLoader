using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using AngelLoader.DataClasses;
using static AngelLoader.Logger;
using static AngelLoader.Misc;

namespace AngelLoader.Ini
{
    internal static partial class Ini
    {
        internal static bool StartsWithFast_NoNullChecks(this string str, string value)
        {
            if (str.Length < value.Length) return false;

            for (int i = 0; i < value.Length; i++)
            {
                if (str[i] != value[i]) return false;
            }

            return true;
        }

        #region BindingFlags

        private const BindingFlags BFlagsEnum = BindingFlags.Instance |
                                                BindingFlags.Static |
                                                BindingFlags.Public |
                                                BindingFlags.NonPublic;

        #endregion

        // This kinda belongs in LanguageIni.cs, but it's separated to prevent it from being removed when that
        // file is re-generated. I could make it so it doesn't get removed, but meh.
        internal static void ReadTranslatedLanguageName(string file)
        {
            StreamReader? sr = null;
            try
            {
                sr = new StreamReader(file, Encoding.UTF8);

                bool inMeta = false;
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    var lineT = line.Trim();
                    if (inMeta && lineT.StartsWithFast_NoNullChecks(
                            nameof(LText.Meta.TranslatedLanguageName) + "="))
                    {
                        var key = file.GetFileNameFast().RemoveExtension();
                        var value = line.TrimStart()
                            .Substring(nameof(LText.Meta.TranslatedLanguageName).Length + 1);
                        Config.LanguageNames[key] = value;
                        return;
                    }
                    else if (lineT == "[" + nameof(LText.Meta) + "]")
                    {
                        inMeta = true;
                    }
                    else if (!lineT.IsEmpty() && lineT[0] == '[' && lineT[lineT.Length - 1] == ']')
                    {
                        inMeta = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Log("There was an error while reading " + file + ".", ex);
            }
            finally
            {
                sr?.Dispose();
            }
        }

        private static DateTime? ReadNullableHexDate(string hexDate)
        {
            var success = long.TryParse(
                hexDate,
                NumberStyles.HexNumber,
                DateTimeFormatInfo.InvariantInfo,
                out long result);

            if (!success) return null;

            try
            {
                var dateTime = DateTimeOffset
                    .FromUnixTimeSeconds(result)
                    .DateTime
                    .ToLocalTime();

                return dateTime;
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }

        private static readonly ReaderWriterLockSlim FMDataIniRWLock = new ReaderWriterLockSlim();

        internal static void WriteFullFMDataIni()
        {
            try
            {
                FMDataIniRWLock.EnterWriteLock();
                WriteFMDataIni(FMDataIniList, Paths.FMDataIni);
            }
            catch (Exception ex)
            {
                Log("Exception writing FM data ini", ex);
            }
            finally
            {
                try
                {
                    FMDataIniRWLock.ExitWriteLock();
                }
                catch (Exception ex)
                {
                    Log("Exception exiting " + nameof(FMDataIniRWLock) + " in " + nameof(WriteFullFMDataIni),
                        ex);
                }
            }
        }

        private static void FillFMHasXFields(FanMission fm, string fieldsString)
        {
            string[] fields = fieldsString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            fm.HasMap = false;
            fm.HasAutomap = false;
            fm.HasScripts = false;
            fm.HasTextures = false;
            fm.HasSounds = false;
            fm.HasObjects = false;
            fm.HasCreatures = false;
            fm.HasMotions = false;
            fm.HasMovies = false;
            fm.HasSubtitles = false;

            if (fields.Length > 0 && fields[0].EqualsI("None")) return;

            for (int i = 0; i < fields.Length; i++)
            {
                var field = fields[i];

                // Need this if block, because we're not iterating through all fields, so can't just have a flat
                // block of fm.HasX = field.EqualsI(X);
                if (field.EqualsI("Map"))
                {
                    fm.HasMap = true;
                }
                else if (field.EqualsI("Automap"))
                {
                    fm.HasAutomap = true;
                }
                else if (field.EqualsI("Scripts"))
                {
                    fm.HasScripts = true;
                }
                else if (field.EqualsI("Textures"))
                {
                    fm.HasTextures = true;
                }
                else if (field.EqualsI("Sounds"))
                {
                    fm.HasSounds = true;
                }
                else if (field.EqualsI("Objects"))
                {
                    fm.HasObjects = true;
                }
                else if (field.EqualsI("Creatures"))
                {
                    fm.HasCreatures = true;
                }
                else if (field.EqualsI("Motions"))
                {
                    fm.HasMotions = true;
                }
                else if (field.EqualsI("Movies"))
                {
                    fm.HasMovies = true;
                }
                else if (field.EqualsI("Subtitles"))
                {
                    fm.HasSubtitles = true;
                }
            }
        }

        private static void CommaCombineHasXFields(FanMission fm, StringBuilder sb)
        {
            //string ret = "";

            // Hmm... doesn't make for good code, but fast...
            bool notEmpty = false;

            if (fm.HasMap == false &&
                fm.HasAutomap == false &&
                fm.HasScripts == false &&
                fm.HasTextures == false &&
                fm.HasSounds == false &&
                fm.HasObjects == false &&
                fm.HasCreatures == false &&
                fm.HasMotions == false &&
                fm.HasMovies == false &&
                fm.HasSubtitles == false)
            {
                sb.AppendLine("None");
                return;
            }

            if (fm.HasMap)
            {
                sb.Append("Map");
                notEmpty = true;
            }
            if (fm.HasAutomap)
            {
                if (notEmpty) sb.Append(",");
                sb.Append("Automap");
                notEmpty = true;
            }
            if (fm.HasScripts)
            {
                if (notEmpty) sb.Append(",");
                sb.Append("Scripts");
                notEmpty = true;
            }
            if (fm.HasTextures)
            {
                if (notEmpty) sb.Append(",");
                sb.Append("Textures");
                notEmpty = true;
            }
            if (fm.HasSounds)
            {
                if (notEmpty) sb.Append(",");
                sb.Append("Sounds");
                notEmpty = true;
            }
            if (fm.HasObjects)
            {
                if (notEmpty) sb.Append(",");
                sb.Append("Objects");
                notEmpty = true;
            }
            if (fm.HasCreatures)
            {
                if (notEmpty) sb.Append(",");
                sb.Append("Creatures");
                notEmpty = true;
            }
            if (fm.HasMotions)
            {
                if (notEmpty) sb.Append(",");
                sb.Append("Motions");
                notEmpty = true;
            }
            if (fm.HasMovies)
            {
                if (notEmpty) sb.Append(",");
                sb.Append("Movies");
                notEmpty = true;
            }
            if (fm.HasSubtitles)
            {
                if (notEmpty) sb.Append(",");
                sb.Append("Subtitles");
            }

            sb.AppendLine();
        }
    }
}
