using System;
using JetBrains.Annotations;

namespace AngelLoader
{
    internal static class DifficultySupport
    {
        internal static readonly int SupportedDifficultyCount = Enum.GetValues(typeof(DifficultyIndex)).Length;

        [Flags]
        public enum Difficulty : uint
        {
            [UsedImplicitly]
            None = 0,
            Normal = 1,
            Hard = 2,
            Expert = 4,
            Extreme = 8
        }

        public enum DifficultyIndex : uint
        {
            Normal,
            Hard,
            Expert,
            Extreme
        }

        /// <summary>
        /// Converts a <see cref="Difficulty"/> to a <see cref="DifficultyIndex"/>. *Narrowing conversion, so make sure the difficulty has been checked for convertibility first!
        /// </summary>
        /// <param name="difficulty"></param>
        public static DifficultyIndex DifficultyToDifficultyIndex(Difficulty difficulty)
        {
            Misc.AssertR(difficulty != Difficulty.None, nameof(difficulty) + " was out of range: " + difficulty);

            return difficulty switch
            {
                Difficulty.Normal => DifficultyIndex.Normal,
                Difficulty.Hard => DifficultyIndex.Hard,
                Difficulty.Expert => DifficultyIndex.Expert,
                _ => DifficultyIndex.Extreme
            };
        }

        /// <summary>
        /// Converts a <see cref="DifficultyIndex"/> to a <see cref="Difficulty"/>. Widening conversion, so it will always succeed.
        /// </summary>
        /// <param name="difficultyIndex"></param>
        public static Difficulty DifficultyIndexToDifficulty(DifficultyIndex difficultyIndex) => difficultyIndex switch
        {
            DifficultyIndex.Normal => Difficulty.Normal,
            DifficultyIndex.Hard => Difficulty.Hard,
            DifficultyIndex.Expert => Difficulty.Expert,
            _ => Difficulty.Extreme
        };
    }
}
