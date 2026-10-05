namespace KaiChe.Core.Content
{
    public static class ContentIds
    {
        public const string StarterSkill = "SKILL_0001";
        public const string IceBall = "SKILL_0004";
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
        {            Id = id;
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
        {            Id = id;
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

        public static readonly SkillDefinition IceBall =
            new SkillDefinition(
                ContentIds.IceBall,
                "아이스볼",
                14f,
                1.15f,
                6.5f,
                9.5f);

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