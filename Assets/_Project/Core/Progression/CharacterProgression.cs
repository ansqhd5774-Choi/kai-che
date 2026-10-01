using System;
using System.Collections.Generic;

namespace KaiChe.Core.Progression
{
    public enum CharacterAttribute
    {
        Body = 0,
        Agility = 1,
        Perception = 2,
        Intelligence = 3,
        Wisdom = 4,
        Will = 5,
        Good = 6,
        Evil = 7
    }

    public static class ProgressionRules
    {
        public const int MaxLevel = 9999;
        public const double HoursToLevel100 = 112d;
        public const double TargetDailyPlayHours = 8d;
        public const double TargetYearsToMaxLevel = 10d;
        public const double TargetTotalPlayHours = 29220d;
        // Prototype-only values. These are not the final 10-year XP curve.
        public const int PrototypeBaseXpToNextLevel = 5;
        public const int PrototypeXpStepPerLevel = 3;
        public const int PrototypeAttributePointsPerLevel = 3;

        public static long GetPrototypeExperienceRequiredForNextLevel(
            int level)
        {
            if (level >= MaxLevel)
                return long.MaxValue;

            int safeLevel = Math.Max(1, level);
            return PrototypeBaseXpToNextLevel
                + (long)(safeLevel - 1) * PrototypeXpStepPerLevel;
        }
    }

    public sealed class CharacterProgressionState
    {
        private readonly Dictionary<CharacterAttribute, int> _attributes =
            new Dictionary<CharacterAttribute, int>();
        public CharacterProgressionState()
        {
            foreach (CharacterAttribute attribute in
                     Enum.GetValues(typeof(CharacterAttribute)))
            {
                _attributes[attribute] = 0;
            }
        }

        public int Level { get; private set; } = 1;
        public long Experience { get; private set; }
        public long LevelExperience { get; private set; }
        public int UnspentAttributePoints { get; private set; }

        public long ExperienceRequiredForNextLevel =>
            ProgressionRules.GetPrototypeExperienceRequiredForNextLevel(
                Level);

        public int GetAttribute(CharacterAttribute attribute)
        {
            return _attributes[attribute];
        }

        public void RestorePrototypeState(
            int level,
            long experience,
            long levelExperience,
            int unspentAttributePoints,
            int[] attributes)
        {
            Level = Math.Max(1, Math.Min(ProgressionRules.MaxLevel, level));
            Experience = Math.Max(0L, experience);
            LevelExperience = Math.Max(0L, levelExperience);
            UnspentAttributePoints = Math.Max(0, unspentAttributePoints);

            foreach (CharacterAttribute attribute in
                     Enum.GetValues(typeof(CharacterAttribute)))
            {
                int index = (int)attribute;
                int value = attributes != null && index < attributes.Length
                    ? attributes[index]
                    : 0;
                _attributes[attribute] = Math.Max(0, value);
            }

            if (Level >= ProgressionRules.MaxLevel)
                LevelExperience = 0;
        }

        public int AddExperienceAndApplyPrototypeLevelUp(long amount)
        {
            if (amount <= 0)
                return 0;

            Experience += amount;
            LevelExperience += amount;

            int levelsGained = 0;

            while (Level < ProgressionRules.MaxLevel)
            {
                long required =
                    ProgressionRules.GetPrototypeExperienceRequiredForNextLevel(
                        Level);

                if (LevelExperience < required)
                    break;

                LevelExperience -= required;
                Level += 1;
                levelsGained += 1;
                UnspentAttributePoints +=
                    ProgressionRules.PrototypeAttributePointsPerLevel;
            }

            if (Level >= ProgressionRules.MaxLevel)
                LevelExperience = 0;

            return levelsGained;
        }

        public bool TrySpendAttributePoints(
            CharacterAttribute attribute,
            int amount = 1)
        {
            if (amount <= 0)
                return false;

            if (UnspentAttributePoints < amount)
                return false;

            _attributes[attribute] += amount;
            UnspentAttributePoints -= amount;
            return true;
        }
        public void GrantLevelsForPrototype(int levels)
        {
            if (levels <= 0)
                return;

            int targetLevel = Math.Min(
                ProgressionRules.MaxLevel,
                Level + levels);
            int grantedLevels = targetLevel - Level;

            Level = targetLevel;
            UnspentAttributePoints +=
                grantedLevels
                * ProgressionRules.PrototypeAttributePointsPerLevel;
        }
    }
}
