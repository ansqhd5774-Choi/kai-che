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
        public int UnspentAttributePoints { get; private set; }

        public int GetAttribute(CharacterAttribute attribute)
        {
            return _attributes[attribute];
        }
        public void AddExperience(long amount)
        {
            if (amount <= 0) return;
            Experience += amount;
        }

        public void GrantLevels(int levels, int attributePointsPerLevel)
        {
            if (levels <= 0) return;
            if (attributePointsPerLevel < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(attributePointsPerLevel));

            int targetLevel = Math.Min(
                ProgressionRules.MaxLevel,
                Level + levels);
            int grantedLevels = targetLevel - Level;

            Level = targetLevel;
            UnspentAttributePoints +=
                grantedLevels * attributePointsPerLevel;
        }
        public bool TrySpendAttributePoints(
            CharacterAttribute attribute,
            int amount = 1)
        {
            if (amount <= 0) return false;
            if (UnspentAttributePoints < amount) return false;

            _attributes[attribute] += amount;
            UnspentAttributePoints -= amount;
            return true;
        }

        public void GrantAttributePointsForPrototype(int amount)
        {
            if (amount <= 0) return;
            UnspentAttributePoints += amount;
        }
    }
}
