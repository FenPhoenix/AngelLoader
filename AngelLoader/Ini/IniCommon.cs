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

        private static void ClearFMHasXFields(FanMission fm)
        {
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
                if (field.EqualsI(nameof(fm.HasMap)))
                {
                    fm.HasMap = true;
                }
                else if (field.EqualsI(nameof(fm.HasAutomap)))
                {
                    fm.HasAutomap = true;
                }
                else if (field.EqualsI(nameof(fm.HasScripts)))
                {
                    fm.HasScripts = true;
                }
                else if (field.EqualsI(nameof(fm.HasTextures)))
                {
                    fm.HasTextures = true;
                }
                else if (field.EqualsI(nameof(fm.HasSounds)))
                {
                    fm.HasSounds = true;
                }
                else if (field.EqualsI(nameof(fm.HasObjects)))
                {
                    fm.HasObjects = true;
                }
                else if (field.EqualsI(nameof(fm.HasCreatures)))
                {
                    fm.HasCreatures = true;
                }
                else if (field.EqualsI(nameof(fm.HasMotions)))
                {
                    fm.HasMotions = true;
                }
                else if (field.EqualsI(nameof(fm.HasMovies)))
                {
                    fm.HasMovies = true;
                }
                else if (field.EqualsI(nameof(fm.HasSubtitles)))
                {
                    fm.HasSubtitles = true;
                }
            }
        }

        private static string CommaCombineHasXFields(FanMission fm)
        {
            string ret = "";

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
                return "None";
            }

            if (fm.HasMap == true)
            {
                ret += nameof(fm.HasMap);
                notEmpty = true;
            }
            if (fm.HasAutomap == true)
            {
                if (notEmpty) ret += ",";
                ret += nameof(fm.HasAutomap);
                notEmpty = true;
            }
            if (fm.HasScripts == true)
            {
                if (notEmpty) ret += ",";
                ret += nameof(fm.HasScripts);
                notEmpty = true;
            }
            if (fm.HasTextures == true)
            {
                if (notEmpty) ret += ",";
                ret += nameof(fm.HasTextures);
                notEmpty = true;
            }
            if (fm.HasSounds == true)
            {
                if (notEmpty) ret += ",";
                ret += nameof(fm.HasSounds);
                notEmpty = true;
            }
            if (fm.HasObjects == true)
            {
                if (notEmpty) ret += ",";
                ret += nameof(fm.HasObjects);
                notEmpty = true;
            }
            if (fm.HasCreatures == true)
            {
                if (notEmpty) ret += ",";
                ret += nameof(fm.HasCreatures);
                notEmpty = true;
            }
            if (fm.HasMotions == true)
            {
                if (notEmpty) ret += ",";
                ret += nameof(fm.HasMotions);
                notEmpty = true;
            }
            if (fm.HasMovies == true)
            {
                if (notEmpty) ret += ",";
                ret += nameof(fm.HasMovies);
                notEmpty = true;
            }
            if (fm.HasSubtitles == true)
            {
                if (notEmpty) ret += ",";
                ret += nameof(fm.HasSubtitles);
            }

            return ret;
        }
    }
}
