using System;
using System.Collections.Generic;

namespace KaiChe.Core.Progression
{
    [Flags]
    public enum SkillTags
    {
        None = 0,
        Projectile = 1 << 0,
        Orbit = 1 << 1,
        Lightning = 1 << 2,
        Pierce = 1 << 3,
        MultiHit = 1 << 4,
        Chain = 1 << 5,
        Fusion = 1 << 6,
        Frost = 1 << 7
    }

    public sealed class SkillProgressionDefinition
    {
        public string Id { get; }
        public string DisplayName { get; }
        public string ParentSkillId { get; }
        public int ParentMasteryRequired { get; }
        public string FusionSourceA { get; }
        public string FusionSourceB { get; }
        public int FusionMasteryRequired { get; }
        public SkillTags Tags { get; }
        public bool IsBaseSkill =>
            string.IsNullOrEmpty(ParentSkillId)
            && string.IsNullOrEmpty(FusionSourceA);

        public bool IsFusionSkill =>
            !string.IsNullOrEmpty(FusionSourceA)
            && !string.IsNullOrEmpty(FusionSourceB);

        public SkillProgressionDefinition(
            string id,
            string displayName,
            SkillTags tags,
            string parentSkillId = null,
            int parentMasteryRequired = 0,
            string fusionSourceA = null,
            string fusionSourceB = null,
            int fusionMasteryRequired = 0)
        {
            Id = id;
            DisplayName = displayName;
            Tags = tags;
            ParentSkillId = parentSkillId;
            ParentMasteryRequired = parentMasteryRequired;
            FusionSourceA = fusionSourceA;
            FusionSourceB = fusionSourceB;
            FusionMasteryRequired = fusionMasteryRequired;
        }
    }
    public static class SkillProgressionCatalog
    {
        public const string PulseShot = "SKILL_0001";
        public const string OrbitBlade = "SKILL_0002";
        public const string ArcSpark = "SKILL_0003";
        public const string IceBall = "SKILL_0004";
        public const string PiercingPulse = "SKILL_0101";
        public const string TwinOrbit = "SKILL_0102";
        public const string ChainSpark = "SKILL_0103";
        public const string PulseOrbit = "SKILL_1001";

        public static readonly SkillProgressionDefinition[] All =
        {
            new SkillProgressionDefinition(
                PulseShot, "Pulse Shot", SkillTags.Projectile),
            new SkillProgressionDefinition(
                OrbitBlade, "Orbit Blade", SkillTags.Orbit),
            new SkillProgressionDefinition(
                ArcSpark, "Arc Spark", SkillTags.Lightning),
            new SkillProgressionDefinition(
                IceBall, "Ice Ball", SkillTags.Projectile | SkillTags.Frost),
            new SkillProgressionDefinition(
                PiercingPulse, "Piercing Pulse",
                SkillTags.Projectile | SkillTags.Pierce,
                PulseShot, 30),
            new SkillProgressionDefinition(
                TwinOrbit, "Twin Orbit",
                SkillTags.Orbit | SkillTags.MultiHit,
                OrbitBlade, 30),
            new SkillProgressionDefinition(
                ChainSpark, "Chain Spark",
                SkillTags.Lightning | SkillTags.Chain,
                ArcSpark, 30),
            new SkillProgressionDefinition(
                PulseOrbit, "Pulse Orbit",
                SkillTags.Projectile | SkillTags.Orbit | SkillTags.Fusion,
                fusionSourceA: PiercingPulse,
                fusionSourceB: TwinOrbit,
                fusionMasteryRequired: 20)
        };

        public static SkillProgressionDefinition Find(string skillId)
        {
            for (int i = 0; i < All.Length; i++)
            {
                if (All[i].Id == skillId)
                    return All[i];
            }

            return null;
        }
    }
    public sealed class SkillMasteryState
    {
        private readonly Dictionary<string, int> _mastery =
            new Dictionary<string, int>();
        private readonly HashSet<string> _unlocked =
            new HashSet<string>();

        public SkillMasteryState()
        {
            for (int i = 0; i < SkillProgressionCatalog.All.Length; i++)
            {
                SkillProgressionDefinition definition =
                    SkillProgressionCatalog.All[i];

                _mastery[definition.Id] = 0;
                if (definition.IsBaseSkill)
                    _unlocked.Add(definition.Id);
            }
        }

        public string LastUnlockedSkillId { get; private set; }
        public int UnlockedSkillCount => _unlocked.Count;

        public int GetMastery(string skillId)
        {
            return _mastery.TryGetValue(skillId, out int value)
                ? value
                : 0;
        }
        public bool IsUnlocked(string skillId)
        {
            return _unlocked.Contains(skillId);
        }

        public void RecordUse(string skillId, int amount = 1)
        {
            if (amount <= 0 || !_mastery.ContainsKey(skillId))
                return;

            _mastery[skillId] += amount;
            EvaluateUnlocks();
        }

        private void EvaluateUnlocks()
        {
            bool unlockedAny;

            do
            {
                unlockedAny = false;

                for (int i = 0; i < SkillProgressionCatalog.All.Length; i++)
                {
                    SkillProgressionDefinition definition =
                        SkillProgressionCatalog.All[i];

                    if (_unlocked.Contains(definition.Id))
                        continue;

                    if (CanUnlock(definition))
                    {
                        _unlocked.Add(definition.Id);
                        LastUnlockedSkillId = definition.Id;
                        unlockedAny = true;
                    }
                }
            }
            while (unlockedAny);
        }

        private bool CanUnlock(SkillProgressionDefinition definition)
        {
            if (definition.IsFusionSkill)
            {
                return _unlocked.Contains(definition.FusionSourceA)
                    && _unlocked.Contains(definition.FusionSourceB)
                    && GetMastery(definition.FusionSourceA)
                        >= definition.FusionMasteryRequired
                    && GetMastery(definition.FusionSourceB)
                        >= definition.FusionMasteryRequired;
            }

            if (!string.IsNullOrEmpty(definition.ParentSkillId))
            {
                return _unlocked.Contains(definition.ParentSkillId)
                    && GetMastery(definition.ParentSkillId)
                        >= definition.ParentMasteryRequired;
            }

            return false;
        }
    }
}
