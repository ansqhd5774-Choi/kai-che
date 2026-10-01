namespace KaiChe.Core.Content
{
    public static class ContentIds
    {
        public const string StarterSkill = "SKILL_0001";
        public const string StarterMonster = "MONSTER_0001";
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
                "기본 마력탄",
                20f,
                0.55f,
                6.2f,
                11f);

        public static readonly MonsterDefinition StarterMonster =
            new MonsterDefinition(
                ContentIds.StarterMonster,
                "포복 마수",
                40f,
                1.35f,
                18f,
                1);
    }
}