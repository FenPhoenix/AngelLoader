using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using static System.StringComparison;

namespace AL_Common
{
    [PublicAPI]
    public static class Common
    {
        #region Fields / classes

        // Class instead of enum so we don't have to keep casting its fields
        public static class ByteSize
        {
            public const int KB = 1024;
            public const int MB = KB * 1024;
            public const int GB = MB * 1024;
        }

        /// <summary>
        /// Stores a filename/index pair for quick lookups into a zip file.
        /// </summary>
        public sealed class NameAndIndex
        {
            public readonly string Name;
            public readonly int Index;

            public NameAndIndex(string name, int index)
            {
                Name = name;
                Index = index;
            }

            public NameAndIndex(string name)
            {
                Name = name;
                Index = -1;
            }
        }

        #region Preset char arrays

        public static readonly byte[] RTFHeaderBytes =
        {
            (byte)'{',
            (byte)'\\',
            (byte)'r',
            (byte)'t',
            (byte)'f',
            (byte)'1'
        };

        // Perf, for passing to Split(), Trim() etc. so we don't allocate all the time
        public static readonly char[] CA_Comma = { ',' };
        public static readonly char[] CA_Semicolon = { ';' };
        public static readonly char[] CA_CommaSemicolon = { ',', ';' };
        public static readonly char[] CA_CommaSpace = { ',', ' ' };
        public static readonly char[] CA_Backslash = { '\\' };
        //internal static readonly char[] CA_ForwardSlash = { '/' };
        public static readonly char[] CA_BS_FS = { '\\', '/' };
        public static readonly char[] CA_BS_FS_Space = { '\\', '/', ' ' };
        public static readonly char[] CA_Plus = { '+' };

        #endregion

        #endregion

        #region Methods

        #region String

        #region ASCII-specific

        [PublicAPI]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAsciiUpper(this char c) => c is >= 'A' and <= 'Z';

        [PublicAPI]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAsciiLower(this char c) => c is >= 'a' and <= 'z';

        [PublicAPI]
        public static bool EqualsIAscii(this char char1, char char2) =>
            char1 == char2 ||
            (char1.IsAsciiUpper() && char2.IsAsciiLower() && char1 == char2 - 32) ||
            (char1.IsAsciiLower() && char2.IsAsciiUpper() && char1 == char2 + 32);

        [PublicAPI]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAsciiAlpha(this char c) => c.IsAsciiUpper() || c.IsAsciiLower();

        [PublicAPI]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAsciiNumeric(this char c) => c is >= '0' and <= '9';

        [PublicAPI]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAsciiAlphanumeric(this char c) => c.IsAsciiAlpha() || c.IsAsciiNumeric();

