namespace KaiChe.Core.Content
{
    public static class ContentIds
    {
        public const string StarterSkill = "SKILL_0001";
        public const string WindBlade = "SKILL_0002";
        public const string ArcSpark = "SKILL_0003";
        public const string IceBall = "SKILL_0004";
        public const string PoisonOrb = "SKILL_0005";
        public const string StoneShot = "SKILL_0006";
        public const string LightOrb = "SKILL_0007";
        public const string DarkOrb = "SKILL_0008";
        public const string WaterOrb = "SKILL_0009";
        public const string MagicArrow = "SKILL_0010";

        public const string StarterMonster = "MONSTER_0001";
        public const string RunnerMonster = "MONSTER_0002";
        public const string BruteMonster = "MONSTER_0003";
        public const string BossMonster = "MONSTER_0004";
    }

    public sealed class SkillDefinition
    {
        public string Id { get; }
        public string DisplayName { get; }
        public float Damage { get; }
        public float Cooldown { get; }
        public float Range { get; }
        public float ProjectileSpeed { get; }

        public SkillDefinition(
            string id,
            string displayName,
            float damage,
            float cooldown,
            float range,
            float projectileSpeed)
        {
            Id = id;
            DisplayName = displayName;
            Damage = damage;
            Cooldown = cooldown;
            Range = range;
            ProjectileSpeed = projectileSpeed;
        }
    }

    public sealed class MonsterDefinition
    {
        public string Id { get; }
        public string DisplayName { get; }
        public float MaxHealth { get; }
        public float MoveSpeed { get; }
        public float ContactDamagePerSecond { get; }
        public int ExperienceReward { get; }

        public MonsterDefinition(
            string id,
            string displayName,
            float maxHealth,
            float moveSpeed,
            float contactDamagePerSecond,
            int experienceReward)
        {
            Id = id;
            DisplayName = displayName;
            MaxHealth = maxHealth;
            MoveSpeed = moveSpeed;
            ContactDamagePerSecond = contactDamagePerSecond;
            ExperienceReward = experienceReward;
        }
    }

    public static class RuntimeContentCatalog
    {
        public static readonly SkillDefinition StarterSkill =
            new SkillDefinition(
                ContentIds.StarterSkill,
                "파이어볼",
                20f,
                0.55f,
                6.2f,
                11f);

        public static readonly SkillDefinition WindBlade =
            new SkillDefinition(
                ContentIds.WindBlade,
                "윈드 블레이드",
                16f,
                0.70f,
                6.8f,
                13f);

        public static readonly SkillDefinition ArcSpark =
            new SkillDefinition(
                ContentIds.ArcSpark,
                "아크 스파크",
                13f,
                0.62f,
                6.0f,
                12.5f);

        public static readonly SkillDefinition IceBall =
            new SkillDefinition(
                ContentIds.IceBall,
                "아이스볼",
                14f,
                1.15f,
                6.5f,
                9.5f);

        public static readonly SkillDefinition PoisonOrb =
            new SkillDefinition(
                ContentIds.PoisonOrb,
                "포이즌 오브",
                12f,
                0.92f,
                6.1f,
                9.0f);

        public static readonly SkillDefinition StoneShot =
            new SkillDefinition(
                ContentIds.StoneShot,
                "스톤 샷",
                24f,
                1.05f,
                5.6f,
                8.5f);

        public static readonly SkillDefinition LightOrb =
            new SkillDefinition(
                ContentIds.LightOrb,
                "라이트 오브",
                15f,
                0.78f,
                6.6f,
                11.5f);

        public static readonly SkillDefinition DarkOrb =
            new SkillDefinition(
                ContentIds.DarkOrb,
                "다크 오브",
                19f,
                0.88f,
                6.0f,
                10.0f);

        public static readonly SkillDefinition WaterOrb =
            new SkillDefinition(
                ContentIds.WaterOrb,
                "워터 오브",
                14f,
                0.72f,
                6.4f,
                10.5f);

        public static readonly SkillDefinition MagicArrow =
            new SkillDefinition(
                ContentIds.MagicArrow,
                "매직 애로우",
                17f,
                0.50f,
                7.2f,
                14.0f);

        public static readonly SkillDefinition[] BaseSkills =
        {
            StarterSkill,
            WindBlade,
            ArcSpark,
            IceBall,
            PoisonOrb,
            StoneShot,
            LightOrb,
            DarkOrb,
            WaterOrb,
            MagicArrow
        };

        public static SkillDefinition FindSkill(string skillId)
        {
            for (int i = 0; i < BaseSkills.Length; i++)
            {
                if (BaseSkills[i].Id == skillId)
                    return BaseSkills[i];
            }

            return null;
        }

        public static readonly MonsterDefinition StarterMonster =
            new MonsterDefinition(
                ContentIds.StarterMonster,
                "포복 마수",
                40f,
                1.35f,
                18f,
                1);

        public static readonly MonsterDefinition RunnerMonster =
            new MonsterDefinition(
                ContentIds.RunnerMonster,
                "질주 마수",
                26f,
                2.15f,
                12f,
                1);

        public static readonly MonsterDefinition BruteMonster =
            new MonsterDefinition(
                ContentIds.BruteMonster,
                "철갑 마수",
                95f,
                0.82f,
                28f,
                3);

        public static readonly MonsterDefinition BossMonster =
            new MonsterDefinition(
                ContentIds.BossMonster,
                "균열 군주",
                420f,
                0.72f,
                34f,
                12);

        public static readonly MonsterDefinition[] Monsters =
        {
            StarterMonster,
            RunnerMonster,
            BruteMonster,
            BossMonster
        };
    }
}
