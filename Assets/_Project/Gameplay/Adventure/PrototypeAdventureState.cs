using UnityEngine;

namespace KaiChe.Gameplay.Adventure
{
    public static class PrototypeAdventureState
    {
        private const string Prefix = "KaiChe.Adventure.";
        public const float MaxHealth = 100f;

        private static bool _loaded;

        public static int PotionCount { get; private set; }
        public static float Health { get; private set; } = MaxHealth;

        public static void EnsureLoaded()
        {
            if (_loaded) return;

            PotionCount = PlayerPrefs.GetInt(Prefix + "PotionCount", 0);
            Health = Mathf.Clamp(
                PlayerPrefs.GetFloat(Prefix + "Health", MaxHealth),
                0f,
                MaxHealth);
            _loaded = true;
        }
        public static void SetHealth(float health)
        {
            EnsureLoaded();
            Health = Mathf.Clamp(health, 0f, MaxHealth);
            PlayerPrefs.SetFloat(Prefix + "Health", Health);
            PlayerPrefs.Save();
        }

        public static void AddPotion(int amount = 1)
        {
            if (amount <= 0) return;
            EnsureLoaded();

            PotionCount += amount;
            SaveInventory();
        }

        public static void Rest()
        {
            EnsureLoaded();
            SetHealth(MaxHealth);
        }

        private static void SaveInventory()
        {
            PlayerPrefs.SetInt(Prefix + "PotionCount", PotionCount);
            PlayerPrefs.Save();
        }
    }
}