        [PublicAPI]
        public static bool IsAsciiAlphaUpper(this string str)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (!str[i].IsAsciiUpper()) return false;
            }
            return true;
        }

        #endregion

        #region Empty / whitespace checks

        /// <summary>
        /// Returns true if <paramref name="value"/> is null or empty.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [ContractAnnotation("null => true")]
        public static bool IsEmpty([NotNullWhen(false)] this string? value) => string.IsNullOrEmpty(value);

        /// <summary>
        /// Returns true if <paramref name="value"/> is null, empty, or whitespace.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        [ContractAnnotation("null => true")]
        public static bool IsWhiteSpace([NotNullWhen(false)] this string? value) => string.IsNullOrWhiteSpace(value);

        #endregion

        #region Equals

        /// <summary>
        /// Determines whether this string and a specified <see langword="string"/> object have the same value.
        /// Uses <see cref="StringComparison.OrdinalIgnoreCase"/>.
        /// </summary>
        public static bool EqualsI(this string first, string second) => first.Equals(second, OrdinalIgnoreCase);

        public static bool EqualsTrue(this string value) => string.Equals(value, bool.TrueString, OrdinalIgnoreCase);

        #endregion

        #region Contains

        public static bool Contains(this string value, char character) => value.IndexOf(character) >= 0;

        [PublicAPI]
        public static bool Contains(this string value, string substring, StringComparison comparison) =>
            !value.IsEmpty() && !substring.IsEmpty() && value.IndexOf(substring, comparison) >= 0;

        /// <summary>
        /// Determines whether a string contains a specified substring. Uses
        /// <see cref="StringComparison.OrdinalIgnoreCase"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="substring"></param>
        /// <returns></returns>
        [PublicAPI]
        public static bool ContainsI(this string value, string substring) => value.Contains(substring, OrdinalIgnoreCase);

        /// <summary>
        /// Determines whether a List&lt;string&gt; contains a specified element. Uses <see cref="StringComparison.OrdinalIgnoreCase"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="substring"></param>
        /// <returns></returns>
        public static bool ContainsI(this string[] value, string substring)
        {
            for (int i = 0; i < value.Length; i++) if (value[i].Equals(substring, OrdinalIgnoreCase)) return true;
            return false;
        }

        /// <summary>
        /// Determines whether a string[] contains a specified element. Uses <see cref="StringComparison.OrdinalIgnoreCase"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="substring"></param>
        /// <returns></returns>
        public static bool ContainsI(this List<string> value, string substring)
        {
            for (int i = 0; i < value.Count; i++) if (value[i].Equals(substring, OrdinalIgnoreCase)) return true;
            return false;
        }

        #endregion

        #endregion

        #region Numeric

        #region Clamping

        /// <summary>
        /// Clamps a number to between min and max.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static T Clamp<T>(this T value, T min, T max) where T : IComparable<T> =>
            value.CompareTo(min) < 0 ? min : value.CompareTo(max) > 0 ? max : value;

        /// <summary>
        /// If <paramref name="value"/> is less than zero, returns zero. Otherwise, returns <paramref name="value"/>
        /// unchanged.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int ClampToZero(this int value) => Math.Max(value, 0);

        public static float ClampZeroToOne(this float value) => value.Clamp(0, 1.0f);

        #endregion

        public static double GetPercentFromValue_Double(int current, int total) => total == 0 ? 0 : (double)(100 * current) / total;
        public static int GetPercentFromValue_Int(int current, int total) => total == 0 ? 0 : (100 * current) / total;
        //public static long GetValueFromPercent(double percent, long total) => (long)((percent / 100) * total);
        public static int GetValueFromPercent(double percent, int total) => (int)((percent / 100d) * total);
        public static int GetValueFromPercent_Rounded(double percent, int total) => (int)Math.Round((percent / 100d) * total, 1, MidpointRounding.AwayFromZero);
        public static double GetValueFromPercent_Double(double percent, int total) => percent / 100d * total;

        #endregion

        #region File/Path

        #region Path-specific string queries (separator-agnostic)

        public static bool PathContainsI(this List<string> value, string substring)
        {
            for (int i = 0; i < value.Count; i++) if (value[i].PathEqualsI(substring)) return true;
            return false;
        }

        public static bool PathContainsI(this string[] value, string substring)
        {
            for (int i = 0; i < value.Length; i++) if (value[i].PathEqualsI(substring)) return true;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDirSep(this char character) => character is '/' or '\\';

        #region Disabled until needed

        /*
        [PublicAPI, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartsWithDirSep(this string value) => value.Length > 0 && value[0].IsDirSep();
        */

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool EndsWithDirSep(this string value) => value.Length > 0 && value[value.Length - 1].IsDirSep();

        // Note: We hardcode '/' and '\' for now because we can get paths from archive files too, where the dir
        // sep chars are in no way guaranteed to match those of the OS.
        // Not like any OS is likely to use anything other than '/' or '\' anyway.

        // We hope not to have to call this too often, but it's here as a fallback.
        public static string CanonicalizePath(string value) => value.Replace('/', '\\');

        /// <summary>
        /// Returns true if <paramref name="value"/> contains either directory separator character.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool ContainsDirSep(this string value)
        {
            for (int i = 0; i < value.Length; i++) if (value[i].IsDirSep()) return true;
            return false;
        }

        /// <summary>
        /// Counts the total occurrences of both directory separator characters in <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public static int CountDirSeps(this string value, int start = 0)
        {
            int count = 0;
            for (int i = start; i < value.Length; i++) if (value[i].IsDirSep()) count++;
            return count;
        }

        public static bool DirSepCountIsAtLeast(this string value, int count, int start = 0)
        {
            int foundCount = 0;
            for (int i = start; i < value.Length; i++)
            {
                if (value[i].IsDirSep()) foundCount++;
                if (foundCount == count) return true;
            }

            return false;
        }

        /// <summary>
        /// Returns the number of directory separators in a string, earlying-out once it's counted <paramref name="maxToCount"/>
        /// occurrences.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="maxToCount">The maximum number of occurrences to count before earlying-out.</param>
        /// <returns></returns>
        public static int CountDirSepsUpToAmount(this string value, int maxToCount)
        {
            int foundCount = 0;
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i].IsDirSep())
                {
                    foundCount++;
                    if (foundCount == maxToCount) break;
                }
            }

            return foundCount;
        }

        /// <summary>
        /// Returns the last index of either directory separator character in <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static int LastIndexOfDirSep(this string value)
        {
            int i1 = value.LastIndexOf('/');
            int i2 = value.LastIndexOf('\\');

            return i1 == -1 && i2 == -1 ? -1 : Math.Max(i1, i2);
        }

        /// <summary>
        /// Path equality check ignoring case and directory separator differences.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool PathEqualsI(this string first, string second)
        {
            if (first == second) return true;

            int firstLen = first.Length;
            if (firstLen != second.Length) return false;

            for (int i = 0; i < firstLen; i++)
            {
                char fc = first[i];
                char sc = second[i];

                if (fc > 127 || sc > 127)
                {
                    // Non-ASCII slow path
                    return first.Equals(second, OrdinalIgnoreCase) ||
                           CanonicalizePath(first).Equals(CanonicalizePath(second), OrdinalIgnoreCase);
                }

                if (!AsciiPathCharsConsideredEqual_Win(fc, sc)) return false;
            }

            return true;
        }

        /// <summary>
        /// Path starts-with check ignoring case and directory separator differences.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool PathStartsWithI(this string first, string second)
        {
            if (first.Length < second.Length) return false;

            for (int i = 0; i < second.Length; i++)
            {
                char fc = first[i];
                char sc = second[i];

                if (fc > 127 || sc > 127)
                {
                    // Non-ASCII slow path
                    return first.StartsWith(second, OrdinalIgnoreCase) ||
                           CanonicalizePath(first).StartsWith(CanonicalizePath(second), OrdinalIgnoreCase);
                }

                if (!AsciiPathCharsConsideredEqual_Win(fc, sc)) return false;
            }

            return true;
        }

        /// <summary>
        /// Path ends-with check ignoring case and directory separator differences.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        public static bool PathEndsWithI(this string first, string second)
        {
            if (first.Length < second.Length) return false;

            for (int fi = first.Length - second.Length, si = 0; fi < first.Length; fi++, si++)
            {
                char fc = first[fi];
                char sc = second[si];

                if (fc > 127 || sc > 127)
                {
                    // Non-ASCII slow path
                    return first.EndsWith(second, OrdinalIgnoreCase) ||
                           CanonicalizePath(first).EndsWith(CanonicalizePath(second), OrdinalIgnoreCase);
                }

                if (!AsciiPathCharsConsideredEqual_Win(fc, sc)) return false;
            }

            return true;
        }

        #region Disabled until needed

        /*
        public static bool PathContainsI_Dir(this List<string> value, string substring)
        {
            for (int i = 0; i < value.Count; i++) if (value[i].PathEqualsI_Dir(substring)) return true;
            return false;
        }

        public static bool PathContainsI_Dir(this string[] value, string substring)
        {
            for (int i = 0; i < value.Length; i++) if (value[i].PathEqualsI_Dir(substring)) return true;
            return false;
        }
        */

        /*
        /// <summary>
        /// Counts the total occurrences of both directory separator characters in <paramref name="value"/>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public static int CountDirSeps(this string value, int start = 0)
        {
            int count = 0;
            for (int i = start; i < value.Length; i++) if (value[i].IsDirSep()) count++;
            return count;
        }
        */

        /*
        /// <summary>
        /// Counts dir seps up to <paramref name="count"/> occurrences and then returns, skipping further counting.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="count"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        internal static bool DirSepCountIsAtLeast(this string value, int count, int start = 0)
        {
            int foundCount = 0;
            for (int i = start; i < value.Length; i++)
            {
                if (value[i].IsDirSep()) foundCount++;
                if (foundCount == count) return true;
            }

            return false;
        }
        */

        #endregion

        #region Equality / StartsWith / EndsWith

        public static bool PathSequenceEqualI_Dir(this IList<string> first, IList<string> second)
        {
            int firstCount;
            if ((firstCount = first.Count) != second.Count) return false;

            for (int i = 0; i < firstCount; i++) if (!first[i].PathEqualsI_Dir(second[i])) return false;
            return true;
        }

        public static bool PathSequenceEqualI(this IList<string> first, IList<string> second)
        {
            int firstCount;
            if ((firstCount = first.Count) != second.Count) return false;

            for (int i = 0; i < firstCount; i++) if (!first[i].PathEqualsI(second[i])) return false;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool AsciiPathCharsConsideredEqual_Win(char char1, char char2) =>
            char1.EqualsIAscii(char2) ||
            (char1.IsDirSep() && char2.IsDirSep());

        /// <summary>
        /// Path equality check ignoring case and directory separator differences. Directory version: Ignores
        /// trailing path separators.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        [PublicAPI]
        public static bool PathEqualsI_Dir(this string first, string second) => first.TrimEnd(CA_BS_FS).PathEqualsI(second.TrimEnd(CA_BS_FS));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StartsWithDirSep(this string value) => value.Length > 0 && value[0].IsDirSep();

        #region Disabled until needed

        /*
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool EndsWithDirSep(this string value) => value.Length > 0 && value[value.Length - 1].IsDirSep();
        */

        #endregion

        #endregion

        #endregion

        /// <summary>
        /// Just removes the extension from a filename, without the rather large overhead of
        /// Path.GetFileNameWithoutExtension().
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string RemoveExtension(this string fileName)
        {
            int i;
            return (i = fileName.LastIndexOf('.')) == -1 ? fileName : fileName.Substring(0, i);
        }

        /// <summary>
        /// Determines whether this string ends with a file extension. Obviously only makes sense for strings
        /// that are supposed to be file names.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool HasFileExtension(this string value)
        {
            int lastDotIndex = value.LastIndexOf('.');
            return lastDotIndex > value.LastIndexOf('/') ||
                   lastDotIndex > value.LastIndexOf('\\');
        }

        #endregion

        #region Count chars

        /// <summary>
        /// Returns the number of times a character appears in a string.
        /// Avoids whatever silly overhead junk Count(predicate) is doing.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="character"></param>
        /// <returns></returns>
        public static int CountChars(this string value, char character)
        {
            int count = 0;
            for (int i = 0; i < value.Length; i++) if (value[i] == character) count++;

            return count;
        }

        /// <summary>
        /// Returns the number of times a character appears in a string, earlying-out once it's counted <paramref name="maxToCount"/>
        /// occurrences.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="character"></param>
        /// <param name="maxToCount">The maximum number of occurrences to count before earlying-out.</param>
        /// <returns></returns>
        public static int CountCharsUpToAmount(this string value, char character, int maxToCount)
        {
            int foundCount = 0;
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == character)
                {
                    foundCount++;
                    if (foundCount == maxToCount) break;
                }
            }

            return foundCount;
        }

        public static bool CharCountIsAtLeast(this string value, char character, int count, int start = 0)
        {
            int foundCount = 0;
            for (int i = start; i < value.Length; i++)
            {
                if (value[i] == character)
                {
                    foundCount++;
                    if (foundCount == count) return true;
                }
            }

            return false;
        }

        #endregion

        #region Enumerable

        #region Array initialization

        /// <summary>
        /// Returns an array of type <typeparamref name="T"/> with all elements initialized to non-null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="length"></param>
        public static T[] InitializedArray<T>(int length) where T : new()
        {
            T[] ret = new T[length];
            for (int i = 0; i < length; i++) ret[i] = new T();
            return ret;
        }

        /// <summary>
        /// Returns an array of type <typeparamref name="T"/> with all elements initialized to <paramref name="value"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="length"></param>
        /// <param name="value">The value to initialize all elements with.</param>
        public static T[] InitializedArray<T>(int length, T value) where T : new()
        {
            T[] ret = new T[length];
            for (int i = 0; i < length; i++) ret[i] = value;
            return ret;
        }

        /// <summary>
        /// Returns two arrays of type <typeparamref name="T1"/> and <typeparamref name="T2"/> respectively,
        /// with all elements initialized to non-null. Uses a single assignment loop for performance.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="length"></param>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        public static void InitializeArrays<T1, T2>(int length,
            out T1[] array1,
            out T2[] array2)
            where T1 : new()
            where T2 : new()
        {
            array1 = new T1[length];
            array2 = new T2[length];
            for (int i = 0; i < length; i++)
            {
                array1[i] = new T1();
                array2[i] = new T2();
            }
        }

        #endregion

        public static T[] CombineArrays<T>(params T[][] arrays)
        {
            int totalLen = 0;
            for (int i = 0; i < arrays.Length; i++)
            {
                totalLen += arrays[i].Length;
            }

            T[] ret = new T[totalLen];

            int pos = 0;
            for (int i = 0; i < arrays.Length; i++)
            {
                T[] array = arrays[i];
                int arrayLen = array.Length;

                Array.Copy(array, 0, ret, pos, arrayLen);

                pos += arrayLen;
            }

            return ret;
        }

        #region Clear and add

        public static void ClearAndAdd<T>(this List<T> list, T item)
        {
            list.Clear();
            list.Add(item);
        }

        public static void ClearAndAdd<T>(this List<T> list, IEnumerable<T> items)
        {
            list.Clear();
            list.AddRange(items);
        }

        public static void ClearAndAdd<T>(this List<T> list, List<T> items)
        {
            list.Clear();
            list.AddRange(items);
        }

        public static void ClearAndAdd<T>(this List<T> list, T[] items)
        {
            list.Clear();
            list.AddRange(items);
        }

        #endregion

        public static void DisposeAndClear<T>(this T[] array) where T : IDisposable?
        {
            for (int i = 0; i < array.Length; i++) array[i]?.Dispose();
        }

        public static void DisposeAndClear<T>(this T[] array, int start, int end) where T : IDisposable?
        {
            for (int i = start; i < end; i++) array[i]?.Dispose();
        }

        #endregion

        public static int FindIndexOfByteSequence(List<byte> input, byte[] pattern, int start = 0)
        {
            byte firstByte = pattern[0];
            int index = input.IndexOf(firstByte, start);

            while (index > -1)
            {
                for (int i = 0; i < pattern.Length; i++)
                {
                    if (index + i >= input.Count) return -1;
                    if (pattern[i] != input[index + i])
                    {
                        if ((index = input.IndexOf(firstByte, index + i)) == -1) return -1;
                        break;
                    }

                    if (i == pattern.Length - 1) return index;
                }
            }

            return -1;
        }

        public static bool EqualsIfNotNull(this object? sender, object? equals) => sender != null && equals != null && sender == equals;

        #endregion
    }
}
